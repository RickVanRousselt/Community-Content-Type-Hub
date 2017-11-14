using System;
using System.Threading;
using Community.ContenType.DeploymentHub.Domain.Events;
using log4net;

namespace Community.ContenType.DeploymentHub.DomainServices.Loggers
{
    public class Log4NetLogger : IEventListener
    {
        //private static readonly ILog Logger = LogManager.GetLogger(typeof(Log4NetLogger));
         readonly ILog _logger = LogManager.GetLogger(typeof(Log4NetLogger) + Thread.CurrentThread.ManagedThreadId.ToString());
        //readonly ILog _logger = LogManager.GetLogger("Log4NetLogger", "Instance logger: " + Thread.CurrentThread.ManagedThreadId.ToString());

        public void Handle(ProvisionActionExecutedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(ProvisionActionFailedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PublishRequestInitiatedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PublishRequestInitiateFailedEvent e)
        {
            _logger.Warn(e);
        }

        public void Handle(PublishActionsCalculatedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PublishSiteColumnVerifiedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PublishContentTypeVerifiedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PublishActionsVerifiedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PublishSiteColumnActionExecutedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PublishSiteColumnActionFailedEvent e)
        {
            _logger.Warn(e);
        }

        public void Handle(PublishContentTypeActionExecutedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PublishContentTypeActionFailedEvent e)
        {
            _logger.Warn(e);
        }

        public void Handle(PublishActionsExecutedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PublishActionsFailedEvent e)
        {
            _logger.Warn(e);
        }

        public void Handle(PushRequestInitiatedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PushRequestInitiateFailedEvent e)
        {
            _logger.Warn(e);
        }

        public void Handle(PushActionsCalculatedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PushSiteColumnVerifiedEvent e)
        {
            //Heartbeat test for long running operation http://stackoverflow.com/questions/36128943/long-running-azure-webjob-keeps-stopping
            Console.WriteLine(@"Site Column Pushed");
            _logger.Info(e);
        }

        public void Handle(PushContentTypeVerifiedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PushActionsVerifiedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PushSiteColumnActionExecutedEvent e)
        {
            //Heartbeat test for long running operation http://stackoverflow.com/questions/36128943/long-running-azure-webjob-keeps-stopping
            Console.WriteLine(@"Site Column Pushed");
            _logger.Info(e);
        }

        public void Handle(PushSiteColumnActionFailedEvent e)
        {
            _logger.Warn(e);
        }

        public void Handle(PushContentTypeActionExecutedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PushContentTypeActionFailedEvent e)
        {
            _logger.Warn(e);
        }

        public void Handle(PushActionsExecutedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PushActionsFailedEvent e)
        {
            _logger.Warn(e);
        }

        public void Handle(PullRequestInitiatedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PullActionsFailedEvent e)
        {
            _logger.Warn(e);
        }

        public void Handle(PromoteRequestInitiatedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PromoteRequestInitiateFailedEvent e)
        {
            _logger.Warn(e);
        }

        public void Handle(PromoteActionsCalculatedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PromoteSiteColumnVerifiedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PromoteContentTypeVerifiedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PromoteActionsVerifiedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PromoteSiteColumnActionExecutedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PromoteSiteColumnActionFailedEvent e)
        {
            _logger.Warn(e);
        }

        public void Handle(PromoteContentTypeActionExecutedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PromoteContentTypeActionFailedEvent e)
        {
            _logger.Warn(e);
        }

        public void Handle(PromoteActionsExecutedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PromoteActionsFailedEvent e)
        {
            _logger.Warn(e);
        }

        public void Handle(PublishActionsImpactUpdatedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PushActionsImpactUpdatedEvent e)
        {
            _logger.Info(e);
        }

        public void Handle(PromoteActionsImpactUpdatedEvent e)
        {
            _logger.Info(e);
        }
    }
}
