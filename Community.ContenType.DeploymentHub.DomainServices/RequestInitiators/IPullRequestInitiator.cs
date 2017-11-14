using System;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.DomainServices.RequestInitiators
{
    public interface IPullRequestInitiator
    {
        void InitiateRequest(Uri sitecollectionUrl, Hub hub, string deploymentGroup, bool enableVerifiers);
    }
}