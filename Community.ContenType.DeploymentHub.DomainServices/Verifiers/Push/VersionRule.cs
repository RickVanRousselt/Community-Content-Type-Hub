using System;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using Community.ContenType.DeploymentHub.Domain.Verifiers;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;

namespace Community.ContenType.DeploymentHub.DomainServices.Verifiers.Push
{
    public class VersionRule : VerificationRuleBase<PushSiteColumnAction, PushContentTypeAction>, IPushVerificationRule
    {
        public override VerificationImpactLevel Level => VerificationImpactLevel.Single;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(VersionRule));
        private readonly IDbEntryRepository _dbEntryRepository;

        public VersionRule(IDbEntryRepository dbEntryRepository)
        {
            _dbEntryRepository = dbEntryRepository;
        }

        public override VerificationRuleResult Verify(PushSiteColumnAction action)
        {
            using (Logger.MethodTraceLogger(action.SiteColumn.Title, action.TargetSiteCollection))
            {
                Console.WriteLine(@"Verify version");
                return _dbEntryRepository.IsVersionDeployed(action)
                    ? VerificationRuleResult.Failed($"SiteCollection {action.TargetSiteCollection} already has version {action.SiteColumn.Version} of {action.SiteColumn}")
                    : VerificationRuleResult.Success;
            }
        }

        public override VerificationRuleResult Verify(PushContentTypeAction action)
        {
            using (Logger.MethodTraceLogger(action.ContentType.Title, action.TargetSiteCollection))
            {
                Console.WriteLine(@"Verify version");
                return _dbEntryRepository.IsVersionDeployed(action)
                    ? VerificationRuleResult.Failed($"SiteCollection {action.TargetSiteCollection} already has version {action.ContentType.Version} of {action.ContentType}")
                    : VerificationRuleResult.Success;
            }
        }
    }
}
