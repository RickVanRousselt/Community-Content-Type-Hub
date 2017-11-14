using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Actions.Promote;
using Community.ContenType.DeploymentHub.Domain.Actions.Publish;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.DomainServices.Processors
{
    public interface IProcessorFactory<in TSiteColumnAction, in TContentTypeAction> 
        where TSiteColumnAction : ActionBase
        where TContentTypeAction : ActionBase
    {
        ISiteColumnProcessor<TSiteColumnAction> CreateSiteColumnProcessor(SiteCollection siteCollection);
        IContentTypeProcessor<TContentTypeAction> CreateContentTypeProcessor(SiteCollection siteCollection);
    }

    public interface IPublishProcessorFactory : IProcessorFactory<PublishSiteColumnAction, PublishContentTypeAction> { }
    public interface IPushProcessorFactory : IProcessorFactory<PushSiteColumnAction, PushContentTypeAction> { }
    public interface IPromoteProcessorFactory : IProcessorFactory<PromoteSiteColumnAction, PromoteContentTypeAction> { }
}