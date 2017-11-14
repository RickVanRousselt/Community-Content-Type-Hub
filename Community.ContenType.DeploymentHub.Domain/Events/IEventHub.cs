namespace Community.ContenType.DeploymentHub.Domain.Events
{
    public interface IProvisionEventHub
    {
        void Publish(ProvisionActionExecutedEvent e);
        void Publish(ProvisionActionFailedEvent e);
    }

    public interface IPublishEventHub
    {
        void Publish(PublishRequestInitiatedEvent e);
        void Publish(PublishRequestInitiateFailedEvent e);
        void Publish(PublishActionsCalculatedEvent e);
        void Publish(PublishSiteColumnVerifiedEvent e);
        void Publish(PublishContentTypeVerifiedEvent e);
        void Publish(PublishActionsVerifiedEvent e);
        void Publish(PublishActionsImpactUpdatedEvent e);
        void Publish(PublishSiteColumnActionExecutedEvent e);
        void Publish(PublishSiteColumnActionFailedEvent e);
        void Publish(PublishContentTypeActionExecutedEvent e);
        void Publish(PublishContentTypeActionFailedEvent e);
        void Publish(PublishActionsExecutedEvent e);
        void Publish(PublishActionsFailedEvent e);
    }

    public interface IPushEventHub
    {
        void Publish(PushRequestInitiatedEvent e);
        void Publish(PushRequestInitiateFailedEvent e);
        void Publish(PushActionsCalculatedEvent e);
        void Publish(PushSiteColumnVerifiedEvent e);
        void Publish(PushContentTypeVerifiedEvent e);
        void Publish(PushActionsVerifiedEvent e);
        void Publish(PushActionsImpactUpdatedEvent e);
        void Publish(PushSiteColumnActionExecutedEvent e);
        void Publish(PushSiteColumnActionFailedEvent e);
        void Publish(PushContentTypeActionExecutedEvent e);
        void Publish(PushContentTypeActionFailedEvent e);
        void Publish(PushActionsExecutedEvent e);
        void Publish(PushActionsFailedEvent e);
    }

    public interface IPullEventHub
    {
        void Publish(PullRequestInitiatedEvent e);
        void Publish(PullActionsFailedEvent e);
    }

    public interface IPromoteEventHub
    {
        void Publish(PromoteRequestInitiatedEvent e);
        void Publish(PromoteRequestInitiateFailedEvent e);
        void Publish(PromoteActionsCalculatedEvent e);
        void Publish(PromoteSiteColumnVerifiedEvent e);
        void Publish(PromoteContentTypeVerifiedEvent e);
        void Publish(PromoteActionsVerifiedEvent e);
        void Publish(PromoteActionsImpactUpdatedEvent e);
        void Publish(PromoteSiteColumnActionExecutedEvent e);
        void Publish(PromoteSiteColumnActionFailedEvent e);
        void Publish(PromoteContentTypeActionExecutedEvent e);
        void Publish(PromoteContentTypeActionFailedEvent e);
        void Publish(PromoteActionsExecutedEvent e);
        void Publish(PromoteActionsFailedEvent e);
    }

    public interface IEventHub : IProvisionEventHub, IPublishEventHub, IPushEventHub, IPullEventHub, IPromoteEventHub
    {
        void Subscribe(IEventListener listener);
    }
}