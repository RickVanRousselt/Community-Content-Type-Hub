using System;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;

namespace Community.ContenType.DeploymentHub.DomainServices.Loggers
{
    public class ContentTypeSyncListLogger : IEventListener
    {
        private readonly IContentTypeSyncLogListRepository _contentTypeSyncLogListRepository;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(StatusListLogger));

        public ContentTypeSyncListLogger(IContentTypeSyncLogListRepository contentTypeSyncLogListRepository)
        {
            _contentTypeSyncLogListRepository = contentTypeSyncLogListRepository;
        }

        #region Provision

        //No target site collection in provision step
        public void Handle(ProvisionActionExecutedEvent e)
        {
        }

        public void Handle(ProvisionActionFailedEvent e)
        {
        }

        #endregion

        #region Publish

        //No target site collection in publish step
        public void Handle(PublishRequestInitiatedEvent e)
        {
        }

        public void Handle(PublishRequestInitiateFailedEvent e)
        {
        }

        public void Handle(PublishActionsCalculatedEvent e)
        {
        }

        public void Handle(PublishSiteColumnVerifiedEvent e)
        {
        }

        public void Handle(PublishContentTypeVerifiedEvent e)
        {
        }

        public void Handle(PublishActionsVerifiedEvent e)
        {
        }

        public void Handle(PublishActionsImpactUpdatedEvent e)
        {
        }

        public void Handle(PublishSiteColumnActionExecutedEvent e)
        {
        }

        public void Handle(PublishSiteColumnActionFailedEvent e)
        {
        }

        public void Handle(PublishContentTypeActionExecutedEvent e)
        {
        }

        public void Handle(PublishContentTypeActionFailedEvent e)
        {
        }

        public void Handle(PublishActionsExecutedEvent e)
        {
        }

        public void Handle(PublishActionsFailedEvent e)
        {
        }

        #endregion

        #region Push

        public void Handle(PushRequestInitiatedEvent e)
        {
        }

        public void Handle(PushRequestInitiateFailedEvent e)
        {
        }

        public void Handle(PushActionsCalculatedEvent e)
        {
        }

        public void Handle(PushActionsVerifiedEvent e)
        {
        }

        public void Handle(PushActionsImpactUpdatedEvent e)
        {
        }

        public void Handle(PushContentTypeVerifiedEvent e)
        {
        }

        public void Handle(PushSiteColumnVerifiedEvent e)
        {
        }

        public void Handle(PushSiteColumnActionExecutedEvent e)
        {
            try
            {
                _contentTypeSyncLogListRepository.AddAddSyncLogEntry($"Site column {e.Action.SiteColumn.Title} added",
    $"{e.Action.ActionContext.Hub} pushed {e.Action.SiteColumn.Title} by {e.Action.ActionContext.InitiatingUser} with StatusListId on hub {e.Action.ActionContext.StatusListItemId}",
    e.Action.TargetSiteCollection);
            }
            catch (Exception ex)
            {
                Logger.Error("Error when logging to sync log on target", ex);
            }

        }

        public void Handle(PushSiteColumnActionFailedEvent e)
        {
            try
            {
                _contentTypeSyncLogListRepository.AddSyncLogFailEntry($"Site column {e.Action.SiteColumn.Title} Failed",
                $"{e.Action.ActionContext.Hub} failed to push {e.Action.SiteColumn.Title} by {e.Action.ActionContext.InitiatingUser} with StatusListId on hub {e.Action.ActionContext.StatusListItemId}",
                e.Exception.Message.Substring(0, Math.Min(e.Exception.Message.Length, 250)), e.Exception.StackTrace.Substring(0, Math.Min(e.Exception.StackTrace.Length, 250)), e.Timestamp, e.Action.TargetSiteCollection);
            }
            catch (Exception ex)
            {
                Logger.Error("Error when logging to sync log on target", ex);
            }
        }

        public void Handle(PushContentTypeActionExecutedEvent e)
        {
            try
            {
                _contentTypeSyncLogListRepository.AddAddSyncLogEntry($"Content Type {e.Action.ContentType.Title} added",
               $"{e.Action.ActionContext.Hub} pushed {e.Action.ContentType.Title} by {e.Action.ActionContext.InitiatingUser} with StatusListId on hub {e.Action.ActionContext.StatusListItemId}",
               e.Action.TargetSiteCollection);
            }
            catch (Exception ex)
            {
                Logger.Error("Error when logging to sync log on target", ex);
            }
        }

