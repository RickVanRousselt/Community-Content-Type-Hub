using System;
using System.Linq;
using System.Text;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Actions.Promote;
using Community.ContenType.DeploymentHub.Domain.Actions.Publish;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.DomainServices.Loggers.Resources;
using log4net;

namespace Community.ContenType.DeploymentHub.DomainServices.Loggers
{
   //TODO remove counts
    public class StatusListLogger : IEventListener
    {
        private readonly IStatusListRepository _statusListRepository;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(StatusListLogger));

        public StatusListLogger(IStatusListRepository statusListRepository)
        {
            _statusListRepository = statusListRepository;
        }

        #region Provision
        public void Handle(ProvisionActionExecutedEvent e)
        {
            
        }

        public void Handle(ProvisionActionFailedEvent e)
        {
            //If provisioning failed then statuslist can be not created
        }
        #endregion

        #region Publish
        public void Handle(PublishRequestInitiatedEvent e)
        {
            var substate = $"Starting publish of {e.ContentTypeInfos.Count} Content Types by {e.ActionContext.InitiatingUser}";
            var message = $"Content Types to process are {string.Join(",", e.ContentTypeInfos.Select(c => c.Title))}";
            e.ActionContext.StatusListItemId = _statusListRepository.AddPublishEntry(
                JobState.Pending,
                substate,
                e.Timestamp,
                message,
                e.ContentTypeInfos);
        }

        public void Handle(PublishRequestInitiateFailedEvent e)
        {
            var summary = string.Format(Summary.PublishFailedMail,
             e.ActionContext.InitiatingUser,
             e.ActionContext.Hub,
             e.Exception.Message,
             e.ActionContext.ActionCollectionId,
             e.ActionContext.StatusListItemId,
             e.Exception);
            _statusListRepository.UpdateFinalEntry(e.ActionContext.StatusListItemId, JobState.Failure, $"Failure publishing Content Types because:{e.Exception.Message}", e.Timestamp, summary);
        }

        public void Handle(PublishActionsCalculatedEvent e)
        {
            LogActionStatus(e.ActionCollection, JobState.Running, $"Finished calculating {e.ActionCollection.TotalActionCount} publish actions.");
        }

        public void Handle(PublishActionsVerifiedEvent e)
        {
            LogActionStatus(e.ActionCollection, JobState.Running, $"Finished pre-checks: {e.ActionCollection.ValidActionCount}/{e.ActionCollection.TotalActionCount} publish actions are valid.");
        }

        public void Handle(PublishActionsImpactUpdatedEvent e)
        {
            LogActionStatus(e.ActionCollection, JobState.Running, $"Pre-check of currenct action has impact on other actions: Updating status");
        }

        public void Handle(PublishSiteColumnVerifiedEvent e)
        {
            var substate = $"{e.Action.SiteColumn.Name} checked against {e.Rule.Name} was declared {(e.Result.IsCompliant ? "Valid" : "Invalid")} because: {e.Result.Reason}";
            _statusListRepository.UpdateEntry(e.Action.ActionContext.StatusListItemId, JobState.Running, substate);
        }

        public void Handle(PublishContentTypeVerifiedEvent e)
        {
            var substate = $"{e.Action.ContentType.Title} checked against {e.Rule.Name} was declared {(e.Result.IsCompliant ? "Success" : "Invalid")} because: {e.Result.Reason}";
            _statusListRepository.UpdateEntry(e.Action.ActionContext.StatusListItemId, JobState.Running, substate);
        }

        public void Handle(PublishSiteColumnActionExecutedEvent e)
        {
            var substate = $"[{e.Index}/{e.TotalCount}] {e.Action} completed";
            _statusListRepository.UpdateEntry(e.Action.ActionContext.StatusListItemId, JobState.Running, substate);
        }

        public void Handle(PublishSiteColumnActionFailedEvent e)
        {
            var substate = $"[{e.Index}/{e.TotalCount}] {e.Action} failed: {e.Exception.Message}";
            _statusListRepository.UpdateEntry(e.Action.ActionContext.StatusListItemId, JobState.Failure, substate);
        }

        public void Handle(PublishContentTypeActionExecutedEvent e)
        {
            var substate = $"[{e.Index}/{e.TotalCount}] {e.Action} completed";
            _statusListRepository.UpdateEntry(e.Action.ActionContext.StatusListItemId, JobState.Running, substate);
        }

