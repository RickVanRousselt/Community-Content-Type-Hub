using OfficeDevPnP.Core;
using System;
using System.Linq;
using System.Xml.Linq;
using log4net;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.DocumentSet;

namespace Community.ContenType.DeploymentHub.SharePoint.Extensions
{
    public static class ContentTypeExtensions
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ContentTypeExtensions));

        // Overwrite default PNP core behaviour for performance reasons.
        public static void CreateContentTypeFromXml(this Web web, XDocument xDocument)
        {
            var ns = xDocument.Root?.Name.Namespace;

            var contentTypes = xDocument.Descendants(ns + "ContentType");

            foreach (var schema in contentTypes)
            {
                var id = schema.Attribute("ID")?.Value;
                var name = schema.Attribute("Name")?.Value;

                var description = schema.Attribute("Description")?.Value ?? string.Empty;
                var group = schema.Attribute("Group")?.Value ?? string.Empty;

                var newContentType = web.CreateContentType(name, description, id, group);
                
                var fieldRefs = schema.Descendants(ns + "FieldRefs").Elements(ns + "FieldRef");
                foreach (var fieldRef in fieldRefs)
                {
                    var fieldRefId = fieldRef.Attribute("ID")?.Value;
                    var required = fieldRef.Attribute("Required") != null && bool.Parse(fieldRef.Attribute("Required").Value);
                    var hidden = fieldRef.Attribute("Hidden") != null && bool.Parse(fieldRef.Attribute("Hidden").Value);
                    web.AddFieldToContentTypeById(newContentType, fieldRefId, required, hidden);
                }

                if (id.StartsWith(BuiltInContentTypeId.DocumentSet))
                {
                    ProcessDocumentSet(web, newContentType, schema, ns);
                }
            }
        }

        private static void ProcessDocumentSet(Web web, ContentType contentType, XElement schema, XNamespace ns)
        {
            var template = DocumentSetTemplate.GetDocumentSetTemplate(web.Context, contentType);
            web.Context.Load(template, t => t.AllowedContentTypes, t => t.SharedFields, t => t.WelcomePageFields);
            web.Context.ExecuteQueryWithIncrementalRetry();

            AddAllowedContentTypes(web, schema, ns, template);
            AddSharedFields(web, schema, ns, template);
            AddWelcomePageFields(web, schema, ns, template);

            web.Context.ExecuteQueryWithIncrementalRetry();
        }

        private static void AddAllowedContentTypes(Web web, XElement schema, XNamespace ns, DocumentSetTemplate template)
        {
            var allowedContentTypes = schema.Descendants(ns + "AllowedContentTypes").Elements(ns + "AllowedContentType");
            foreach (var allowedContentType in allowedContentTypes)
            {
                var id = allowedContentType.Attribute("ID")?.Value;
                var act = web.GetContentTypeById(id);
                if (act != null && !template.AllowedContentTypes.Any(a => a.StringValue == id))
                {
                    template.AllowedContentTypes.Add(act.Id);
                    template.Update(true);
                }
            }
        }

        private static void AddSharedFields(Web web, XElement schema, XNamespace ns, DocumentSetTemplate template)
        {
            var sharedFields = schema.Descendants(ns + "SharedFields").Elements(ns + "SharedField");
            foreach (var sharedField in sharedFields)
            {
                var id = new Guid(sharedField.Attribute("ID")?.Value);
                var field = web.GetFieldById<Field>(id);
                if (field != null && !template.SharedFields.Any(a => a.Id == field.Id))
                {
                    template.SharedFields.Add(field);
                    template.Update(true);
                }
            }
        }

        private static void AddWelcomePageFields(Web web, XElement schema, XNamespace ns, DocumentSetTemplate template)
        {
            var welcomePageFields = schema.Descendants(ns + "WelcomePageFields").Elements(ns + "WelcomePageField");
            foreach (var welcomePageField in welcomePageFields)
            {
                var id = new Guid(welcomePageField.Attribute("ID")?.Value);
                var field = web.GetFieldById<Field>(id);
                if (field != null && !template.WelcomePageFields.Any(a => a.Id == field.Id))
                {
                    template.WelcomePageFields.Add(field);
                    template.Update(true);
                }
            }
        }

        private static ContentType CreateContentType(this Web web, string name, string description, string id, string group, ContentType parentContentType = null)
        {
            Logger.Debug($"Creating content type {name} with id:{id}");

            var contentTypes = web.ContentTypes;

            var creationInformation = new ContentTypeCreationInformation
            {
                Name = name,
                Id = id,
                Description = description,
                Group = group,
                ParentContentType = parentContentType
            };

            var newContentType = contentTypes.Add(creationInformation);
            web.Context.ExecuteQueryWithIncrementalRetry();

            return newContentType;
        }

        private static void AddFieldToContentTypeById(this Web web, ContentType ct, string fieldId, bool required = false, bool hidden = false)
        {
            var field = web.Fields.GetById(new Guid(fieldId));
            web.Context.Load(field, f => f.Id, f => f.SchemaXmlWithResourceTokens);
            // content type has to be reloaded every time... otherwise fieldlink id is not loaded
            web.Context.Load(ct, c => c.Id, c => c.FieldLinks.Include(fl => fl.Id));
            web.Context.ExecuteQueryWithIncrementalRetry();
            AddFieldToContentType(web, ct, field, required, hidden);
        }

        private static void AddFieldToContentType(this Web web, ContentType contentType, Field field, bool required = false, bool hidden = false)
        {
            Logger.Debug($"Adding field with id: {field.Id} to content type with id {contentType.Id}");

            var fieldLink = contentType.FieldLinks.FirstOrDefault(fld => fld.Id == field.Id);
            if (fieldLink == null)
            {
                var fieldElement = XElement.Parse(field.SchemaXmlWithResourceTokens);
                fieldElement.SetAttributeValue("AllowDeletion", "TRUE"); // Default behavior when adding a field to a CT from the UI.
                field.SchemaXml = fieldElement.ToString();
                var fldInfo = new FieldLinkCreationInformation { Field = field };
                contentType.FieldLinks.Add(fldInfo);
                contentType.Update(true);
                web.Context.ExecuteQueryWithIncrementalRetry();

                fieldLink = contentType.FieldLinks.GetById(field.Id);
            }

            fieldLink.EnsureProperties(f => f.Required, f => f.Hidden);

            if (required != fieldLink.Required || hidden != fieldLink.Hidden)
            {
                fieldLink.Required = required;
                fieldLink.Hidden = hidden;
                contentType.Update(true);
                web.Context.ExecuteQueryWithIncrementalRetry();
            }
        }
    }
}