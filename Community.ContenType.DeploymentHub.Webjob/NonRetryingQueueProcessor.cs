using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Queues;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Community.ContenType.DeploymentHub.Webjob
{
    internal class NonRetryingQueueProcessor : QueueProcessor
    {
        public NonRetryingQueueProcessor(QueueProcessorFactoryContext context)
            : base(context)
        {
        }

        public override async Task CompleteProcessingMessageAsync(CloudQueueMessage message, FunctionResult result,
            CancellationToken cancellationToken)
        {
            await DeleteMessageAsync(message, cancellationToken);
        }
        
    }
}