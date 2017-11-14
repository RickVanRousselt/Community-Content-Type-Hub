using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Community.ContenType.DeploymentHub.SharePoint.Repositories
{
    public class PublishedSiteColumnsListRepository : PublishedListRepository<PublishedSiteColumn>,
        IPublishedSiteColumnsListRepository
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PublishedSiteColumnsListRepository));

        private const string NameDisplayName = "Site Column Name";
        private const string IdDisplayName = "Site Column Id";
        private const string LastPublishedDisplayName = "Last Published";
        private const string TermPathDisplayName = "Term Path";
        private const string DefaultTermPathDisplayName = "Default Term Path";

        public PublishedSiteColumnsListRepository(IClientContextProvider clientContextProvider)
            : base(clientContextProvider, Lists.PublishedSiteColumns) { }

        public void EnsurePublishedSiteColumnsLibrary()
        {
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var web = clientContext.Site.RootWeb;
                if (!ListExists(web, Lists.PublishedSiteColumns))
                {
                    var library = CreateDocumentLibrary(web, Lists.PublishedSiteColumns,
                        "Published Site Columns Library of Community Content Type Hub");
                    library.EnableVersioning = true;
                    library.EnableMinorVersions = true;
                    library.MajorVersionLimit = 150;
                    library.MajorWithMinorVersionsLimit = 20;
                    library.UpdateListVersioning(true);
                    library.Update();
                    clientContext.ExecuteQueryWithIncrementalRetry();
                    AddField(library, "SiteColumnName", NameDisplayName, "Text", true, false);
                    AddField(library, "SiteColumnId", IdDisplayName, "Text", true, false);
                    AddField(library, "LastPublished", LastPublishedDisplayName, "DateTime", true, false);
                    AddField(library, "TermPath", TermPathDisplayName, "Text", false, false);
                    AddField(library, "DefaultTermPath", DefaultTermPathDisplayName, "Text", false, false);
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
                if (ListExists(web, Lists.PublishedSiteColumns))
                {
                    var list = web.Lists.GetByTitle(Lists.PublishedSiteColumns);
                    clientContext.Load(list);
                    clientContext.ExecuteQueryWithIncrementalRetry();

                    var view = list.DefaultView;
                    view.ViewFields.Add("_UIVersionString");
                    view.Update();
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
            }
        }

        public PublishedSiteColumn PublishSiteColumn(SiteColumn siteColumn)
        {
            using (Logger.MethodTraceLogger(siteColumn))
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var lastPublished = DateTime.UtcNow;
                var web = clientContext.Site.RootWeb;
                var publishedContentTypesList = web.Lists.GetByTitle(Lists.PublishedSiteColumns);

                //var siteColumn = siteColumn;
                var fle = new FileCreationInformation
                {
                    Content = Encoding.UTF8.GetBytes(siteColumn.Schema.ToString()),
                    Url = siteColumn.Name + ".xml",
                    Overwrite = true
                };

                var upload = publishedContentTypesList.RootFolder.Files.Add(fle);
                upload.ListItemAllFields["SiteColumnName"] = siteColumn.InternalName;
                upload.ListItemAllFields["SiteColumnId"] = siteColumn.Id;
                upload.ListItemAllFields["LastPublished"] = lastPublished;
                var taxColumn = siteColumn as TaxonomySiteColumn;
                if (taxColumn != null)
                {
                    upload.ListItemAllFields["TermPath"] = taxColumn.Path.ToString();
                    upload.ListItemAllFields["DefaultTermPath"] = taxColumn.DefaultValuePath.Select(path => path.ToString()).Else(string.Empty);
                }

                upload.ListItemAllFields.Update();
                upload.Publish("Published By Community Content Type Hub");
                clientContext.Load(upload, col => col.MajorVersion);
                clientContext.ExecuteQueryWithIncrementalRetry();

                var taxonomySiteColumn = siteColumn as TaxonomySiteColumn;
                return taxonomySiteColumn != null
                    ? new PublishedTaxonomySiteColumn(
                        taxonomySiteColumn.Name,
                        taxonomySiteColumn.Id,
                        lastPublished,
                        taxonomySiteColumn.Schema,
                        upload.MajorVersion,
                        taxonomySiteColumn.Path,
                        taxonomySiteColumn.DefaultValuePath)
                    : new PublishedSiteColumn(
                        siteColumn.Name,
                        siteColumn.Id,
                        lastPublished,
                        siteColumn.Schema,
                        upload.MajorVersion);
            }
        }

        public bool CheckSiteColumnsForUpdate(SiteColumn siteColumn)
        {
            using (Logger.MethodTraceLogger(siteColumn.Name))
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var maybeListItem = FindListItemForSiteColumn(clientContext, siteColumn.Id);
                return maybeListItem
                    .Select(li => IsListItemFileEqualToSchema(li, siteColumn.Schema))
                    .Else(false);
            }
        }

        public bool CheckSiteColumnDataType(SiteColumn siteColumn)
        {
            using (Logger.MethodTraceLogger(siteColumn.Name))
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var maybeListItem = FindListItemForSiteColumn(clientContext, siteColumn.Id);
                return maybeListItem
                    .Select(li => IsListItemDataTypeEqual(li, siteColumn.Schema))
                    .Else(true);
            }
        }


        private static May<ListItem> FindListItemForSiteColumn(ClientContext clientContext, Guid siteColumnId)
        {
            using (Logger.MethodTraceLogger(siteColumnId))
            {

                var web = clientContext.Site.RootWeb;
                var publishedContentTypesList = web.Lists.GetByTitle(Lists.PublishedSiteColumns);

                var query = new CamlQuery
                {
                    ViewXml = $"<Where><Eq><FieldRef Name=\'SiteColumnId\'/><Value Type=\'Text\'>{siteColumnId}</Value></Eq></Where>"
                };

                var existingItemCollection = publishedContentTypesList.GetItems(query);
                clientContext.Load(existingItemCollection, ic => ic.Include(f => f.File, f => f["SiteColumnId"], f => f.Client_Title));
                clientContext.ExecuteQueryWithIncrementalRetry();

                return existingItemCollection
                    .Where(x => x["SiteColumnId"] as string == siteColumnId.ToString())
                    .MayFirst();
            }
        }

        private static bool IsListItemFileEqualToSchema(ListItem listItem, XDocument siteColumnSchema)
        {
            using (Logger.MethodTraceLogger(listItem.Client_Title))
            {
                var fileStream = listItem.File.OpenBinaryStream();
                listItem.Context.ExecuteQueryWithIncrementalRetry();
                using (var stream = fileStream.Value)
                {
                    var listItemContent = XDocument.Load(stream);
                    return XNode.DeepEquals(listItemContent, siteColumnSchema);
                }
            }
        }

        private static bool IsListItemDataTypeEqual(ListItem listItem, XDocument siteColumnSchema)
        {
            using (Logger.MethodTraceLogger(listItem.Client_Title))
            {
                var fileStream = listItem.File.OpenBinaryStream();
                listItem.Context.ExecuteQueryWithIncrementalRetry();
                using (var stream = fileStream.Value)
                {
                    
                    var listItemContent = XDocument.Load(stream);
                    var listItemType = GetAttributeValue(listItemContent, "Type");
                    var publishedType = GetAttributeValue(siteColumnSchema, "Type");
                    Logger.Debug($"Type found in listItem in library:{listItemType} - Type found in SharePoint SiteColumn:{publishedType}");
                    
                    if(listItemType.Where(x => x == "TaxonomyFieldType" || x == "TaxonomyFieldTypeMulti").HasValue) return true;

                    return listItemType == publishedType;
                }
            }
        }

        protected override PublishedSiteColumn MapListItemToPublishedItemWithSchema(ListItem listItem, ClientContext clientContext)
        {
            var fileRef = listItem.File.ServerRelativeUrl;
            var fileInfo = File.OpenBinaryDirect(clientContext, fileRef);
            using (Logger.MethodTraceLogger(listItem.Client_Title))
            using (var stream = fileInfo.Stream)
            {
                var xdoc = XDocument.Load(stream);

                var termPath = listItem["TermPath"];
                if (termPath != null)
                {
                    var path = listItem["DefaultTermPath"];
                    var defaultValuePath = path != null ? new TermPath(path.ToString()) : May<TermPath>.NoValue;

                    return new PublishedTaxonomySiteColumn(listItem["SiteColumnName"].ToString(),
                        new Guid(listItem["SiteColumnId"].ToString()),
                        Convert.ToDateTime(listItem["LastPublished"]),
                        xdoc,
                        listItem.File.MajorVersion, new TermPath(termPath.ToString()), defaultValuePath);
                }
                return new PublishedSiteColumn(listItem["SiteColumnName"].ToString(),
                    new Guid(listItem["SiteColumnId"].ToString()),
                    Convert.ToDateTime(listItem["LastPublished"]),
                    xdoc,
                    listItem.File.MajorVersion);
            }

        }

        protected override PublishedSiteColumn MapListItemToPublishedItemNoSchema(ListItem item)
        {
            return new PublishedSiteColumn(item["SiteColumnName"].ToString(),
                new Guid(item["SiteColumnId"].ToString()),
                Convert.ToDateTime(item["LastPublished"]),
                null,
                item.File.MajorVersion);
        }

        protected override Expression<Func<ListItemCollection, object>> GetRetrievals() => 
            items => items.Include(
                item => item.File.MajorVersion,
                item => item.File,
                item => item.Client_Title,
                item => item["SiteColumnName"],
                item => item["SiteColumnId"],
                item => item["TermPath"],
                item => item["DefaultTermPath"],
                item => item["LastPublished"]);

        public PublishedSiteColumnList GetList() => new PublishedSiteColumnList(new HashSet<PublishedSiteColumn>(GetAllPublishedItemsWithSchema()));

        private static May<string> GetAttributeValue(XDocument document, string name) => document.Root?.Attribute(name)?.Value.Maybe() ?? May.NoValue;
    }
}