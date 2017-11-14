using System;
using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Requests;
using Community.ContenType.DeploymentHub.Contracts.Messages;
using Community.ContenType.DeploymentHub.DomainServices;
using log4net;
using Microsoft.SharePoint.Client;

namespace Community.ContenType.DeploymentHub.SharePoint.Repositories
{
    public class StatusListRepository : ListRepository, IStatusListRepository
    {
        private enum ActionType
        {
            Publish,
            Push,
            Promote,
            Pull
        }

        private static readonly ILog Logger = LogManager.GetLogger(typeof(StatusListRepository));

        private const string TitleColumnName = "Title";
        private const string StartColumnName = "Start";
        private const string EndColumnName = "End";
        private const string StateColumnName = "State";
        private const string SubstateColumnName = "Substate";
        private const string MessageColumnName = "Message";
        private const string ContentTypeIdsColumnName = "ContentTypeIds";
        private const string PullSiteCollectionUrlColumnName = "PullSiteCollectionUrl";
        private const string PullDeploymentGroupColumnName = "PullDeploymentGroup";

        public StatusListRepository(IClientContextProvider clientContextProvider)
            : base(clientContextProvider)
        {
        }

        public void EnsureStatusList()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var states = new Queue<string>(Enum
                    .GetValues(typeof(JobState))
                    .Cast<JobState>()
                    .ToList()
                    .OrderBy(x => (int)x)
                    .Select(x => x.ToString()));
                var web = clientContext.Site.RootWeb;
                if (!ListExists(web, Lists.Status))
                {
                    var list = CreateGenericList(web, Lists.Status, "Status List of Community Content Type Hub");
                    list.EnableVersioning = true;
                    list.MajorVersionLimit = 500;
                    list.UpdateListVersioning(true, false);
                    RenameField(list, TitleColumnName, "Type"); //cf. JobState
                    AddField(list, StartColumnName, "Start", "DateTime", true, false);
                    AddField(list, EndColumnName, "End", "DateTime", false, false);
                    AddChoiceField(list, StateColumnName, "State", states.Dequeue(), states, true);
                    AddField(list, SubstateColumnName, "Substate", "Note", true, false);
                    AddField(list, MessageColumnName, "Message", "Note", true, false);
                    AddField(list, ContentTypeIdsColumnName, "Content Type IDs", "Note", true, false);
                    AddField(list, PullSiteCollectionUrlColumnName, "PullSiteCollectionUrl", "Note", false, true);
                    AddField(list, PullDeploymentGroupColumnName, "PullDeploymentGroup", "Note", false, true);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
            }
        }

        public void UpdateView()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {

                var web = clientContext.Site.RootWeb;
                if (ListExists(web, Lists.Status))
                {
                    var list = web.Lists.GetByTitle(Lists.Status);
                    clientContext.Load(list);
                    clientContext.ExecuteQueryWithIncrementalRetry();

                    var view = list.DefaultView;
                    view.ViewQuery = "<OrderBy><FieldRef Name=\"Last_x0020_Modified\" Ascending=\"false\" /></OrderBy>";
                    view.Update();
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
            }
        }

        public void UpdateListV1()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {

                var web = clientContext.Site.RootWeb;
                if (ListExists(web, Lists.Status))
                {
                    var list = web.Lists.GetByTitle(Lists.Status);
                    clientContext.Load(list);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                    list.EnableVersioning = false;
                    list.UpdateListVersioning(false, false);
                    var messagefield = list.Fields.GetByInternalNameOrTitle(MessageColumnName);
                    messagefield.DeleteObject();
                    list.Update();
                    clientContext.ExecuteQueryWithIncrementalRetry();
                    AddHtmlNoteField(list, MessageColumnName, "Message");
                    list.Update();
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
            }
        }