        public void Handle(PublishContentTypeActionFailedEvent e)
        {
            var substate = $"[{e.Index}/{e.TotalCount}] {e.Action} failed: {e.Exception.Message}";
            _statusListRepository.UpdateEntry(e.Action.ActionContext.StatusListItemId, JobState.Failure, substate);
        }

        public void Handle(PublishActionsExecutedEvent e)
        {
            var finalState = e.ActionCollection.GetSuccessContentTypeActions().Any()
                ? JobState.Success
                : JobState.Failure;
            var siteColumnActions = new StringBuilder();
            foreach (var s in e.ActionCollection.SiteColumnActions)
            {
                siteColumnActions.Append($"<li>{s.Key.SiteColumn.Name} - {s.Value}</li>");
            }
            var contentTypeActions = new StringBuilder();
            foreach (var s in e.ActionCollection.ContentTypeActions)
            {
                contentTypeActions.Append($"<li>{s.Key.ContentType.Title} - {s.Value}</li>");
            }
            var summary = string.Format(Summary.PublishExecutedMail,
                e.ActionContext.InitiatingUser,
                e.ActionContext.Hub,
                e.ActionCollection.SiteColumnActions.Count,
                e.ActionCollection.ContentTypeActions.Count,
                e.ActionCollection.SuccessActionCount,
                e.ActionCollection.GetSuccessSiteColumnActions().Count(),
                siteColumnActions,
                e.ActionCollection.GetSuccessSiteColumnActions().Count(),
                contentTypeActions,
                e.ActionCollection.ActionContext.ActionCollectionId,
                e.ActionCollection.ActionContext.StatusListItemId);
            _statusListRepository.UpdateFinalEntry(e.ActionCollection.ActionContext.StatusListItemId, finalState, "Publish completed", e.Timestamp, summary);
        }

        private void LogActionStatus(PublishActionCollection actionCollection, JobState state, string message)
        {
            try
            {
                var siteColumnStatusLookup = actionCollection.SiteColumnActions.ToLookup(a => a.Value.GetType());
                var toCheckSiteColumns = siteColumnStatusLookup[typeof(ToVerifyStatus)].Count();
                var validSiteColumns = siteColumnStatusLookup[typeof(ValidStatus)].Count();
                var invalidSiteColumns = siteColumnStatusLookup[typeof(InvalidStatus<PublishSiteColumnAction,PublishContentTypeAction>)].Count();
                var succesSiteColumns = siteColumnStatusLookup[typeof(SucceededStatus)].Count();
                var failedSiteColumns = siteColumnStatusLookup[typeof(FailedStatus)].Count();

                var contentTypeStatusLookup = actionCollection.ContentTypeActions.ToLookup(a => a.Value.GetType());
                var toCheckContentTypes = contentTypeStatusLookup[typeof(ToVerifyStatus)].Count();
                var validContentTypes = contentTypeStatusLookup[typeof(ValidStatus)].Count();
                var invalidContentTypes = contentTypeStatusLookup[typeof(InvalidStatus<PublishSiteColumnAction, PublishContentTypeAction>)].Count();
                var succesContentTypes = contentTypeStatusLookup[typeof(SucceededStatus)].Count();
                var failedContentTypes = contentTypeStatusLookup[typeof(FailedStatus)].Count();

                Logger.Info("The actions in the collection are:");
                foreach (var kvp in actionCollection.ContentTypeActions)
                {
                    Logger.Info($"{kvp.Key.ContentType.Title} and ID:{kvp.Key.ContentType.Id} has ActionStatus={kvp.Value}");
                }
                foreach (var kvp in actionCollection.SiteColumnActions)
                {
                    Logger.Info($"{kvp.Key.SiteColumn.Name} and ID:{kvp.Key.SiteColumn.Id} has ActionStatus={kvp.Value}");
                }
                Logger.Info($"Updating status list item with push entry:{actionCollection.ActionContext.StatusListItemId} - State: {state} - Substate:{message} - ActionCollectionId:{actionCollection.ActionContext.ActionCollectionId} - Site Columns: Invalid:{invalidSiteColumns}, Success:{succesSiteColumns}, Failed:{failedSiteColumns}, Valid:{validSiteColumns}, ToVerify:{toCheckSiteColumns} -  Content Types: Invalid:{invalidContentTypes}, Success:{succesContentTypes}, Failed:{failedContentTypes}, Valid:{validContentTypes}, ToVerify:{toCheckContentTypes}");
                _statusListRepository.UpdateEntry(actionCollection.ActionContext.StatusListItemId, state, message);
            }
            catch (Exception ex)
            {
                Logger.Error("Error when updating StatusList", ex);
            }
        }

