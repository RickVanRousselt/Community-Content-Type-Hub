using System;
using System.Collections.Generic;
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
    public class PushRequestInitiator : IPublishPushPromoteRequestInitiator
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PushRequestInitiator));

        private readonly IUserRepository _userRepository;
        private readonly IQueueRepository _queueRepository;
        private readonly IPushEventHub _eventHub;

        public PushRequestInitiator(
            IUserRepository userRepository,
            IQueueRepository queueRepository,
            IPushEventHub eventHub)
        {
            _userRepository = userRepository;
            _queueRepository = queueRepository;
            _eventHub = eventHub;
        }

        public void InitiateRequest(ISet<ContentTypeInfo> contentTypesToPush, Hub hub, bool enableVerifiers)
        {
            using (Logger.MethodTraceLogger(contentTypesToPush, hub))
            {
                var actionContext = new ActionContext(hub, new MailAddress("unknown@test.be"), 0, Guid.NewGuid());

                try
                {
                    var initiatingUser = _userRepository.GetInitiatingUser();
                    actionContext = actionContext.WithInitiatingUser(initiatingUser);
                    _eventHub.Publish(new PushRequestInitiatedEvent(actionContext, contentTypesToPush));
                    var info = PushRequestInfo.FromActionContext(actionContext, enableVerifiers);
                    _queueRepository.AddToQueue(info);
                }
                catch (Exception e)
                {
                    _eventHub.Publish(new PushRequestInitiateFailedEvent(actionContext, contentTypesToPush, e));
                    throw new PushRequestFailedException("Failed to Push Content Types", e);
                }
            }
        }
    }

    public class PushRequestFailedException : Exception
    {
        public PushRequestFailedException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}