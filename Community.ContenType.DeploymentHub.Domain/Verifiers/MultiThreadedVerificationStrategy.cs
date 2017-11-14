using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Community.ContenType.DeploymentHub.Domain.Actions;
using log4net;
using MoreLinq;

namespace Community.ContenType.DeploymentHub.Domain.Verifiers
{
    public class MultiThreadedVerificationStrategy<TSiteColumnAction, TContentTypeAction> : IVerificationStrategy<TSiteColumnAction, TContentTypeAction>
        where TSiteColumnAction : ActionBase
        where TContentTypeAction : ActionBase
    {
        private readonly IVerificationStrategy<TSiteColumnAction, TContentTypeAction> _baseVerifier;
        private readonly int _numberOfThreads;
        private readonly TaskFactory _taskFactory = new TaskFactory();
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MultiThreadedVerificationStrategy<TSiteColumnAction, TContentTypeAction>));

        public MultiThreadedVerificationStrategy( //decorator
            IVerificationStrategy<TSiteColumnAction, TContentTypeAction> baseVerifier,
            int numberOfThreads)
        {
            _baseVerifier = baseVerifier;
            _numberOfThreads = numberOfThreads;
        }

        public IList<IActionStatus> VerifyActions(IReadOnlyCollection<TSiteColumnAction> actions)
        {
            var tasks = CreateAndStartVerifierTasks(actions).ToList();
            Logger.Info($"Verifying {actions.Count} SiteColumnActions using {tasks.Count} threads.");
            Task.WaitAll(tasks.Cast<Task>().ToArray());
            return tasks.SelectMany(t => t.Result).ToList();
        }

        public IList<IActionStatus> VerifyActions(IReadOnlyCollection<TContentTypeAction> actions)
        {
            var tasks = CreateAndStartVerifierTasks(actions).ToList();
            Logger.Info($"Verifying {actions.Count} ContentTypeActions using {tasks.Count} threads.");
            Task.WaitAll(tasks.Cast<Task>().ToArray());
            return tasks.SelectMany(t => t.Result).ToList();
        }

        private IEnumerable<IEnumerable<T>> Batch<T>(IReadOnlyCollection<T> input)
        {
            var queueSize = Math.Max(1, (int)Math.Ceiling((double)input.Count / _numberOfThreads));
            return input.Batch(queueSize);
        }

        private IEnumerable<Task<IList<IActionStatus>>> CreateAndStartVerifierTasks(IReadOnlyCollection<TSiteColumnAction> actions) =>
            Batch(actions).Select(x => x.ToList()).Select(batch => _taskFactory.StartNew(() => _baseVerifier.VerifyActions(batch)));

        private IEnumerable<Task<IList<IActionStatus>>> CreateAndStartVerifierTasks(IReadOnlyCollection<TContentTypeAction> actions) =>
            Batch(actions).Select(x => x.ToList()).Select(batch => _taskFactory.StartNew(() => _baseVerifier.VerifyActions(batch)));
    }
}