using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using Community.ContenType.DeploymentHub.Domain.Events;

namespace Community.ContenType.DeploymentHub.Domain.Verifiers
{
    public class PushVerificationRuleCollection : VerificationRuleCollection<IPushVerificationRule, PushSiteColumnAction, PushContentTypeAction>
    {
        private readonly IPushEventHub _eventHub;

        public PushVerificationRuleCollection(IPushEventHub eventHub, IList<IPushVerificationRule> verificationRules, IEnumerable<string> activeRuleNames) 
            : base(verificationRules, activeRuleNames)
        {
            _eventHub = eventHub;
        }

        protected override void PublishSiteColumnEvent(PushSiteColumnAction action, IPushVerificationRule rule, VerificationRuleResult result) =>
            _eventHub.Publish(new PushSiteColumnVerifiedEvent(action, rule, result));

        protected override void PublishContentTypeEvent(PushContentTypeAction action, IPushVerificationRule rule, VerificationRuleResult result) =>
            _eventHub.Publish(new PushContentTypeVerifiedEvent(action, rule, result));
    }
}