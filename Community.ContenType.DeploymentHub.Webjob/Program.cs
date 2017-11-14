using System;
using Microsoft.Azure.WebJobs;

namespace Community.ContenType.DeploymentHub.Webjob
{
    public static class Program
    {
        public static void Main()
        {
            var config = new JobHostConfiguration();
            var secureSettingsProvider = new Factories.Settings.SettingsProvider();
            config.Queues.BatchSize = secureSettingsProvider.NumberOfJobThreads;
            config.Queues.MaxDequeueCount = secureSettingsProvider.NumberOfJobRetries;
            config.Queues.MaxPollingInterval = secureSettingsProvider.MaxPollingInterval;
            config.Queues.VisibilityTimeout = TimeSpan.FromDays(5);
            config.Queues.QueueProcessorFactory = new CustomQueueProcessorFactory();
            var host = new JobHost(config);
            host.RunAndBlock();
        }
    }
}