        public void Handle(PublishActionsFailedEvent e)
        {
            var substate = $"Publish failed: {e.Exception.Message}";

            var summary = string.Format(Summary.PublishFailedMail,
                e.ActionContext.InitiatingUser,
                e.ActionContext.Hub,
                e.Exception.Message,
                e.ActionContext.ActionCollectionId,
                e.ActionContext.StatusListItemId,
                e.Exception);
            _statusListRepository.UpdateFinalEntry(e.ActionContext.StatusListItemId, JobState.Failure, substate, e.Timestamp, summary);

        }

        #endregion

        #region Push
        public void Handle(PushRequestInitiatedEvent e)
        {
            e.ActionContext.StatusListItemId = _statusListRepository.AddPushEntry(
                JobState.Pending,
                $"Requested Push of {e.ContentTypeInfos.Count} content types by {e.ActionContext.InitiatingUser}",
                e.Timestamp,
                $"Content Types to process are {string.Join(",", e.ContentTypeInfos.Select(c => c.Title))}",
                e.ContentTypeInfos);
        }

        public void Handle(PushRequestInitiateFailedEvent e)
        {
            var substate = $"Push failed: {e.Exception.Message}";
            var summary = string.Format(Summary.PushFailedMail,
                e.ActionContext.InitiatingUser,
                e.ActionContext.Hub,
                e.Exception.Message,
                e.ActionContext.ActionCollectionId,
                e.ActionContext.StatusListItemId,
                e.Exception);

           _statusListRepository.UpdateFinalEntry(e.ActionContext.StatusListItemId, JobState.Failure, substate, e.Timestamp, summary);
        }

        public void Handle(PushActionsCalculatedEvent e)
        {
            LogActionStatus(e.ActionCollection, JobState.Running, $"Finished calculating {e.ActionCollection.TotalActionCount} push actions. Running pre-checks...");
        }

        public void Handle(PushActionsVerifiedEvent e)
        {
            LogActionStatus(e.ActionCollection, JobState.Running, $"Finished pre-checks: {e.ActionCollection.ValidActionCount}/{e.ActionCollection.TotalActionCount} push actions are valid.");
        }

        public void Handle(PushActionsImpactUpdatedEvent e)
        {
            LogActionStatus(e.ActionCollection, JobState.Running, $"Pre-check of currenct action has impact on other actions: Updating status");
        }

        public void Handle(PushSiteColumnVerifiedEvent e)
        {
            var substate = $"{e.Action.SiteColumn.Title} checked against {e.Rule.Name} was declared {(e.Result.IsCompliant ? "Success" : "Invalid")} because:{e.Result.Reason}";
            _statusListRepository.UpdateEntry(e.Action.ActionContext.StatusListItemId, JobState.Running, substate);
        }

        public void Handle(PushContentTypeVerifiedEvent e)
        {
            var substate = $"{e.Action.ContentType.Title} checked against {e.Rule.Name} was declared {(e.Result.IsCompliant ? "Success" : "Invalid")} because:{e.Result.Reason}";
            _statusListRepository.UpdateEntry(e.Action.ActionContext.StatusListItemId, JobState.Running, substate);
        }

        public void Handle(PushSiteColumnActionExecutedEvent e)
        {
            var substate = $"[{e.Index}/{e.TotalCount}] {e.Action} completed on {e.Action.TargetSiteCollection}";
            _statusListRepository.UpdateEntry(e.ActionContext.StatusListItemId, JobState.Running, substate);
        }

        public void Handle(PushSiteColumnActionFailedEvent e)
        {
            var substate = $"[{e.Index}/{e.TotalCount}] {e.Action} on {e.Action.TargetSiteCollection} failed: {e.Exception.Message}";
            _statusListRepository.UpdateEntry(e.Action.ActionContext.StatusListItemId, JobState.Failure, substate);
        }

