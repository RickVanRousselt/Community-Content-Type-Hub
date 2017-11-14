using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.Contracts.Repositories;

namespace Community.ContenType.DeploymentHub.DomainServices.Loggers
{
    public class MasterDbLogger : IEventListener
    {
        private readonly IDbEntryRepository _dbEntryRepository;

        public MasterDbLogger(IDbEntryRepository dbEntryRepository)
        {
            _dbEntryRepository = dbEntryRepository;
        }

        #region Provision

        public void Handle(ProvisionActionExecutedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(ProvisionActionFailedEvent e)
        {
           _dbEntryRepository.Log(e);
        }

        #endregion

        #region Publish
        public void Handle(PublishRequestInitiatedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PublishRequestInitiateFailedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PublishActionsCalculatedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PublishActionsVerifiedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PublishActionsImpactUpdatedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PublishSiteColumnVerifiedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PublishContentTypeVerifiedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PublishSiteColumnActionExecutedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PublishSiteColumnActionFailedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PublishContentTypeActionExecutedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PublishContentTypeActionFailedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PublishActionsExecutedEvent e)
        {
            _dbEntryRepository.Log(e);
        }
        public void Handle(PublishActionsFailedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        #endregion

        #region Push
        public void Handle(PushRequestInitiatedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PushRequestInitiateFailedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PushActionsCalculatedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PushActionsVerifiedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PushActionsImpactUpdatedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PushSiteColumnVerifiedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PushContentTypeVerifiedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PushSiteColumnActionExecutedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PushSiteColumnActionFailedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PushContentTypeActionExecutedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PushContentTypeActionFailedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PushActionsExecutedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PushActionsFailedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        #endregion

        #region Pull

        public void Handle(PullRequestInitiatedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PullActionsFailedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        #endregion

        #region Promote

        public void Handle(PromoteRequestInitiatedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PromoteRequestInitiateFailedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PromoteActionsCalculatedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PromoteSiteColumnVerifiedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PromoteContentTypeVerifiedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PromoteActionsVerifiedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PromoteActionsImpactUpdatedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PromoteSiteColumnActionExecutedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PromoteSiteColumnActionFailedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PromoteContentTypeActionExecutedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PromoteContentTypeActionFailedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PromoteActionsExecutedEvent e)
        {
            _dbEntryRepository.Log(e);
        }

        public void Handle(PromoteActionsFailedEvent e)
        {
            _dbEntryRepository.Log(e);
        }
        
        #endregion
    }
}
