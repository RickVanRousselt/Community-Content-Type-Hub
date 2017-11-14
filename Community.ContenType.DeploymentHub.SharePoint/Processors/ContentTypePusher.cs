using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.DomainServices.Processors;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using Community.ContenType.DeploymentHub.SharePoint.Extensions;
using Community.ContenType.DeploymentHub.SharePoint.Updaters.ContentType;
using log4net;
using Microsoft.SharePoint.Client;

namespace Community.ContenType.DeploymentHub.SharePoint.Processors
{
    public class ContentTypePusher : IContentTypeProcessor<PushContentTypeAction>
    {
        private readonly IClientContextProvider _targetContextProvider;
        private readonly IPublishedDocTemplateListRepository _docTemplateListRepository;
        private readonly IPushEventHub _eventHub;
        private readonly SiteCollection _siteCollection;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ContentTypePusher));


        public ContentTypePusher(
            IClientContextProvider targetContextProvider,
            IPublishedDocTemplateListRepository docTemplateListRepository,
            IPushEventHub eventHub,
            SiteCollection siteCollection)
        {
            _targetContextProvider = targetContextProvider;
            _docTemplateListRepository = docTemplateListRepository;
            _eventHub = eventHub;
            _siteCollection = siteCollection;
        }

        private IActionStatus Execute(PushContentTypeAction action, int index, int validActionsCount)
        {
            using (Logger.MethodTraceLogger(action, index, validActionsCount))
            {
                if (!action.TargetSiteCollection.Equals(_siteCollection))
                {
                    throw new ArgumentException(
                        $"This {nameof(ContentTypePusher)} can only process actions for {_siteCollection}.");
                }
                try
                {
                    using (var clientContext = _targetContextProvider.CreateClientContext())
                    {
                        return clientContext.Site.RootWeb.ContentTypeExistsById(action.ContentType.Id)
                            ? ExecuteUpdate(action, index, validActionsCount)
                            : ExecuteCreate(action, index, validActionsCount);
                    }
                }
                catch (Exception ex)
                {
                    _eventHub.Publish(new PushContentTypeActionFailedEvent(action, index, validActionsCount, ex));

                    if (bool.Parse(ConfigurationManager.AppSettings["ThrowOnExceptions"]))
                    {
                        throw;
                    }
                    return new FailedStatus(ex);
                }
               
            }
        }

        private IActionStatus ExecuteCreate(PushContentTypeAction action, int index, int validActionCount)
        {
            using (Logger.MethodTraceLogger(action, index, validActionCount))
            using (var clientContext = _targetContextProvider.CreateClientContext())
            {
                try
                {
                    var web = clientContext.Site.RootWeb;
                    web.CreateContentTypeFromXml(action.ContentType.Schema);

                    var updater = new ContentTypeUpdater(_targetContextProvider, _docTemplateListRepository,
                        action.ContentType);
                    updater.Update(); //update fields that XML updater ignored
                    _eventHub.Publish(new PushContentTypeActionExecutedEvent(action, index, validActionCount, action.TargetSiteCollection));

                    return new SucceededStatus();
                }
                catch (ServerException ex)
                {
                    _eventHub.Publish(new PushContentTypeActionFailedEvent(action, index, validActionCount, ex));
                    return new FailedStatus(ex);
                }
                catch (Exception ex)
                {
                    _eventHub.Publish(new PushContentTypeActionFailedEvent(action, index, validActionCount, ex));

                    if (bool.Parse(ConfigurationManager.AppSettings["ThrowOnExceptions"]))
                    {
                        throw;
                    }
                    return new FailedStatus(ex);
                }
            }
        }

        private IActionStatus ExecuteUpdate(PushContentTypeAction action, int index, int validActionCount)
        {
            using (Logger.MethodTraceLogger(action, index, validActionCount))
            {
                try
                {
                    var updater = new ContentTypeUpdater(_targetContextProvider, _docTemplateListRepository, action.ContentType);
                    updater.Update();
                    _eventHub.Publish(new PushContentTypeActionExecutedEvent(action, index, validActionCount, action.TargetSiteCollection));
                    return new SucceededStatus();
                }
                catch (Exception ex)
                {
                    _eventHub.Publish(new PushContentTypeActionFailedEvent(action, index, validActionCount, ex));
                    return new FailedStatus(ex);
                }
            }
        }

        public IList<IActionStatus> ExecuteAll(IReadOnlyList<PushContentTypeAction> actions) =>
            actions.Select((t, i) => Execute(t, i + 1, actions.Count)).ToList();
    }
}