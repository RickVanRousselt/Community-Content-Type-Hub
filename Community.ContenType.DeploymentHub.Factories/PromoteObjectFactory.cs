using System;
using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Actions.Promote;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.Domain.Verifiers;
using Community.ContenType.DeploymentHub.DomainServices;
using Community.ContenType.DeploymentHub.DomainServices.Calculators;
using Community.ContenType.DeploymentHub.DomainServices.Processors;
using Community.ContenType.DeploymentHub.DomainServices.RequestInitiators;
using Community.ContenType.DeploymentHub.DomainServices.Verifiers.Promote;
using Community.ContenType.DeploymentHub.Azure.Repositories;
using Community.ContenType.DeploymentHub.Common.ClientContextProviders;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using Community.ContenType.DeploymentHub.SharePoint.Processors;
using Community.ContenType.DeploymentHub.SharePoint.Repositories;
using Community.ContenType.DeploymentHub.SharePoint.Updaters.SiteColumn;
using Community.ContenType.DeploymentHub.Jobs;

namespace Community.ContenType.DeploymentHub.Factories
{
    public class PromoteObjectFactory : ObjectFactory<PromoteSiteColumnAction, PromoteContentTypeAction>, IPromoteVerificationRuleCollectionFactory, IPromoteProcessorFactory
    {
        public static PromoteObjectFactory FromRealUserContext() =>
            new PromoteObjectFactory(
                new UserClientContextForSPHostProvider(),
                siteCollection => new UserPasswordOnlineClientContextProvider(siteCollection.Url, SettingsProvider.User, SettingsProvider.Password));

        public static PromoteObjectFactory FromFunctionalUserContext(Hub hub) =>
            new PromoteObjectFactory(
                new UserPasswordOnlineClientContextProvider(hub.Url, SettingsProvider.User, SettingsProvider.Password),
                siteCollection => new UserPasswordOnlineClientContextProvider(siteCollection.Url, SettingsProvider.PromoteUser, SettingsProvider.PromotePassword));

        private PromoteObjectFactory(
            IClientContextProvider hubClientContextProvider, 
            Func<SiteCollection, IClientContextProvider> targetClientContextProviderGenerator) 
            : base(hubClientContextProvider, targetClientContextProviderGenerator) { }

        private IVerificationStrategy<PromoteSiteColumnAction, PromoteContentTypeAction> CreateVerificationStrategy(bool enableVerifiers)
        {
            return enableVerifiers
                ? new MultiThreadedVerificationStrategy<PromoteSiteColumnAction, PromoteContentTypeAction>(new SingleThreadedVerificationStrategy<PromoteVerificationRuleCollection, IPromoteVerificationRule, PromoteSiteColumnAction, PromoteContentTypeAction>(this), SettingsProvider.NumberOfThreads)
                : new NaiveVerificationStrategy<PromoteSiteColumnAction, PromoteContentTypeAction>() as IVerificationStrategy<PromoteSiteColumnAction, PromoteContentTypeAction>;
        }

        public PromoteVerificationRuleCollection CreateVerificationRules()
        {
            var hub = CreateEventHub();
            var hubAllowedRule = new HubAllowedRuleCached(CreateWebPropertyRepository);
            var checkAccess = new IsTargetSiteCollectionAccessibleRuleCached(CreateUserRepository);
            var canTermPathBeMappedRule = new CanTermPathBeMappedRuleCached(CreateTermStoreRepository);

            return new PromoteVerificationRuleCollection(
                hub,
                new List<IPromoteVerificationRule> { hubAllowedRule, checkAccess, canTermPathBeMappedRule },
                SettingsProvider.PromoteActiveRuleNames);
        }

        private IPromoteActionCalculator CreateActionCalculator()
        {
            var publishedSiteColumnsListRepository = new PublishedSiteColumnsListRepository(HubClientContextProvider);
            var publishedContentTypesListRepository = new PublishedContentTypesListRepository(HubClientContextProvider);
            var configurationListRepository = new ConfigurationListRepository(HubClientContextProvider);
            return new PromoteActionCalculator(publishedSiteColumnsListRepository, publishedContentTypesListRepository, configurationListRepository);
        }

        public override ISiteColumnProcessor<PromoteSiteColumnAction> CreateSiteColumnProcessor(SiteCollection siteCollection)
        {
            var eventHub = CreateEventHub();
            var targetHub = new Hub(siteCollection.Url); //target for promote is a hub
            var targetClientContextProvider = TargetClientContextProviderGenerator(targetHub);
            var termStoreRepository = new TermStoreRepository(targetClientContextProvider);
            var manualSiteColumnUpdater = new ManualSiteColumnUpdater(targetClientContextProvider, termStoreRepository);
            var updater = SettingsProvider.UseXmlUpdater
                ? new XmlSiteColumnUpdater(targetClientContextProvider, manualSiteColumnUpdater)
                : (ISiteColumnUpdater) manualSiteColumnUpdater;
            return new SiteColumnPromotor(targetClientContextProvider, eventHub, targetHub, updater);
        }

        public override IContentTypeProcessor<PromoteContentTypeAction> CreateContentTypeProcessor(SiteCollection siteCollection)
        {
            var eventHub = CreateEventHub();
            var targetHub = new Hub(siteCollection.Url); //target for promote is a hub
            var targetClientContextProvider = TargetClientContextProviderGenerator(targetHub);
            var publishedDocTemplateListRepository = new PublishedDocTemplateListRepository(HubClientContextProvider);
            return new ContentTypePromotor(targetClientContextProvider, eventHub, targetHub, publishedDocTemplateListRepository);
        }

        public IPublishPushPromoteRequestInitiator CreateRequestInitiator(IPromoteEventHub eventHub)
        {
            var queueRepostory = SettingsProvider.UseDispatchQueue
                ? (IQueueRepository) new DispatchedQueueRepository(SettingsProvider.AzureStorageConnectionString)
                : new QueueRepository(SettingsProvider.AzureStorageConnectionString);

            var userRepository = new UserRepository(HubClientContextProvider);
            return new PromoteRequestInitiator(userRepository, queueRepostory, eventHub);
        }

        private IPromoteRequestRetriever CreateRetriever() => 
            new StatusListRepository(HubClientContextProvider);

        public IConfigurationListRepository CreateConfigurationListRepository() => 
            new ConfigurationListRepository(HubClientContextProvider);

        public PromoteJob CreatePromoteJob(bool enableVerifiers)
        {
            var eventHub = CreateEventHub();
            var retriever = CreateRetriever();
            var actionCalculator = CreateActionCalculator();
            var verificationStrategy = CreateVerificationStrategy(enableVerifiers);
            var actionCollectionProcessor = CreateActionProcessingStrategy();
            var sourcePropertyConfigurator = CreateSourcePropertyConfigurator();
            return new PromoteJob(eventHub, retriever, actionCalculator, verificationStrategy, sourcePropertyConfigurator, actionCollectionProcessor);
        }
    }
}