using System.Web.Mvc;
using log4net;

namespace Community.ContenType.DeploymentHubWeb.Controllers
{
    public class BaseController : Controller
    {
        private static ILog Logger { get; } = LogManager.GetLogger(typeof(BaseController));
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Logger.IsDebugEnabled) Logger.DebugFormat("Entering BaseController");

            ViewBag.SPHostUrl = Request.QueryString["SPHostUrl"];
            ViewBag.SPLanguage = Request.QueryString["SPLanguage"];
            ViewBag.SPClientTag = Request.QueryString["SPClientTag"];
            ViewBag.SPProductNumber = Request.QueryString["SPProductNumber"];
            ViewBag.SPHostTitle = Request.QueryString["SPHostTitle"];
            ViewBag.PreviousPage = Request.UrlReferrer;
            if (Logger.IsDebugEnabled) Logger.Debug(Request.QueryString["SPHostUrl"]);
            base.OnActionExecuting(filterContext);

            if (Logger.IsDebugEnabled) Logger.DebugFormat("Exit BaseController");
        }
    }
}