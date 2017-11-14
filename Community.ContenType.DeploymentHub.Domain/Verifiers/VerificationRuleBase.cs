using System;

namespace Community.ContenType.DeploymentHub.Domain.Verifiers
{
    public abstract class VerificationRuleBase<TSiteColumnAction, TContentTypeAction> : IVerificationRule<TSiteColumnAction, TContentTypeAction>, IEquatable<VerificationRuleBase<TSiteColumnAction, TContentTypeAction>>
    {
        public abstract VerificationImpactLevel Level { get; }

        public string Name => GetType().Name;

        public abstract VerificationRuleResult Verify(TContentTypeAction contentTypeAction);
        public abstract VerificationRuleResult Verify(TSiteColumnAction siteColumnAction);

        public override string ToString() => Name;
        public bool Equals(VerificationRuleBase<TSiteColumnAction, TContentTypeAction> other) => other != null && Name == other.Name;
        public override bool Equals(object obj) => obj != null && Equals(obj as VerificationRuleBase<TSiteColumnAction, TContentTypeAction>);
        public override int GetHashCode() => Name.GetHashCode();
    }
}