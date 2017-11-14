using System;
using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Domain.Verifiers;

namespace Community.ContenType.DeploymentHub.Domain.Actions
{
    public interface IActionStatus { }

    public class ToVerifyStatus : IActionStatus
    {
        public override string ToString() => "To Verify";
    }

    public class ValidStatus : IActionStatus
    {
        public override string ToString() => "Valid";
    }

    public class InvalidStatus<TSiteColumAction, TContentTypeAction> : IActionStatus
    {
        public ISet<IVerificationRule<TSiteColumAction, TContentTypeAction>> ViolatedRules { get; }
        public VerificationImpactLevel MaxImpactLevel => ViolatedRules.Select(x => x.Level).Max();

        public InvalidStatus(IEnumerable<IVerificationRule<TSiteColumAction, TContentTypeAction>> violatedRules)
        {
            ViolatedRules = new HashSet<IVerificationRule<TSiteColumAction, TContentTypeAction>>(violatedRules);
        }

        public override string ToString() => "Invalid because the following rules where violated: " + string.Join(",", ViolatedRules.Select(x => x.Name + "(" +  x.Level + ")"));
    }

    public class SucceededStatus : IActionStatus
    {
        public override string ToString() => "Succeeded";
    }

    public class FailedStatus : IActionStatus
    {
        public Exception Ex { get; }

        public FailedStatus(Exception ex)
        {
            Ex = ex;
        }

        public override string ToString() => "Failed: " + Ex.Message;
    }
}
