using Community.ContenType.DeploymentHub.Domain.Requests;
using Community.ContenType.DeploymentHub.Contracts.Messages;
using Community.ContenType.DeploymentHub.Contracts.Repositories;

namespace Community.ContenType.DeploymentHub.DomainServices
{
    public class PullRequestRetriever : IPullRequestRetriever
    {
        private readonly IStatusListRepository _statusListRepository;
        private readonly ITermStoreRepository _termStoreRepository;

        public PullRequestRetriever(IStatusListRepository statusListRepository, ITermStoreRepository termStoreRepository)
        {
            _statusListRepository = statusListRepository;
            _termStoreRepository = termStoreRepository;
        }

        public PullRequest GetPendingRequest(PullRequestInfo info)
        {
            var request = _statusListRepository.GetPendingRequest(info);
            request.DeploymentGroup = _termStoreRepository.GetDeploymentGroupById(request.DeploymentGroup.TermId);
            return request;
        }
    }
}
