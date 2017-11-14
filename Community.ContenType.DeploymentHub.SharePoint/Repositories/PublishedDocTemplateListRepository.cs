using System;
using System.IO;
using System.Linq;
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
    public class PublishedDocTemplateListRepository : ListRepository, IPublishedDocTemplateListRepository
    {

        private static readonly ILog Logger = LogManager.GetLogger(typeof(StatusListRepository));

        private const string NameDisplayName = "Content Type Name";
        private const string IdDisplayName = "Content Type Id";
        private const string LastPublishedDisplayName = "Last Published";

        public PublishedDocTemplateListRepository(IClientContextProvider clientContextProvider) : base(clientContextProvider)
        {
        }

        public void EnsureDocTemplateLib()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var web = clientContext.Site.RootWeb;
                if (!ListExists(web, Lists.PublishedDocTempates))
                {
                    var library = CreateDocumentLibrary(web, Lists.PublishedDocTempates, "Published Document Templates Library of Community Content Type Hub");
                    library.EnableVersioning = true;
                    library.EnableMinorVersions = true;
                    library.MajorVersionLimit = 150;
                    library.MajorWithMinorVersionsLimit = 20;
                    library.UpdateListVersioning(true);
                    library.Update();
                    clientContext.ExecuteQueryWithIncrementalRetry();
                    AddField(library, "ContentTypeName", NameDisplayName, "Text", true, false);
                    AddField(library, "PublishedContentTypeId", IdDisplayName, "Text", true, false);
                    AddField(library, "LastPublished", LastPublishedDisplayName, "DateTime", true, false);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
            }
        }

        public void PublishDocumentTemplate(Domain.Core.ContentType contentType)
        {
            Logger.Debug("Removing old document template with another name from library if exists");
            RemoveTemplate(contentType);
            Logger.Debug("Adding or overwriting new document template");
            AddOrOverWriteTempate(contentType);
        }

        private void AddOrOverWriteTempate(Domain.Core.ContentType contentType)
        {
            using (Logger.MethodTraceLogger(contentType))
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var web = clientContext.Site.RootWeb;
                var publishedDocTemplateLib = web.Lists.GetByTitle(Lists.PublishedDocTempates);
                var timestamp = DateTime.UtcNow;
                var filename = contentType.DocumentTemplate;

                var file = clientContext.Web.GetFileByServerRelativeUrl(contentType.DocumentTemplateUrl);
                clientContext.Load(file);

                ClientResult<Stream> stream = file.OpenBinaryStream();
                clientContext.ExecuteQueryWithIncrementalRetry();

                using (var streamReader = new MemoryStream())
                {
                    stream.Value.CopyTo(streamReader);
                    // This step is done to avoid an error with the copying the data to the target location 
                    streamReader.Seek(0, SeekOrigin.Begin);
                    // This variable is used in FileCreationInformation to avoid too large files data problem. More info: https://msdn.microsoft.com/en-us/library/office/dn904536.aspx
                    var creationInformation = new FileCreationInformation
                    {
                        ContentStream = streamReader,
                        Url = filename,
                        Overwrite = true
                    };

                    var upload = publishedDocTemplateLib.RootFolder.Files.Add(creationInformation);
                    upload.ListItemAllFields["ContentTypeName"] = contentType.Title;
                    upload.ListItemAllFields["PublishedContentTypeId"] = contentType.Id;
                    upload.ListItemAllFields["LastPublished"] = timestamp;
                    upload.ListItemAllFields["Title"] = filename;
                    upload.ListItemAllFields.Update();
                    upload.Publish("Published By Community Content Type Hub");
                    clientContext.Load(upload, up => up.MajorVersion);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
            }
        }

        private void RemoveTemplate(Domain.Core.ContentType contentType)
        {
            using (Logger.MethodTraceLogger(contentType))
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var web = clientContext.Site.RootWeb;
                var publishedDocTemplateLib = web.Lists.GetByTitle(Lists.PublishedDocTempates);

                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = $"<View><Query><Where><And><Eq><FieldRef Name='PublishedContentTypeId' /><Value Type='Text'>{contentType.Id}</Value></Eq><Neq><FieldRef Name='Title' /><Value Type='Text'>{contentType.DocumentTemplate}</Value></Neq></And></Where></Query></View>";

                ListItemCollection listItems = publishedDocTemplateLib.GetItems(camlQuery);

                clientContext.Load(listItems);
                clientContext.ExecuteQueryWithIncrementalRetry();

                if (!listItems.Any()) return;
                listItems[0].DeleteObject();
                clientContext.ExecuteQueryWithIncrementalRetry();
            }
        }

        public May<Tuple<string, Stream>> GetTemplate(ContentTypeInfo contentType)
        {
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var publishedDocTemplateLib = clientContext.Site.RootWeb.Lists.GetByTitle(Lists.PublishedDocTempates);

                var camlQuery = new CamlQuery();
                camlQuery.ViewXml = $"<View><Query><Where><Eq><FieldRef Name='PublishedContentTypeId' /><Value Type='Text'>{contentType.Id}</Value></Eq></Where></Query></View>";

                var listItems = publishedDocTemplateLib.GetItems(camlQuery);
                clientContext.Load(listItems, items => items.Include(f => f.File, f => f.File.Name));
                clientContext.ExecuteQueryWithIncrementalRetry();

                //TODO create proper class for template
                return listItems.MaySingle().Select(li => Tuple.Create(li.File.Name, GetFileStream(li)));
            }
        }

        private static Stream GetFileStream(ListItem item)
        {
            var stream = item.File.OpenBinaryStream();
            item.Context.ExecuteQueryWithIncrementalRetry();
            var syncedStream = Stream.Synchronized(stream.Value);
            return syncedStream;
        }
    }
}
