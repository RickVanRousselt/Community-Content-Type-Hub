using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Domain.Actions;

namespace Community.ContenType.DeploymentHub.Domain.Verifiers
{
    public class NaiveVerificationStrategy<TSiteColumnAction, TContentTypeAction> : IVerificationStrategy<TSiteColumnAction, TContentTypeAction>
        where TSiteColumnAction : ActionBase
        where TContentTypeAction : ActionBase
    {
        public IList<IActionStatus> VerifyActions(IReadOnlyCollection<TContentTypeAction> actions) => 
            Enumerable.Repeat(new ValidStatus(), actions.Count).Cast<IActionStatus>().ToList();

        public IList<IActionStatus> VerifyActions(IReadOnlyCollection<TSiteColumnAction> actions) => 
            Enumerable.Repeat(new ValidStatus(), actions.Count).Cast<IActionStatus>().ToList();
    }
}