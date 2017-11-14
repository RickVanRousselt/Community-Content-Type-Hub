using System;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain;
using Community.ContenType.DeploymentHub.Domain.Actions.Promote;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Verifiers;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.DomainServices.Verifiers.Promote
{
    public class HubAllowedRule : VerificationRuleBase<PromoteSiteColumnAction, PromoteContentTypeAction>, IPromoteVerificationRule
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HubAllowedRule));
        private readonly Func<SiteCollection, IWebPropertyRepository> _webPropertyRepositoryGenerator;

        public override VerificationImpactLevel Level => VerificationImpactLevel.SiteCollection;

        public HubAllowedRule(Func<SiteCollection, IWebPropertyRepository> webPropertyRepositoryGenerator)
        {
            _webPropertyRepositoryGenerator = webPropertyRepositoryGenerator;
        }

        public override VerificationRuleResult Verify(PromoteSiteColumnAction action)
        {
            using (Logger.MethodTraceLogger(action.SiteColumn.Title, action.TargetHub.Url))
            {
                try
                {
                    return HasTargetGivenPushSourceOrNone(action.ActionContext.Hub, action.Target)
                        ? VerificationRuleResult.Success
                        : VerificationRuleResult.Failed(
                            $"Site {action.TargetHub} is already receiving from another Content Type Deployment Hub. A Site Collection cannot receive from more than one Hub");

                }
                catch (Exception ex)
                {
                    Logger.Error("Error getting webproperty in HubAllowedRule", ex);
                    return VerificationRuleResult.Failed($"Site {action.TargetHub} is not accessible.");
                }
            }
        }

        public override VerificationRuleResult Verify(PromoteContentTypeAction action)
        {
            using (Logger.MethodTraceLogger(action.ContentType.Title, action.TargetHub.Url))
            {
                try
                {
                    return HasTargetGivenPushSourceOrNone(action.ActionContext.Hub, action.Target)
                        ? VerificationRuleResult.Success
                        : VerificationRuleResult.Failed($"Site {action.TargetHub} is already receiving from another Content Type Deployment Hub. A Site Collection cannot receive from more than one Hub");
                }
                catch (Exception ex)
                {
                    Logger.Error("Error getting webproperty in HubAllowedRule", ex);
                    return VerificationRuleResult.Failed($"Site {action.TargetHub} is not accessible.");
                }
            }
        }

        private bool HasTargetGivenPushSourceOrNone(Hub sourceHub, SiteCollection target) =>
            MaybeGetPushSource(target)
                .Select(source => source.Equals(sourceHub))
                .Else(true);

        protected virtual May<Hub> MaybeGetPushSource(SiteCollection target) => 
            _webPropertyRepositoryGenerator(target)
                .MaybeGetProperty<string>(Configs.PushSource)
                .Select(p => new Hub(new Uri(p)));
    }
}