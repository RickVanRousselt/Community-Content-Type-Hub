using System;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Domain.Actions.Promote
{
    public class PromoteSiteColumnAction : ActionBase, IEquatable<PromoteSiteColumnAction>
    {
        public Hub TargetHub { get; }
        public PublishedSiteColumn SiteColumn { get; }
        public override SiteCollection Target => TargetHub;

        public PromoteSiteColumnAction(ActionContext actionContext, Hub targetHub, PublishedSiteColumn siteColumn)
            : base(actionContext)
        {
            if (actionContext.Hub.Url.Host == targetHub.Url.Host)
            {
                throw new ArgumentException($"PromoteSiteColumnAction must be between different tenants. Hub={actionContext.Hub}. Target={targetHub}");
            }
            TargetHub = targetHub;
            SiteColumn = siteColumn;
        }

        public override string ToString() => $"{GetType().Name}: {SiteColumn} -> {TargetHub}";

        public bool Equals(PromoteSiteColumnAction other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(ActionContext, other.ActionContext) && Equals(TargetHub, other.TargetHub) && Equals(SiteColumn, other.SiteColumn);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PromoteSiteColumnAction) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ActionContext?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (TargetHub?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (SiteColumn?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}