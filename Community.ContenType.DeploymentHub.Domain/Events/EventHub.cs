using System.Collections.Generic;

namespace Community.ContenType.DeploymentHub.Domain.Events
{
    public class EventHub : IEventHub
    {
        private readonly List<IEventListener> _listeners = new List<IEventListener>();

        public void Subscribe(IEventListener listener) => _listeners.Add(listener);

        public void Publish(ProvisionActionExecutedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(ProvisionActionFailedEvent e) => _listeners.ForEach(l => l.Handle(e));

        public void Publish(PublishRequestInitiatedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PublishRequestInitiateFailedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PublishActionsCalculatedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PublishSiteColumnVerifiedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PublishContentTypeVerifiedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PublishActionsVerifiedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PublishActionsImpactUpdatedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PublishSiteColumnActionExecutedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PublishSiteColumnActionFailedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PublishContentTypeActionExecutedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PublishContentTypeActionFailedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PublishActionsExecutedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PublishActionsFailedEvent e) => _listeners.ForEach(l => l.Handle(e));

        public void Publish(PushRequestInitiatedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PushRequestInitiateFailedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PushActionsCalculatedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PushSiteColumnVerifiedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PushContentTypeVerifiedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PushActionsVerifiedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PushActionsImpactUpdatedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PushSiteColumnActionExecutedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PushSiteColumnActionFailedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PushContentTypeActionExecutedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PushContentTypeActionFailedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PushActionsExecutedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PushActionsFailedEvent e) => _listeners.ForEach(l => l.Handle(e));

        public void Publish(PullRequestInitiatedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PullActionsFailedEvent e) => _listeners.ForEach(l => l.Handle(e));

        public void Publish(PromoteRequestInitiatedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PromoteRequestInitiateFailedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PromoteActionsCalculatedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PromoteSiteColumnVerifiedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PromoteContentTypeVerifiedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PromoteActionsVerifiedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PromoteActionsImpactUpdatedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PromoteSiteColumnActionExecutedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PromoteSiteColumnActionFailedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PromoteContentTypeActionExecutedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PromoteContentTypeActionFailedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PromoteActionsExecutedEvent e) => _listeners.ForEach(l => l.Handle(e));
        public void Publish(PromoteActionsFailedEvent e) => _listeners.ForEach(l => l.Handle(e));
    }
}