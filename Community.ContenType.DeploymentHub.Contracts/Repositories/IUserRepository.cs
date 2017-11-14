using System.Net.Mail;

namespace Community.ContenType.DeploymentHub.Contracts.Repositories
{
    public interface IUserRepository
    {
        MailAddress GetInitiatingUser();
        MailAddress GetInitiatingUserSafe();
        bool IsCurrentUserSiteAdmin();
        string GetInitiatingUserTitle();
    }
}
