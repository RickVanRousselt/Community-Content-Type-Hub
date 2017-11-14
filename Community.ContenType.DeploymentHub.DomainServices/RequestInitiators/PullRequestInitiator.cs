using System;
using System.Net.Mail;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.Contracts.Messages;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;

namespace Community.ContenType.DeploymentHub.DomainServices.RequestInitiators
{
    public class PullRequestInitiator : IPullRequestInitiator
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PullRequestInitiator));

        private readonly IQueueRepository _queueRepository;
        private readonly IPullEventHub _eventHub;

        public PullRequestInitiator(IQueueRepository queueRepository, IPullEventHub eventHub)
        {
            _queueRepository = queueRepository;
            _eventHub = eventHub;
        }

        public void InitiateRequest(Uri sitecollectionUrl, Hub hub, string deploymentGroup, bool enableVerifiers)
        {
            using (Logger.MethodTraceLogger())

            {
                var actionContext = new ActionContext(hub, new MailAddress("Pull.CtDt@test.be"), 0, Guid.NewGuid());
                _eventHub.Publish(new PullRequestInitiatedEvent(actionContext, sitecollectionUrl, deploymentGroup));
                var info = PullRequestInfo.FromActionContext(actionContext, enableVerifiers);
                _queueRepository.AddToQueue(info);
            }
        }
    }
}
