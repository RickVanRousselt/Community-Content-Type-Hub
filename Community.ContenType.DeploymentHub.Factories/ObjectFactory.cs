using System;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.Domain.Processors;
using Community.ContenType.DeploymentHub.DomainServices;
using Community.ContenType.DeploymentHub.DomainServices.Loggers;
using Community.ContenType.DeploymentHub.DomainServices.Processors;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using Community.ContenType.DeploymentHub.SharePoint.Repositories;

namespace Community.ContenType.DeploymentHub.Factories
{
    public abstract class ObjectFactory<TSiteColumnAction, TContentTypeAction> : HubOnlyObjectFactory, IProcessorFactory<TSiteColumnAction, TContentTypeAction>
        where TSiteColumnAction : ActionBase
        where TContentTypeAction : ActionBase
    {
        protected readonly Func<SiteCollection, IClientContextProvider> TargetClientContextProviderGenerator;

        protected ObjectFactory(
            IClientContextProvider hubClientContextProvider, 
            Func<SiteCollection, IClientContextProvider> targetClientContextProviderGenerator) 
            : base(hubClientContextProvider)
        {
            TargetClientContextProviderGenerator = targetClientContextProviderGenerator;
        }

        public override IEventHub CreateEventHub()
        {
            var eventHub = base.CreateEventHub();
            var contentTypeSyncLogListRepository = new ContentTypeSyncLogListRepository(TargetClientContextProviderGenerator);
            eventHub.Subscribe(new ContentTypeSyncListLogger(contentTypeSyncLogListRepository));
            return eventHub;
        }

        protected SiteColumnRepository CreateSiteColumnRepository(SiteCollection collection)
        {
            var provider = TargetClientContextProviderGenerator(collection);
            var termStoreRepository = new TermStoreRepository(provider);
            return new SiteColumnRepository(provider, termStoreRepository);
        }

        protected ContentTypeRepository CreateContentTypeRepository(SiteCollection collection) =>
            new ContentTypeRepository(TargetClientContextProviderGenerator(collection));

        protected TermStoreRepository CreateTermStoreRepository(SiteCollection collection) =>
            new TermStoreRepository(TargetClientContextProviderGenerator(collection));

        protected IUserRepository CreateUserRepository(SiteCollection collection) => 
            new UserRepository(TargetClientContextProviderGenerator(collection));

        protected IWebPropertyRepository CreateWebPropertyRepository(SiteCollection siteCollection) =>
            new WebPropertyRepository(TargetClientContextProviderGenerator(siteCollection));

        protected ISourcePropertyConfigurator CreateSourcePropertyConfigurator() =>
            new MultiThreadedSourcePropertyConfigurator(new SourcePropertyConfigurator(CreateWebPropertyRepository), SettingsProvider.NumberOfThreads);

        protected IActionProcessingStrategy<TSiteColumnAction, TContentTypeAction> CreateActionProcessingStrategy() =>
            new MultitThreadedActionProcessingStrategy<TSiteColumnAction, TContentTypeAction>(this, SettingsProvider.NumberOfThreads);

        public abstract ISiteColumnProcessor<TSiteColumnAction> CreateSiteColumnProcessor(SiteCollection siteCollection);
        public abstract IContentTypeProcessor<TContentTypeAction> CreateContentTypeProcessor(SiteCollection siteCollection);
    }
}