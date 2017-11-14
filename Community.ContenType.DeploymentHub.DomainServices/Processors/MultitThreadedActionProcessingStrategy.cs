using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Processors;
using log4net;

namespace Community.ContenType.DeploymentHub.DomainServices.Processors
{
    public class MultitThreadedActionProcessingStrategy<TSiteColumnAction, TContentTypeAction> : IActionProcessingStrategy<TSiteColumnAction, TContentTypeAction>
        where TSiteColumnAction : ActionBase
        where TContentTypeAction : ActionBase
    {
        private readonly IProcessorFactory<TSiteColumnAction, TContentTypeAction> _processorFactory;
        private readonly int _numberOfThreads;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MultitThreadedActionProcessingStrategy<TSiteColumnAction, TContentTypeAction>));

        public MultitThreadedActionProcessingStrategy(IProcessorFactory<TSiteColumnAction, TContentTypeAction> processorFactory, int numberOfThreads)
        {
            _processorFactory = processorFactory;
            _numberOfThreads = numberOfThreads;
        }

        public Dictionary<TContentTypeAction, IActionStatus> ProcessContentTypeActions(IEnumerable<IGrouping<SiteCollection, TContentTypeAction>> actions)
        {
            var processorsWithActions = actions.ToDictionary(
                    g => _processorFactory.CreateContentTypeProcessor(g.Key),
                    g => g.ToList());

            var counts = string.Join(", ", processorsWithActions.Select(kvp => kvp.Value.Count));
            Logger.Debug($"Processing {processorsWithActions.Sum(kvp => kvp.Value.Count)} ContentTypeAction in {processorsWithActions.Count} Site Collections.");
            Logger.Debug("Counts per site: " + counts);

            return processorsWithActions
                .AsParallel()
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .WithDegreeOfParallelism(_numberOfThreads)
                .SelectMany(kvp => ProcessContentTypeActionsInternal(kvp.Key, kvp.Value))
                .ToDictionary(t => t.Item1, t => t.Item2);
        }

        public Dictionary<TSiteColumnAction, IActionStatus> ProcessSiteColumnActions(IEnumerable<IGrouping<SiteCollection, TSiteColumnAction>> actions)
        {
            var processorsWithActions = actions.ToDictionary(
                    g => _processorFactory.CreateSiteColumnProcessor(g.Key),
                    g => g.ToList());

            var counts = string.Join(", ", processorsWithActions.Select(kvp => kvp.Value.Count));
            Logger.Debug($"Processing {processorsWithActions.Sum(kvp => kvp.Value.Count)} SiteColumnAction in {processorsWithActions.Count} Site Collections.");
            Logger.Debug("Counts per site: " + counts);

            return processorsWithActions
                .AsParallel()
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .WithDegreeOfParallelism(_numberOfThreads)
                .SelectMany(kvp => ProcessSiteColumnActionsInternal(kvp.Key, kvp.Value))
                .ToDictionary(t => t.Item1, t => t.Item2);
        }

        private static IEnumerable<Tuple<TContentTypeAction, IActionStatus>> ProcessContentTypeActionsInternal(IContentTypeProcessor<TContentTypeAction> processor, IReadOnlyList<TContentTypeAction> actions)
        {
            Debug.WriteLine($"[{DateTime.UtcNow:O}][{Thread.CurrentThread.ManagedThreadId}]Processing {actions.Count} ContentTypeAction for target {actions.Select(a => a.Target).Distinct().Single()}.");
        
            var actionStatuses = processor.ExecuteAll(actions);
          
            return actions.Zip(actionStatuses, Tuple.Create);
        }

        private static IEnumerable<Tuple<TSiteColumnAction, IActionStatus>> ProcessSiteColumnActionsInternal(ISiteColumnProcessor<TSiteColumnAction> processor, IReadOnlyList<TSiteColumnAction> actions)
        {
            Debug.WriteLine($"[{DateTime.UtcNow:O}][{Thread.CurrentThread.ManagedThreadId}]Processing {actions.Count} SiteColumnAction for target {actions.Select(a => a.Target).Distinct().Single()}.");
            var actionStatuses = processor.ExecuteAll(actions);

            return actions.Zip(actionStatuses, Tuple.Create);
        }
    }
}
 