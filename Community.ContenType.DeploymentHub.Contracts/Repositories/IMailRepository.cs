using Community.ContenType.DeploymentHub.Domain.Events;

namespace Community.ContenType.DeploymentHub.Contracts.Repositories
{
    public interface IMailRepository
    {
        void SendMail(PublishActionsExecutedEvent publishActionsExecutedEvent, string mailTemplate);
        void SendMail(PublishActionsFailedEvent publishActionsFailedEvent, string mailTemplate);
        void SendMail(PushActionsExecutedEvent pushActionsExecutedEvent, string mailTemplate);
        void SendMail(PushActionsFailedEvent pushActionsExecutedEvent, string mailTemplate);
        void SendMail(PromoteActionsExecutedEvent promoteActionsExecutedEvent, string mailTemplate);
        void SendMail(PromoteActionsFailedEvent promoteActionsFailedEvent, string mailTemplate);

    }
}