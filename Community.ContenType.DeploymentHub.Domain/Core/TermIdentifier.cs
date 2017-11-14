using System;

namespace Community.ContenType.DeploymentHub.Domain.Core
{
    public class TermIdentifier : IEquatable<TermIdentifier>
    {
        public Guid TermSetId { get; }
        public Guid AnchorId { get; } // = TermId
        public bool IsTermSetIdentifier => AnchorId == Guid.Empty;

        private TermIdentifier(Guid termSetId, Guid anchorId)
        {
            TermSetId = termSetId;
            AnchorId = anchorId;
        }

        public static TermIdentifier FromTermSet(Guid termsetId) => new TermIdentifier(termsetId, Guid.Empty);
        public static TermIdentifier FromTerm(Guid termsetId, Guid termId) => new TermIdentifier(termsetId, termId);
        public static TermIdentifier FromTaxonomyField(Guid termsetId, Guid anchorId) => new TermIdentifier(termsetId, anchorId);

        public override string ToString() => 
            IsTermSetIdentifier
                ? $"TermIdentifier {TermSetId}"
                : $"TermIdentifier {TermSetId}/{AnchorId}";

        public bool Equals(TermIdentifier other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TermSetId.Equals(other.TermSetId) && AnchorId.Equals(other.AnchorId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TermIdentifier) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (TermSetId.GetHashCode() * 397) ^ AnchorId.GetHashCode();
            }
        }
    }
}
