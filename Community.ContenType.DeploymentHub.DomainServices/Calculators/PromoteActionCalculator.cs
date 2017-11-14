using System;
using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Actions.Promote;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Requests;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;

namespace Community.ContenType.DeploymentHub.DomainServices.Calculators
{
    public class PromoteActionCalculator : IPromoteActionCalculator
    {
        private readonly IPublishedSiteColumnsListRepository _publishedSiteColumnsListRepository;
        private readonly IPublishedContentTypesListRepository _publishedContentTypesListRepository;
        private readonly IConfigurationListRepository _configurationListRepository;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PromoteActionCalculator));

        public PromoteActionCalculator(
            IPublishedSiteColumnsListRepository publishedSiteColumnsListRepository,
            IPublishedContentTypesListRepository publishedContentTypesListRepository,
            IConfigurationListRepository configurationListRepository)
        {
            _publishedSiteColumnsListRepository = publishedSiteColumnsListRepository;
            _publishedContentTypesListRepository = publishedContentTypesListRepository;
            _configurationListRepository = configurationListRepository;
        }

        public PromoteActionCollection CalculateActions(PromoteRequest pushRequest)
        {
            using (Logger.MethodTraceLogger(pushRequest))
            {
                var publishedSiteColumnList = _publishedSiteColumnsListRepository.GetList();
                var publishedContentTypeList = _publishedContentTypesListRepository.GetList();

                var configs = _configurationListRepository.GetConfigs();
                if (!configs.ContainsKey(Configs.PromoteTarget))
                {
                    throw new Exception($"Config '{Configs.PromoteTarget}' must be present in the Configuration List before attempting a Promote action.");
                }

                var targetHub = new Hub(new Uri(configs[Configs.PromoteTarget]));
                var actionCollection = new PromoteActionCollection(pushRequest.ActionContext);
                foreach (var publishedContentType in publishedContentTypeList.GetContentTypes(pushRequest.ContentTypeIds))
                {
                    AddActionsForContentType(actionCollection, publishedContentType, targetHub, publishedSiteColumnList, pushRequest.ActionContext);
                }

                return actionCollection;
            }
        }

        private static void AddActionsForContentType(
            PromoteActionCollection actionCollection,
            PublishedContentType publishedContentType, 
            Hub targetHub,
            PublishedSiteColumnList publishedSiteColumnList, 
            ActionContext actionContext)
        {
            actionCollection.Add(new PromoteContentTypeAction(actionContext, publishedContentType, targetHub));
            var siteColumnInfos = publishedContentType.ExtractSiteColumnInfos();
            var siteColumnIds = new HashSet<Guid>(siteColumnInfos.Select(info => info.Id));
            foreach (var publishedSiteColumn in publishedSiteColumnList.GetSiteColumns(siteColumnIds))
            {
                var promoteSiteColumnAction = new PromoteSiteColumnAction(actionContext, targetHub, publishedSiteColumn);
                actionCollection.Add(promoteSiteColumnAction);
            }
        }
    }
}