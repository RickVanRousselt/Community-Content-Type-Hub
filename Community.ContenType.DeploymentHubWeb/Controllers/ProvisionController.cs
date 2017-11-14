using System;
using System.Web.Mvc;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.DomainServices.Provision;
using Community.ContenType.DeploymentHub.Factories;
using log4net;

namespace Community.ContenType.DeploymentHubWeb.Controllers
{
    public class ProvisionController : BaseController
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ProvisionController));
        private readonly IMainProvisionerBase _provisionService;

        public ProvisionController()
        {
            var factory = ProvisionObjectFactory.FromRealUserContext();
            var eventHub = factory.CreateEventHub();
            _provisionService = factory.CreateMainProvisioner(eventHub);
        }

        public ActionResult Index()
        {
            using (Logger.MethodTraceLogger())
            {
                try
                {
                    var hub = new Hub(new Uri(ViewBag.SPHostUrl));
                    _provisionService.Provision(hub);
                    return View();
                }
                catch (ProvisionOperationFailedException e)
                {
                    Logger.Error(e.Message, e);
                    throw;
                }
            }
        }
    }
}