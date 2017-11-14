using Community.ContenType.DeploymentHub.Domain.Core;
using log4net;

namespace Community.ContenType.DeploymentHub.Domain.Actions
{
    public abstract class ActionBase 
    {

        private static readonly ILog Logger = LogManager.GetLogger(typeof(ActionBase));

        public ActionContext ActionContext { get; }
        public abstract SiteCollection Target { get; }

        protected ActionBase(ActionContext actionContext)
        {
            ActionContext = actionContext;
        }

        public bool Equals(ActionBase other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(ActionContext, other.ActionContext) && Equals(Target, other.Target);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ActionBase) obj);
        }

        public override int GetHashCode() => ActionContext?.GetHashCode() ?? 0;

        public override string ToString() => $"{GetType().Name}";

    }
}
