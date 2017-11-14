using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Domain.Processors
{
    public interface IActionProcessingStrategy<TSiteColumnAction, TContentTypeAction>
        where TSiteColumnAction : ActionBase
        where TContentTypeAction : ActionBase
    {
        Dictionary<TContentTypeAction, IActionStatus> ProcessContentTypeActions(IEnumerable<IGrouping<SiteCollection, TContentTypeAction>> actions);
        Dictionary<TSiteColumnAction, IActionStatus> ProcessSiteColumnActions(IEnumerable<IGrouping<SiteCollection, TSiteColumnAction>> actions);
    }
}