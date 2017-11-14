using System.Linq;
using Community.ContenType.DeploymentHub.Domain;
using Community.ContenType.DeploymentHub.Contracts.Repositories;

namespace Community.ContenType.DeploymentHub.SharePoint.Repositories
{
    public class GroupingRepository : IGroupingRepository
    {
        private readonly ISiteCollectionGroupingListRepository _siteCollectionGroupingListRepository;
        private readonly IContentTypeGroupingListRepository _contentTypeGroupingListRepository;

        public GroupingRepository(ISiteCollectionGroupingListRepository siteCollectionGroupingListRepository, IContentTypeGroupingListRepository contentTypeGroupingListRepository1)
        {
            _siteCollectionGroupingListRepository = siteCollectionGroupingListRepository;
            _contentTypeGroupingListRepository = contentTypeGroupingListRepository1;
        }

        public Groupings GetGroupings() => new Groupings(
            _siteCollectionGroupingListRepository.GetSiteCollectionGroupPairs().ToList(), 
            _contentTypeGroupingListRepository.GetContentTypeGroupPairs().ToList());
    }
}
