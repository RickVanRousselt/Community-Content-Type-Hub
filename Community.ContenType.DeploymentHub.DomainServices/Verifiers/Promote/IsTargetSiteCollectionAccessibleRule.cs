using System;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Actions.Promote;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Verifiers;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;

namespace Community.ContenType.DeploymentHub.DomainServices.Verifiers.Promote
{
    public class IsTargetSiteCollectionAccessibleRule : VerificationRuleBase<PromoteSiteColumnAction, PromoteContentTypeAction>, IPromoteVerificationRule
    {
        private readonly Func<SiteCollection, IUserRepository> _userRepositoryGenerator;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IsTargetSiteCollectionAccessibleRule));
        public override VerificationImpactLevel Level => VerificationImpactLevel.SiteCollection;

        public IsTargetSiteCollectionAccessibleRule(Func<SiteCollection, IUserRepository> userRepositoryGenerator)
        {
            _userRepositoryGenerator = userRepositoryGenerator;
        }

        public override VerificationRuleResult Verify(PromoteSiteColumnAction action)
        {
            using (Logger.MethodTraceLogger(action.SiteColumn.Title, action.TargetHub.Url))
            {
                return IsCurrentUserSiteAdmin(action.TargetHub)
                    ? VerificationRuleResult.Success 
                    : VerificationRuleResult.Failed($"Functional user does not have (sufficient) access to target site collection: {action.TargetHub}");
            }
        }

        public override VerificationRuleResult Verify(PromoteContentTypeAction action)
        {
            using (Logger.MethodTraceLogger(action.ContentType.Title, action.TargetHub.Url))
            {
                return IsCurrentUserSiteAdmin(action.TargetHub)
                    ? VerificationRuleResult.Success
                    : VerificationRuleResult.Failed($"Functional user does not have (sufficient) access to target site collection: {action.TargetHub}");
            }
        }

        protected virtual bool IsCurrentUserSiteAdmin(Hub target) => 
            _userRepositoryGenerator(target).IsCurrentUserSiteAdmin();
    }
}
