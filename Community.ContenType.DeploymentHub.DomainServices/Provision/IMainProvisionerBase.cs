using System;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.DomainServices.Provision
{
    public interface IMainProvisionerBase
    {
        Version GetLastDataVersion();
        void Provision(Hub hub);
        void SetNextVersionProvisioner(IVersionProvisioner versionProvisioner);
        bool IsProvisioned();
    }
}