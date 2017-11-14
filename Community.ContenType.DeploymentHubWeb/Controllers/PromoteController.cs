using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain;
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
    public class PromoteController : BaseController
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PromoteController));
        private readonly IGroupingRepository _groupingRepository;
        private readonly IPublishPushPromoteRequestInitiator _requestInitiator;
        private readonly IPublishedContentTypesListRepository _publishedContentTypesListRepository;
        private readonly IConfigurationListRepository _configurationListRepository;
        private readonly IContentTypeRepository _contentTypeRepository;

        private const string ContentTypesTempDataName = "ContentTypes";

        public PromoteController()
        {
            try
            {
                var factory = PromoteObjectFactory.FromRealUserContext();
                var eventHub = factory.CreateEventHub();
                _groupingRepository = factory.CreateGroupingRepository();
                _requestInitiator = factory.CreateRequestInitiator(eventHub);
                _publishedContentTypesListRepository = factory.CreatePublishedContentTypesListRepository();
                _configurationListRepository = factory.CreateConfigurationListRepository();
                _contentTypeRepository = factory.CreateContentTypeRepository();
            }
            catch (Exception ex)
            {
                Logger.Error("Error while getting promote content type view.", ex);
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

                var configs = _configurationListRepository.GetConfigs();
                var promoteSourceFound = configs.ContainsKey(Configs.PromoteTarget) && !string.IsNullOrEmpty(configs[Configs.PromoteTarget]);

                string nextPillar = "";
                if (promoteSourceFound)
                {
                    nextPillar = _configurationListRepository.GetConfigs()[Configs.PromoteTarget];
                }

                return View(new PromoteIndexViewModel(treeData, promoteSourceFound, nextPillar));
            }
        }

        [HttpPost]
        public ActionResult ConfirmPromote(string selectedContentTypeData)
        {
            using (Logger.MethodTraceLogger())
            {
                Logger.InfoFormat("Selected Content Type Data: {0}", selectedContentTypeData);

                var uniqueContentTypeIds = JsonConvert.DeserializeObject<HashSet<string>>(selectedContentTypeData);
                var contentTypesToConfirm = _contentTypeRepository.GetContentTypes(uniqueContentTypeIds);
                TempData[ContentTypesTempDataName] = contentTypesToConfirm;

                return View(contentTypesToConfirm);
            }
        }

        public ActionResult Promote(bool overrideChecksData)
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