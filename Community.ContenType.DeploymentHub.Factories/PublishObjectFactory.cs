using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Actions.Publish;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.Domain.Processors;
using Community.ContenType.DeploymentHub.Domain.Verifiers;
using Community.ContenType.DeploymentHub.DomainServices;
using Community.ContenType.DeploymentHub.DomainServices.Calculators;
using Community.ContenType.DeploymentHub.DomainServices.Processors;
using Community.ContenType.DeploymentHub.DomainServices.RequestInitiators;
using Community.ContenType.DeploymentHub.DomainServices.Verifiers.Publish;
using Community.ContenType.DeploymentHub.Azure.Repositories;
using Community.ContenType.DeploymentHub.Common.ClientContextProviders;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using Community.ContenType.DeploymentHub.SharePoint.Processors;
using Community.ContenType.DeploymentHub.SharePoint.Repositories;
using Community.ContenType.DeploymentHub.Jobs;

namespace Community.ContenType.DeploymentHub.Factories
{
    public class PublishObjectFactory : HubOnlyObjectFactory, IPublishProcessorFactory, IPublishVerificationRuleCollectionFactory
    {
        public static PublishObjectFactory FromRealUserContext() =>
            new PublishObjectFactory(new UserClientContextForSPHostProvider());

        public static PublishObjectFactory FromFunctionalUserContext(Hub hub) =>
            new PublishObjectFactory(new UserPasswordOnlineClientContextProvider(hub.Url, SettingsProvider.User, SettingsProvider.Password));

        private PublishObjectFactory(IClientContextProvider hubClientContextProvider) 
            : base(hubClientContextProvider) { }

        private IVerificationStrategy<PublishSiteColumnAction, PublishContentTypeAction> CreateVerificationStrategy(bool enableVerifiers)
        {
            return enableVerifiers
                ? new MultiThreadedVerificationStrategy<PublishSiteColumnAction, PublishContentTypeAction>(new SingleThreadedVerificationStrategy<PublishVerificationRuleCollection, IPublishVerificationRule, PublishSiteColumnAction, PublishContentTypeAction>(this), SettingsProvider.NumberOfThreads)
                : new NaiveVerificationStrategy<PublishSiteColumnAction, PublishContentTypeAction>() as IVerificationStrategy<PublishSiteColumnAction, PublishContentTypeAction>;
        }

        public PublishVerificationRuleCollection CreateVerificationRules()
        {
            var hub = CreateEventHub();
            var publishedSiteColumnsListRepository = new PublishedSiteColumnsListRepository(HubClientContextProvider);
            var checkUpdate = new CheckSiteColumnsNeedUpdateRule(publishedSiteColumnsListRepository);
            var checkType = new CheckSiteColumnTypeRule(publishedSiteColumnsListRepository);

            return new PublishVerificationRuleCollection(
                hub,
                new List<IPublishVerificationRule> {checkUpdate, checkType },
                SettingsProvider.PublishActiveRuleNames);
        }

        private IPublishActionCalculator CreateActionCalculator()
        {
            var termStoreRepository = new TermStoreRepository(HubClientContextProvider);
            var contentTypeRepository = new ContentTypeRepository(HubClientContextProvider);
            var siteColumnRepository = new SiteColumnRepository(HubClientContextProvider, termStoreRepository);

            return new PublishActionCalculator(contentTypeRepository, siteColumnRepository);
        }

        public ISiteColumnProcessor<PublishSiteColumnAction> CreateSiteColumnProcessor(SiteCollection siteCollection)
        {
            var eventHub = CreateEventHub();
            var publishedSiteColumnsListRepository = new PublishedSiteColumnsListRepository(HubClientContextProvider);
            return new SiteColumnPublisher(publishedSiteColumnsListRepository, eventHub);
        }

        public IContentTypeProcessor<PublishContentTypeAction> CreateContentTypeProcessor(SiteCollection siteCollection)
        {
            var eventHub = CreateEventHub();
            var publishedContentTypesListRepository = new PublishedContentTypesListRepository(HubClientContextProvider);
            var docTemplateRepository = new PublishedDocTemplateListRepository(HubClientContextProvider);
            return new ContentTypePublisher(publishedContentTypesListRepository, eventHub, docTemplateRepository);
        }

        public IPublishPushPromoteRequestInitiator CreateRequestInitiator(IPublishEventHub eventHub)
        {
            var queueRepostory = SettingsProvider.UseDispatchQueue
                ? (IQueueRepository) new DispatchedQueueRepository(SettingsProvider.AzureStorageConnectionString)
                : new QueueRepository(SettingsProvider.AzureStorageConnectionString);

            var userRepository = new UserRepository(HubClientContextProvider);

            return new PublishRequestInitiator(userRepository, queueRepostory, eventHub);
        }

        private IPublishRequestRetriever CreateRetriever() => new StatusListRepository(HubClientContextProvider);

        private IActionProcessingStrategy<PublishSiteColumnAction, PublishContentTypeAction> CreateActionProcessingStrategy() =>
            new MultitThreadedActionProcessingStrategy<PublishSiteColumnAction, PublishContentTypeAction>(this, SettingsProvider.NumberOfThreads);

        public PublishJob CreatePublishJob(bool enableVerifiers)
        {
            var eventHub = CreateEventHub();
            var retriever = CreateRetriever();
            var actionCalculator = CreateActionCalculator();
            var verificationStrategy = CreateVerificationStrategy(enableVerifiers);
            var actionCollectionProcessor = CreateActionProcessingStrategy();
            return new PublishJob(eventHub, retriever, actionCalculator, verificationStrategy, actionCollectionProcessor);
        }
    }
}