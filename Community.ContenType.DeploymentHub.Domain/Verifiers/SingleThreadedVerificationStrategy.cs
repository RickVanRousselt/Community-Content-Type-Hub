using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Domain.Actions;

namespace Community.ContenType.DeploymentHub.Domain.Verifiers
{
    public class SingleThreadedVerificationStrategy<TVerificationRuleCollection, TVerificationRule, TSiteColumnAction, TContentTypeAction> : IVerificationStrategy<TSiteColumnAction, TContentTypeAction>
        where TVerificationRuleCollection : VerificationRuleCollection<TVerificationRule, TSiteColumnAction, TContentTypeAction>
        where TVerificationRule : IVerificationRule<TSiteColumnAction, TContentTypeAction>
        where TSiteColumnAction : ActionBase
        where TContentTypeAction : ActionBase
    {
        private readonly IVerificationRuleCollectionFactory<TVerificationRuleCollection, TVerificationRule, TSiteColumnAction, TContentTypeAction> _rulesFactory;

        public SingleThreadedVerificationStrategy(IVerificationRuleCollectionFactory<TVerificationRuleCollection, TVerificationRule, TSiteColumnAction, TContentTypeAction> rulesFactory)
        {
            _rulesFactory = rulesFactory;
        }

        public IList<IActionStatus> VerifyActions(IReadOnlyCollection<TSiteColumnAction> actions) => 
            _rulesFactory.CreateVerificationRules().AreActionsValid(actions).ToList();

        public IList<IActionStatus> VerifyActions(IReadOnlyCollection<TContentTypeAction> actions) => 
            _rulesFactory.CreateVerificationRules().AreActionsValid(actions).ToList();
    }
}