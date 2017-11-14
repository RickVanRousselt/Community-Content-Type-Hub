using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Linq;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Microsoft.SharePoint.Client;
using Strilanc.Value;
using ContentType = Community.ContenType.DeploymentHub.Domain.Core.ContentType;

namespace Community.ContenType.DeploymentHub.SharePoint.Repositories
{
    public class PublishedContentTypesListRepository : PublishedListRepository<PublishedContentType>, IPublishedContentTypesListRepository
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PublishedContentTypesListRepository));

        private const string NameDisplayName = "Content Type Name";
        private const string IdDisplayName = "Content Type Id";
        private const string LastPublishedDisplayName = "Last Published";


        public PublishedContentTypesListRepository(IClientContextProvider clientContextProvider)
            : base(clientContextProvider, Lists.PublishedContentTypes) { }

        public void EnsurePublishedContentTypesLibrary()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var web = clientContext.Site.RootWeb;
                if (!ListExists(web, Lists.PublishedContentTypes))
                {
                    var library = CreateDocumentLibrary(web, Lists.PublishedContentTypes,
                        "Published Content Types Library of Community Content Type Hub");
                    library.EnableVersioning = true;
                    library.EnableMinorVersions = true;
                    library.MajorVersionLimit = 150;
                    library.MajorWithMinorVersionsLimit = 20;
                    library.UpdateListVersioning(true);
                    library.Update();
                    clientContext.ExecuteQueryWithIncrementalRetry();
                    //Title = filename
                    AddField(library, "ContentTypeName", NameDisplayName, "Text", true, false);
                    AddField(library, "PublishedContentTypeId", IdDisplayName, "Text", true, false);
                    AddField(library, "LastPublished", LastPublishedDisplayName, "DateTime", true, false);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
            }
        }

        public void UpdateView()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {

                var web = clientContext.Site.RootWeb;
                if (ListExists(web, Lists.PublishedContentTypes))
                {
                    var list = web.Lists.GetByTitle(Lists.PublishedContentTypes);
                    clientContext.Load(list);
                    clientContext.ExecuteQueryWithIncrementalRetry();

                    var view = list.DefaultView;
                    view.ViewFields.Add("_UIVersionString");
                    view.Update();
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
            }
        }

        public PublishedContentType PublishContentType(ContentType contentType)
        {
            using (Logger.MethodTraceLogger(contentType))
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var web = clientContext.Site.RootWeb;
                var publishedContentTypesList = web.Lists.GetByTitle(Lists.PublishedContentTypes);
                var timestamp = DateTime.UtcNow;
                var filename = contentType.Title + ".xml";
                var creationInformation = new FileCreationInformation
                {
                    Content = Encoding.UTF8.GetBytes(contentType.Schema.ToString()),
                    Url = filename,
                    Overwrite = true
                };
                var upload = publishedContentTypesList.RootFolder.Files.Add(creationInformation);
                upload.ListItemAllFields["ContentTypeName"] = contentType.Title;
                upload.ListItemAllFields["PublishedContentTypeId"] = contentType.Id;
                upload.ListItemAllFields["LastPublished"] = timestamp;
                upload.ListItemAllFields.Update();
                upload.Publish("Published By Community Content Type Hub");
                clientContext.Load(upload, up => up.MajorVersion);
                clientContext.ExecuteQueryWithIncrementalRetry();

                //doctemplate



                return new PublishedContentType(contentType.Title, contentType.Id, timestamp, contentType.Schema, upload.MajorVersion);
            }
        }

        public May<PublishedContentType> GetPublishedContentType(ContentType contentType)
        {
            using (Logger.MethodTraceLogger(contentType))
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {

                var web = clientContext.Site.RootWeb;
                var publishedContentTypesList = web.Lists.GetByTitle(Lists.PublishedContentTypes);
                var query = new CamlQuery
                {
                    ViewXml = $"<Where><Eq><FieldRef Name=\'PublishedContentTypeId\'/><Value Type=\'Text\'>{contentType.Id}</Value></Eq></Where>"
                };
                var returnedItems = publishedContentTypesList.GetItems(query);
                clientContext.Load(returnedItems, GetRetrievals());
                clientContext.ExecuteQueryWithIncrementalRetry();
                clientContext.ExecuteQueryWithIncrementalRetry();
                // ReSharper disable once AccessToDisposedClosure
                return returnedItems
                    .MaySingle()
                    .Select(li => MapListItemToPublishedItemWithSchema(li, clientContext));
            }
        }

        protected override PublishedContentType MapListItemToPublishedItemWithSchema(ListItem listItem, ClientContext clientContext)
        {
            var fileRef = listItem.File.ServerRelativeUrl;
            var fileInfo = File.OpenBinaryDirect(clientContext, fileRef);
            using (var stream = fileInfo.Stream)
            {
                var xdoc = XDocument.Load(stream);
                return new PublishedContentType(listItem["ContentTypeName"].ToString(),
                    listItem["PublishedContentTypeId"].ToString(),
                    Convert.ToDateTime(listItem["LastPublished"]),
                    xdoc,
                    listItem.File.MajorVersion);
            }

        }

        protected override PublishedContentType MapListItemToPublishedItemNoSchema(ListItem listItem)
        {
            return new PublishedContentType(listItem["ContentTypeName"].ToString(),
                listItem["PublishedContentTypeId"].ToString(),
                Convert.ToDateTime(listItem["LastPublished"]).ToLocalTime(),
                null,
                listItem.File.MajorVersion);
        }

        protected override Expression<Func<ListItemCollection, object>> GetRetrievals() =>
            items => items.Include(
                item => item.File.MajorVersion,
                item => item.File,
                item => item.Client_Title,
                item => item["ContentTypeName"],
                item => item["PublishedContentTypeId"],
                item => item["LastPublished"]);

        public PublishedContentTypeList GetList() => new PublishedContentTypeList(new HashSet<PublishedContentType>(GetAllPublishedItemsWithSchema()));
    }
}