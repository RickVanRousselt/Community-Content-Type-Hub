using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Domain.Actions;

namespace Community.ContenType.DeploymentHub.Domain.Verifiers
{
    public abstract class VerificationRuleCollection<TVerificationRule, TSiteColumnAction, TContentTypeAction>
        where TVerificationRule : IVerificationRule<TSiteColumnAction, TContentTypeAction>
        where TSiteColumnAction : ActionBase
        where TContentTypeAction : ActionBase
    {
        private readonly IList<TVerificationRule> _verificationRules;
        private readonly HashSet<string> _activeRuleNames;

        private IEnumerable<TVerificationRule> ActiveRules => _verificationRules.Where(rule => _activeRuleNames.Contains(rule.Name));

        protected VerificationRuleCollection(IList<TVerificationRule> verificationRules, IEnumerable<string> activeRuleNames)
        {
            _verificationRules = verificationRules;
            _activeRuleNames = new HashSet<string>(activeRuleNames);
        }

        private IActionStatus IsActionValid(TSiteColumnAction action)
        {
            var brokenRuleResults = ActiveRules
                .ToDictionary(rule => rule, rule => rule.Verify(action))
                .Where(kvp => !kvp.Value.IsCompliant).ToList();
            brokenRuleResults.ForEach(kvp => PublishSiteColumnEvent(action, kvp.Key, kvp.Value));

            var brokenRules = brokenRuleResults.Select(kvp => kvp.Key).Cast<IVerificationRule<TSiteColumnAction, TContentTypeAction>>();
            return brokenRuleResults.Any()
                ? new InvalidStatus<TSiteColumnAction, TContentTypeAction>(brokenRules)
                : new ValidStatus() as IActionStatus;
        }

        private IActionStatus IsActionValid(TContentTypeAction action)
        {
            var brokenRuleResults = ActiveRules
                .ToDictionary(rule => rule, rule => rule.Verify(action))
                .Where(kvp => !kvp.Value.IsCompliant).ToList();
            brokenRuleResults.ForEach(kvp => PublishContentTypeEvent(action, kvp.Key, kvp.Value));

            var brokenRules = brokenRuleResults.Select(kvp => kvp.Key).Cast<IVerificationRule<TSiteColumnAction, TContentTypeAction>>();
            return brokenRuleResults.Any()
                ? new InvalidStatus<TSiteColumnAction, TContentTypeAction>(brokenRules)
                : new ValidStatus() as IActionStatus;
        }

        public IEnumerable<IActionStatus> AreActionsValid(IEnumerable<TSiteColumnAction> actions) => actions.Select(IsActionValid);
        public IEnumerable<IActionStatus> AreActionsValid(IEnumerable<TContentTypeAction> actions) => actions.Select(IsActionValid);

        protected abstract void PublishSiteColumnEvent(TSiteColumnAction action, TVerificationRule rule, VerificationRuleResult result);
        protected abstract void PublishContentTypeEvent(TContentTypeAction action, TVerificationRule rule, VerificationRuleResult result);
    }
}