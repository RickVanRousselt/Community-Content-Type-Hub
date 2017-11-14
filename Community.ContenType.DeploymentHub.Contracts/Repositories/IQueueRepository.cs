using Community.ContenType.DeploymentHub.Contracts.Messages;

namespace Community.ContenType.DeploymentHub.Contracts.Repositories
{
    public interface IQueueRepository
    {
        void AddToQueue(PublishRequestInfo info);
        void AddToQueue(PushRequestInfo info);
        void AddToQueue(PromoteRequestInfo info);
        void AddToQueue(PullRequestInfo info);
    }
}