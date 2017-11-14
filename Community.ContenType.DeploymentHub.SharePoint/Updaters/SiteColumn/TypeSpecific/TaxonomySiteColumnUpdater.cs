using System;
using System.Xml.XPath;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Microsoft.SharePoint.Client.Taxonomy;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.SharePoint.Updaters.SiteColumn.TypeSpecific
{
    internal class TaxonomySiteColumnUpdater : TypeSpecificSiteColumnUpdater<TaxonomyField>
    {
        private readonly ITermStoreRepository _termStoreRepository;
        private readonly PublishedTaxonomySiteColumn _publishedSiteColumn;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TaxonomySiteColumnUpdater));

        public TaxonomySiteColumnUpdater(ITermStoreRepository termStoreRepository, TaxonomyField siteColumn, PublishedTaxonomySiteColumn publishedSiteColumn)
            : base(siteColumn, publishedSiteColumn)
        {
            _termStoreRepository = termStoreRepository;
            _publishedSiteColumn = publishedSiteColumn;
        }

        private May<string> GetTaxonomyProperty(string name) => 
            Schema.XPathSelectElement($"Customization/ArrayOfProperty/Property[Name={name}]/Value")?.Value.Maybe() ?? May.NoValue;

        private May<bool> AllowMultipleValues => GetAttributeValue("Mult").Select(ParseBool);
        private May<bool> IsPathRendered => GetTaxonomyProperty("IsPathRendered").Select(ParseBool);
        private May<bool> Open => GetTaxonomyProperty("Open").Select(ParseBool);

        protected override void UpdateDefaultValue()
        {
            //contains ID for taxonomy fields, convert with path
            var maybeId = _publishedSiteColumn.DefaultValuePath.Bind(_termStoreRepository.MaybeGetTermIdentifierByPath);
            maybeId.ElseDo(() => Logger.Warn($"Could not find TermIdentifier for DefaultValuePath {_publishedSiteColumn.DefaultValuePath} in {_publishedSiteColumn} or default value was empty."));
            var maybeTuple = DefaultValue.Combine(maybeId, Tuple.Create);
            maybeTuple
                .Select(tuple => $"-1;#{_publishedSiteColumn.Title}|{tuple.Item2.AnchorId:D}")
                .IfHasValueThenDo(x => SiteColumn.DefaultValue = x);
        }

        protected override void UpdateSpecificFields()
        {
            var maybeId = _termStoreRepository.MaybeGetTermIdentifierByPath(_publishedSiteColumn.Path);
            SiteColumn.Context.Load(SiteColumn, s => s.SspId);
            SiteColumn.Context.ExecuteQueryWithIncrementalRetry();
            var maySspId = _termStoreRepository.MaybeGetNewSspId(SiteColumn.SspId);
            maySspId.IfHasValueThenDo(i => SiteColumn.SspId = i);

            maybeId.IfHasValueThenDo(identifier =>
            {
                SiteColumn.TermSetId = identifier.TermSetId;
                SiteColumn.AnchorId = identifier.AnchorId;
            });

           

            maybeId.ElseDo(() => Logger.Warn($"Could not find TermIdentifier for Path {_publishedSiteColumn.Path} in {_publishedSiteColumn}."));

            AllowMultipleValues.IfHasValueThenDo(x => SiteColumn.AllowMultipleValues = x);
            IsPathRendered.IfHasValueThenDo(x => SiteColumn.IsPathRendered = x);
            Open.IfHasValueThenDo(x => SiteColumn.Open = x);
        }
    }
}