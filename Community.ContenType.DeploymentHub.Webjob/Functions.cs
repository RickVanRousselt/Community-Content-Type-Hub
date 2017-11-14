using System.IO;
using Microsoft.Azure.WebJobs;
using System.Threading;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Factories;
using Community.ContenType.DeploymentHub.Jobs;
using log4net;
using Community.ContenType.DeploymentHub.Contracts.Messages;
using System;
using Community.ContenType.DeploymentHub.Common.Configuration;
using Community.ContenType.DeploymentHub.Contracts;

namespace Community.ContenType.DeploymentHub.Webjob
{
    public class Functions
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Functions));

        private static void RunJobWithInfo<TRequestInfo>(IJob<TRequestInfo> job, TRequestInfo info, TextWriter logger)
            where TRequestInfo : IRequestInfo
        {
            var logAppenders = AzureQueueLoggingConfig.RegisterAppender(logger, info.ActionCollectionId);
            using (Logger.MethodTraceLogger(info))
            {
                try
                {
                    job.Run(info);
                }
                finally
                {
                    FlagDispatcherQueueMessageComplete(info.ActionCollectionId);
                    AzureQueueLoggingConfig.UnregisterAppender(logAppenders);
                }
            }
        }

        private static void FlagDispatcherQueueMessageComplete(Guid actionCollectionId)
        {
            try
            {
                var factory = new DispatchFactory();
                var dispatchMessagehandler = factory.CreateDispatchMessageHandler();
                dispatchMessagehandler.FlagRequestFinished(actionCollectionId);
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected exception while cleaing up Dispatcher queue message. Exception: " + ex);
            } 
        }

        public static void ProcessPublishRequestInfo([QueueTrigger(QueueNames.Publish)] PublishRequestInfo info, CancellationToken token, TextWriter logger)
        {
            var job = PublishObjectFactory.FromFunctionalUserContext(new Hub(info.Hub)).CreatePublishJob(info.EnableVerifiers);
            RunJobWithInfo(job, info, logger);
        }

        public static void ProcessPushRequestInfo([QueueTrigger(QueueNames.Push)] PushRequestInfo info, CancellationToken token, TextWriter logger)
        {
            var job = PushObjectFactory.FromFunctionalUserContext(new Hub(info.Hub)).CreatePushJob(info.EnableVerifiers);
            RunJobWithInfo(job, info, logger);
        }

        public static void ProcessPromoteRequestInfo([QueueTrigger(QueueNames.Promote)] PromoteRequestInfo info, CancellationToken token, TextWriter logger)
        {
            var job = PromoteObjectFactory.FromFunctionalUserContext(new Hub(info.Hub)).CreatePromoteJob(info.EnableVerifiers);
            RunJobWithInfo(job, info, logger);
        }

        public static void ProcessPullRequestInfo([QueueTrigger(QueueNames.Pull)] PullRequestInfo info, CancellationToken token, TextWriter logger)
        {
            var job = PullObjectFactory.FromFunctionalUserContext(new Hub(info.Hub)).CreatePullJob(info.EnableVerifiers);
            RunJobWithInfo(job, info, logger);
        }
    }
}