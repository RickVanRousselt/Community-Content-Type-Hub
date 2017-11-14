using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using Community.ContenType.DeploymentHub.SharePoint.Updaters.SiteColumn.TypeSpecific;
using log4net;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.SharePoint.Updaters.SiteColumn
{
    public class ManualSiteColumnUpdater : ISiteColumnUpdater
    {
        private readonly IClientContextProvider _targetContextProvider;
        private readonly ITermStoreRepository _termStoreRepository;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ManualSiteColumnUpdater));

        public ManualSiteColumnUpdater(IClientContextProvider targetContextProvider, ITermStoreRepository termStoreRepository)
        {
            _targetContextProvider = targetContextProvider;
            _termStoreRepository = termStoreRepository;
        }

        public void Update(PublishedSiteColumn publishedSiteColumn)
        {
            Update(publishedSiteColumn, false);
        }

        internal void Update(PublishedSiteColumn publishedSiteColumn, bool taxonomyOnly)
        {
            using (var clientContext = _targetContextProvider.CreateClientContext())
            {
                var targetSiteColumn = clientContext.Site.RootWeb.Fields.GetById(publishedSiteColumn.Id);
                clientContext.Load(targetSiteColumn, x => x.TypeAsString);
                clientContext.ExecuteQueryWithIncrementalRetry();
                var maybeUpdater = CreateTypeSpecificSiteColumnUpdater(publishedSiteColumn, targetSiteColumn, clientContext);
                maybeUpdater.IfHasValueThenDo(updater => updater.Update(taxonomyOnly));
                maybeUpdater.ElseDo(() => Logger.Warn($"The type of {publishedSiteColumn} is not supported by this update method, skipping action..."));
            }
        }

        private May<ITypeSpecificSiteColumnUpdater> CreateTypeSpecificSiteColumnUpdater(PublishedSiteColumn publishedSiteColumn, Field targetSiteColumn, ClientContext clientContext)
        {
            var publishedTaxonomySiteColumn = publishedSiteColumn as PublishedTaxonomySiteColumn;
            var targetTaxonomySiteColumn = CastToTaxonomyField(targetSiteColumn, clientContext);
            if (publishedSiteColumn == null ^ targetTaxonomySiteColumn == null) //one is null, the other not
            {
                Logger.Warn($"{publishedSiteColumn} has been converted to or from a TaxonomyField. It will not be updated.");
                return May.NoValue;
            }

            return publishedTaxonomySiteColumn == null
                ? CreateFieldUpdaterForRegularFieldIfTypeIsSupported(publishedSiteColumn, targetSiteColumn)
                : new TaxonomySiteColumnUpdater(_termStoreRepository, targetTaxonomySiteColumn, publishedTaxonomySiteColumn);
        }

        private static TaxonomyField CastToTaxonomyField(Field targetSiteColumn, ClientContext clientContext)
        {
            if (targetSiteColumn.TypeAsString == "TaxonomyFieldType" ||
                targetSiteColumn.TypeAsString == "TaxonomyFieldTypeMulti")
            {
                return clientContext.CastTo<TaxonomyField>(targetSiteColumn);
            }
            return null;
        }

        private static May<ITypeSpecificSiteColumnUpdater> CreateFieldUpdaterForRegularFieldIfTypeIsSupported(PublishedSiteColumn publishedSiteColumn, Field targetSiteColumn)
        {
            var type = TypeSpecificSiteColumnUpdater<Field>.GetType(publishedSiteColumn);
            switch (type)
            {
                case FieldType.Text:
                    return new TextSiteColumnUpdater(targetSiteColumn as FieldText, publishedSiteColumn);
                case FieldType.Note:
                    return new NoteSiteColumnUpdater(targetSiteColumn as FieldMultiLineText, publishedSiteColumn);
                case FieldType.Choice:
                    return new ChoiceSiteColumnUpdater(targetSiteColumn as FieldChoice, publishedSiteColumn);
                case FieldType.MultiChoice:
                    return new MultiChoiceSiteColumnUpdater(targetSiteColumn as FieldMultiChoice, publishedSiteColumn);
                case FieldType.Number:
                    return new NumberSiteColumnUpdater(targetSiteColumn as FieldNumber, publishedSiteColumn);
                case FieldType.Currency:
                    return new CurrencySiteColumnUpdater(targetSiteColumn as FieldCurrency, publishedSiteColumn);
                case FieldType.User:
                    return new UserSiteColumnUpdater(targetSiteColumn as FieldUser, publishedSiteColumn);
                default:
                    return May.NoValue;
            }
        }
    }
}