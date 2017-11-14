using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Domain
{
    public class Groupings
    {
        private readonly ICollection<KeyValuePair<DeploymentGroup, SiteCollection>> _siteCollectionGroupings;
        private readonly ICollection<KeyValuePair<DeploymentGroup, ContentTypeInfo>> _contentTypeGroupings;

        public Groupings(
            ICollection<KeyValuePair<DeploymentGroup, SiteCollection>> siteCollectionGroupings, 
            ICollection<KeyValuePair<DeploymentGroup, ContentTypeInfo>> contentTypeGroupings)
        {
            _siteCollectionGroupings = siteCollectionGroupings;
            _contentTypeGroupings = contentTypeGroupings;
        }

        public ISet<ContentTypeInfo> ContentTypes => new HashSet<ContentTypeInfo>(_contentTypeGroupings.Select(g => g.Value));
        public ISet<DeploymentGroup> DeploymentGroups => new HashSet<DeploymentGroup>(_siteCollectionGroupings.Select(g => g.Key));
        public ISet<SiteCollection> SiteCollections => new HashSet<SiteCollection>(_siteCollectionGroupings.Select(g => g.Value));


        public ILookup<DeploymentGroup, SiteCollection> GetSiteCollectionsPerDeploymentGroup() =>
            _siteCollectionGroupings.ToLookup(kvp => kvp.Key, kvp => kvp.Value);

        public IEnumerable<SiteCollection> GetSiteCollectionsFor(DeploymentGroup deploymentGroup) =>
            GetSiteCollectionsPerDeploymentGroup()[deploymentGroup];

        public ILookup<SiteCollection, DeploymentGroup> GetDeploymentGroupsPerSiteCollection() =>
            _siteCollectionGroupings.ToLookup(kvp => kvp.Value, kvp => kvp.Key);

        public IEnumerable<DeploymentGroup> GetDeploymentGroupsFor(SiteCollection siteCollection) =>
            GetDeploymentGroupsPerSiteCollection()[siteCollection];


        public ILookup<DeploymentGroup, ContentTypeInfo> GetContentTypesPerDeploymentGroup() =>
            _contentTypeGroupings.ToLookup(kvp => kvp.Key, kvp => kvp.Value);

        public ISet<ContentTypeInfo> GetContentTypesFor(DeploymentGroup deploymentGroup) =>
            new HashSet<ContentTypeInfo>(GetContentTypesPerDeploymentGroup()[deploymentGroup]);

        public ILookup<ContentTypeInfo, DeploymentGroup> GetDeploymentGroupsPerContentType() =>
            _contentTypeGroupings.ToLookup(kvp => kvp.Value, kvp => kvp.Key);

        public IEnumerable<DeploymentGroup> GetDeploymentGroupsFor(ContentTypeInfo contentTypeInfo) =>
            GetDeploymentGroupsPerContentType()[contentTypeInfo];

        public IEnumerable<ContentTypeInfo> GetContentTypesByIds(ISet<string> ids) =>
            _contentTypeGroupings.Select(kvp => kvp.Value).Where(ct => ids.Contains(ct.Id));


        private IEnumerable<KeyValuePair<ContentTypeInfo, SiteCollection>> GetJoinedGroupings() => 
            _contentTypeGroupings.Join(_siteCollectionGroupings,
                kvp => kvp.Key,
                kvp => kvp.Key,
                (kvp1, kvp2) => new KeyValuePair<ContentTypeInfo, SiteCollection>(kvp1.Value, kvp2.Value));

        public ILookup<ContentTypeInfo, SiteCollection> GetSiteCollectionsPerContentType() => 
            GetJoinedGroupings().ToLookup(x => x.Key, x => x.Value);

        public ILookup<SiteCollection, ContentTypeInfo> GetContentTypesPerSiteCollection() => 
            GetJoinedGroupings().ToLookup(x => x.Value, x => x.Key);

        public IEnumerable<SiteCollection> GetSiteCollectionsFor(ContentTypeInfo contentTypeInfo) =>
            GetSiteCollectionsPerContentType()[contentTypeInfo];

        public IEnumerable<ContentTypeInfo> GetContentTypesFor(SiteCollection siteCollection) =>
            GetContentTypesPerSiteCollection()[siteCollection];
    }
}