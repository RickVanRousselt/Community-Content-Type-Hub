using System;
using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.Domain.Verifiers;
using Community.ContenType.DeploymentHub.DomainServices;
using Community.ContenType.DeploymentHub.DomainServices.Calculators;
using Community.ContenType.DeploymentHub.DomainServices.Loggers;
using Community.ContenType.DeploymentHub.DomainServices.Processors;
using Community.ContenType.DeploymentHub.DomainServices.RequestInitiators;
using Community.ContenType.DeploymentHub.DomainServices.Verifiers.Push;
using Community.ContenType.DeploymentHub.Azure.Repositories;
using Community.ContenType.DeploymentHub.Common.ClientContextProviders;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.SharePoint.Repositories;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using Community.ContenType.DeploymentHub.SharePoint.Processors;
using Community.ContenType.DeploymentHub.SharePoint.Updaters.SiteColumn;
using Community.ContenType.DeploymentHub.Jobs;

namespace Community.ContenType.DeploymentHub.Factories
{
    public class PullObjectFactory : ObjectFactory<PushSiteColumnAction, PushContentTypeAction>, IPushVerificationRuleCollectionFactory, IPushProcessorFactory
    {
        public static PullObjectFactory FromFunctionalUserContext(Hub hub) =>
            new PullObjectFactory(
                new UserPasswordOnlineClientContextProvider(hub.Url, SettingsProvider.User, SettingsProvider.Password),
                siteCollection => new UserPasswordOnlineClientContextProvider(siteCollection.Url, SettingsProvider.User, SettingsProvider.Password));

        private PullObjectFactory(
            IClientContextProvider hubClientContextProvider,
            Func<SiteCollection, IClientContextProvider> targetClientContextProviderGenerator)
            : base(hubClientContextProvider, targetClientContextProviderGenerator) { }

        private IVerificationStrategy<PushSiteColumnAction, PushContentTypeAction> CreateVerificationStrategy(bool enableVerifiers)
        {
            return enableVerifiers
                ? new MultiThreadedVerificationStrategy<PushSiteColumnAction, PushContentTypeAction>(new SingleThreadedVerificationStrategy<PushVerificationRuleCollection, IPushVerificationRule, PushSiteColumnAction, PushContentTypeAction>(this), SettingsProvider.NumberOfThreads)
                : new NaiveVerificationStrategy<PushSiteColumnAction, PushContentTypeAction>() as IVerificationStrategy<PushSiteColumnAction, PushContentTypeAction>;
        }

        public PushVerificationRuleCollection CreateVerificationRules()
        {
            var hub = CreateEventHub();
            var masterDbRepository = new DbEntryRepository(SettingsProvider.AzureStorageConnectionString);
            var checkVersion = new VersionRule(masterDbRepository);
            var checkAccess = new IsTargetSiteCollectionAccessibleRule(CreateUserRepository);
            var checkName = new NameExistsRule(CreateSiteColumnRepository, CreateContentTypeRepository);
            var checkReadOnly = new IsSetToReadOnlyRule(CreateContentTypeRepository);
            var hubAllowedRule = new HubAllowedRule(CreateWebPropertyRepository);

            return new PushVerificationRuleCollection(
                hub,
                new List<IPushVerificationRule> { checkVersion, checkAccess, checkName, checkReadOnly, hubAllowedRule },
                SettingsProvider.PushActiveRuleNames);
        }

        private IPullActionCalculator CreateActionCalculator()
        {
            var contentTypeRepository = new ContentTypeRepository(HubClientContextProvider);
            var publishedSiteColumnsListRepository = new PublishedSiteColumnsListRepository(HubClientContextProvider);
            var publishedContentTypesListRepository = new PublishedContentTypesListRepository(HubClientContextProvider);
            var contentTypeGroupingListRepository = new ContentTypeGroupingListRepository(HubClientContextProvider, contentTypeRepository);
            var siteCollectionGroupingListRepository = new SiteCollectionGroupingListRepository(HubClientContextProvider);
            var groupingRepository = new GroupingRepository(siteCollectionGroupingListRepository, contentTypeGroupingListRepository);
            return new PullActionCalculator(groupingRepository, publishedSiteColumnsListRepository, publishedContentTypesListRepository);
        }

