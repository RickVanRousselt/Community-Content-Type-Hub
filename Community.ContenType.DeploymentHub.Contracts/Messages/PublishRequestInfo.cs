using System;
using System.Net.Mail;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Contracts.Messages
{
    public class PublishRequestInfo : IRequestInfo
    {
        public int StatusListItemId { get; set; }
        public Uri Hub { get; set; }
        public string InitiatingUser { get; set; }
        public Guid ActionCollectionId { get; set; }
        public bool EnableVerifiers { get; set; }

        public ActionContext GetActionContext() => new ActionContext(new Hub(Hub), new MailAddress(InitiatingUser), StatusListItemId, ActionCollectionId);

        public PublishRequestInfo(Uri hub, string initiatingUser, int statusListItemId, Guid actionCollectionId, bool enableVerifiers)
        {
            Hub = hub;
            InitiatingUser = initiatingUser;
            StatusListItemId = statusListItemId;
            ActionCollectionId = actionCollectionId;
            EnableVerifiers = enableVerifiers;
        }

        public override string ToString() => $"PublishRequestInfo: {Hub} - {StatusListItemId} - {ActionCollectionId}";

        public static PublishRequestInfo FromActionContext(ActionContext actionContext, bool enableVerifiers) =>
            new PublishRequestInfo(actionContext.Hub.Url, actionContext.InitiatingUser.Address, actionContext.StatusListItemId, actionContext.ActionCollectionId, enableVerifiers);
    }
}