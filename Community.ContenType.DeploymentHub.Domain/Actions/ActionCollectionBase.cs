using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Processors;
using Community.ContenType.DeploymentHub.Domain.Verifiers;
using log4net;

namespace Community.ContenType.DeploymentHub.Domain.Actions
{
    public abstract class ActionCollectionBase<TSiteColumnAction, TContentTypeAction>
        where TSiteColumnAction : ActionBase
        where TContentTypeAction : ActionBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ActionCollectionBase<TSiteColumnAction, TContentTypeAction>));

        private readonly Dictionary<TContentTypeAction, IActionStatus> _contentTypeActionsInternal = new Dictionary<TContentTypeAction, IActionStatus>();
        private readonly Dictionary<TSiteColumnAction, IActionStatus> _siteColumnActionsInternal = new Dictionary<TSiteColumnAction, IActionStatus>();

        public Dictionary<TContentTypeAction, IActionStatus> ContentTypeActions => _contentTypeActionsInternal.ToDictionary(x => x.Key, x => x.Value);
        public Dictionary<TSiteColumnAction, IActionStatus> SiteColumnActions => _siteColumnActionsInternal.ToDictionary(x => x.Key, x => x.Value);

        private Dictionary<ActionBase, IActionStatus> GetAllActions()
        {
            var ctPairs = _contentTypeActionsInternal.Select(kvp => new KeyValuePair<ActionBase, IActionStatus>(kvp.Key, kvp.Value));
            var scPairs = _siteColumnActionsInternal.Select(kvp => new KeyValuePair<ActionBase, IActionStatus>(kvp.Key, kvp.Value));
            return ctPairs.Concat(scPairs).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public ISet<SiteCollection> GetAllTargets()
        {
            var siteColumnTargets = GetValidSiteColumnActionsPerTargetInOrder().Select(a => a.Key);
            var contentTypeTargets = GetValidContentTypeActionsPerTargetInOrder().Select(a => a.Key);
            return new HashSet<SiteCollection>(siteColumnTargets.Concat(contentTypeTargets));
        }

        public ActionContext ActionContext { get; }

        public int SiteColumnActionCount => _siteColumnActionsInternal.Count;
        public int ContentTypeActionCount => _contentTypeActionsInternal.Count;
        public int TotalActionCount => _siteColumnActionsInternal.Count + _contentTypeActionsInternal.Count;
        public int ValidActionCount => _siteColumnActionsInternal.Count(kvp => kvp.Value is ValidStatus) + _contentTypeActionsInternal.Count(kvp => kvp.Value is ValidStatus);
        public int SuccessActionCount => SiteColumnActions.Count(kvp => kvp.Value is SucceededStatus) + ContentTypeActions.Count(kvp => kvp.Value is SucceededStatus);

        protected ActionCollectionBase(ActionContext actionContext)
        {
            ActionContext = actionContext;
        }

        public IEnumerable<TSiteColumnAction> GetValidSiteColumnActions() => _siteColumnActionsInternal.Where(kvp => kvp.Value is ValidStatus).Select(kvp => kvp.Key);
        public IEnumerable<TContentTypeAction> GetValidContentTypeActions() => _contentTypeActionsInternal.Where(kvp => kvp.Value is ValidStatus).Select(kvp => kvp.Key);

        public abstract IEnumerable<IGrouping<SiteCollection, TSiteColumnAction>> GetValidSiteColumnActionsPerTargetInOrder();
        public abstract IEnumerable<IGrouping<SiteCollection, TContentTypeAction>> GetValidContentTypeActionsPerTargetInOrder();

        public IEnumerable<TSiteColumnAction> GetSuccessSiteColumnActions() => _siteColumnActionsInternal.Where(kvp => kvp.Value is SucceededStatus).Select(kvp => kvp.Key);
        public IEnumerable<TContentTypeAction> GetSuccessContentTypeActions() => _contentTypeActionsInternal.Where(kvp => kvp.Value is SucceededStatus).Select(kvp => kvp.Key);

        public bool AllActionsSucceeded => _contentTypeActionsInternal.Values.All(v => v is SucceededStatus) &&
                                           _siteColumnActionsInternal.Values.All(v => v is SucceededStatus);

        public void Add(TContentTypeAction action)
        {
            if (!action.ActionContext.Equals(ActionContext))
            {
                throw new InvalidOperationException($"Unexpected ActionContext: {action.ActionContext}");
            }
            if (!_contentTypeActionsInternal.ContainsKey(action))
            {
                _contentTypeActionsInternal.Add(action, new ToVerifyStatus());
            }
        }

        public void Add(TSiteColumnAction action)
        {
            if (!action.ActionContext.Equals(ActionContext))
            {
                throw new InvalidOperationException($"Unexpected ActionContext: {action.ActionContext}");
            }
            if (!_siteColumnActionsInternal.ContainsKey(action))
            {
                _siteColumnActionsInternal.Add(action, new ToVerifyStatus());
            }
        }

        private void ChangeStatus(TContentTypeAction action, IActionStatus status) => _contentTypeActionsInternal[action] = status;
        private void ChangeStatus(TSiteColumnAction action, IActionStatus status) => _siteColumnActionsInternal[action] = status;
        private void ChangeStatusesForSiteCollection(IActionStatus status, SiteCollection siteCollection)
        {
            using (Logger.MethodTraceLogger(status))
            {
                foreach (var action in _siteColumnActionsInternal.Keys.Where(a => a.Target.Equals(siteCollection)).ToList())
                {
                    _siteColumnActionsInternal[action] = status;
                }
                foreach (var action in _contentTypeActionsInternal.Keys.Where(a => a.Target.Equals(siteCollection)).ToList())
                {
                    _contentTypeActionsInternal[action] = status;
                }
            }
        }

        public void VerifyActions(IVerificationStrategy<TSiteColumnAction, TContentTypeAction> strategy)
        {
            using (Logger.MethodTraceLogger(strategy))
            {
                Logger.Debug("Starting verification of site columns");
                VerifySiteColumnActions(strategy);
                Logger.Debug("Starting verification of content types");
                VerifyContentTypeActions(strategy);
            }
        }

        private void VerifySiteColumnActions(IVerificationStrategy<TSiteColumnAction, TContentTypeAction> strategy)
        {
            using (Logger.MethodTraceLogger(strategy))
            {
                var actions = _siteColumnActionsInternal.Keys.ToList();
                var actionStatuses = strategy.VerifyActions(actions);
                foreach (var tuple in actions.Zip(actionStatuses, Tuple.Create))
                {
                    ChangeStatus(tuple.Item1, tuple.Item2);
                }
            }
        }

        private void VerifyContentTypeActions(IVerificationStrategy<TSiteColumnAction, TContentTypeAction> strategy)
        {
            using (Logger.MethodTraceLogger(strategy))
            {
                var actions = _contentTypeActionsInternal.Keys.ToList();
                var actionStatuses = strategy.VerifyActions(actions);
                foreach (var tuple in actions.Zip(actionStatuses, Tuple.Create))
                {
                    ChangeStatus(tuple.Item1, tuple.Item2);
                }
            }
        }

        public void UpdateImpact()
        {
            using (Logger.MethodTraceLogger())
            {
                var invalidActionPairs = GetAllActions()
                    .Select(kvp => Tuple.Create(kvp.Key, kvp.Value as InvalidStatus<TSiteColumnAction, TContentTypeAction>))
                    .Where(t => t.Item2 != null)
                    .ToList();

                var highImpactPairs = invalidActionPairs.Where(t => t.Item2.MaxImpactLevel == VerificationImpactLevel.SiteCollection);
                var grouped = highImpactPairs.GroupBy(x => x.Item1.Target);
                foreach (var g in grouped)
                {
                    var targetSite = g.Key;
                    Logger.Info($"Setting all actions for {targetSite} to invalid because one or more high impact verifiers failed.");
                    var violatedRules = g.SelectMany(x => x.Item2.ViolatedRules);
                    var invalidStatus = new InvalidStatus<TSiteColumnAction, TContentTypeAction>(violatedRules);
                    ChangeStatusesForSiteCollection(invalidStatus, targetSite);
                }
            }
        }

        public void ProcessActions(IActionProcessingStrategy<TSiteColumnAction, TContentTypeAction> strategy)
        {
            using (Logger.MethodTraceLogger())
            {
                Logger.Debug("Executing site column actions");
                var stopwatch = Stopwatch.StartNew();
                Logger.Info("Starting Processing site column actions");

                ProcessSiteColumnActions(strategy);
                Logger.Info("STOP Processing site column actions " + stopwatch.Elapsed);

                stopwatch = Stopwatch.StartNew();
                Logger.Info("Starting Processing Content Type actions");
                Logger.Debug("Executing content type actions");
                ProcessContentTypeActions(strategy);
                Logger.Info("STOP Processing Content Type actions " + stopwatch.Elapsed);
            }
        }

        private void ProcessContentTypeActions(IActionProcessingStrategy<TSiteColumnAction, TContentTypeAction> strategy)
        {
            using (Logger.MethodTraceLogger())
            {
                var actions = GetValidContentTypeActionsPerTargetInOrder();
                var dictionary = strategy.ProcessContentTypeActions(actions);
                foreach (var kvp in dictionary)
                {
                    ChangeStatus(kvp.Key, kvp.Value);
                }
            }
        }

        private void ProcessSiteColumnActions(IActionProcessingStrategy<TSiteColumnAction, TContentTypeAction> strategy)
        {
            using (Logger.MethodTraceLogger())
            {
                var actions = GetValidSiteColumnActionsPerTargetInOrder();
                var dictionary = strategy.ProcessSiteColumnActions(actions);
                foreach (var kvp in dictionary)
                {
                    ChangeStatus(kvp.Key, kvp.Value);
                }
            }
        }
    }
}