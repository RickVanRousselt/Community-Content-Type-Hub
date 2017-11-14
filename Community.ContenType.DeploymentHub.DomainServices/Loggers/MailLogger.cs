using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.DomainServices.Loggers.Resources;
using Community.ContenType.DeploymentHub.Contracts.Repositories;

namespace Community.ContenType.DeploymentHub.DomainServices.Loggers
{
    public class MailLogger : IEventListener
    {
        private readonly IMailRepository _mailRepository;

        public MailLogger(IMailRepository mailRepository)
        {
            _mailRepository = mailRepository;
        }

        #region Provision

        public void Handle(ProvisionActionExecutedEvent e)
        {
        
        }

        public void Handle(ProvisionActionFailedEvent e)
        {
           
        }

        #endregion

        #region Publish
        public void Handle(PublishRequestInitiatedEvent e)
        {
          
        }

        public void Handle(PublishRequestInitiateFailedEvent e)
        {
           
        }

        public void Handle(PublishActionsCalculatedEvent e)
        {
           
        }

        public void Handle(PublishSiteColumnVerifiedEvent e)
        {
          
        }

        public void Handle(PublishContentTypeVerifiedEvent e)
        {
          
        }

        public void Handle(PublishActionsVerifiedEvent e)
        {
          
        }

        public void Handle(PublishActionsImpactUpdatedEvent e)
        {

        }

        public void Handle(PublishSiteColumnActionExecutedEvent e)
        {
           
        }

        public void Handle(PublishSiteColumnActionFailedEvent e)
        {
           
        }

        public void Handle(PublishContentTypeActionExecutedEvent e)
        {
            
        }

        public void Handle(PublishContentTypeActionFailedEvent e)
        {
           
        }

        public void Handle(PublishActionsExecutedEvent e)
        {
            _mailRepository.SendMail(e, Summary.PublishExecutedMail);
        }

        public void Handle(PublishActionsFailedEvent e)
        {
            _mailRepository.SendMail(e, Summary.PublishFailedMail);
        }

        #endregion

        #region Push

        public void Handle(PushRequestInitiatedEvent e)
        {
          
        }

        public void Handle(PushRequestInitiateFailedEvent e)
        {
      
        }

        public void Handle(PushActionsCalculatedEvent e)
        {
          
        }

        public void Handle(PushSiteColumnVerifiedEvent e)
        {
          
        }

        public void Handle(PushContentTypeVerifiedEvent e)
        {

        }

        public void Handle(PushActionsVerifiedEvent e)
        {
         
        }

        public void Handle(PushActionsImpactUpdatedEvent e)
        {

        }

        public void Handle(PushSiteColumnActionExecutedEvent e)
        {
        
        }

        public void Handle(PushSiteColumnActionFailedEvent e)
        {
           
        }

        public void Handle(PushContentTypeActionExecutedEvent e)
        {
          
        }

        public void Handle(PushContentTypeActionFailedEvent e)
        {
           
        }

        public void Handle(PushActionsExecutedEvent e)
        {
            _mailRepository.SendMail(e, Summary.PushExecutedMail);
        }
        public void Handle(PushActionsFailedEvent e)
        {
            _mailRepository.SendMail(e, Summary.PushFailedMail);
        }

        #endregion

        #region Pull

        public void Handle(PullRequestInitiatedEvent e)
        {
            
        }

        public void Handle(PullActionsFailedEvent e)
        {
        }

        #endregion

        #region Promote
        public void Handle(PromoteRequestInitiatedEvent e)
        {
          
        }

        public void Handle(PromoteRequestInitiateFailedEvent e)
        {
          
        }

        public void Handle(PromoteActionsCalculatedEvent e)
        {
          
        }

        public void Handle(PromoteSiteColumnVerifiedEvent e)
        {
            
        }

        public void Handle(PromoteContentTypeVerifiedEvent e)
        {
            
        }

        public void Handle(PromoteActionsVerifiedEvent e)
        {
           
        }

        public void Handle(PromoteActionsImpactUpdatedEvent e)
        {

        }

        public void Handle(PromoteSiteColumnActionExecutedEvent e)
        {
           
        }

        public void Handle(PromoteSiteColumnActionFailedEvent e)
        {
          
        }

        public void Handle(PromoteContentTypeActionExecutedEvent e)
        {
          
        }

        public void Handle(PromoteContentTypeActionFailedEvent e)
        {
          
        }

        public void Handle(PromoteActionsExecutedEvent e)
        {
            _mailRepository.SendMail(e, Summary.PromoteExecutedMail);
        }

        public void Handle(PromoteActionsFailedEvent e)
        {
            _mailRepository.SendMail(e, Summary.PromoteFailedMail);
        }
        
        #endregion
    }
}
