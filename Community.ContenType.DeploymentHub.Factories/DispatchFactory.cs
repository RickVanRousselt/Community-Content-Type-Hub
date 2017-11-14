using Community.ContenType.DeploymentHub.DomainServices;
using Community.ContenType.DeploymentHub.Factories.Settings;
using Community.ContenType.DeploymentHub.Azure.Repositories;
using Community.ContenType.DeploymentHub.Contracts.Repositories;

namespace Community.ContenType.DeploymentHub.Factories
{
    public class DispatchFactory
    {
        private static readonly ISettingsProvider SettingsProvider = new SettingsProvider();

        public IDispatchedQueueRepository CreateDispatchRepository() => 
            new DispatchedQueueRepository(SettingsProvider.AzureStorageConnectionString);

        public IQueueRepository CreateQueueRepository() => 
            new QueueRepository(SettingsProvider.AzureStorageConnectionString);

        public DispatchMessageHandler CreateDispatchMessageHandler() => 
            new DispatchMessageHandler(CreateDispatchRepository(), SettingsProvider.UseDispatchQueue);
    }
}
