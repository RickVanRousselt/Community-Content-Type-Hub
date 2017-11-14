using System;
using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Actions.Publish;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.DomainServices.Processors;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;

namespace Community.ContenType.DeploymentHub.SharePoint.Processors
{
    public class ContentTypePublisher : IContentTypeProcessor<PublishContentTypeAction>
    {
        private readonly IPublishedContentTypesListRepository _publishedContentTypesListRepository;
        private readonly IPublishEventHub _eventHub;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ContentTypePublisher));
        private readonly IPublishedDocTemplateListRepository _docTemplateListRepository;

        public ContentTypePublisher(IPublishedContentTypesListRepository publishedContentTypesListRepository, IPublishEventHub eventHub, IPublishedDocTemplateListRepository docTemplateListRepository)
        {
            _publishedContentTypesListRepository = publishedContentTypesListRepository;
            _eventHub = eventHub;
            _docTemplateListRepository = docTemplateListRepository;
        }

        private IActionStatus Execute(PublishContentTypeAction action, int index, int validActionsCount)
        {
            using (Logger.MethodTraceLogger(action, index, validActionsCount))
            {
                try
                {
                    var publishedContentType = _publishedContentTypesListRepository.PublishContentType(action.ContentType);
                    if (!string.IsNullOrWhiteSpace(action.ContentType.DocumentTemplate) && (action.ContentType.DocumentTemplate != action.ContentType.DocumentTemplateUrl))
                    {
                       _docTemplateListRepository.PublishDocumentTemplate(action.ContentType);
                    }
                    _eventHub.Publish(new PublishContentTypeActionExecutedEvent(action, index, validActionsCount, publishedContentType));
                    return new SucceededStatus();
                }
                catch (Exception ex)
                {
                    _eventHub.Publish(new PublishContentTypeActionFailedEvent(action, index, validActionsCount, ex));
                    return new FailedStatus(ex);
                }
            }
        }

        public IList<IActionStatus> ExecuteAll(IReadOnlyList<PublishContentTypeAction> actions) => 
            actions.Select((t, i) => Execute(t, i + 1, actions.Count)).ToList();
    }
}