using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.DomainServices.RequestInitiators;
using Community.ContenType.DeploymentHub.Factories;
using log4net;
using Newtonsoft.Json;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using Community.ContenType.DeploymentHubWeb.ModelBuilders;
using Community.ContenType.DeploymentHubWeb.ViewModels;

namespace Community.ContenType.DeploymentHubWeb.Controllers
{
    public class PublishController : BaseController
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PublishController));
        private readonly IGroupingRepository _listItemRepository;
        private readonly IPublishedContentTypesListRepository _publishedContentTypesListRepository;
        private readonly IPublishPushPromoteRequestInitiator _requestInitiator;
        private readonly IContentTypeRepository _contentTypeRepository;

        private const string ContentTypesTempDataName = "ContentTypes";

        public PublishController()
        {
            try
            {
                var factory = PublishObjectFactory.FromRealUserContext();
                var eventHub = factory.CreateEventHub();
                _listItemRepository = factory.CreateGroupingRepository();
                _publishedContentTypesListRepository = factory.CreatePublishedContentTypesListRepository();
                _requestInitiator = factory.CreateRequestInitiator(eventHub);
                _contentTypeRepository = factory.CreateContentTypeRepository();
            }
            catch (Exception ex)
            {
                Logger.Error("Error while getting publish content type view.", ex);
                throw;
            }
        }


        public ActionResult Index()
        {
            using (Logger.MethodTraceLogger())
            {
                var groupings = _listItemRepository.GetGroupings();
                Logger.Info($"Groupings returned: {groupings}");

                var publishedContentTypes = _publishedContentTypesListRepository.GetAllPublishedItemsNoSchema().ToList();
                Logger.Info($"Published Content Types returned: {publishedContentTypes}");

                var builder = new ContentTypeSelectionModelBuilder(groupings);
                var treeData = builder.GetDeploymentGroupSubTreesIgnoreSiteCollections(publishedContentTypes).ToList();
                Logger.Info($"TreeData to return: {treeData}");
                
                return View(new TreeViewModel(treeData));
            }
        }


        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPublish(string selectedContentTypeData)
        {
            using (Logger.MethodTraceLogger())
            {
                Logger.InfoFormat("Selected Content Type Data: {0}", selectedContentTypeData);

                var selectedContentTypes = JsonConvert.DeserializeObject<HashSet<string>>(selectedContentTypeData);
                var contentTypesToConfirm = _contentTypeRepository.GetContentTypes(selectedContentTypes);
                TempData[ContentTypesTempDataName] = contentTypesToConfirm;
                return View(contentTypesToConfirm);
            }
        }

   
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Publish(bool overrideChecksData)
        {
            using (Logger.MethodTraceLogger())
            {
                var contentTypes = (ISet<ContentType>)TempData[ContentTypesTempDataName];
                var contentTypeInfos = new HashSet<ContentTypeInfo>(contentTypes);
                _requestInitiator.InitiateRequest(contentTypeInfos, new Hub(new Uri(ViewBag.SPHostUrl)), !overrideChecksData); 
                return View();
            }
        }
    }
}