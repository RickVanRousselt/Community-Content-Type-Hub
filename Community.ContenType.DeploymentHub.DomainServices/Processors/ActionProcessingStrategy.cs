using System;
using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Processors;

namespace Community.ContenType.DeploymentHub.DomainServices.Processors
{
    public class ActionProcessingStrategy<TSiteColumnAction, TContentTypeAction> : IActionProcessingStrategy<TSiteColumnAction, TContentTypeAction>
        where TSiteColumnAction : ActionBase
        where TContentTypeAction : ActionBase
    {
        private readonly IProcessorFactory<TSiteColumnAction, TContentTypeAction> _processorFactory;

        public ActionProcessingStrategy(IProcessorFactory<TSiteColumnAction, TContentTypeAction> processorFactory)
        {
            _processorFactory = processorFactory;
        }

        public Dictionary<TContentTypeAction, IActionStatus> ProcessContentTypeActions(IEnumerable<IGrouping<SiteCollection, TContentTypeAction>> actions) => 
            ProcessContentTypeActionsInternal(actions).ToDictionary(t => t.Item1, t => t.Item2);

        public Dictionary<TSiteColumnAction, IActionStatus> ProcessSiteColumnActions(IEnumerable<IGrouping<SiteCollection, TSiteColumnAction>> actions) => 
            ProcessSiteColumnActionsInternal(actions).ToDictionary(t => t.Item1, t => t.Item2);

        private IEnumerable<Tuple<TContentTypeAction, IActionStatus>> ProcessContentTypeActionsInternal(IEnumerable<IGrouping<SiteCollection, TContentTypeAction>> groupedActions)
        {
            foreach (var group in groupedActions)
            {
                var processor = _processorFactory.CreateContentTypeProcessor(group.Key);
                var actions = group.ToList();
                var actionStatuses = processor.ExecuteAll(actions);

                foreach (var tuple in actions.Zip(actionStatuses, Tuple.Create))
                {
                    yield return tuple;
                }
            }
        }

        private IEnumerable<Tuple<TSiteColumnAction, IActionStatus>> ProcessSiteColumnActionsInternal(IEnumerable<IGrouping<SiteCollection, TSiteColumnAction>> groupedActions)
        {
            foreach (var group in groupedActions)
            {
                var processor = _processorFactory.CreateSiteColumnProcessor(group.Key);
                var actions = group.ToList();
                var actionStatuses = processor.ExecuteAll(actions);
                foreach (var tuple in actions.Zip(actionStatuses, Tuple.Create))
                {
                    yield return tuple;
                }
            }
        }
    }
}