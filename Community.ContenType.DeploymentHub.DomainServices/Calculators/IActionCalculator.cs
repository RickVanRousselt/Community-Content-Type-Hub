using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Actions.Promote;
using Community.ContenType.DeploymentHub.Domain.Actions.Publish;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using Community.ContenType.DeploymentHub.Domain.Requests;

namespace Community.ContenType.DeploymentHub.DomainServices.Calculators
{
    public interface IActionCalculator<in TRequest, out TActionCollection, TSiteColumnAction, TContentTypeAction>
        where TRequest : IRequest
        where TActionCollection : ActionCollectionBase<TSiteColumnAction, TContentTypeAction>
        where TSiteColumnAction : ActionBase
        where TContentTypeAction : ActionBase
    {
        TActionCollection CalculateActions(TRequest info);
    }

    public interface IPublishActionCalculator : IActionCalculator<PublishRequest, PublishActionCollection, PublishSiteColumnAction, PublishContentTypeAction> { }
    public interface IPushActionCalculator : IActionCalculator<PushRequest, PushActionCollection, PushSiteColumnAction, PushContentTypeAction> { }
    public interface IPullActionCalculator : IActionCalculator<PullRequest, PushActionCollection, PushSiteColumnAction, PushContentTypeAction> { }
    public interface IPromoteActionCalculator : IActionCalculator<PromoteRequest, PromoteActionCollection, PromoteSiteColumnAction, PromoteContentTypeAction> { }
}
