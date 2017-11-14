using Community.ContenType.DeploymentHub.Domain.Requests;
using Community.ContenType.DeploymentHub.Contracts.Messages;

namespace Community.ContenType.DeploymentHub.DomainServices
{
    public interface IRequestRetriever<in TRequestInfo, out TRequest>
        where TRequestInfo : IRequestInfo
        where TRequest : IRequest
    {
        TRequest GetPendingRequest(TRequestInfo info);
    }

    public interface IPublishRequestRetriever : IRequestRetriever<PublishRequestInfo, PublishRequest> { }
    public interface IPushRequestRetriever : IRequestRetriever<PushRequestInfo, PushRequest> { }
    public interface IPullRequestRetriever : IRequestRetriever<PullRequestInfo, PullRequest> { }
    public interface IPromoteRequestRetriever : IRequestRetriever<PromoteRequestInfo, PromoteRequest> { }
}