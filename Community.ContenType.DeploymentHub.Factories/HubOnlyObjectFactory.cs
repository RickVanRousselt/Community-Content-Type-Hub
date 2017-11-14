using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.DomainServices.Loggers;
using Community.ContenType.DeploymentHub.Factories.Settings;
using Community.ContenType.DeploymentHub.Azure.Repositories;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using Community.ContenType.DeploymentHub.SharePoint.Repositories;

namespace Community.ContenType.DeploymentHub.Factories
{
    public abstract class HubOnlyObjectFactory
    {
        protected static readonly ISettingsProvider SettingsProvider = new SettingsProvider();
        protected readonly IClientContextProvider HubClientContextProvider;

        protected HubOnlyObjectFactory(IClientContextProvider hubClientContextProvider)
        {
            HubClientContextProvider = hubClientContextProvider;
        }

        public virtual IEventHub CreateEventHub()
        {
            var statusListRepository = new StatusListRepository(HubClientContextProvider);
            var masterDbRepository = new DbEntryRepository(SettingsProvider.AzureStorageConnectionString);
            var mailRepository = new MailRepository(
                SettingsProvider.MailUser, SettingsProvider.MailPassword,
                SettingsProvider.SmtpServer, SettingsProvider.SmtpPort);

            var eventHub = new EventHub();
            //Note: subscribe orders matters
            //StatusListLogger updates event.ActionContext.StatusListItemId
            eventHub.Subscribe(new StatusListLogger(statusListRepository));
            eventHub.Subscribe(new MasterDbLogger(masterDbRepository));
            eventHub.Subscribe(new MailLogger(mailRepository));
            eventHub.Subscribe(new Log4NetLogger());
            return eventHub;
        }

        public IGroupingRepository CreateGroupingRepository()
        {
            var siteCollectionGroupingListRepository = new SiteCollectionGroupingListRepository(HubClientContextProvider);
            var contentTypeRepository = new ContentTypeRepository(HubClientContextProvider);
            var contentTypeGroupingListRepository = new ContentTypeGroupingListRepository(HubClientContextProvider, contentTypeRepository);
            return new GroupingRepository(siteCollectionGroupingListRepository, contentTypeGroupingListRepository);
        }

        public IPublishedContentTypesListRepository CreatePublishedContentTypesListRepository() => 
            new PublishedContentTypesListRepository(HubClientContextProvider);

        public IContentTypeRepository CreateContentTypeRepository() => new ContentTypeRepository(HubClientContextProvider);
    }
}