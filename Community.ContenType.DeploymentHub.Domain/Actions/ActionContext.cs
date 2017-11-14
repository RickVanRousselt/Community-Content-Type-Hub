using System;
using System.Net.Mail;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Domain.Actions
{
    public class ActionContext : IEquatable<ActionContext>
    {
        public Hub Hub { get; }
        public MailAddress InitiatingUser { get; }
        public int StatusListItemId { get; set; } //can be updated by first event handler and then read by others
        public Guid ActionCollectionId { get; }

        public ActionContext(Hub hub, MailAddress initiatingUser, int statusListItemId, Guid actionCollectionId)
        {
            Hub = hub;
            InitiatingUser = initiatingUser;
            StatusListItemId = statusListItemId;
            ActionCollectionId = actionCollectionId;
        }

        public ActionContext WithInitiatingUser(MailAddress inititatingUser) => 
            new ActionContext(Hub, inititatingUser, StatusListItemId, ActionCollectionId);

        public override string ToString() => $"{InitiatingUser}@{Hub}: {StatusListItemId} - {ActionCollectionId}";

        public bool Equals(ActionContext other) => other != null && ActionCollectionId.Equals(other.ActionCollectionId);

        public override bool Equals(object obj)
        {
            return obj != null && Equals(obj as ActionContext);
        }

        public override int GetHashCode() => ActionCollectionId.GetHashCode();
    }
}