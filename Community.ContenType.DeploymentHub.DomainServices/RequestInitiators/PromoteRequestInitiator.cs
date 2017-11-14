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
    public class PromoteRequestInitiator : IPublishPushPromoteRequestInitiator
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PromoteRequestInitiator));

        private readonly IUserRepository _userRepository;
        private readonly IQueueRepository _queueRepository;
        private readonly IPromoteEventHub _eventHub;

        public PromoteRequestInitiator(
            IUserRepository userRepository,
            IQueueRepository queueRepository,
            IPromoteEventHub eventHub)
        {
            _userRepository = userRepository;
            _queueRepository = queueRepository;
            _eventHub = eventHub;
        }

        public void InitiateRequest(ISet<ContentTypeInfo> contentTypesToPromote, Hub hub, bool enableVerifiers)
        {
            using (Logger.MethodTraceLogger(contentTypesToPromote, hub))
            {
                var actionContext = new ActionContext(hub, new MailAddress("unknown@test.be"), 0, Guid.NewGuid());

                try
                {
                    var initiatingUser = _userRepository.GetInitiatingUser();
                    actionContext = actionContext.WithInitiatingUser(initiatingUser);
                    _eventHub.Publish(new PromoteRequestInitiatedEvent(actionContext, contentTypesToPromote));
                    var info = PromoteRequestInfo.FromActionContext(actionContext, enableVerifiers);
                    _queueRepository.AddToQueue(info);
                }
                catch (Exception e)
                {
                    _eventHub.Publish(new PromoteRequestInitiateFailedEvent(actionContext, contentTypesToPromote, e));
                    throw new PromoteRequestFailedException("Failed to Promote Content Types", e);
                }
            }
        }
    }

    public class PromoteRequestFailedException : Exception
    {
        public PromoteRequestFailedException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}