        public void Handle(PushContentTypeActionExecutedEvent e)
        {
            var substate = $"[{e.Index}/{e.TotalCount}] {e.Action} completed on {e.Action.TargetSiteCollection}";
            _statusListRepository.UpdateEntry(e.Action.ActionContext.StatusListItemId, JobState.Running, substate);
        }

        public void Handle(PushContentTypeActionFailedEvent e)
        {
            var substate = $"[{e.Index}/{e.TotalCount}] {e.Action} on {e.Action.TargetSiteCollection} failed: {e.Exception.Message}";
            _statusListRepository.UpdateEntry(e.Action.ActionContext.StatusListItemId, JobState.Failure, substate);
        }

        public void Handle(PushActionsExecutedEvent e)
        {

            var finalState = e.ActionCollection.GetSuccessContentTypeActions().Any()
                ? JobState.Success
                : JobState.Failure; 
            var siteColumnActions = new StringBuilder();
            foreach (var s in e.ActionCollection.SiteColumnActions)
            {
                siteColumnActions.Append($"<li>{s.Key.SiteColumn.Title} - {s.Value}</li>");
            }
            var contentTypeActions = new StringBuilder();
            foreach (var s in e.ActionCollection.ContentTypeActions)
            {
                contentTypeActions.Append($"<li>{s.Key.ContentType.Title} - {s.Value}</li>");
            }
            var siteCollections = new StringBuilder();
            var totalSiteCollections = 0;
            foreach (var s in e.ActionCollection.ContentTypeActions.Keys.Select(x => x.TargetSiteCollection.Url.ToString()).Distinct())
            {
                siteCollections.Append($"<li>{s}</li>");
                totalSiteCollections++;
            }

            var summary = string.Format(Summary.PushExecutedMail,
                e.ActionContext.InitiatingUser,
                e.ActionContext.Hub,
                e.ActionCollection.SiteColumnActionCount,
                e.ActionCollection.ContentTypeActionCount,
                totalSiteCollections,
                e.ActionCollection.SuccessActionCount,
                e.ActionCollection.GetSuccessSiteColumnActions().Count(),
                siteColumnActions,
                e.ActionCollection.GetSuccessContentTypeActions().Count(),
                contentTypeActions,
                siteCollections,
                e.ActionCollection.ActionContext.ActionCollectionId,
                e.ActionCollection.ActionContext.StatusListItemId);

            _statusListRepository.UpdateFinalEntry(e.ActionContext.StatusListItemId, finalState, "Push completed", e.Timestamp, summary);
        }

        public void Handle(PushActionsFailedEvent e)
        {
            var substate = $"Push failed because {e.Exception.Message}";
            var summary = string.Format(Summary.PushFailedMail,
                e.ActionContext.InitiatingUser,
                e.ActionContext.Hub,
                e.Exception.Message,
                e.ActionContext.ActionCollectionId,
                e.ActionContext.StatusListItemId,
                e.Exception);

            _statusListRepository.UpdateFinalEntry(e.ActionContext.StatusListItemId, JobState.Failure, substate, e.Timestamp, summary);
        }