        public override ISiteColumnProcessor<PushSiteColumnAction> CreateSiteColumnProcessor(SiteCollection siteCollection)
        {
            var eventHub = CreateEventHub();
            var targetClientContextProvider = TargetClientContextProviderGenerator(siteCollection);
            var termStoreRepository = new TermStoreRepository(targetClientContextProvider);
            var manualSiteColumnUpdater = new ManualSiteColumnUpdater(targetClientContextProvider, termStoreRepository);
            var updater = SettingsProvider.UseXmlUpdater
                ? new XmlSiteColumnUpdater(targetClientContextProvider, manualSiteColumnUpdater)
                : (ISiteColumnUpdater) manualSiteColumnUpdater;
            return new SiteColumnPusher(targetClientContextProvider, eventHub, siteCollection, updater);
        }

        public override IContentTypeProcessor<PushContentTypeAction> CreateContentTypeProcessor(SiteCollection siteCollection)
        {
            var eventHub = CreateEventHub();
            var targetClientContextProvider = TargetClientContextProviderGenerator(siteCollection);
            var publishedDocTemplateListRepository = new PublishedDocTemplateListRepository(HubClientContextProvider);
            return new ContentTypePusher(targetClientContextProvider, publishedDocTemplateListRepository, eventHub, siteCollection);
        }

        public IPullRequestInitiator CreateRequestInitiator(IPullEventHub eventHub)
        {
            var queueRepostory = SettingsProvider.UseDispatchQueue
                ? (IQueueRepository) new DispatchedQueueRepository(SettingsProvider.AzureStorageConnectionString)
                : new QueueRepository(SettingsProvider.AzureStorageConnectionString);

            return new PullRequestInitiator(queueRepostory, eventHub);
        }

        private IPullRequestRetriever CreateRetriever() => 
            new PullRequestRetriever(
                new StatusListRepository(HubClientContextProvider), 
                new TermStoreRepository(HubClientContextProvider));

        private ISiteCollectionGroupingListRepository CreateSiteCollectionGroupingListRepository() => 
            new SiteCollectionGroupingListRepository(HubClientContextProvider);

        public override IEventHub CreateEventHub()
        {
            //without email logger!
            var statusListRepository = new StatusListRepository(HubClientContextProvider);
            var masterDbRepository = new DbEntryRepository(SettingsProvider.AzureStorageConnectionString);
            var contentTypeSyncLogListRepository = new ContentTypeSyncLogListRepository(TargetClientContextProviderGenerator);

            var eventHub = new EventHub();
            //Note: subscribe orders matters
            //StatusListLogger updates event.ActionContext.StatusListItemId
            eventHub.Subscribe(new StatusListLogger(statusListRepository));
            eventHub.Subscribe(new MasterDbLogger(masterDbRepository));
            eventHub.Subscribe(new ContentTypeSyncListLogger(contentTypeSyncLogListRepository));
            eventHub.Subscribe(new Log4NetLogger());
            return eventHub;
        }

        public PullJob CreatePullJob(bool enableVerifiers)
        {
            var eventHub = CreateEventHub();
            var retriever = CreateRetriever();
            var actionCalculator = CreateActionCalculator();
            var verificationStrategy = CreateVerificationStrategy(enableVerifiers);
            var siteCollectionGroupingListRepository = CreateSiteCollectionGroupingListRepository();
            var groupingRepository = CreateGroupingRepository();
            var actionCollectionProcessor = CreateActionProcessingStrategy();
            var sourcePropertyConfigurator = CreateSourcePropertyConfigurator();
            return new PullJob(eventHub, retriever, actionCalculator, verificationStrategy, siteCollectionGroupingListRepository, groupingRepository, sourcePropertyConfigurator, actionCollectionProcessor);
        }
    }
}