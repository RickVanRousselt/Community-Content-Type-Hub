using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Actions;

namespace Community.ContenType.DeploymentHub.Domain.Requests
{
    public class PromoteRequest : IRequest
    {
        public ActionContext ActionContext { get; }
        public ISet<string> ContentTypeIds { get; }

        public PromoteRequest(ActionContext actionContext, ISet<string> contentTypeIds)
        {
            ActionContext = actionContext;
            ContentTypeIds = contentTypeIds;
        }

        public override string ToString() => $"PromoteRequest for CT IDs: {string.Join(", ", ContentTypeIds)}";
    }
}