        private void LogActionStatus(PushActionCollection actionCollection, JobState state, string message)
        {
            using (Logger.MethodTraceLogger(actionCollection))
            {
                try
                {
                    var siteColumnStatusLookup = actionCollection.SiteColumnActions.ToLookup(a => a.Value.GetType());
                    var toCheckSiteColumns = siteColumnStatusLookup[typeof(ToVerifyStatus)].Count();
                    var validSiteColumns = siteColumnStatusLookup[typeof(ValidStatus)].Count();
                    var invalidSiteColumns = siteColumnStatusLookup[typeof(InvalidStatus<PushSiteColumnAction, PushContentTypeAction>)].Count();
                    var succesSiteColumns = siteColumnStatusLookup[typeof(SucceededStatus)].Count();
                    var failedSiteColumns = siteColumnStatusLookup[typeof(FailedStatus)].Count();

                    var contentTypeStatusLookup =
                        actionCollection.ContentTypeActions.ToLookup(a => a.Value.GetType());
                    var toCheckContentTypes = contentTypeStatusLookup[typeof(ToVerifyStatus)].Count();
                    var validContentTypes = contentTypeStatusLookup[typeof(ValidStatus)].Count();
                    var invalidContentTypes = contentTypeStatusLookup[typeof(InvalidStatus<PushSiteColumnAction, PushContentTypeAction>)].Count();
                    var succesContentTypes = contentTypeStatusLookup[typeof(SucceededStatus)].Count();
                    var failedContentTypes = contentTypeStatusLookup[typeof(FailedStatus)].Count();


                
                    Logger.Info("The actions in the collection are:");
                    foreach (var kvp in actionCollection.ContentTypeActions)
                    {
                        Logger.Info($"{kvp.Key.ContentType.Title} with id:{kvp.Key.ContentType.Id} in site coll:{kvp.Key.TargetSiteCollection} with ActionStatus={kvp.Value}");
                    }
                    foreach (var kvp in actionCollection.SiteColumnActions)
                    {
                        Logger.Info($"{kvp.Key.SiteColumn.Title} with id:{kvp.Key.SiteColumn.Id} in site coll:{kvp.Key.TargetSiteCollection} with ActionStatus={kvp.Value}");
                    }
                    Logger.Info($"Updating status list item with push entry:{actionCollection.ActionContext.StatusListItemId} - State: {state} - Substate:{message} - ActionCollectionId:{actionCollection.ActionContext.ActionCollectionId} - Site Columns: Invalid:{invalidSiteColumns}, Success:{succesSiteColumns}, Failed:{failedSiteColumns}, Valid:{validSiteColumns}, ToVerify:{toCheckSiteColumns} -  Content Types: Invalid:{invalidContentTypes}, Success:{succesContentTypes}, Failed:{failedContentTypes}, Valid:{validContentTypes}, ToVerify:{toCheckContentTypes}");
                    _statusListRepository.UpdateEntry(actionCollection.ActionContext.StatusListItemId, state, message);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error when updating StatusList", ex);
                }
            }
        }

        #endregion

        #region Pull

        public void Handle(PullRequestInitiatedEvent e)
        {
            e.ActionContext.StatusListItemId = _statusListRepository.AddPullEntry(
                JobState.Pending,
                $"Requested Pull for {e.SiteCollectionUrl} in deploymentgroup {e.ActionContext.InitiatingUser}",
                e.Timestamp,
                $"Pull operation for {e.SiteCollectionUrl} in deploymentgroup",
                e.SiteCollectionUrl,
                e.DeploymentGroup
                );
        }

        public void Handle(PullActionsFailedEvent e)
        {
            var substate = $"{e.ActionContext.ActionCollectionId} failed because {e.Exception.Message}";
            var summary = string.Format(Summary.PushFailedMail,
              e.ActionContext.InitiatingUser,
              e.ActionContext.Hub,
              e.Exception.Message,
              e.ActionContext.ActionCollectionId,
              e.ActionContext.StatusListItemId,
              e.Exception);
            _statusListRepository.UpdateFinalEntry(e.ActionContext.StatusListItemId, JobState.Failure, substate, e.Timestamp, summary);
        }

        #endregion

        #region Promote
        public void Handle(PromoteRequestInitiatedEvent e)
        {
            e.ActionContext.StatusListItemId = _statusListRepository.AddPromoteEntry(
                 JobState.Pending,
                 $"Requested promote of {e.ContentTypeInfos.Count} content types by {e.ActionContext.InitiatingUser}",
                 e.Timestamp,
                 $"Content Types to process are {string.Join(",", e.ContentTypeInfos.Select(c => c.Title))}",
                 e.ContentTypeInfos);
        }

        public void Handle(PromoteRequestInitiateFailedEvent e)
        {
            var substate = $"Promote initiation failed because: {e.Exception.Message}";
            var summary = string.Format(Summary.PushFailedMail,
              e.ActionContext.InitiatingUser,
              e.ActionContext.Hub,
              e.Exception.Message,
              e.ActionContext.ActionCollectionId,
              e.ActionContext.StatusListItemId,
              e.Exception);
            _statusListRepository.UpdateFinalEntry(e.ActionContext.StatusListItemId, JobState.Failure, substate, e.Timestamp, summary);
        }

