using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Actions;

namespace Community.ContenType.DeploymentHub.DomainServices.Processors
{
    public interface IContentTypeProcessor<in TAction> where TAction : ActionBase
    {
        IList<IActionStatus> ExecuteAll(IReadOnlyList<TAction> actions);
    }
}