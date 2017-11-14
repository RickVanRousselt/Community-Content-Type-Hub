using System;
using Community.ContenType.DeploymentHub.Domain.Actions.Publish;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.Domain.Processors;
using Community.ContenType.DeploymentHub.Domain.Verifiers;
using Community.ContenType.DeploymentHub.DomainServices;
using Community.ContenType.DeploymentHub.DomainServices.Calculators;
using Community.ContenType.DeploymentHub.Contracts.Messages;
using log4net;

namespace Community.ContenType.DeploymentHub.Jobs
{
    public class PublishJob : IJob<PublishRequestInfo>
    {
        private readonly IEventHub _eventHub;
        private readonly IPublishActionCalculator _actionCalculator;
        private readonly IVerificationStrategy<PublishSiteColumnAction, PublishContentTypeAction> _verifier;
        private readonly IActionProcessingStrategy<PublishSiteColumnAction, PublishContentTypeAction> _actionCollectionProcessor;
        private readonly IPublishRequestRetriever _requestRetriever;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PublishJob));

        public PublishJob(
            IEventHub eventHub,
            IPublishRequestRetriever requestRetriever,
            IPublishActionCalculator actionCalculator,
            IVerificationStrategy<PublishSiteColumnAction, PublishContentTypeAction> verifier,
            IActionProcessingStrategy<PublishSiteColumnAction, PublishContentTypeAction> actionCollectionProcessor)
        {
            _eventHub = eventHub;
            _requestRetriever = requestRetriever;
            _actionCalculator = actionCalculator;
            _verifier = verifier;
            _actionCollectionProcessor = actionCollectionProcessor;
        }

        public void Run(PublishRequestInfo info)
        {
            try
            {
                var request = _requestRetriever.GetPendingRequest(info);
                Logger.Info("Publish: start CalculateActions");
                var actionCollection = _actionCalculator.CalculateActions(request);
                _eventHub.Publish(new PublishActionsCalculatedEvent(request.ActionContext, actionCollection));

                Logger.Info("Publish: start VerifyAll");
                actionCollection.VerifyActions(_verifier);
                _eventHub.Publish(new PublishActionsVerifiedEvent(request.ActionContext, actionCollection));
                Logger.Info("Publish: start Update Impact");
                actionCollection.UpdateImpact();
                _eventHub.Publish(new PublishActionsImpactUpdatedEvent(request.ActionContext, actionCollection));

                Logger.Info("Publish: start PerformPublishSiteColumnActions & PerformPublishContentTypeActions");
                actionCollection.ProcessActions(_actionCollectionProcessor);
                _eventHub.Publish(new PublishActionsExecutedEvent(request.ActionContext, actionCollection));
            }
            catch (Exception ex)
            {
                _eventHub.Publish(new PublishActionsFailedEvent(info.GetActionContext(), ex));
                throw;
            }
        }
    }
}