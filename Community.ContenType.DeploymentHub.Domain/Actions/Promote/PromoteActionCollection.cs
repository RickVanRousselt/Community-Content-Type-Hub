using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Domain.Actions.Promote
{
    public class PromoteActionCollection : ActionCollectionBase<PromoteSiteColumnAction, PromoteContentTypeAction>
    {
        public PromoteActionCollection(ActionContext actionContext) : base(actionContext) { }

        public override IEnumerable<IGrouping<SiteCollection, PromoteContentTypeAction>> GetValidContentTypeActionsPerTargetInOrder() =>
            GetValidContentTypeActions()
                .OrderBy(a => a.ContentType.Id)
                .GroupBy(a => a.TargetHub);

        public override IEnumerable<IGrouping<SiteCollection, PromoteSiteColumnAction>> GetValidSiteColumnActionsPerTargetInOrder() =>
            GetValidSiteColumnActions()
                .GroupBy(a => a.TargetHub);
    }
}