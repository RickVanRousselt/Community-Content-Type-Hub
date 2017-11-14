using System;
using System.Linq;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Verifiers;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.DomainServices.Verifiers.Push
{
    public class NameExistsRule : VerificationRuleBase<PushSiteColumnAction, PushContentTypeAction>, IPushVerificationRule
    {
        private readonly Func<SiteCollection, ISiteColumnRepository> _siteColumnRepositoryGenerator;
        private readonly Func<SiteCollection, IContentTypeRepository> _contentTypeRepositoryGenerator;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(VersionRule));
        public override VerificationImpactLevel Level => VerificationImpactLevel.SiteCollection;

        public NameExistsRule(
            Func<SiteCollection, ISiteColumnRepository> siteColumnRepositoryGenerator,
            Func<SiteCollection, IContentTypeRepository> contentTypeRepositoryGenerator)
        {
            _siteColumnRepositoryGenerator = siteColumnRepositoryGenerator;
            _contentTypeRepositoryGenerator = contentTypeRepositoryGenerator;
        }

        public override VerificationRuleResult Verify(PushSiteColumnAction action)
        {
            using (Logger.MethodTraceLogger(action.SiteColumn.Title, action.TargetSiteCollection))
            {
                Console.WriteLine(@"Verify name exitst");
                var maybeSiteColumn = _siteColumnRepositoryGenerator(action.TargetSiteCollection).MaybeGetSiteColumn(action.SiteColumn.Title);

                var siteColumnExistsWithDifferentId = maybeSiteColumn.Select(sc => sc.Id != action.SiteColumn.Id).Else(false);
                return siteColumnExistsWithDifferentId
                    ? VerificationRuleResult.Failed($"Site {action.TargetSiteCollection} already has a different Site Column with name '{action.SiteColumn.Title}'") 
                    : VerificationRuleResult.Success;
            }
        }

        public override VerificationRuleResult Verify(PushContentTypeAction action)
        {
            using (Logger.MethodTraceLogger(action.ContentType.Title, action.TargetSiteCollection))
            {
                Console.WriteLine(@"Verify name exitst");
                var contentTypeInfos = _contentTypeRepositoryGenerator(action.TargetSiteCollection).GetContentTypeInfos();
                var maybeContentType = contentTypeInfos.Where(ct => ct.Title == action.ContentType.Title).MaySingle();

                var contentTypeExistsWithDifferentId = maybeContentType.Select(sc => sc.Id != action.ContentType.Id).Else(false);
                return contentTypeExistsWithDifferentId
                    ? VerificationRuleResult.Failed($"Site {action.TargetSiteCollection} already has a content type with name:{action.ContentType.Title}") 
                    : VerificationRuleResult.Success;
            }
        }
    }
}
