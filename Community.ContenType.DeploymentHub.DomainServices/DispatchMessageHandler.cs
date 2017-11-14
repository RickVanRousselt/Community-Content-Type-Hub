using System;
using Community.ContenType.DeploymentHub.Contracts.Repositories;

namespace Community.ContenType.DeploymentHub.DomainServices
{
    public class DispatchMessageHandler
    {
        private readonly IDispatchedQueueRepository _dispatchedQueueRepository;
        private readonly bool _useDispatchQueue;

        public DispatchMessageHandler(IDispatchedQueueRepository dispatchedQueueRepository, bool useDispatchQueue)
        {
            _dispatchedQueueRepository = dispatchedQueueRepository;
            _useDispatchQueue = useDispatchQueue;
        }

        public void FlagRequestFinished(Guid actionCollectionId)
        {
            if (_useDispatchQueue)
            {
                _dispatchedQueueRepository.DeleteDispatchQueueMessage(actionCollectionId);
            }
        }
    }
}
