using System;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Verifiers;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;

namespace Community.ContenType.DeploymentHub.DomainServices.Verifiers.Push
{
    public class IsTargetSiteCollectionAccessibleRule : VerificationRuleBase<PushSiteColumnAction, PushContentTypeAction>, IPushVerificationRule
    {
        private readonly Func<SiteCollection, IUserRepository> _userRepositoryGenerator;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IsTargetSiteCollectionAccessibleRule));
        public override VerificationImpactLevel Level => VerificationImpactLevel.SiteCollection;

        public IsTargetSiteCollectionAccessibleRule(Func<SiteCollection, IUserRepository> userRepositoryGenerator)
        {
            _userRepositoryGenerator = userRepositoryGenerator;
        }

        public override VerificationRuleResult Verify(PushSiteColumnAction action)
        {
            using (Logger.MethodTraceLogger(action.SiteColumn.Title, action.TargetSiteCollection))
            {
                Console.WriteLine(@"Target site collection accessible");
                return IsCurrentUserSiteAdmin(action.TargetSiteCollection)
                    ? VerificationRuleResult.Success
                    : VerificationRuleResult.Failed($"Functional user does not have (sufficient) access to target site collection: {action.TargetSiteCollection}");
            }
        }

        public override VerificationRuleResult Verify(PushContentTypeAction action)
        {
            using (Logger.MethodTraceLogger(action.ContentType.Title, action.TargetSiteCollection))
            {
                Console.WriteLine(@"Target site collection accessible");
                return IsCurrentUserSiteAdmin(action.TargetSiteCollection)
                    ? VerificationRuleResult.Success
                    : VerificationRuleResult.Failed($"Functional user does not have (sufficient) access to target site collection: {action.TargetSiteCollection}");
            }
        }

        protected virtual bool IsCurrentUserSiteAdmin(SiteCollection target) =>
            _userRepositoryGenerator(target).IsCurrentUserSiteAdmin();
    }
}
