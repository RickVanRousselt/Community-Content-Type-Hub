using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Requests;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Contracts.Repositories;

namespace Community.ContenType.DeploymentHub.DomainServices.Calculators
{
    public class PullActionCalculator : IPullActionCalculator
    {
        private readonly IGroupingRepository _groupingRepository;
        private readonly IPublishedSiteColumnsListRepository _publishedSiteColumnsListRepository;
        private readonly IPublishedContentTypesListRepository _publishedContentTypesListRepository;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PullActionCalculator));

        public PullActionCalculator(
            IGroupingRepository groupingRepository,
            IPublishedSiteColumnsListRepository publishedSiteColumnsListRepository,
            IPublishedContentTypesListRepository publishedContentTypesListRepository)
        {
            _groupingRepository = groupingRepository;
            _publishedSiteColumnsListRepository = publishedSiteColumnsListRepository;
            _publishedContentTypesListRepository = publishedContentTypesListRepository;
        }

        public PushActionCollection CalculateActions(PullRequest info)
        {
            using (Logger.MethodTraceLogger(info))
            {
                var siteCollection = new SiteCollection(info.SiteCollectionUrl);
                var groupings = _groupingRepository.GetGroupings();
                var contentTypes = new HashSet<string>(groupings.GetContentTypesFor(info.DeploymentGroup).Select(c => c.Id));

                var publishedSiteColumnList = _publishedSiteColumnsListRepository.GetList();
                var publishedContentTypeList = _publishedContentTypesListRepository.GetList();

                var actionCollection = new PushActionCollection(info.ActionContext);

                foreach (var publishedContentType in publishedContentTypeList.GetContentTypes(contentTypes))
                {
                    actionCollection.Add(new PushContentTypeAction(info.ActionContext, publishedContentType, siteCollection));
                    var siteColumnInfos = publishedContentType.ExtractSiteColumnInfos();
                    var siteColumnIds = new HashSet<Guid>(siteColumnInfos.Select(info1 => info1.Id));
                    foreach (var publishedSiteColumn in publishedSiteColumnList.GetSiteColumns(siteColumnIds))
                    {
                        actionCollection.Add(new PushSiteColumnAction(info.ActionContext, siteCollection, publishedSiteColumn));
                    }
                }

                return actionCollection;
            }
        }
    }
}
