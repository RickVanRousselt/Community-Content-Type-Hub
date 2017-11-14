using System;
using System.Diagnostics;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.Domain.Processors;
using Community.ContenType.DeploymentHub.Domain.Verifiers;
using Community.ContenType.DeploymentHub.DomainServices;
using Community.ContenType.DeploymentHub.DomainServices.Calculators;
using Community.ContenType.DeploymentHub.Contracts.Messages;
using log4net;

namespace Community.ContenType.DeploymentHub.Jobs
{
    public class PushJob : IJob<PushRequestInfo>
    {
        private readonly IEventHub _eventHub;
        private readonly IPushActionCalculator _actionCalculator;
        private readonly IVerificationStrategy<PushSiteColumnAction, PushContentTypeAction> _verifier;
        private readonly ISourcePropertyConfigurator _sourcePropertyConfigurator;
        private readonly IActionProcessingStrategy<PushSiteColumnAction, PushContentTypeAction> _actionCollectionProcessor;
        private readonly IPushRequestRetriever _requestRetriever;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PushJob));

        public PushJob(
            IEventHub eventHub, 
            IPushRequestRetriever requestRetriever,
            IPushActionCalculator actionCalculator, 
            IVerificationStrategy<PushSiteColumnAction, PushContentTypeAction> verifier,
            ISourcePropertyConfigurator sourcePropertyConfigurator,
            IActionProcessingStrategy<PushSiteColumnAction, PushContentTypeAction> actionCollectionProcessor)
        {
            _eventHub = eventHub;
            _requestRetriever = requestRetriever;
            _actionCalculator = actionCalculator;
            _verifier = verifier;
            _sourcePropertyConfigurator = sourcePropertyConfigurator;
            _actionCollectionProcessor = actionCollectionProcessor;
        }

        public void Run(PushRequestInfo info)
        {
            try
            {
                var request = _requestRetriever.GetPendingRequest(info);

                var stopwatch = Stopwatch.StartNew();
                Logger.Info("Start Calculate ");
                Logger.Info("Push: start CalculateActions");
                var actionCollection = _actionCalculator.CalculateActions(request);
                _eventHub.Publish(new PushActionsCalculatedEvent(request.ActionContext, actionCollection));
                Logger.Info("Stop Calculate " + stopwatch.Elapsed);

                stopwatch = Stopwatch.StartNew();
                Logger.Info("Start Verify");

                Logger.Info("Push: start VerifyAll");
                actionCollection.VerifyActions(_verifier);
                _eventHub.Publish(new PushActionsVerifiedEvent(request.ActionContext, actionCollection));
                actionCollection.UpdateImpact();
                _eventHub.Publish(new PushActionsImpactUpdatedEvent(request.ActionContext, actionCollection));
                Logger.Info("Stop Verify " + stopwatch.Elapsed);


                stopwatch = Stopwatch.StartNew();
                Logger.Info("Start Config ");
                Logger.Info("Push: setting push sources on all target Site Collections");
                _sourcePropertyConfigurator.ConfigureSources(actionCollection.GetAllTargets(), actionCollection.ActionContext.Hub);
                Logger.Info("Stop Config " + stopwatch.Elapsed);

                stopwatch = Stopwatch.StartNew();
                Logger.Info("Start process actions ");
                Logger.Info("Push: start PerformPushSiteColumnActions & PerformPushContentTypeActions");
                actionCollection.ProcessActions(_actionCollectionProcessor);
                _eventHub.Publish(new PushActionsExecutedEvent(request.ActionContext, actionCollection));
                Logger.Info("End process actions " + stopwatch.Elapsed);

            }
            catch (Exception ex)
            {
                _eventHub.Publish(new PushActionsFailedEvent(info.GetActionContext(), ex));
                throw;
            }
        }
    }
}