        private ListItem GetPendingRequest(ActionType actionType, ActionContext actionContext)
        {
            using (Logger.MethodTraceLogger(actionType, actionContext))
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                try
                {
                    var statusList = clientContext.Site.RootWeb.Lists.GetByTitle(Lists.Status);
                    var listItem = statusList.GetItemById(actionContext.StatusListItemId);
                    clientContext.Load(listItem);
                    clientContext.ExecuteQueryWithIncrementalRetry();

                    var type = (ActionType)Enum.Parse(typeof(ActionType), listItem[TitleColumnName].ToString());
                    var state = (JobState)Enum.Parse(typeof(JobState), listItem[StateColumnName].ToString());
                    if (type != actionType || state != JobState.Pending)
                    {
                        throw new ApplicationException(
                            $"PublishStatusListItemId {actionContext.StatusListItemId} has unexpected type ({type}) or state ({state}).");
                    }

                    return listItem;
                }
                catch (ServerException e)
                {
                    Logger.Error(e.Message, e);
                    throw new ApplicationException("Unexpected exception while trying to get pending request.", e);
                }
            }
        }

        private static HashSet<string> GetContentTypeIds(ListItem listItem) =>
            new HashSet<string>(listItem[ContentTypeIdsColumnName]
                .ToString()
                .Trim()
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim()));

        private static PublishRequest MapListItemToPublishRequest(ActionContext actionContext, ListItem listItem) =>
            new PublishRequest(actionContext, GetContentTypeIds(listItem));

        private static PushRequest MapListItemToPushRequest(ActionContext actionContext, ListItem listItem) =>
            new PushRequest(actionContext, GetContentTypeIds(listItem));

        private static PullRequest MapListItemToPullRequest(ActionContext actionContext, ListItem listItem)
        {
            var siteCollectionUrl = new Uri(listItem[PullSiteCollectionUrlColumnName].ToString());
            var deploymentGroup = new DeploymentGroup("<to fetch>",
                new Guid(listItem[PullDeploymentGroupColumnName].ToString()));
            return new PullRequest(actionContext, siteCollectionUrl, deploymentGroup);
        }

        private static PromoteRequest MapListItemToPromoteRequest(ActionContext actionContext, ListItem listItem) =>
            new PromoteRequest(actionContext, GetContentTypeIds(listItem));

        public PublishRequest GetPendingRequest(PublishRequestInfo info)
        {
            var actionContext = info.GetActionContext();
            return MapListItemToPublishRequest(actionContext, GetPendingRequest(ActionType.Publish, actionContext));
        }

        public PushRequest GetPendingRequest(PushRequestInfo info)
        {
            var actionContext = info.GetActionContext();
            return MapListItemToPushRequest(actionContext, GetPendingRequest(ActionType.Push, actionContext));
        }

        public PullRequest GetPendingRequest(PullRequestInfo info)
        {
            var actionContext = info.GetActionContext();
            return MapListItemToPullRequest(actionContext, GetPendingRequest(ActionType.Pull, actionContext));
        }

        public PromoteRequest GetPendingRequest(PromoteRequestInfo info)
        {
            var actionContext = info.GetActionContext();
            return MapListItemToPromoteRequest(actionContext, GetPendingRequest(ActionType.Promote, actionContext));
        }

        private int AddEntry(ActionType type, JobState state, string substate, DateTime startDate, string message,
            ISet<ContentTypeInfo> contentTypeInfos)
        {
            using (Logger.MethodTraceLogger(substate, startDate, state, message))
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var statusList = clientContext.Site.RootWeb.Lists.GetByTitle(Lists.Status);
                var itemCreateInfo = new ListItemCreationInformation();
                var listItem = statusList.AddItem(itemCreateInfo);
                listItem[TitleColumnName] = type.ToString();
                listItem[StartColumnName] = startDate;
                listItem[StateColumnName] = state.ToString();
                listItem[SubstateColumnName] = substate;
                listItem[MessageColumnName] = message;
                listItem[ContentTypeIdsColumnName] = string.Join("|", contentTypeInfos.Select(c => c.Id));
                listItem.Update();
                clientContext.ExecuteQueryWithIncrementalRetry();
                return listItem.Id;
            }
        }

        public int AddPublishEntry(JobState state, string substate, DateTime startDate, string message,
                ISet<ContentTypeInfo> contentTypeInfos) =>
            AddEntry(ActionType.Publish, state, substate, startDate, message, contentTypeInfos);

        public int AddPushEntry(JobState state, string substate, DateTime startDate, string message,
                ISet<ContentTypeInfo> contentTypeInfos) =>
            AddEntry(ActionType.Push, state, substate, startDate, message, contentTypeInfos);

        public int AddPromoteEntry(JobState state, string substate, DateTime startDate, string message,
                ISet<ContentTypeInfo> contentTypeInfos) =>
            AddEntry(ActionType.Promote, state, substate, startDate, message, contentTypeInfos);

        public int AddPullEntry(JobState state, string substate, DateTime startDate, string message,
            Uri siteCollectionUrl, string deploymentGroup)
        {
            using (Logger.MethodTraceLogger(substate, startDate, state, message))
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var statusList = clientContext.Site.RootWeb.Lists.GetByTitle(Lists.Status);
                var itemCreateInfo = new ListItemCreationInformation();
                var listItem = statusList.AddItem(itemCreateInfo);
                listItem[TitleColumnName] = ActionType.Pull.ToString();
                listItem[StartColumnName] = startDate;
                listItem[StateColumnName] = state.ToString();
                listItem[SubstateColumnName] = substate;
                listItem[MessageColumnName] = message;
                listItem[ContentTypeIdsColumnName] = "*";
                listItem[PullSiteCollectionUrlColumnName] = siteCollectionUrl.AbsoluteUri;
                listItem[PullDeploymentGroupColumnName] = deploymentGroup;
                listItem.Update();
                clientContext.ExecuteQueryWithIncrementalRetry();
                return listItem.Id;
            }
        }

        public void UpdateEntry(int listItemId, JobState state, string substate)
        {
            try
            {
                using (Logger.MethodTraceLogger(listItemId, substate, state))
                using (var clientContext = ClientContextProvider.CreateClientContext())
                {
                    var statusList = clientContext.Site.RootWeb.Lists.GetByTitle(Lists.Status);
                    var listItem = statusList.GetItemById(listItemId);
                    listItem[StateColumnName] = state.ToString();
                    listItem[SubstateColumnName] = substate;
                    listItem.Update();
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Error when updating status list", ex);
            }

        }

        public void UpdateFinalEntry(int listItemId, JobState state, string substate, DateTime endDate, string summary)
        {

            try
            {
                using (Logger.MethodTraceLogger(listItemId, substate, endDate, state))
                using (var clientContext = ClientContextProvider.CreateClientContext())
                {

                    var statusList = clientContext.Site.RootWeb.Lists.GetByTitle(Lists.Status);
                    var listItem = statusList.GetItemById(listItemId);
                    listItem[EndColumnName] = endDate;
                    listItem[StateColumnName] = state.ToString();
                    listItem[SubstateColumnName] = substate;
                    listItem[MessageColumnName] = summary;
                    listItem.Update();
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error updating status list", ex);
            }

        }

    }
}