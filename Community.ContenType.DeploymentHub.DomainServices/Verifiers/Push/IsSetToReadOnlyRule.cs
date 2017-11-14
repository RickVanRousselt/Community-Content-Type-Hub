using System;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Verifiers;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;

namespace Community.ContenType.DeploymentHub.DomainServices.Verifiers.Push
{
    public class IsSetToReadOnlyRule : VerificationRuleBase<PushSiteColumnAction, PushContentTypeAction>, IPushVerificationRule
    {
        private readonly Func<SiteCollection, IContentTypeRepository> _contentTypeRepositoryGenerator;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IsSetToReadOnlyRule));
        public override VerificationImpactLevel Level => VerificationImpactLevel.SiteCollection;

        public IsSetToReadOnlyRule(Func<SiteCollection, IContentTypeRepository> contentTypeRepositoryGenerator)
        {
            _contentTypeRepositoryGenerator = contentTypeRepositoryGenerator;
        }

        public override VerificationRuleResult Verify(PushSiteColumnAction action) => VerificationRuleResult.Success;

        public override VerificationRuleResult Verify(PushContentTypeAction action)
        {
            using (Logger.MethodTraceLogger(action.ContentType.Title, action.TargetSiteCollection))
            {
                Console.WriteLine(@"Verify Set to read only");
                return _contentTypeRepositoryGenerator(action.TargetSiteCollection).IsContentTypeSetToReadOnly(action.ContentType) 
                    ? VerificationRuleResult.Success 
                    : VerificationRuleResult.Failed($"Content type:{action.ContentType.Title} on site: {action.TargetSiteCollection} is not set to Read-only");
            }
        }
    }
}
