using System;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.DomainServices.Provision
{
    public interface IVersionProvisioner
    {
        Version GetVersion();
        void Provision(Hub hub);
    }
}
