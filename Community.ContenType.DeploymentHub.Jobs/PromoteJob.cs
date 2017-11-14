using System;
using Community.ContenType.DeploymentHub.Domain.Actions.Promote;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.Domain.Processors;
using Community.ContenType.DeploymentHub.Domain.Verifiers;
using Community.ContenType.DeploymentHub.DomainServices;
using Community.ContenType.DeploymentHub.DomainServices.Calculators;
using Community.ContenType.DeploymentHub.Contracts.Messages;
using log4net;

namespace Community.ContenType.DeploymentHub.Jobs
{
    public class PromoteJob : IJob<PromoteRequestInfo>
    {
        private readonly IEventHub _eventHub;
        private readonly IPromoteActionCalculator _actionCalculator;
        private readonly IVerificationStrategy<PromoteSiteColumnAction, PromoteContentTypeAction> _verifier;
        private readonly ISourcePropertyConfigurator _sourcePropertyConfigurator;
        private readonly IActionProcessingStrategy<PromoteSiteColumnAction, PromoteContentTypeAction> _actionCollectionProcessor;
        private readonly IPromoteRequestRetriever _requestRetriever;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PromoteJob));

        public PromoteJob(
            IEventHub eventHub, 
            IPromoteRequestRetriever requestRetriever, 
            IPromoteActionCalculator actionCalculator,
            IVerificationStrategy<PromoteSiteColumnAction, PromoteContentTypeAction> verifier, 
            ISourcePropertyConfigurator sourcePropertyConfigurator,
            IActionProcessingStrategy<PromoteSiteColumnAction, PromoteContentTypeAction> actionCollectionProcessor)
        {
            _eventHub = eventHub;
            _requestRetriever = requestRetriever;
            _actionCalculator = actionCalculator;
            _verifier = verifier;
            _sourcePropertyConfigurator = sourcePropertyConfigurator;
            _actionCollectionProcessor = actionCollectionProcessor;
        }

        public void Run(PromoteRequestInfo info)
        {
            try
            {
                var request = _requestRetriever.GetPendingRequest(info);

                Logger.Info("Promote: start CalculateActions");
                var actionCollection = _actionCalculator.CalculateActions(request);
                _eventHub.Publish(new PromoteActionsCalculatedEvent(request.ActionContext, actionCollection));

                Logger.Info("Promote: start VerifyAll");
                actionCollection.VerifyActions(_verifier);
                _eventHub.Publish(new PromoteActionsVerifiedEvent(request.ActionContext, actionCollection));
                actionCollection.UpdateImpact();
                _eventHub.Publish(new PromoteActionsImpactUpdatedEvent(request.ActionContext, actionCollection));

                Logger.Info("Promote: setting push sources on all target Site Collections");
                _sourcePropertyConfigurator.ConfigureSources(actionCollection.GetAllTargets(), actionCollection.ActionContext.Hub);

                Logger.Info("Promote: start PerformPromoteSiteColumnActions & PerformPromoteContentTypeActions");
                actionCollection.ProcessActions(_actionCollectionProcessor);
                _eventHub.Publish(new PromoteActionsExecutedEvent(request.ActionContext, actionCollection));
            }
            catch (Exception ex)
            {
                _eventHub.Publish(new PromoteActionsFailedEvent(info.GetActionContext(), ex));
                throw;
            }
        }
    }
}