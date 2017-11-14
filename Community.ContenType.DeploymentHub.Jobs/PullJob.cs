using System;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.DomainServices;
using Community.ContenType.DeploymentHub.DomainServices.Calculators;
using log4net;
using System.Linq;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Processors;
using Community.ContenType.DeploymentHub.Domain.Verifiers;
using Community.ContenType.DeploymentHub.Contracts.Messages;
using Community.ContenType.DeploymentHub.Contracts.Repositories;

namespace Community.ContenType.DeploymentHub.Jobs
{
    public class PullJob : IJob<PullRequestInfo>
    {
        private readonly IEventHub _eventHub;
        private readonly IPullActionCalculator _actionCalculator;
        private readonly IVerificationStrategy<PushSiteColumnAction, PushContentTypeAction> _verifier;
        private readonly IPullRequestRetriever _pullRequestRetriever;
        private readonly ISiteCollectionGroupingListRepository _siteCollectionGroupingListRepository; 
        private readonly IGroupingRepository _groupingRepository;
        private readonly ISourcePropertyConfigurator _sourcePropertyConfigurator;
        private readonly IActionProcessingStrategy<PushSiteColumnAction, PushContentTypeAction> _actionCollectionProcessor;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PullJob));

        public PullJob(
            IEventHub eventHub,
            IPullRequestRetriever pullRequestRetriever, 
            IPullActionCalculator actionCalculator,
            IVerificationStrategy<PushSiteColumnAction, PushContentTypeAction> verifier,
            ISiteCollectionGroupingListRepository siteCollectionGroupingListRepository,
            IGroupingRepository groupingRepository,
            ISourcePropertyConfigurator sourcePropertyConfigurator,
            IActionProcessingStrategy<PushSiteColumnAction, PushContentTypeAction> actionCollectionProcessor)
        {
            _eventHub = eventHub;
            _pullRequestRetriever = pullRequestRetriever;
            _actionCalculator = actionCalculator;
            _verifier = verifier;
            _siteCollectionGroupingListRepository = siteCollectionGroupingListRepository;
            _groupingRepository = groupingRepository;
            _sourcePropertyConfigurator = sourcePropertyConfigurator;
            _actionCollectionProcessor = actionCollectionProcessor;
        }

        public void Run(PullRequestInfo info)
        {
            try
            {
                var request = _pullRequestRetriever.GetPendingRequest(info);
                Logger.Info("Pull: add sitecollection to deploymentgroup");
                CreateSiteCollectionInDeploymentGroup(request.SiteCollectionUrl, request.DeploymentGroup);

                Logger.Info("Pull: start CalculateActions");
                var actionCollection = _actionCalculator.CalculateActions(request);
                _eventHub.Publish(new PushActionsCalculatedEvent(request.ActionContext, actionCollection));

                Logger.Info("Pull: start VerifyAll");
                actionCollection.VerifyActions(_verifier);
                _eventHub.Publish(new PushActionsVerifiedEvent(request.ActionContext, actionCollection));
                actionCollection.UpdateImpact();
                _eventHub.Publish(new PushActionsImpactUpdatedEvent(request.ActionContext, actionCollection));

                Logger.Info("Pull: setting push sources on all target Site Collections");
                _sourcePropertyConfigurator.ConfigureSources(actionCollection.GetAllTargets(), actionCollection.ActionContext.Hub);

                Logger.Info("Pull: start PerformPushSiteColumnActions & PerformPushContentTypeActions");
                actionCollection.ProcessActions(_actionCollectionProcessor);
                _eventHub.Publish(new PushActionsExecutedEvent(request.ActionContext, actionCollection));
            }
            catch (Exception ex)
            {
                _eventHub.Publish(new PullActionsFailedEvent(info.GetActionContext(), ex));
                throw;
            }
        }

        private void CreateSiteCollectionInDeploymentGroup(Uri pullRequestSiteCollectionUrl, DeploymentGroup deploymentGroup)
        {
            var groupings = _groupingRepository.GetGroupings();
            var existigSiteCollections = groupings.GetSiteCollectionsFor(deploymentGroup);

            if (existigSiteCollections.All(sc => sc.Url != pullRequestSiteCollectionUrl))
            {
                _siteCollectionGroupingListRepository.AddSiteCollectionToDeploymentGroup(deploymentGroup, pullRequestSiteCollectionUrl);
            }
        }
    }
}