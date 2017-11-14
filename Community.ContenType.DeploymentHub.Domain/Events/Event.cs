using System;
using Community.ContenType.DeploymentHub.Domain.Actions;

namespace Community.ContenType.DeploymentHub.Domain.Events
{
    public abstract class Event
    {
        public ActionContext ActionContext { get; }
        public DateTime Timestamp { get; }

        protected Event(ActionContext actionContext)
        {
            ActionContext = actionContext;
            Timestamp = DateTime.UtcNow;
        }

        public override string ToString() => $"{GetType().Name} triggered";
    }
}