using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Actions.Publish;
using Community.ContenType.DeploymentHub.Domain.Events;

namespace Community.ContenType.DeploymentHub.Domain.Verifiers
{
    public class PublishVerificationRuleCollection : VerificationRuleCollection<IPublishVerificationRule, PublishSiteColumnAction, PublishContentTypeAction>
    {
        private readonly IPublishEventHub _eventHub;

        public PublishVerificationRuleCollection(IPublishEventHub eventHub, IList<IPublishVerificationRule> verificationRules, IEnumerable<string> activeRuleNames)
            : base(verificationRules, activeRuleNames)
        {
            _eventHub = eventHub;
        }

        protected override void PublishSiteColumnEvent(PublishSiteColumnAction action, IPublishVerificationRule rule, VerificationRuleResult result) =>
            _eventHub.Publish(new PublishSiteColumnVerifiedEvent(action, rule, result));

        protected override void PublishContentTypeEvent(PublishContentTypeAction action, IPublishVerificationRule rule, VerificationRuleResult result) =>
            _eventHub.Publish(new PublishContentTypeVerifiedEvent(action, rule, result));
    }
}