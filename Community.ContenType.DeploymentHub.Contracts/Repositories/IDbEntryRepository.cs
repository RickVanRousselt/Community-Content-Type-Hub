using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using Community.ContenType.DeploymentHub.Domain.Events;

namespace Community.ContenType.DeploymentHub.Contracts.Repositories
{
    public interface IDbEntryRepository
    {
        void Log(ProvisionActionExecutedEvent e);
        void Log(ProvisionActionFailedEvent e);

        void Log(PublishRequestInitiatedEvent e);
        void Log(PublishSiteColumnActionExecutedEvent e);
        void Log(PublishSiteColumnActionFailedEvent e);
        void Log(PublishContentTypeActionExecutedEvent e);
        void Log(PublishContentTypeActionFailedEvent e);
        void Log(PublishRequestInitiateFailedEvent e);
        void Log(PublishActionsCalculatedEvent e);
        void Log(PublishActionsVerifiedEvent e);
        void Log(PublishActionsImpactUpdatedEvent e);
        void Log(PublishActionsExecutedEvent e);
        void Log(PublishActionsFailedEvent e);
        void Log(PublishSiteColumnVerifiedEvent e);
        void Log(PublishContentTypeVerifiedEvent e);


        void Log(PushRequestInitiatedEvent e);
        void Log(PushSiteColumnActionExecutedEvent e);
        void Log(PushSiteColumnActionFailedEvent e);
        void Log(PushContentTypeActionExecutedEvent e);
        void Log(PushContentTypeActionFailedEvent e);
        void Log(PushRequestInitiateFailedEvent e);
        void Log(PushActionsCalculatedEvent e);
        void Log(PushActionsVerifiedEvent e);
        void Log(PushActionsImpactUpdatedEvent e);
        void Log(PushActionsExecutedEvent e);
        void Log(PushActionsFailedEvent e);
        void Log(PushSiteColumnVerifiedEvent e);
        void Log(PushContentTypeVerifiedEvent e);

        void Log(PullRequestInitiatedEvent e);
        void Log(PullActionsFailedEvent e);

        void Log(PromoteRequestInitiatedEvent e);
        void Log(PromoteRequestInitiateFailedEvent e);
        void Log(PromoteActionsCalculatedEvent e);
        void Log(PromoteActionsVerifiedEvent e);
        void Log(PromoteActionsImpactUpdatedEvent e);
        void Log(PromoteContentTypeVerifiedEvent e);
        void Log(PromoteSiteColumnVerifiedEvent e);
        void Log(PromoteSiteColumnActionExecutedEvent e);
        void Log(PromoteSiteColumnActionFailedEvent e);
        void Log(PromoteContentTypeActionExecutedEvent e);
        void Log(PromoteContentTypeActionFailedEvent e);
        void Log(PromoteActionsExecutedEvent e);
        void Log(PromoteActionsFailedEvent e);
        bool IsVersionDeployed(PushSiteColumnAction action);
        bool IsVersionDeployed(PushContentTypeAction action);
    }
}
