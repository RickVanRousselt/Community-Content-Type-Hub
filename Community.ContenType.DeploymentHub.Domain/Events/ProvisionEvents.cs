using System;
using Community.ContenType.DeploymentHub.Domain.Actions;

namespace Community.ContenType.DeploymentHub.Domain.Events
{
    public class ProvisionActionExecutedEvent : Event
    {
        public Version Version { get;}

        public ProvisionActionExecutedEvent(ActionContext actionContext, Version version) 
            : base(actionContext)
        {
            Version = version;
        }

        public override string ToString() => base.ToString() + $": version {Version}";
    }

    public class ProvisionActionFailedEvent : Event
    {
        public Version Version { get; }
        public Exception Exception { get; }

        public ProvisionActionFailedEvent(ActionContext actionContext, Version version, Exception exception)
            : base(actionContext)
        {
            Version = version;
            Exception = exception;
        }

        public override string ToString() => base.ToString() + $": version={Version}, ex={Exception}";
    }
}
