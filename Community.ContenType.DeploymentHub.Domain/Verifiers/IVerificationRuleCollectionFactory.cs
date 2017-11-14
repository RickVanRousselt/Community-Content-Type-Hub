using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Actions.Promote;
using Community.ContenType.DeploymentHub.Domain.Actions.Publish;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;

namespace Community.ContenType.DeploymentHub.Domain.Verifiers
{
    public interface IVerificationRuleCollectionFactory<out TVerificationRuleCollection, TVerificationRule, TSiteColumnAction, TContentTypeAction>
        where TVerificationRuleCollection : VerificationRuleCollection<TVerificationRule, TSiteColumnAction, TContentTypeAction>
        where TVerificationRule : IVerificationRule<TSiteColumnAction, TContentTypeAction>
        where TSiteColumnAction : ActionBase
        where TContentTypeAction : ActionBase
    {
        TVerificationRuleCollection CreateVerificationRules();
    }

    public interface IPublishVerificationRuleCollectionFactory : IVerificationRuleCollectionFactory<PublishVerificationRuleCollection, IPublishVerificationRule, PublishSiteColumnAction, PublishContentTypeAction> { }
    public interface IPushVerificationRuleCollectionFactory : IVerificationRuleCollectionFactory<PushVerificationRuleCollection, IPushVerificationRule, PushSiteColumnAction, PushContentTypeAction> { }
    public interface IPromoteVerificationRuleCollectionFactory : IVerificationRuleCollectionFactory<PromoteVerificationRuleCollection, IPromoteVerificationRule, PromoteSiteColumnAction, PromoteContentTypeAction> { }
}
