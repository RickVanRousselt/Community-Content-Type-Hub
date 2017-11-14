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
    public class PublishRequestInitiator : IPublishPushPromoteRequestInitiator
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PublishRequestInitiator));

        private readonly IUserRepository _userRepository;
        private readonly IQueueRepository _queueRepository;
        private readonly IPublishEventHub _eventHub;

        public PublishRequestInitiator(IUserRepository userRepository, IQueueRepository queueRepository, IPublishEventHub eventHub)
        {
            _userRepository = userRepository;
            _queueRepository = queueRepository;
            _eventHub = eventHub;
        }

        public void InitiateRequest(ISet<ContentTypeInfo> contentTypesToPublish, Hub hub, bool enableVerifiers)
        {
            using (Logger.MethodTraceLogger(contentTypesToPublish, hub))
            {
                var actionContext = new ActionContext(hub, new MailAddress("unknown@test.be"), 0, Guid.NewGuid());

                try
                {
                    var initiatingUser = _userRepository.GetInitiatingUser();
                    actionContext = actionContext.WithInitiatingUser(initiatingUser);
                    _eventHub.Publish(new PublishRequestInitiatedEvent(actionContext, contentTypesToPublish));
                    var publishRequestInfo = PublishRequestInfo.FromActionContext(actionContext, enableVerifiers);
                    _queueRepository.AddToQueue(publishRequestInfo);
                }
                catch (Exception e)
                {
                    _eventHub.Publish(new PublishRequestInitiateFailedEvent(actionContext, contentTypesToPublish, e));
                    throw new PublishRequestFailedException("Failed to Publish Content Types", e);
                }
            }
        }
    }

    public class PublishRequestFailedException : Exception
    {
        public PublishRequestFailedException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}