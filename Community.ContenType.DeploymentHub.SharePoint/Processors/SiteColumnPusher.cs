using System;
using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.DomainServices.Processors;
using Community.ContenType.DeploymentHub.SharePoint.Updaters.SiteColumn;
using log4net;
using Microsoft.SharePoint.Client;

namespace Community.ContenType.DeploymentHub.SharePoint.Processors
{
    public class SiteColumnPusher : ISiteColumnProcessor<PushSiteColumnAction>
    {
        private readonly IClientContextProvider _targetContextProvider;
        private readonly SiteCollection _siteCollection;
        private readonly ISiteColumnUpdater _siteColumnUpdater;
        private readonly IPushEventHub _eventHub;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SiteColumnPusher));

        public SiteColumnPusher(
            IClientContextProvider targetContextProvider,
            IPushEventHub eventHub,
            SiteCollection siteCollection,
            ISiteColumnUpdater siteColumnUpdater)
        {
            _targetContextProvider = targetContextProvider;
            _siteCollection = siteCollection;
            _siteColumnUpdater = siteColumnUpdater;
            _eventHub = eventHub;
        }

        public IActionStatus Execute(PushSiteColumnAction action, int index, int validActionsCount)
        {
            using (Logger.MethodTraceLogger(action, index, validActionsCount))
            {
                if (!action.TargetSiteCollection.Equals(_siteCollection))
                {
                    throw new ArgumentException($"This SiteColumnPusher can only process actions for {_siteCollection}.");
                }
                try
                {
                    using (var clientContext = _targetContextProvider.CreateClientContext())
                    {
                        return clientContext.Site.RootWeb.FieldExistsById(action.SiteColumn.Id)
                            ? ExecuteUpdate(action, index, validActionsCount)
                            : ExecuteCreate(action, index, validActionsCount);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"{nameof(Execute)} for {action} failed.", ex);
                    _eventHub.Publish(new PushSiteColumnActionFailedEvent(action, index, validActionsCount, ex));
                    return new FailedStatus(ex);
                }
                
            }
        }

        private IActionStatus ExecuteCreate(PushSiteColumnAction action, int index, int validActionsCount)
        {
            using (Logger.MethodTraceLogger(action, index, validActionsCount))
            using (var clientContext = _targetContextProvider.CreateClientContext())
            {
                try
                {
                    Logger.Debug($"Starting {nameof(ExecuteCreate)} for {action}");
                    var web = clientContext.Site.RootWeb;
                    web.Fields.AddFieldAsXml(action.SiteColumn.SchemaWithoutVersion.ToString(), true, AddFieldOptions.AddFieldInternalNameHint);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                    _eventHub.Publish(new PushSiteColumnActionExecutedEvent(action, index, validActionsCount));
                    return new SucceededStatus();
                }
                catch (Exception ex)
                {
                    Logger.Error($"{nameof(ExecuteCreate)} for {action} failed.", ex);
                    _eventHub.Publish(new PushSiteColumnActionFailedEvent(action, index, validActionsCount, ex));
                    return new FailedStatus(ex);
                }
            }
        }

        private IActionStatus ExecuteUpdate(PushSiteColumnAction action, int index, int validActionsCount)
        {
            using (Logger.MethodTraceLogger(action, index, validActionsCount))
            {
                try
                {
                    Logger.Debug($"Starting {nameof(ExecuteUpdate)} for {action}");
                    _siteColumnUpdater.Update(action.SiteColumn);

                    _eventHub.Publish(new PushSiteColumnActionExecutedEvent(action, index, validActionsCount));
                    return new SucceededStatus();
                }
                catch (Exception ex)
                {
                    Logger.Error($"{nameof(ExecuteUpdate)} for {action} failed.", ex);
                    _eventHub.Publish(new PushSiteColumnActionFailedEvent(action, index, validActionsCount, ex));
                    return new FailedStatus(ex);
                }
            }
        }

        public IList<IActionStatus> ExecuteAll(IReadOnlyList<PushSiteColumnAction> actions) => 
            actions.Select((t, i) => Execute(t, i + 1, actions.Count)).ToList();
    }
}