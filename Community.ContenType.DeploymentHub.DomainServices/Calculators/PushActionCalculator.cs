using System;
using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using Community.ContenType.DeploymentHub.Domain.Requests;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;

namespace Community.ContenType.DeploymentHub.DomainServices.Calculators
{
    public class PushActionCalculator : IPushActionCalculator
    {
        private readonly IGroupingRepository _groupingRepository;
        private readonly IPublishedSiteColumnsListRepository _publishedSiteColumnsListRepository;
        private readonly IPublishedContentTypesListRepository _publishedContentTypesListRepository;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PushActionCalculator));

        public PushActionCalculator(
            IGroupingRepository groupingRepository,
            IPublishedSiteColumnsListRepository publishedSiteColumnsListRepository,
            IPublishedContentTypesListRepository publishedContentTypesListRepository)
        {
            _groupingRepository = groupingRepository;
            _publishedSiteColumnsListRepository = publishedSiteColumnsListRepository;
            _publishedContentTypesListRepository = publishedContentTypesListRepository;
        }

        public PushActionCollection CalculateActions(PushRequest pushRequest)
        {
            using (Logger.MethodTraceLogger(pushRequest))
            {
                var groupings = _groupingRepository.GetGroupings();

                var publishedSiteColumnList = _publishedSiteColumnsListRepository.GetList();
                var publishedContentTypeList = _publishedContentTypesListRepository.GetList();

                var actionCollection = new PushActionCollection(pushRequest.ActionContext);
                foreach (var publishedContentType in publishedContentTypeList.GetContentTypes(pushRequest.ContentTypeIds))
                {
                    foreach (var siteCollection in groupings.GetSiteCollectionsFor(publishedContentType))
                    {
                        actionCollection.Add(new PushContentTypeAction(pushRequest.ActionContext, publishedContentType, siteCollection));
                        var siteColumnInfos = publishedContentType.ExtractSiteColumnInfos();
                        var siteColumnIds = new HashSet<Guid>(siteColumnInfos.Select(info => info.Id));
                        foreach (var publishedSiteColumn in publishedSiteColumnList.GetSiteColumns(siteColumnIds))
                        {
                            actionCollection.Add(new PushSiteColumnAction(pushRequest.ActionContext, siteCollection, publishedSiteColumn));
                        }
                    }
                }

                return actionCollection;
            }
        }
    }
}