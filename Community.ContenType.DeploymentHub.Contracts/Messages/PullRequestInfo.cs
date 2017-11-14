using System;
using System.Net.Mail;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Contracts.Messages
{
    public class PullRequestInfo : IRequestInfo
    {
        public int StatusListItemId { get; set; }
        public Uri Hub { get; set; }
        public string InitiatingUser { get; set; }
        public Guid ActionCollectionId { get; set; }
        public bool EnableVerifiers { get; set; }

        public PullRequestInfo(Uri hub, string initiatingUser, int statusListItemId, Guid actionCollectionId, bool enableVerifiers)
        {
            Hub = hub;
            InitiatingUser = initiatingUser;
            StatusListItemId = statusListItemId;
            ActionCollectionId = actionCollectionId;
            EnableVerifiers = enableVerifiers;
        }

        public PullRequestInfo()
        {

        }

        public ActionContext GetActionContext() =>
            new ActionContext(new Hub(Hub), new MailAddress(InitiatingUser), StatusListItemId, ActionCollectionId);

        public static PullRequestInfo FromActionContext(ActionContext context, bool enableVerifiers) =>
            new PullRequestInfo(context.Hub.Url, context.InitiatingUser.Address, context.StatusListItemId, context.ActionCollectionId, enableVerifiers);

        public override string ToString() => $"PullRequestInfo: {Hub} - {StatusListItemId} - {ActionCollectionId}";
    }
}
