using System;
using Community.ContenType.DeploymentHub.Domain.Actions;

namespace Community.ContenType.DeploymentHub.Domain.Events
{
    public class PullRequestInitiatedEvent : Event
    {
        public Uri SiteCollectionUrl { get; }
        public string DeploymentGroup { get; }

        public PullRequestInitiatedEvent(ActionContext actionContext, Uri sitecollectionUrl, string deploymentGroup)
            : base(actionContext)
        {
            SiteCollectionUrl = sitecollectionUrl;
            DeploymentGroup = deploymentGroup;
        }
        public override string ToString() => base.ToString() + $": SiteCol={SiteCollectionUrl}, Group={DeploymentGroup}";
    }

    public class PullActionsFailedEvent : Event
    {
        public Exception Exception { get; }
        public PullActionsFailedEvent(ActionContext actionContext, Exception exception)
            : base(actionContext)
        {
            Exception = exception;
        }
        public override string ToString() => base.ToString() + $": Ex={Exception}";
    }
}
