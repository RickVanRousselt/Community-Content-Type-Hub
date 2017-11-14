using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Domain.Core;
using log4net;

namespace Community.ContenType.DeploymentHub.SharePoint.Updaters.SiteColumn
{
    public class XmlSiteColumnUpdater : ISiteColumnUpdater
    {
        private readonly IClientContextProvider _targetContextProvider;
        private readonly ManualSiteColumnUpdater _fallbackUpdater;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(XmlSiteColumnUpdater));

        public XmlSiteColumnUpdater(IClientContextProvider targetContextProvider, ManualSiteColumnUpdater fallbackUpdater)
        {
            _targetContextProvider = targetContextProvider;
            _fallbackUpdater = fallbackUpdater;
        }

        public void Update(PublishedSiteColumn publishedSiteColumn)
        {
            using (var clientContext = _targetContextProvider.CreateClientContext())
            {
                //If there are problems with a specific type use the fallbackupdater instead of schema xml method.
                if (publishedSiteColumn is PublishedTaxonomySiteColumn)
                {
                    _fallbackUpdater.Update(publishedSiteColumn, true);
                }
                else
                {
                    var sharePointSiteColumn = clientContext.Site.RootWeb.Fields.GetById(publishedSiteColumn.Id);
                    sharePointSiteColumn.SchemaXml = publishedSiteColumn.SchemaWithoutVersion.ToString();
                    sharePointSiteColumn.UpdateAndPushChanges(true);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
            }
        }
    }
}