        public void Handle(PushContentTypeActionFailedEvent e)
        {
            try
            {
                _contentTypeSyncLogListRepository.AddSyncLogFailEntry($"Content Type {e.Action.ContentType.Title} Failed", $"{e.Action.ActionContext.Hub} failed to push {e.Action.ContentType.Title} by {e.Action.ActionContext.InitiatingUser} with StatusListId on hub {e.Action.ActionContext.StatusListItemId}",
    e.Exception.Message.Substring(0, Math.Min(e.Exception.Message.Length, 250)), e.Exception.StackTrace.Substring(0, Math.Min(e.Exception.StackTrace.Length, 250)), e.Timestamp, e.Action.TargetSiteCollection);
            }
            catch (Exception ex)
            {
                Logger.Error("Error when logging to sync log on target", ex);
            }
        }

        public void Handle(PushActionsExecutedEvent e)
        {
        }

        public void Handle(PushActionsFailedEvent e)
        {
        }

        #endregion

        #region Pull

        public void Handle(PullRequestInitiatedEvent e)
        {

        }

        public void Handle(PullActionsFailedEvent e)
        {
        }

        #endregion

        #region Promote
        public void Handle(PromoteRequestInitiatedEvent e)
        {
        }

        public void Handle(PromoteRequestInitiateFailedEvent e)
        {
        }

        public void Handle(PromoteActionsCalculatedEvent e)
        {
        }

        public void Handle(PromoteSiteColumnVerifiedEvent e)
        {
        }

        public void Handle(PromoteContentTypeVerifiedEvent e)
        {
        }

        public void Handle(PromoteActionsVerifiedEvent e)
        {
        }

        public void Handle(PromoteActionsImpactUpdatedEvent e)
        {
        }

        public void Handle(PromoteSiteColumnActionExecutedEvent e)
        {
            try
            {
                _contentTypeSyncLogListRepository.AddAddSyncLogEntry($"Site column {e.Action.SiteColumn.Title} added",
                 $"{e.Action.ActionContext.Hub} promoted {e.Action.SiteColumn.Title} by {e.Action.ActionContext.InitiatingUser} with StatusListId on hub {e.Action.ActionContext.StatusListItemId}",
                 e.Action.TargetHub);
            }
            catch (Exception ex)
            {
                Logger.Error("Error when logging to sync log on target", ex);
            }
        }

        public void Handle(PromoteSiteColumnActionFailedEvent e)
        {
            try
            {
                _contentTypeSyncLogListRepository.AddSyncLogFailEntry($"Site column {e.Action.SiteColumn.Title} Failed",
                $"{e.Action.ActionContext.Hub} failed to promote {e.Action.SiteColumn.Title} by {e.Action.ActionContext.InitiatingUser} with StatusListId on hub {e.Action.ActionContext.StatusListItemId}",
                e.Exception.Message.Substring(0, Math.Min(e.Exception.Message.Length, 250)), e.Exception.StackTrace.Substring(0, Math.Min(e.Exception.StackTrace.Length, 250)), e.Timestamp, e.Action.TargetHub);
            }
            catch (Exception ex)
            {
                Logger.Error("Error when logging to sync log on target", ex);
            }
        }

        public void Handle(PromoteContentTypeActionExecutedEvent e)
        {
            try
            {
                _contentTypeSyncLogListRepository.AddAddSyncLogEntry($"Content Type {e.Action.ContentType.Title} added",
               $"{e.Action.ActionContext.Hub} promted {e.Action.ContentType.Title} by {e.Action.ActionContext.InitiatingUser} with StatusListId:{e.Action.ActionContext.StatusListItemId} on hub {e.Action.ActionContext.Hub}",
               e.Action.TargetHub);
            }
            catch (Exception ex)
            {
                Logger.Error("Error when logging to sync log on target", ex);
            }
        }

        public void Handle(PromoteContentTypeActionFailedEvent e)
        {
            try
            {
                _contentTypeSyncLogListRepository.AddSyncLogFailEntry($"Content Type {e.Action.ContentType.Title} Failed", $"{e.Action.ActionContext.Hub} failed to promote {e.Action.ContentType.Title} by {e.Action.ActionContext.InitiatingUser} with StatusListId:{e.Action.ActionContext.StatusListItemId} on hub {e.Action.ActionContext.Hub}",
                e.Exception.Message.Substring(0, 255), e.Exception.StackTrace.Substring(0, 255), e.Timestamp, e.Action.TargetHub);
            }
            catch (Exception ex)
            {
                Logger.Error("Error when logging to sync log on target", ex);
            }
        }

        public void Handle(PromoteActionsExecutedEvent e)
        {
        }

        public void Handle(PromoteActionsFailedEvent e)
        {
        }

        #endregion
    }
}
