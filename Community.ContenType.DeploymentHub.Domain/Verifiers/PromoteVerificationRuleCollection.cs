using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Actions.Promote;
using Community.ContenType.DeploymentHub.Domain.Events;

namespace Community.ContenType.DeploymentHub.Domain.Verifiers
{
    public class PromoteVerificationRuleCollection : VerificationRuleCollection<IPromoteVerificationRule, PromoteSiteColumnAction, PromoteContentTypeAction>
    {
        private readonly IPromoteEventHub _eventHub;

        public PromoteVerificationRuleCollection(IPromoteEventHub eventHub, IList<IPromoteVerificationRule> verificationRules, IEnumerable<string> activeRuleNames)
            : base(verificationRules, activeRuleNames)
        {
            _eventHub = eventHub;
        }

        protected override void PublishSiteColumnEvent(PromoteSiteColumnAction action, IPromoteVerificationRule rule, VerificationRuleResult result) =>
            _eventHub.Publish(new PromoteSiteColumnVerifiedEvent(action, rule, result));

        protected override void PublishContentTypeEvent(PromoteContentTypeAction action, IPromoteVerificationRule rule, VerificationRuleResult result) =>
            _eventHub.Publish(new PromoteContentTypeVerifiedEvent(action, rule, result));
    }
}