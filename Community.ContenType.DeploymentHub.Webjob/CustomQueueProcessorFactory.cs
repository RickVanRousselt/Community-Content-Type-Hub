using Microsoft.Azure.WebJobs.Host.Queues;

namespace Community.ContenType.DeploymentHub.Webjob
{
    internal class CustomQueueProcessorFactory : IQueueProcessorFactory
    {
        public virtual QueueProcessor Create(QueueProcessorFactoryContext context) => new NonRetryingQueueProcessor(context);
    }
}