using System;
using System.Net.Mail;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Contracts.Messages
{
    public class PushRequestInfo : IRequestInfo
    {
        public int StatusListItemId { get; }
        public Uri Hub { get; }
        public string InitiatingUser { get; set; }
        public Guid ActionCollectionId { get; set; }
        public bool EnableVerifiers { get; set; }

        public PushRequestInfo(Uri hub, string initiatingUser, int statusListItemId, Guid actionCollectionId, bool enableVerifiers)
        {
            Hub = hub;
            InitiatingUser = initiatingUser;
            StatusListItemId = statusListItemId;
            ActionCollectionId = actionCollectionId;
            EnableVerifiers = enableVerifiers;
        }

        public ActionContext GetActionContext() => 
            new ActionContext(new Hub(Hub), new MailAddress(InitiatingUser), StatusListItemId, ActionCollectionId);

        public static PushRequestInfo FromActionContext(ActionContext context, bool enableVerifiers) => 
            new PushRequestInfo(context.Hub.Url, context.InitiatingUser.Address, context.StatusListItemId, context.ActionCollectionId, enableVerifiers);

        public override string ToString() => $"PushRequestInfo: {Hub} - {StatusListItemId} - {ActionCollectionId}";
    }
}