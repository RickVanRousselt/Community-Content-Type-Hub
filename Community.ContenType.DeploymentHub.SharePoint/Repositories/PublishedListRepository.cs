using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Microsoft.SharePoint.Client;

namespace Community.ContenType.DeploymentHub.SharePoint.Repositories
{
    public abstract class PublishedListRepository<T> : ListRepository, IPublishedListRepository<T>
    {
        private readonly string _publishedListName;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PublishedListRepository<T>));

        protected PublishedListRepository(IClientContextProvider clientContextProvider, string publishedListName) 
            : base(clientContextProvider)
        {
            _publishedListName = publishedListName;
        }

        public IEnumerable<T> GetAllPublishedItemsWithSchema()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var web = clientContext.Site.RootWeb;
                var list = web.Lists.GetByTitle(_publishedListName);
                var allItems = GetItemsSafe(list, GetRetrievals());

                foreach (var listItem in allItems)
                {
                    yield return MapListItemToPublishedItemWithSchema(listItem, clientContext);
                }
            }
        }

        public IEnumerable<T> GetAllPublishedItemsNoSchema()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var web = clientContext.Site.RootWeb;
                var list = web.Lists.GetByTitle(_publishedListName);
                var allItems = GetItemsSafe(list, GetRetrievals());

                foreach (var listItem in allItems)
                {
                    yield return MapListItemToPublishedItemNoSchema(listItem);
                }
            }
        }

        protected abstract Expression<Func<ListItemCollection, object>> GetRetrievals();
        protected abstract T MapListItemToPublishedItemNoSchema(ListItem listItem);
        protected abstract T MapListItemToPublishedItemWithSchema(ListItem listItem, ClientContext clientContext);
    }
}