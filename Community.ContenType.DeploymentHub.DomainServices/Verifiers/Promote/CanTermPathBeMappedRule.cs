using System;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Actions.Promote;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Verifiers;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.DomainServices.Verifiers.Promote
{
    public class CanTermPathBeMappedRule : VerificationRuleBase<PromoteSiteColumnAction, PromoteContentTypeAction>, IPromoteVerificationRule
    {
        private readonly Func<SiteCollection, ITermStoreRepository> _termStoreRepositoryGenerator;
        public override VerificationImpactLevel Level => VerificationImpactLevel.SiteCollection;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CanTermPathBeMappedRule));

        public CanTermPathBeMappedRule(Func<SiteCollection, ITermStoreRepository> termStoreRepositoryGenerator)
        {
            _termStoreRepositoryGenerator = termStoreRepositoryGenerator;
        }

        public override VerificationRuleResult Verify(PromoteContentTypeAction contentTypeAction) => 
            VerificationRuleResult.Success;

        public override VerificationRuleResult Verify(PromoteSiteColumnAction siteColumnAction)
        {
            using (Logger.MethodTraceLogger(siteColumnAction.SiteColumn.Title))
            {
                var taxonomySiteColumn = siteColumnAction.SiteColumn as PublishedTaxonomySiteColumn;
                return taxonomySiteColumn == null 
                    ? VerificationRuleResult.Success 
                    : VerifyTaxonomySiteColumn(siteColumnAction.TargetHub, taxonomySiteColumn);
            }
        }

        private VerificationRuleResult VerifyTaxonomySiteColumn(Hub targetHub, PublishedTaxonomySiteColumn taxonomySiteColumn)
        {
            if (!IsValidTermPath(targetHub, taxonomySiteColumn.Path))
            {
                return VerificationRuleResult.Failed($"Could not find term {taxonomySiteColumn} @ {taxonomySiteColumn.Path} in target term store.");
            }
            if (!taxonomySiteColumn.DefaultValuePath.Select(path => IsValidTermPath(targetHub, path)).Else(true))
            {
                return VerificationRuleResult.Failed($"Could not find {taxonomySiteColumn}'s default value @ {taxonomySiteColumn.DefaultValuePath} in target term store.");
            }
            return VerificationRuleResult.Success;
        }

        protected virtual bool IsValidTermPath(Hub targetHub, TermPath termPath) =>
            _termStoreRepositoryGenerator(targetHub).MaybeGetTermIdentifierByPath(termPath).HasValue;
    }
}