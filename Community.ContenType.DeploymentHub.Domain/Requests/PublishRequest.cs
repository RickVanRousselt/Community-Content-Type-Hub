using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Actions;

namespace Community.ContenType.DeploymentHub.Domain.Requests
{
    public class PublishRequest : IRequest
    {
        public ActionContext ActionContext { get; }
        public ISet<string> ContentTypeIds { get; }

        public PublishRequest(ActionContext actionContext, ISet<string> contentTypeIds)
        {
            ActionContext = actionContext;
            ContentTypeIds = contentTypeIds;
        }

        public override string ToString() => $"PublishRequest for CT IDs: {string.Join(", ", ContentTypeIds)}";
    }
}