        public void Handle(PromoteActionsCalculatedEvent e)
        {
            LogActionStatus(e.ActionCollection, JobState.Running, $"Finished calculating {e.ActionCollection.TotalActionCount} promote actions.");
        }

        public void Handle(PromoteSiteColumnVerifiedEvent e)
        {
            var substate = $"{e.Action.SiteColumn.Title} checked against {e.Rule.Name} was declared {(e.Result.IsCompliant ? "Success" : "Invalid")} because:{e.Result.Reason}";
            _statusListRepository.UpdateEntry(e.Action.ActionContext.StatusListItemId, JobState.Running, substate);
        }

        public void Handle(PromoteContentTypeVerifiedEvent e)
        {
            var substate = $"{e.Action.ContentType.Title} checked against {e.Rule.Name} was declared {(e.Result.IsCompliant ? "Success" : "Invalid")} because:{e.Result.Reason}";

            _statusListRepository.UpdateEntry(e.Action.ActionContext.StatusListItemId, JobState.Running, substate);
        }

        public void Handle(PromoteActionsVerifiedEvent e)
        {
            LogActionStatus(e.ActionCollection, JobState.Running, $"Finished pre-checks: {e.ActionCollection.ValidActionCount}/{e.ActionCollection.TotalActionCount} promote actions are valid.");
        }

        public void Handle(PromoteActionsImpactUpdatedEvent e)
        {
            LogActionStatus(e.ActionCollection, JobState.Running, $"Pre-check of currenct action has impact on other actions: Updating status");
        }

        public void Handle(PromoteSiteColumnActionExecutedEvent e)
        {
            var substate = $"[{e.Index}/{e.TotalCount}] {e.Action.SiteColumn.Title} completed on {e.Action.TargetHub}";
            _statusListRepository.UpdateEntry(e.ActionContext.StatusListItemId, JobState.Running, substate);
        }

        public void Handle(PromoteSiteColumnActionFailedEvent e)
        {
            var substate = $"[{e.Index}/{e.TotalCount}] {e.Action.SiteColumn.Title} on {e.Action.TargetHub} failed: {e.Exception.Message}";
            _statusListRepository.UpdateEntry(e.Action.ActionContext.StatusListItemId, JobState.Failure, substate);
        }

        public void Handle(PromoteContentTypeActionExecutedEvent e)
        {
            var substate = $"[{e.Index}/{e.TotalCount}] {e.Action.ContentType.Title} completed on {e.Action.TargetHub}";
            _statusListRepository.UpdateEntry(e.Action.ActionContext.StatusListItemId, JobState.Running, substate);
        }

        public void Handle(PromoteContentTypeActionFailedEvent e)
        {
            var substate = $"[{e.Index}/{e.TotalCount}] {e.Action} on {e.Action.TargetHub} failed: {e.Exception.Message}";
            var summary = string.Format(Summary.PushFailedMail,
             e.ActionContext.InitiatingUser,
             e.ActionContext.Hub,
             e.Exception.Message,
             e.ActionContext.ActionCollectionId,
             e.ActionContext.StatusListItemId,
             e.Exception);
            _statusListRepository.UpdateFinalEntry(e.Action.ActionContext.StatusListItemId, JobState.Failure, substate, e.Timestamp, summary);
        }

        public void Handle(PromoteActionsExecutedEvent e)
        {
            var finalState = e.ActionCollection.GetSuccessContentTypeActions().Any()
               ? JobState.Success
               : JobState.Failure;
            var siteColumnActions = new StringBuilder();
            foreach (var s in e.ActionCollection.SiteColumnActions)
            {
                siteColumnActions.Append($"<li>{s.Key} - {s.Value}</li>");
            }
            var contentTypeActions = new StringBuilder();
            foreach (var s in e.ActionCollection.ContentTypeActions)
            {
                contentTypeActions.Append($"<li>{s.Key} - {s.Value}</li>");
            }
            var siteCollections = new StringBuilder();
            var totalSiteCollections = 0;
            foreach (var s in e.ActionCollection.ContentTypeActions.Keys.Select(x => x.TargetHub.Url.ToString()).Distinct())
            {
                siteCollections.Append($"<li>{s}</li>");
                totalSiteCollections++;
            }

            var summary = string.Format(Summary.PromoteExecutedMail,
                e.ActionContext.InitiatingUser,
                e.ActionContext.Hub,
                e.ActionCollection.SiteColumnActionCount,
                e.ActionCollection.ContentTypeActionCount,
                totalSiteCollections,
                e.ActionCollection.SuccessActionCount,
                e.ActionCollection.GetSuccessSiteColumnActions().Count(),
                siteColumnActions,
                e.ActionCollection.GetSuccessContentTypeActions().Count(),
                contentTypeActions,
                siteCollections,
                e.ActionCollection.ActionContext.ActionCollectionId,
                e.ActionCollection.ActionContext.StatusListItemId);
            _statusListRepository.UpdateFinalEntry(e.ActionContext.StatusListItemId, finalState, "Promote completed", e.Timestamp, summary);
        }

