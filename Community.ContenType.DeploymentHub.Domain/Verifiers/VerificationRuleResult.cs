namespace Community.ContenType.DeploymentHub.Domain.Verifiers
{
    public class VerificationRuleResult
    {
        public bool IsCompliant { get; }
        public string Reason { get; }

        private VerificationRuleResult(bool isCompliant, string reason)
        {
            IsCompliant = isCompliant;
            Reason = reason;
        }

        public static VerificationRuleResult Success = new VerificationRuleResult(true, string.Empty);

        public static VerificationRuleResult Failed(string reason) => new VerificationRuleResult(false, reason);
    }
}
