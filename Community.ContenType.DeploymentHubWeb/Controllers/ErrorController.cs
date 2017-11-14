using System.Web.Mvc;

namespace Community.ContenType.DeploymentHubWeb.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult NotAuthorized(string sphosturl)
        {
            return View();
        }
    }
}