        public void Handle(PromoteActionsFailedEvent e)
        {
            var substate = $"Promote failed because: {e.Exception.Message}";
            var summary = string.Format(Summary.PromoteFailedMail,
                e.ActionContext.InitiatingUser,
                e.ActionContext.Hub,
                e.Exception.Message,
                e.ActionContext.ActionCollectionId,
                e.ActionContext.StatusListItemId,
                e.Exception);
            _statusListRepository.UpdateFinalEntry(e.ActionContext.StatusListItemId, JobState.Failure, substate, e.Timestamp, summary);
        }

        private void LogActionStatus(PromoteActionCollection actionCollection, JobState state, string message)
        {
            using (Logger.MethodTraceLogger(actionCollection))
            {
                try
                {
                    var siteColumnStatusLookup = actionCollection.SiteColumnActions.ToLookup(a => a.Value.GetType());
                    var toCheckSiteColumns = siteColumnStatusLookup[typeof(ToVerifyStatus)].Count();
                    var validSiteColumns = siteColumnStatusLookup[typeof(ValidStatus)].Count();
                    var invalidSiteColumns = siteColumnStatusLookup[typeof(InvalidStatus<PushSiteColumnAction, PushContentTypeAction>)].Count();
                    var succesSiteColumns = siteColumnStatusLookup[typeof(SucceededStatus)].Count();
                    var failedSiteColumns = siteColumnStatusLookup[typeof(FailedStatus)].Count();

                    var contentTypeStatusLookup =
                        actionCollection.ContentTypeActions.ToLookup(a => a.Value.GetType());
                    var toCheckContentTypes = contentTypeStatusLookup[typeof(ToVerifyStatus)].Count();
                    var validContentTypes = contentTypeStatusLookup[typeof(ValidStatus)].Count();
                    var invalidContentTypes = contentTypeStatusLookup[typeof(InvalidStatus<PushSiteColumnAction, PushContentTypeAction>)].Count();
                    var succesContentTypes = contentTypeStatusLookup[typeof(SucceededStatus)].Count();
                    var failedContentTypes = contentTypeStatusLookup[typeof(FailedStatus)].Count();

                    Logger.Info("The actions in the collection are:");
                    foreach (var kvp in actionCollection.ContentTypeActions)
                    {
                        Logger.Info($"{kvp.Key.ContentType.Title} and ID:{kvp.Key.ContentType.Id} has ActionStatus={kvp.Value}");
                    }
                    foreach (var kvp in actionCollection.SiteColumnActions)
                    {
                        Logger.Info($"{kvp.Key.SiteColumn.Title} and ID:{kvp.Key.SiteColumn.Id} has ActionStatus={kvp.Value}");
                    }
                    Logger.Info($"Updating status list item with promote entry:{actionCollection.ActionContext.StatusListItemId} - State: {state} - Substate:{message} - ActionCollectionId:{actionCollection.ActionContext.ActionCollectionId} - Site Columns: Invalid:{invalidSiteColumns}, Success:{succesSiteColumns}, Failed:{failedSiteColumns}, Valid:{validSiteColumns}, ToVerify:{toCheckSiteColumns} -  Content Types: Invalid:{invalidContentTypes}, Success:{succesContentTypes}, Failed:{failedContentTypes}, Valid:{validContentTypes}, ToVerify:{toCheckContentTypes}");
                    _statusListRepository.UpdateEntry(actionCollection.ActionContext.StatusListItemId, state, message);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error when updating StatusList", ex);
                }
            }
        }
        
        #endregion
    }
}