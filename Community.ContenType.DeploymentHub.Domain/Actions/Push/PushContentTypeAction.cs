using System;
using Community.ContenType.DeploymentHub.Domain.Core;
using log4net;

namespace Community.ContenType.DeploymentHub.Domain.Actions.Push
{
    public class PushContentTypeAction : ActionBase, IEquatable<PushContentTypeAction>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PushContentTypeAction));

        public PublishedContentType ContentType { get; }
        public SiteCollection TargetSiteCollection { get; }
        public override SiteCollection Target => TargetSiteCollection;

        public PushContentTypeAction(ActionContext actionContext, PublishedContentType contentType, SiteCollection targetSiteCollection)
            : base(actionContext)
        {
            if (actionContext.Hub.Url.Host != targetSiteCollection.Url.Host)
            {
                throw new ArgumentException($"PushSiteColumnAction must be between sites on the same tenant. Hub={actionContext.Hub}. Target={targetSiteCollection}");
            }
            ContentType = contentType;
            TargetSiteCollection = targetSiteCollection;
        }

        public override string ToString() => $"{GetType().Name}: {ContentType} -> {TargetSiteCollection}";

        public bool Equals(PushContentTypeAction other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(ActionContext, other.ActionContext) && Equals(ContentType, other.ContentType) && Equals(TargetSiteCollection, other.TargetSiteCollection);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PushContentTypeAction) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ActionContext?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (ContentType?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (TargetSiteCollection?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}