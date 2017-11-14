using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Actions.Publish;
using Community.ContenType.DeploymentHub.Domain.Requests;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;

namespace Community.ContenType.DeploymentHub.DomainServices.Calculators
{
    public class PublishActionCalculator : IPublishActionCalculator
    {
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly ISiteColumnRepository _siteColumnRepository;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PublishActionCalculator));

        public PublishActionCalculator(IContentTypeRepository contentTypeRepository, ISiteColumnRepository siteColumnRepository)
        {
            _contentTypeRepository = contentTypeRepository;
            _siteColumnRepository = siteColumnRepository;
        }

        public PublishActionCollection CalculateActions(PublishRequest request)
        {
            using (Logger.MethodTraceLogger(request))
            {
                var contentTypesToProcess = _contentTypeRepository.GetContentTypes(request.ContentTypeIds);
                var publishActionCollection = new PublishActionCollection(request.ActionContext);

                //loop all content types => Actioncollection is filled with list of site columns and list of content types to process
                foreach (var contentType in contentTypesToProcess)
                {
                    var columnsFromContentType = _siteColumnRepository.GetColumnsFromContentType(contentType);
                    foreach (var siteColumn in columnsFromContentType)
                    {
                        var action = new PublishSiteColumnAction(request.ActionContext, siteColumn);
                        publishActionCollection.Add(action);
                    }
                    var contentTypeAction = new PublishContentTypeAction(request.ActionContext, contentType);
                    publishActionCollection.Add(contentTypeAction);
                }
                return publishActionCollection;
            }
        }
    }
}