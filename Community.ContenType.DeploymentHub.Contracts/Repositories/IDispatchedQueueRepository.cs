using System;
using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Contracts.Messages;

namespace Community.ContenType.DeploymentHub.Contracts.Repositories
{
    public interface IDispatchedQueueRepository
    {
        IEnumerable<DispatchQueueMessage> GetAllQueueMessages();
        void UpdateQueueMessageStatus(Guid actionCollectionId, DispatchQueueMessageStatus status);
        void DeleteDispatchQueueMessage(Guid actionCollectionId);
    }
}
