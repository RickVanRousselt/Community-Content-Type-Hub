using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.DomainServices.RequestInitiators
{
    public interface IPublishPushPromoteRequestInitiator
    {
        void InitiateRequest(ISet<ContentTypeInfo> contentTypesToPublish, Hub hub, bool enableVerifiers);
    }
}