using System;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Microsoft.SharePoint.Client;

namespace Community.ContenType.DeploymentHub.SharePoint.Repositories
{
    public class ContentTypeSyncLogListRepository : IContentTypeSyncLogListRepository
    {
        private readonly Func<SiteCollection, IClientContextProvider> _clientContextProviderGenerator;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ContentTypeSyncLogListRepository));

        public ContentTypeSyncLogListRepository(Func<SiteCollection, IClientContextProvider> clientContextProviderGenerator)
        {
            _clientContextProviderGenerator = clientContextProviderGenerator;
        }

        public void AddSyncLogFailEntry(string title, string message, string failureStage, string failureMessage, DateTime? eventDate, SiteCollection siteCollection) =>
            AddEntry(title, message, failureStage, failureMessage, eventDate, siteCollection);
        public void AddAddSyncLogEntry(string title, string message, SiteCollection siteCollection) =>
           AddEntry(title, message, siteCollection);

        private void AddEntry(string title, string message, string failureStage, string failureMessage, DateTime? eventDate, SiteCollection siteCollection)
        {
            using (Logger.MethodTraceLogger(title, message, failureStage, failureMessage, eventDate, siteCollection))
            using (var clientContext = _clientContextProviderGenerator(siteCollection).CreateClientContext())
            {
                var web = clientContext.Site.RootWeb;
                clientContext.Load(web, x => x.ServerRelativeUrl);
                clientContext.ExecuteQueryWithIncrementalRetry();
                var statusList = clientContext.Site.RootWeb.GetList(web.ServerRelativeUrl.TrimStart('/') + "/lists/" + Lists.ContentTypeSyncLog);
                clientContext.ExecuteQueryWithIncrementalRetry();
                if (statusList != null)
                {
                    var itemCreateInfo = new ListItemCreationInformation();
                    var listItem = statusList.AddItem(itemCreateInfo);
                    listItem["Title"] = title;
                    listItem["PublishedObjectName"] = message;
                    listItem["Failure_x0020_Stage"] = failureStage;
                    listItem["Failure_x0020_Message"] = failureMessage;
                    if (eventDate != null) listItem["Failure_x0020_Time"] = eventDate;
                    listItem.Update();
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
            }
        }

        private void AddEntry(string title, string message, SiteCollection siteCollection)
        {
            using (Logger.MethodTraceLogger(title, message, siteCollection))
            using (var clientContext = _clientContextProviderGenerator(siteCollection).CreateClientContext())
            {
                var web = clientContext.Site.RootWeb;
                clientContext.Load(web, x => x.ServerRelativeUrl);
                clientContext.ExecuteQueryWithIncrementalRetry();
                var statusList = clientContext.Site.RootWeb.GetList(web.ServerRelativeUrl.TrimStart('/') + "/lists/" + Lists.ContentTypeSyncLog);
                clientContext.ExecuteQueryWithIncrementalRetry();
                if (statusList != null)
                {
                    var itemCreateInfo = new ListItemCreationInformation();
                    var listItem = statusList.AddItem(itemCreateInfo);
                    listItem["Title"] = title;
                    listItem["PublishedObjectName"] = message;
                    listItem.Update();
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
            }
        }
    }
}