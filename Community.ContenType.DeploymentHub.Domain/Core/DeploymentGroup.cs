using System;

namespace Community.ContenType.DeploymentHub.Domain.Core
{
    public class DeploymentGroup: IEquatable<DeploymentGroup>
    {
        public string Name { get; }
        public Guid TermId { get; }

        public DeploymentGroup(string name, Guid termId)
        {
            Name = name;
            TermId = termId;
        }

        public bool Equals(DeploymentGroup other) => other != null && other.TermId == TermId;

        public override bool Equals(object obj) => obj != null && Equals(obj as DeploymentGroup);

        public override int GetHashCode() => TermId.GetHashCode();

        public override string ToString() => $"[{TermId}]DeploymentGroup '{Name}'";
    }
}
