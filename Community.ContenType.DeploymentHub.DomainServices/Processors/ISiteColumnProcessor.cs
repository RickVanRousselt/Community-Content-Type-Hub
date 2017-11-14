using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Actions;

namespace Community.ContenType.DeploymentHub.DomainServices.Processors
{
    public interface ISiteColumnProcessor<in TAction> where TAction : ActionBase
    {
        IList<IActionStatus> ExecuteAll(IReadOnlyList<TAction> actions);
        IActionStatus Execute(TAction action, int index, int validActionsCount);
    }
}