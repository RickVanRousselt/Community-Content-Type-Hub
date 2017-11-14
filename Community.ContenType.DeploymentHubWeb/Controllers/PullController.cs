using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHubWeb.ApiAuthentication;
using Community.ContenType.DeploymentHubWeb.Models;
using log4net;
using System;
using System.Web.Mvc;
using Community.ContenType.DeploymentHub.Factories;

namespace Community.ContenType.DeploymentHubWeb.Controllers
{
    public class PullController : BaseController
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PullController));
        private readonly IApiKeyValidator _apiKeyValidator;

        public PullController()
        {
            _apiKeyValidator = new ApiKeyValidator();
        }

        public JsonResult SiteCollection(Uri hubSiteCollectionUrl, Uri siteCollectionUrl, string deploymentGroup, string apiKey)
        {
            using (Logger.MethodTraceLogger(hubSiteCollectionUrl, siteCollectionUrl, deploymentGroup, apiKey))
            {
                try
                {
                    var validationResult = _apiKeyValidator.Validate(hubSiteCollectionUrl, siteCollectionUrl, deploymentGroup, apiKey);
                    if (!validationResult.IsValid)
                    {
                        Logger.Warn("Key not valid - " + validationResult.ErrorMessages);
                        return Json(new PullSiteCollectionResult(false, validationResult.ErrorMessages), JsonRequestBehavior.AllowGet);
                    }
                    Logger.Info("Key is valid - Proceeding with pull operation");

                    var hub = new Hub(hubSiteCollectionUrl);
                    var factory = PullObjectFactory.FromFunctionalUserContext(hub);
                    var eventHub = factory.CreateEventHub();
                    var requestInitiator = factory.CreateRequestInitiator(eventHub);
                    requestInitiator.InitiateRequest(siteCollectionUrl, hub, deploymentGroup, true);

                    Logger.Info("Pull request succeeded");

                    return Json(new PullSiteCollectionResult(true, validationResult.ErrorMessages), JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    Logger.Error("A unexpected exception occured during pull sitecollection.", ex);
                    return Json(new PullSiteCollectionResult(false, "A unexpected error occured"), JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}