using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.DomainServices.RequestInitiators;
using Community.ContenType.DeploymentHub.Factories;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Newtonsoft.Json;
using Community.ContenType.DeploymentHubWeb.ViewModels;
using Community.ContenType.DeploymentHubWeb.ModelBuilders;

namespace Community.ContenType.DeploymentHubWeb.Controllers
{
    public class PushController : BaseController
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PushController));
        private readonly IGroupingRepository _groupingRepository;
        private readonly IPublishPushPromoteRequestInitiator _requestInitiator;
        private readonly IPublishedContentTypesListRepository _publishedContentTypesListRepository;
        private readonly IContentTypeRepository _contentTypeRepository;

        private const string ContentTypesTempDataName = "ContentTypes";

        public PushController()
        {
            try
            {
                var factory = PushObjectFactory.FromRealUserContext();
                var eventHub = factory.CreateEventHub();
                _groupingRepository = factory.CreateGroupingRepository();
                _requestInitiator = factory.CreateRequestInitiator(eventHub);
                _publishedContentTypesListRepository = factory.CreatePublishedContentTypesListRepository();
                _contentTypeRepository = factory.CreateContentTypeRepository();
            }
            catch (Exception ex)
            {
                Logger.Error("Error while getting push content type view.", ex);
                throw;
            }
        }

    
        public ActionResult Index()
        {
            using (Logger.MethodTraceLogger())
            {
                var groupings = _groupingRepository.GetGroupings();
                Logger.Info($"groupings returned: {groupings}");

                var publishedContentTypes = _publishedContentTypesListRepository.GetAllPublishedItemsNoSchema().ToList();
                Logger.Info($"Published Content Types returned: {publishedContentTypes}");

                var builder = new ContentTypeSelectionModelBuilder(groupings);
                var treeData = builder.GetDeploymentGroupSubTrees(publishedContentTypes).ToList();
                Logger.Info($"Treedata to return: {treeData}");

                return View(new TreeViewModel(treeData));
            }
        }

        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPush(string selectedContentTypeData)
        {
            using (Logger.MethodTraceLogger())
            {
                Logger.InfoFormat("Selected Content Type Data: {0}", Request.Form["selectedContentTypeData"]);

                var selectedContentTypes = JsonConvert.DeserializeObject<HashSet<string>>(selectedContentTypeData);
                var contentTypesToConfirm = _contentTypeRepository.GetContentTypes(selectedContentTypes);
                TempData[ContentTypesTempDataName] = contentTypesToConfirm;

                var groupings = _groupingRepository.GetGroupings(); //TODO optimize for performance
                var dictionary = contentTypesToConfirm.ToDictionary(x => x, y => groupings.GetSiteCollectionsFor(y).ToList());

                return View(dictionary);
            }
        }

        
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Push(bool overrideChecksData)
        {
            using (Logger.MethodTraceLogger(overrideChecksData))
            {
                var contentTypes = (ISet<ContentType>)TempData[ContentTypesTempDataName];
                var contentTypeInfos = new HashSet<ContentTypeInfo>(contentTypes);
                _requestInitiator.InitiateRequest(contentTypeInfos, new Hub(new Uri(ViewBag.SPHostUrl)), !overrideChecksData); 
                return View();
            }
        }

        [HttpPost] 
        public JsonResult GetSiteCollections(string name, string id)
        {
            using (Logger.MethodTraceLogger(name, id))
            {
                var group = new DeploymentGroup(name, new Guid(id));
                var groupings = _groupingRepository.GetGroupings();
                var siteCollections = groupings.GetSiteCollectionsFor(group);
                Logger.Info($"Result: {siteCollections}");

                return Json(JsonConvert.SerializeObject(siteCollections));
            }
        }
    }
}