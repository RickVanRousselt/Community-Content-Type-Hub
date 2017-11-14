using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Microsoft.SharePoint.Client;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.SharePoint.Updaters.ContentType
{
    internal class ContentTypeUpdater : UpdaterBase
    {
        private readonly IClientContextProvider _targetClientContextProvider;
        private readonly IPublishedDocTemplateListRepository _docTemplateListRepository;
        private readonly PublishedContentType _publishedContentType;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ContentTypeUpdater));

        private May<string> Group => GetAttributeValue("Group");
        private May<string> Description => GetAttributeValue("Description");
        private May<string> DocumentTemplate => Schema.Root?.Element("DocumentTemplate")?.Attribute("TargetName")?.Value.Maybe() ?? May.NoValue;

        internal ContentTypeUpdater(
            IClientContextProvider targetClientContextProvider, 
            IPublishedDocTemplateListRepository docTemplateListRepository,
            PublishedContentType publishedContentType)
            : base(publishedContentType.Schema)
        {
            _targetClientContextProvider = targetClientContextProvider;
            _docTemplateListRepository = docTemplateListRepository;
            _publishedContentType = publishedContentType;
        }


        internal void Update()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = _targetClientContextProvider.CreateClientContext())
            {
                Logger.Info($"Updating {_publishedContentType.Title}");
                var newSiteColumnLinks = _publishedContentType.ExtractSiteColumnLinks();

                var contentType = GetUpdatableContentType(clientContext);
                var oldSiteColumnsInfos = contentType.Fields.Select(f => new SiteColumnInfo(f.Title, f.Id)).ToList();

                var siteColumnsToBeRemoved = oldSiteColumnsInfos.Except(newSiteColumnLinks).ToList();
                Logger.Debug($"Site Columns to remove from content type {string.Join(";", siteColumnsToBeRemoved.Select(x => x.Title))}");
                var siteColumnInfosToAdd = newSiteColumnLinks.Except(oldSiteColumnsInfos).ToList();
                Logger.Debug($"Site Columns to add to content type {string.Join(";", siteColumnInfosToAdd.Select(x => x.Title))}");

                RemoveSiteColumns(siteColumnsToBeRemoved, contentType);
                AddSiteColumns(siteColumnInfosToAdd, contentType, clientContext);

                UpdateSiteColumnLinkProperties(newSiteColumnLinks, contentType, clientContext);
                UpdateProperties(contentType);

                contentType.Update(true);
                clientContext.ExecuteQueryWithIncrementalRetry();
            }
        }

        private Microsoft.SharePoint.Client.ContentType GetUpdatableContentType(ClientContext clientContext)
        {
            using (Logger.MethodTraceLogger())
            {
                var contentType = clientContext.Site.RootWeb.GetContentTypeById(_publishedContentType.Id);
                contentType.ReadOnly = false;
                contentType.Update(true);
                clientContext.Load(contentType, 
                    ct => ct.FieldLinks.Include(fl => fl.Id, fl => fl.Required, fl => fl.Hidden), 
                    ct => ct.Fields.Include(f => f.Id, f => f.Title, f => f.CanBeDeleted, f => f.Required, f => f.Hidden));
                clientContext.ExecuteQueryWithIncrementalRetry();
                return contentType;
            }
        }

        private void UpdateProperties(Microsoft.SharePoint.Client.ContentType contentType)
        {
            using (Logger.MethodTraceLogger())
            {
                Group.IfHasValueThenDo(x => contentType.Group = x);
                Description.IfHasValueThenDo(x => contentType.Description = x);
                DocumentTemplate.IfHasValueThenDo(x => SetDocumentTemplate(x, contentType));
                contentType.ReadOnly = true;
            }
        }

        private void SetDocumentTemplate(string docTemplateUrl, Microsoft.SharePoint.Client.ContentType contentType)
        {
            if (docTemplateUrl.Contains("_cts"))
            {
                UploadAndSetDocumentTemplate(contentType);
            }
            else
            {
                contentType.DocumentTemplate = docTemplateUrl;
            }
        }

        private void UploadAndSetDocumentTemplate(Microsoft.SharePoint.Client.ContentType contentType)
        {
            using (Logger.MethodTraceLogger(contentType))
            {
                var maybeTemplateFromHub = _docTemplateListRepository.GetTemplate(_publishedContentType); //TODO store this info in PublishedContentType?
                maybeTemplateFromHub.IfHasValueThenDo(template => UploadTemplate(contentType, template.Item1, template.Item2));
            }
        }

        private void UploadTemplate(Microsoft.SharePoint.Client.ContentType contentType, string templateFilename, Stream templateStream)
        {
            using (var memoryStream = new MemoryStream())
            {
                templateStream.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                UploadTemplateToTargetSite(memoryStream, templateFilename, contentType.Name);
                contentType.DocumentTemplate = templateFilename;
            }
        }

        private void UploadTemplateToTargetSite(Stream fileStream, string documentTemplate, string contentTypeName)
        {
            using (var targetClientContext = _targetClientContextProvider.CreateClientContext())
            {
                // This variable is used in FileCreationInformation to avoid too large files data problem. More info: https://msdn.microsoft.com/en-us/library/office/dn904536.aspx
                var creationInformation = new FileCreationInformation
                {
                    ContentStream = fileStream,
                    Url = documentTemplate,
                    Overwrite = true
                };
                var target = targetClientContext.Site.RootWeb;
                targetClientContext.Load(target, w => w.ServerRelativeUrl);
                targetClientContext.ExecuteQueryWithIncrementalRetry();
                string resourcePath = $"{target.ServerRelativeUrl}/_cts/{contentTypeName}";
                var folder = target.GetFolderByServerRelativeUrl(resourcePath);
                folder.Files.Add(creationInformation);
                targetClientContext.ExecuteQueryWithIncrementalRetry();
            }
        }

        private static void UpdateSiteColumnLinkProperties(ISet<SiteColumnLink> newSiteColumnLinks, Microsoft.SharePoint.Client.ContentType contentType, ClientContext clientContext)
        {
            using (Logger.MethodTraceLogger())
            {
                foreach (var siteColumnLink in newSiteColumnLinks)
                {
                    Logger.Debug($"Updating Site Column link properties {siteColumnLink.Title} to ReadOnly:{siteColumnLink.ReadOnly} and Hidden:{siteColumnLink.Hidden}");
                    var fieldLink = contentType.FieldLinks.GetById(siteColumnLink.Id);
                    var field = clientContext.Site.RootWeb.GetFieldById(siteColumnLink.Id);
                    clientContext.Load(fieldLink, f => f.Required, f => f.Hidden);
                    clientContext.Load(field, f => f.InternalName, f => f.Group);
                    clientContext.ExecuteQueryWithIncrementalRetry();

                    if (field.InternalName.StartsWith("_") || field.InternalName.StartsWith("ows") || field.Group.StartsWith("_Hidden") || field.InternalName == "TaxCatchAll" || field.InternalName == "TaxCatchAllLabel") continue;
                    
                    siteColumnLink.Hidden.IfHasValueThenDo(x => fieldLink.Hidden = x);
                    siteColumnLink.Required.IfHasValueThenDo(x => fieldLink.Required = x);
                    contentType.Update(true);
                    clientContext.ExecuteQueryWithIncrementalRetry();                
                }
            }
        }

        private void AddSiteColumns(IEnumerable<SiteColumnInfo> siteColumnInfosToAdd, Microsoft.SharePoint.Client.ContentType contentType, ClientContext clientContext)
        {
            using (Logger.MethodTraceLogger())
            {
                foreach (var siteColumnInfo in siteColumnInfosToAdd)
                {
                    contentType.EnsureProperties(c => c.Id, c => c.SchemaXml, c => c.FieldLinks.Include(fl => fl.Id, fl => fl.Required, fl => fl.Hidden));

                    var flink = contentType.FieldLinks.FirstOrDefault(fld => fld.Id == siteColumnInfo.Id);
                    var siteColumnToAdd = clientContext.Site.RootWeb.GetFieldById(siteColumnInfo.Id);
                    clientContext.Load(siteColumnToAdd, x => x.Id, x => x.SchemaXmlWithResourceTokens);
                    clientContext.ExecuteQueryWithIncrementalRetry();

                    if (flink == null || siteColumnInfo.Title != "TaxCatchAllLabel")
                    {
                        Logger.Debug($"Adding Site Column link {siteColumnInfo.Title}");

                        var fieldElement = XElement.Parse(siteColumnToAdd.SchemaXmlWithResourceTokens);
                        fieldElement.SetAttributeValue("AllowDeletion", "TRUE"); // Default behavior when adding a field to a CT from the UI.
                        siteColumnToAdd.SchemaXml = fieldElement.ToString();
                        var fldInfo = new FieldLinkCreationInformation {Field = siteColumnToAdd};
                        contentType.FieldLinks.Add(fldInfo);
                        contentType.Update(true);
                        clientContext.ExecuteQueryWithIncrementalRetry();
                    }
                    Logger.Info($"Adding {siteColumnInfo} to {_publishedContentType as ContentTypeInfo}.");
                }
            }
        }

        private void RemoveSiteColumns(IEnumerable<SiteColumnInfo> siteColumnsToBeRemoved, Microsoft.SharePoint.Client.ContentType contentType)
        {
            using (Logger.MethodTraceLogger(contentType.Name))
            {
                foreach (var siteColumnInfo in siteColumnsToBeRemoved)
                {
                    if (siteColumnInfo.Title != "TaxCatchAll" || siteColumnInfo.Title != "TaxCatchAllLabel")
                    {
                        Logger.Info($"Removing {siteColumnInfo} from {_publishedContentType as ContentTypeInfo}.");
                        var fieldLink = contentType.FieldLinks.GetById(siteColumnInfo.Id);
                        fieldLink.DeleteObject();
                    }
                }
            }
        }
    }
}