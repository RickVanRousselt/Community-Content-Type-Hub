using System;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Contracts;
using Community.ContenType.DeploymentHub.Contracts.Messages;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Community.ContenType.DeploymentHub.Azure.Repositories
{
    public class QueueRepository : IQueueRepository
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(QueueRepository));
        private static Lazy<CloudQueueClient> _queueClient;

        public QueueRepository(string azureStorageConnectionString)
        {
            _queueClient = new Lazy<CloudQueueClient>(() => CloudStorageAccount.Parse(azureStorageConnectionString).CreateCloudQueueClient());
        }

        public void AddToQueue(PublishRequestInfo info)
        {
            using (Logger.MethodTraceLogger(info))
            {
                InternalAddToQueue(info, QueueNames.Publish);
            }
        }

        public void AddToQueue(PullRequestInfo info)
        {
            using (Logger.MethodTraceLogger(info))
            {
                InternalAddToQueue(info, QueueNames.Pull);
            }
        }

        public void AddToQueue(PushRequestInfo info)
        {
            using (Logger.MethodTraceLogger(info))
            {
                InternalAddToQueue(info, QueueNames.Push);
            }
        }

        public void AddToQueue(PromoteRequestInfo info)
        {
            using (Logger.MethodTraceLogger(info))
            {
                InternalAddToQueue(info, QueueNames.Promote);
            }
        }

        private static void InternalAddToQueue(IRequestInfo info, string queueName)
        {
            using (Logger.MethodTraceLogger(info))
            {
                var queue = _queueClient.Value.GetQueueReference(queueName);
                queue.CreateIfNotExists();
                var message = new CloudQueueMessage(JsonConvert.SerializeObject(info));
                queue.AddMessage(message);
            }
        }
    }
}