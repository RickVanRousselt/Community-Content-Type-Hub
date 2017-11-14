using System;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.Contracts.Repositories;

namespace Community.ContenType.DeploymentHub.DomainServices.Provision
{
    class VersionProvisionerV1_0_0_1 : IVersionProvisioner
    {

        private readonly IStatusListRepository _statusListRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPublishedSiteColumnsListRepository _publishedSiteColumnsListRepository;
        private readonly IPublishedContentTypesListRepository _publishedContentTypesListRepository;
        private readonly IProvisionEventHub _eventHub;

        public VersionProvisionerV1_0_0_1(
            IStatusListRepository statusListRepository,
            IUserRepository userRepository,
            IPublishedSiteColumnsListRepository publishedSiteColumnsListRepository,
            IPublishedContentTypesListRepository publishedContentTypesListRepository,
            IProvisionEventHub eventHub)
        {
            _statusListRepository = statusListRepository;
            _userRepository = userRepository;
            _publishedSiteColumnsListRepository = publishedSiteColumnsListRepository;
            _publishedContentTypesListRepository = publishedContentTypesListRepository;
            _eventHub = eventHub;
        }

        public Version GetVersion() => new Version("1.0.0.1");

        public void Provision(Hub hub)
        {
            var actionContext = new ActionContext(hub, _userRepository.GetInitiatingUserSafe(), -1, Guid.Empty);
            try
            {
                _statusListRepository.UpdateView();
                _publishedContentTypesListRepository.UpdateView();
                _publishedSiteColumnsListRepository.UpdateView();

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
