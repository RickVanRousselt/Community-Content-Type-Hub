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
using Community.ContenType.DeploymentHub.SharePoint.Updaters.SiteColumn;
using log4net;
using Microsoft.SharePoint.Client;

namespace Community.ContenType.DeploymentHub.SharePoint.Processors
{
    public class SiteColumnPromotor : ISiteColumnProcessor<PromoteSiteColumnAction>
    {
        private readonly IClientContextProvider _targetContextProvider;
        private readonly Hub _hub;
        private readonly ISiteColumnUpdater _siteColumnUpdater;
        private readonly IPromoteEventHub _eventHub;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SiteColumnPromotor));

        public SiteColumnPromotor(
            IClientContextProvider targetContextProvider,
            IPromoteEventHub eventHub,
            Hub hub,
            ISiteColumnUpdater siteColumnUpdater)
        {
            _targetContextProvider = targetContextProvider;
            _hub = hub;
            _siteColumnUpdater = siteColumnUpdater;
            _eventHub = eventHub;
        }

        public IActionStatus Execute(PromoteSiteColumnAction action, int index, int validActionsCount)
        {
            using (Logger.MethodTraceLogger(action, index, validActionsCount))
            {
                if (!action.TargetHub.Equals(_hub))
                {
                    throw new ArgumentException($"This {nameof(SiteColumnPromotor)} can only process actions for target {_hub}.");
                }
                using (var clientContext = _targetContextProvider.CreateClientContext())
                {
                    return clientContext.Site.RootWeb.FieldExistsById(action.SiteColumn.Id)
                        ? ExecuteUpdate(action, index, validActionsCount)
                        : ExecuteCreate(action, index, validActionsCount);
                }
            }
        }

        private IActionStatus ExecuteCreate(PromoteSiteColumnAction action, int index, int validActionsCount)
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
                    if (action.SiteColumn is PublishedTaxonomySiteColumn)
                    {
                        _siteColumnUpdater.Update(action.SiteColumn);
                    }
                    _eventHub.Publish(new PromoteSiteColumnActionExecutedEvent(action, index, validActionsCount));
                    return new SucceededStatus();
                }
                catch (Exception ex)
                {
                    Logger.Error($"{nameof(ExecuteCreate)} for {action} failed.", ex);
                    _eventHub.Publish(new PromoteSiteColumnActionFailedEvent(action, index, validActionsCount, ex));
                    return new FailedStatus(ex);
                }
            }
        }

        private IActionStatus ExecuteUpdate(PromoteSiteColumnAction action, int index, int validActionsCount)
        {
            using (Logger.MethodTraceLogger(action, index, validActionsCount))
            {
                try
                {
                    Logger.Debug($"Starting {nameof(ExecuteUpdate)} for {action}");
                    _siteColumnUpdater.Update(action.SiteColumn);

                    _eventHub.Publish(new PromoteSiteColumnActionExecutedEvent(action, index, validActionsCount));
                    return new SucceededStatus();
                }
                catch (Exception ex)
                {
                    Logger.Error($"{nameof(ExecuteUpdate)} for {action} failed.", ex);
                    _eventHub.Publish(new PromoteSiteColumnActionFailedEvent(action, index, validActionsCount, ex));
                    return new FailedStatus(ex);
                }
            }
        }

        public IList<IActionStatus> ExecuteAll(IReadOnlyList<PromoteSiteColumnAction> actions) => 
            actions.Select((t, i) => Execute(t, i + 1, actions.Count)).ToList();
    }
}