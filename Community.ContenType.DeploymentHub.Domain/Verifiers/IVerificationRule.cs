using Community.ContenType.DeploymentHub.Domain.Actions.Promote;
using Community.ContenType.DeploymentHub.Domain.Actions.Publish;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;

namespace Community.ContenType.DeploymentHub.Domain.Verifiers
{
    public interface IVerificationRule<in TSiteColumnAction, in TContentTypeAction>
    {
        string Name { get; }
        VerificationImpactLevel Level { get; }
        VerificationRuleResult Verify(TSiteColumnAction siteColumnAction);
        VerificationRuleResult Verify(TContentTypeAction contentTypeAction);
    }

    public interface IPublishVerificationRule : IVerificationRule<PublishSiteColumnAction, PublishContentTypeAction> { }
    public interface IPushVerificationRule : IVerificationRule<PushSiteColumnAction, PushContentTypeAction> { }
    public interface IPromoteVerificationRule : IVerificationRule<PromoteSiteColumnAction, PromoteContentTypeAction> { }
}