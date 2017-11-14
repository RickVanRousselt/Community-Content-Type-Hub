using System;
using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Contracts.Repositories
{
    public interface ISiteCollectionGroupingListRepository
    {
        void EnsureSiteCollectionGroupingList(Guid deploymentGroupsTermSetId);
        IEnumerable<KeyValuePair<DeploymentGroup, SiteCollection>> GetSiteCollectionGroupPairs();
        void AddSiteCollectionToDeploymentGroup(DeploymentGroup deploymentGroup, Uri siteCollectionUrl);
    }
}