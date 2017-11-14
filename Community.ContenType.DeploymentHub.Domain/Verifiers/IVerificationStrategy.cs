using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Actions;

namespace Community.ContenType.DeploymentHub.Domain.Verifiers
{
    public interface IVerificationStrategy<in TSiteColumnAction, in TContentTypeAction>
        where TSiteColumnAction : ActionBase
        where TContentTypeAction : ActionBase
    {
        IList<IActionStatus> VerifyActions(IReadOnlyCollection<TSiteColumnAction> actions);
        IList<IActionStatus> VerifyActions(IReadOnlyCollection<TContentTypeAction> actions);
    }
}