using System;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Domain.Requests
{
    public class PullRequest : IRequest
    {
        public ActionContext ActionContext { get; }
        public Uri SiteCollectionUrl { get; }
        public DeploymentGroup DeploymentGroup { get; set; } //updated by PullRequestRetriever (name field added) 

        public PullRequest(ActionContext actionContext, Uri siteCollectionUrl, DeploymentGroup deploymentGroup)
        {
            ActionContext = actionContext;
            SiteCollectionUrl = siteCollectionUrl;
            DeploymentGroup = deploymentGroup;
        }

        public override string ToString() => $"PullRequest for site {SiteCollectionUrl} and deploymentGroup {DeploymentGroup}";
    }
}
