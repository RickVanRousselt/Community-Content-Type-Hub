using System;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Domain.Actions.Promote
{
    public class PromoteContentTypeAction : ActionBase, IEquatable<PromoteContentTypeAction>
    {
        public PublishedContentType ContentType { get; }
        public Hub TargetHub { get; }
        public override SiteCollection Target => TargetHub;

        public PromoteContentTypeAction(ActionContext actionContext, PublishedContentType contentType, Hub targetHub)
            : base(actionContext)
        {
            ContentType = contentType;
            TargetHub = targetHub;
        }

        public override string ToString()
        {
            return $"{GetType().Name}: {ContentType} -> {TargetHub}";
        }

        public bool Equals(PromoteContentTypeAction other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(ActionContext, other.ActionContext) && Equals(ContentType, other.ContentType) && Equals(TargetHub, other.TargetHub);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PromoteContentTypeAction) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ActionContext?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (ContentType?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (TargetHub?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}