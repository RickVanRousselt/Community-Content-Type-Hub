using System;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.Contracts.Repositories;

namespace Community.ContenType.DeploymentHub.DomainServices.Provision
{
    class VersionProvisionerV1_0_0_4 : IVersionProvisioner
    {

        private readonly IPublishedDocTemplateListRepository _docTemplateListRepository;
        private readonly IProvisionEventHub _eventHub;
        private readonly IUserRepository _userRepository;

        public VersionProvisionerV1_0_0_4(
            IPublishedDocTemplateListRepository docTemplateListRepository,
            IProvisionEventHub eventHub,
            IUserRepository userRepository)
        {
            _docTemplateListRepository = docTemplateListRepository;
            _eventHub = eventHub;
            _userRepository = userRepository;
        }

        public Version GetVersion()
        {
            return new Version("1.0.0.4");
        }

        public void Provision(Hub hub)
        {
            var actionContext = new ActionContext(hub, _userRepository.GetInitiatingUserSafe(), -1, Guid.Empty);
            try
            {
                _docTemplateListRepository.EnsureDocTemplateLib();
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
