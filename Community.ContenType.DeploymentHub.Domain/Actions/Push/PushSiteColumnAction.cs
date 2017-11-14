using System;
using Community.ContenType.DeploymentHub.Domain.Core;
using log4net;

namespace Community.ContenType.DeploymentHub.Domain.Actions.Push
{
    public class PushSiteColumnAction : ActionBase, IEquatable<PushSiteColumnAction>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PushSiteColumnAction));


        public SiteCollection TargetSiteCollection { get; }
        public PublishedSiteColumn SiteColumn { get; }
        public override SiteCollection Target => TargetSiteCollection;

        public PushSiteColumnAction(ActionContext actionContext, SiteCollection targetSiteCollection, PublishedSiteColumn siteColumn)
            : base(actionContext)
        {
            if (actionContext.Hub.Url.Host != targetSiteCollection.Url.Host)
            {
                throw new ArgumentException($"PushSiteColumnAction must be between sites on the same tenant. Hub={actionContext.Hub}. Target={targetSiteCollection}");
            }
            TargetSiteCollection = targetSiteCollection;
            SiteColumn = siteColumn;
        }

        public override string ToString() => $"{GetType().Name}: {SiteColumn} -> {TargetSiteCollection}";

        public bool Equals(PushSiteColumnAction other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(ActionContext, other.ActionContext) && Equals(TargetSiteCollection, other.TargetSiteCollection) && Equals(SiteColumn, other.SiteColumn);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PushSiteColumnAction) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ActionContext?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (TargetSiteCollection?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (SiteColumn?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}