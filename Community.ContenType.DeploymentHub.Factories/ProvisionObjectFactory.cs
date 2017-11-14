using Community.ContenType.DeploymentHub.Common.ClientContextProviders;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.DomainServices.Provision;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using Community.ContenType.DeploymentHub.SharePoint.Repositories;

namespace Community.ContenType.DeploymentHub.Factories
{
    public class ProvisionObjectFactory : HubOnlyObjectFactory
    {
        public static ProvisionObjectFactory FromRealUserContext() =>
            new ProvisionObjectFactory(new UserClientContextForSPHostProvider());

        private ProvisionObjectFactory(IClientContextProvider hubClientContextProvider) 
            : base(hubClientContextProvider) { }

        public IUserRepository CreateUserRepository() =>
            new UserRepository(HubClientContextProvider);

        public IMainProvisionerBase CreateMainProvisioner(IProvisionEventHub eventHub)
        {
            var contentTypeRepository = new ContentTypeRepository(HubClientContextProvider);
            var statusListRepository = new StatusListRepository(HubClientContextProvider);
            var configurationListRepository = new ConfigurationListRepository(HubClientContextProvider);
            var siteCollectionGroupingListRepository = new SiteCollectionGroupingListRepository(HubClientContextProvider);
            var publishedSiteColumnsListRepository = new PublishedSiteColumnsListRepository(HubClientContextProvider);
            var publishedContentTypesListRepository = new PublishedContentTypesListRepository(HubClientContextProvider);
            var contentTypeGroupingListRepository = new ContentTypeGroupingListRepository(HubClientContextProvider, contentTypeRepository);
            var termStoreRepository = new TermStoreRepository(HubClientContextProvider);
            var webPropertyRepository = new WebPropertyRepository(HubClientContextProvider);
            var userRepository = new UserRepository(HubClientContextProvider);
            var docTemplateRepository = new PublishedDocTemplateListRepository(HubClientContextProvider);

            return new MainProvisioner(
                webPropertyRepository,
                statusListRepository, 
                termStoreRepository,
                configurationListRepository, 
                contentTypeGroupingListRepository,
                siteCollectionGroupingListRepository,
                publishedSiteColumnsListRepository,
                publishedContentTypesListRepository,
                userRepository,
                eventHub,
                docTemplateRepository);
        }
    }
}