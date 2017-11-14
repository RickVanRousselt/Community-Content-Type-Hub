using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Community.ContenType.DeploymentHub.Common.ClientContextProviders;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.SharePoint;
using log4net;

namespace Community.ContenType.DeploymentHubWeb.Controllers
{
    public class AuthenticationController : Controller
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AuthenticationController));
        private readonly IClientContextProvider _clientContextProvider;

        public AuthenticationController()
        {
            _clientContextProvider = new UserClientContextForSPHostProvider();
        }

        public ActionResult Login()
        {
            var authenticated = Authenticate();

            if (!authenticated) return RedirectToAction("NotAuthorized", "Error", new {SpHostUrl = Request.QueryString["SPHostUrl"]});

            var redirectUrl = HttpContext.Request.Url.Port != 80 
                ? $"https://{HttpContext.Request.Url.Host}:{HttpContext.Request.Url.Port}{Request.Params["returnUrl"]}"
                : $"https://{HttpContext.Request.Url.Host}{Request.Params["returnUrl"]}";

            return new RedirectResult(redirectUrl);
        }

        private bool Authenticate()
        {
            var clientContext = _clientContextProvider.CreateClientContext();

            var user = clientContext.Web.CurrentUser;
            clientContext.Load(user);
            clientContext.ExecuteQueryWithIncrementalRetry();

            var userName = user.LoginName.Split('|').Last();
            var admins = ConfigurationManager.AppSettings.Get("AdminUsers").Split(';');

            var loggedOnUserIsAdmin = admins.Contains(userName);

            if (loggedOnUserIsAdmin)
            {
                FormsAuthentication.SetAuthCookie(user.LoginName, false);
            }

            return loggedOnUserIsAdmin;
        }
    }
}