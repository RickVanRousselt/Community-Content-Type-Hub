using Community.ContenType.DeploymentHub.Common.Configuration;
using Community.ContenType.DeploymentHub.Factories;
using Community.ContenType.DeploymentHub.Jobs;

namespace Community.ContenType.DeploymentHub.Dispatcher
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            WebJobLoggingConfig.RegisterAppender();

            var factory = new DispatchFactory();
            var job = CreateJob(factory);

            return job.Run();
        }

        private static DispatchJob CreateJob(DispatchFactory factory)
        {
            var dispatchRepository = factory.CreateDispatchRepository();
            var queueRepository = factory.CreateQueueRepository();
            return new DispatchJob(dispatchRepository, queueRepository);
        }
    }
}
