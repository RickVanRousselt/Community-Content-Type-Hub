namespace Community.ContenType.DeploymentHub.Domain.Events
{
    public interface IEventListener
    {
        void Handle(ProvisionActionExecutedEvent e);
        void Handle(ProvisionActionFailedEvent e);

        void Handle(PublishRequestInitiatedEvent e);
        void Handle(PublishRequestInitiateFailedEvent e);
        void Handle(PublishActionsCalculatedEvent e);
        void Handle(PublishSiteColumnVerifiedEvent e);
        void Handle(PublishContentTypeVerifiedEvent e);
        void Handle(PublishActionsVerifiedEvent e);
        void Handle(PublishActionsImpactUpdatedEvent e);
        void Handle(PublishSiteColumnActionExecutedEvent e);
        void Handle(PublishSiteColumnActionFailedEvent e);
        void Handle(PublishContentTypeActionExecutedEvent e);
        void Handle(PublishContentTypeActionFailedEvent e);
        void Handle(PublishActionsExecutedEvent e);
        void Handle(PublishActionsFailedEvent e);

        void Handle(PushRequestInitiatedEvent e);
        void Handle(PushRequestInitiateFailedEvent e);
        void Handle(PushActionsCalculatedEvent e);
        void Handle(PushSiteColumnVerifiedEvent e);
        void Handle(PushContentTypeVerifiedEvent e);
        void Handle(PushActionsVerifiedEvent e);
        void Handle(PushActionsImpactUpdatedEvent e);
        void Handle(PushSiteColumnActionExecutedEvent e);
        void Handle(PushSiteColumnActionFailedEvent e);
        void Handle(PushContentTypeActionExecutedEvent e);
        void Handle(PushContentTypeActionFailedEvent e);
        void Handle(PushActionsExecutedEvent e);
        void Handle(PushActionsFailedEvent e);

        void Handle(PullRequestInitiatedEvent e);
        void Handle(PullActionsFailedEvent e);

        void Handle(PromoteRequestInitiatedEvent e);
        void Handle(PromoteRequestInitiateFailedEvent e);
        void Handle(PromoteActionsCalculatedEvent e);
        void Handle(PromoteSiteColumnVerifiedEvent e);
        void Handle(PromoteContentTypeVerifiedEvent e);
        void Handle(PromoteActionsVerifiedEvent e);
        void Handle(PromoteActionsImpactUpdatedEvent e);
        void Handle(PromoteSiteColumnActionExecutedEvent e);
        void Handle(PromoteSiteColumnActionFailedEvent e);
        void Handle(PromoteContentTypeActionExecutedEvent e);
        void Handle(PromoteContentTypeActionFailedEvent e);
        void Handle(PromoteActionsExecutedEvent e);
        void Handle(PromoteActionsFailedEvent e);
    }
}