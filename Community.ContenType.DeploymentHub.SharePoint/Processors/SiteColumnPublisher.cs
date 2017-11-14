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
    public class SiteColumnPublisher : ISiteColumnProcessor<PublishSiteColumnAction>
    {
        private readonly IPublishEventHub _eventHub;
        private readonly IPublishedSiteColumnsListRepository _publishedSiteColumnsListRepository;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SiteColumnPublisher));

        public SiteColumnPublisher(IPublishedSiteColumnsListRepository publishedSiteColumnsListRepository, IPublishEventHub eventHub)
        {
            _eventHub = eventHub;
            _publishedSiteColumnsListRepository = publishedSiteColumnsListRepository;
        }

        public IActionStatus Execute(PublishSiteColumnAction action, int index, int validActionsCount)
        {
            using (Logger.MethodTraceLogger(action, index, validActionsCount))
            {
                try
                {
                    var publishSiteColumn = _publishedSiteColumnsListRepository.PublishSiteColumn(action.SiteColumn);
                    _eventHub.Publish(new PublishSiteColumnActionExecutedEvent(action, index, validActionsCount, publishSiteColumn));
                    return new SucceededStatus();
                }
                catch (Exception ex)
                {
                    _eventHub.Publish(new PublishSiteColumnActionFailedEvent(action, index, validActionsCount, ex));
                    return new FailedStatus(ex);
                }
            }
        }

        public IList<IActionStatus> ExecuteAll(IReadOnlyList<PublishSiteColumnAction> actions) => 
            actions.Select((t, i) => Execute(t, i + 1, actions.Count)).ToList();
    }
}