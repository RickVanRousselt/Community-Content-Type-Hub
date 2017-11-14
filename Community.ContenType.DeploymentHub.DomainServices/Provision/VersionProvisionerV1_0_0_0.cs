using System;
using Community.ContenType.DeploymentHub.Domain;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.Contracts.Repositories;

namespace Community.ContenType.DeploymentHub.DomainServices.Provision
{
    class VersionProvisionerV1_0_0_0 : IVersionProvisioner
    {

        private readonly IStatusListRepository _statusListRepository;
        private readonly ITermStoreRepository _termStoreRepository;
        private readonly IConfigurationListRepository _configurationListRepository;
        private readonly IContentTypeGroupingListRepository _contentTypeGroupingListRepository;
        private readonly ISiteCollectionGroupingListRepository _siteCollectionGroupingListRepository;
        private readonly IPublishedSiteColumnsListRepository _publishedSiteColumnsListRepository;
        private readonly IPublishedContentTypesListRepository _publishedContentTypesListRepository;
        private readonly IUserRepository _userRepository;
        private readonly IProvisionEventHub _eventHub;

        public VersionProvisionerV1_0_0_0(
            IStatusListRepository statusListRepository,
            IConfigurationListRepository configurationListRepository,
            ISiteCollectionGroupingListRepository siteCollectionGroupingListRepository,
            IContentTypeGroupingListRepository contentTypeGroupingListRepository,
            IPublishedSiteColumnsListRepository publishedSiteColumnsListRepository,
            IPublishedContentTypesListRepository publishedContentTypesListRepository,
            ITermStoreRepository termStoreRepository,
            IUserRepository userRepository,
            IProvisionEventHub eventHub)
        {
            _statusListRepository = statusListRepository;
            _configurationListRepository = configurationListRepository;
            _siteCollectionGroupingListRepository = siteCollectionGroupingListRepository;
            _contentTypeGroupingListRepository = contentTypeGroupingListRepository;
            _publishedSiteColumnsListRepository = publishedSiteColumnsListRepository;
            _publishedContentTypesListRepository = publishedContentTypesListRepository;
            _termStoreRepository = termStoreRepository;
            _userRepository = userRepository;
            _eventHub = eventHub;
        }

        public Version GetVersion() => new Version("1.0.0.0");

        public void Provision(Hub hub)
        {
            var actionContext = new ActionContext(hub, _userRepository.GetInitiatingUserSafe(), -1, Guid.Empty);
            try
            {
                _configurationListRepository.EnsureConfigurationList();
                var configs = _configurationListRepository.GetConfigs();
                if (configs.ContainsKey(Configs.PushSource) && !string.IsNullOrEmpty(configs[Configs.PushSource]))
                {
                    throw new InvalidOperationException(
                        "Cannot convert a Site Collection that is the Push target of another Hub to a Hub.");
                }

                _statusListRepository.EnsureStatusList();
                _publishedSiteColumnsListRepository.EnsurePublishedSiteColumnsLibrary();
                _publishedContentTypesListRepository.EnsurePublishedContentTypesLibrary();

                var deploymentGroupsTermSetId = _termStoreRepository.EnsureDeploymentGroupsTermSet();
                _siteCollectionGroupingListRepository.EnsureSiteCollectionGroupingList(deploymentGroupsTermSetId);
                _contentTypeGroupingListRepository.EnsureContentTypeGroupingList(deploymentGroupsTermSetId);
                _eventHub.Publish(new ProvisionActionExecutedEvent(actionContext, GetVersion()));
            }
            catch (Exception ex)
            {
                _eventHub.Publish(new ProvisionActionFailedEvent(actionContext, GetVersion(), ex));
                throw new ProvisionOperationFailedException(
                    "Failed to Provision assets for Community Content Type Hub", ex);
            }
        }
    }
}