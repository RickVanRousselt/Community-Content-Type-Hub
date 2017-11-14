using System;
using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Contracts.Repositories
{
    public interface IContentTypeGroupingListRepository
    {
        void EnsureContentTypeGroupingList(Guid deploymentGroupsTermSetId);
        IEnumerable<KeyValuePair<DeploymentGroup, ContentTypeInfo>> GetContentTypeGroupPairs();
    }
}