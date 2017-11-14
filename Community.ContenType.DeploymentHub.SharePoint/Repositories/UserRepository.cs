using System;
using System.Net;
using System.Net.Mail;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Microsoft.SharePoint.Client;

namespace Community.ContenType.DeploymentHub.SharePoint.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IClientContextProvider _clientContextProvider;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UserRepository));

        public UserRepository(IClientContextProvider clientContextProvider)
        {
            _clientContextProvider = clientContextProvider;
        }

        public MailAddress GetInitiatingUser()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = _clientContextProvider.CreateClientContext())
            {
                var web = clientContext.Web;
                clientContext.Load(web.CurrentUser);
                clientContext.ExecuteQueryWithIncrementalRetry();
                return new MailAddress(clientContext.Web.CurrentUser.Email);
            }
        }

        public MailAddress GetInitiatingUserSafe()
        {
            try
            {
                return GetInitiatingUser();
            }
            catch (Exception)
            {
                return new MailAddress("unknown@test.be");
            }
        }

        public string GetInitiatingUserTitle()
        {
            using (var context = _clientContextProvider.CreateClientContext())
            {
                context.Load(context.Web.CurrentUser);
                context.ExecuteQueryWithIncrementalRetry();
                return context.Web.CurrentUser.Title;
            }
        }

        public bool IsCurrentUserSiteAdmin()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = _clientContextProvider.CreateClientContext())
            {
                try
                {
                    var web = clientContext.Site.RootWeb;
                    clientContext.Load(web, w => w.CurrentUser.IsSiteAdmin);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                    return clientContext.Site.RootWeb.CurrentUser.IsSiteAdmin;
                }
                catch (WebException ex)
                {
                    var httpWebResponse = ex.Response as HttpWebResponse;
                    if (httpWebResponse?.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        return false;
                    }
                    throw;
                }
                catch (ServerUnauthorizedAccessException)
                {
                    return false;
                }
            }
        }
    }
}