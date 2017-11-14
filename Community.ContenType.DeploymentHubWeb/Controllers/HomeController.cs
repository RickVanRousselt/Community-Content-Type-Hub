using System.Web.Mvc;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.DomainServices.Provision;
using Community.ContenType.DeploymentHub.Factories;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;

namespace Community.ContenType.DeploymentHubWeb.Controllers
{
    public class HomeController : BaseController
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HomeController));
        private readonly IMainProvisionerBase _provisionService;
        private readonly IUserRepository _userRepository;

        public HomeController()
        {
            var factory = ProvisionObjectFactory.FromRealUserContext();
            var eventHub = factory.CreateEventHub();
            _provisionService = factory.CreateMainProvisioner(eventHub);
            _userRepository = factory.CreateUserRepository();
        }

      
        public ActionResult Index()
        {
            using (Logger.MethodTraceLogger())
            {
                ViewBag.UserName = _userRepository.GetInitiatingUserTitle();
                ViewBag.FunctionalUserAccess = _userRepository.IsCurrentUserSiteAdmin();
                ViewBag.IsHub = _provisionService.IsProvisioned();
                return View();
            }
        }
    }
}
