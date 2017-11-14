using System.Collections.Generic;

namespace Community.ContenType.DeploymentHub.Contracts.Repositories
{
    public interface IConfigurationListRepository
    {
        void EnsureConfigurationList();
        Dictionary<string, string> GetConfigs();
    }
}