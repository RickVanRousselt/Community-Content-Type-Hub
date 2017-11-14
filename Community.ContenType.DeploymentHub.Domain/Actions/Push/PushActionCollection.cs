using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Domain.Actions.Push
{
    public class PushActionCollection : ActionCollectionBase<PushSiteColumnAction, PushContentTypeAction>
    {
        public PushActionCollection(ActionContext actionContext) : base(actionContext) { }

        public override IEnumerable<IGrouping<SiteCollection, PushContentTypeAction>> GetValidContentTypeActionsPerTargetInOrder() =>
            GetValidContentTypeActions()
                .OrderBy(a => a.ContentType.Id)
                .GroupBy(a => a.TargetSiteCollection);

        public override IEnumerable<IGrouping<SiteCollection, PushSiteColumnAction>> GetValidSiteColumnActionsPerTargetInOrder() =>
            GetValidSiteColumnActions()
                .GroupBy(a => a.TargetSiteCollection);
    }
}