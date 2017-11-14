using System;
using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Actions.Promote;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.DomainServices.Processors;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using Community.ContenType.DeploymentHub.SharePoint.Updaters.ContentType;
using log4net;
using Microsoft.SharePoint.Client;

namespace Community.ContenType.DeploymentHub.SharePoint.Processors
{
    public class ContentTypePromotor : IContentTypeProcessor<PromoteContentTypeAction>
    {
        private readonly IClientContextProvider _targetContextProvider;
        private readonly IPromoteEventHub _eventHub;
        private readonly Hub _hub;
        private readonly IPublishedDocTemplateListRepository _docTemplateListRepository;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ContentTypePromotor));

        public ContentTypePromotor(
            IClientContextProvider targetContextProvider,
            IPromoteEventHub eventHub,
            Hub hub,
            IPublishedDocTemplateListRepository docTemplateListRepository)
        {
            _targetContextProvider = targetContextProvider;
            _eventHub = eventHub;
            _hub = hub;
            _docTemplateListRepository = docTemplateListRepository;
        }

        private IActionStatus Execute(PromoteContentTypeAction action, int index, int validActionsCount)
        {
            using (Logger.MethodTraceLogger(action, index, validActionsCount))
            {
                if (!action.TargetHub.Equals(_hub))
                {
                    throw new ArgumentException($"This {nameof(ContentTypePromotor)} can only process actions for {_hub}.");
                }
                using (var clientContext = _targetContextProvider.CreateClientContext())
                {
                    return clientContext.Site.RootWeb.ContentTypeExistsById(action.ContentType.Id)
                        ? ExecuteUpdate(action, index, validActionsCount)
                        : ExecuteCreate(action, index, validActionsCount);
                }
            }
        }

        private IActionStatus ExecuteCreate(PromoteContentTypeAction action, int index, int validActionCount)
        {
            using (Logger.MethodTraceLogger(action, index, validActionCount))
            using (var clientContext = _targetContextProvider.CreateClientContext())
            {
                try
                {
                    var web = clientContext.Site.RootWeb;
                    web.CreateContentTypeFromXML(action.ContentType.Schema);

                    var updater = new ContentTypeUpdater(_targetContextProvider, _docTemplateListRepository, action.ContentType);
                    updater.Update(); //update fields that XML updater ignored

                    _eventHub.Publish(new PromoteContentTypeActionExecutedEvent(action, index, validActionCount));
                    return new SucceededStatus();
                }
                catch (ServerException ex)
                {
                    _eventHub.Publish(new PromoteContentTypeActionFailedEvent(action, index, validActionCount, ex));
                    return new FailedStatus(ex);
                }
            }
        }

        private IActionStatus ExecuteUpdate(PromoteContentTypeAction action, int index, int validActionCount)
        {
            using (Logger.MethodTraceLogger(action, index, validActionCount))
            {
                try
                {
                    var updater = new ContentTypeUpdater(_targetContextProvider, _docTemplateListRepository, action.ContentType);
                    updater.Update();
                    _eventHub.Publish(new PromoteContentTypeActionExecutedEvent(action, index, validActionCount));
                    return new SucceededStatus();
                }
                catch (Exception ex)
                {
                    _eventHub.Publish(new PromoteContentTypeActionFailedEvent(action, index, validActionCount, ex));
                    return new FailedStatus(ex);
                }
            }
        }

        public IList<IActionStatus> ExecuteAll(IReadOnlyList<PromoteContentTypeAction> actions) => 
            actions.Select((t, i) => Execute(t, i + 1, actions.Count)).ToList();
    }
}