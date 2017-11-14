using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Common;
using log4net;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;

namespace Community.ContenType.DeploymentHub.SharePoint.Repositories
{
    public class ListRepository
    {
        protected readonly IClientContextProvider ClientContextProvider;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ContentTypeGroupingListRepository));

        public ListRepository(IClientContextProvider clientContextProvider)
        {
            ClientContextProvider = clientContextProvider;
        }

        private Guid GetTermStoreId()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var taxonomySession = TaxonomySession.GetTaxonomySession(clientContext);
                var termStore = taxonomySession.GetDefaultSiteCollectionTermStore();
                termStore.UpdateCache();
                clientContext.Load(termStore, ts => ts.Id);
                clientContext.ExecuteQueryWithIncrementalRetry();
                return termStore.Id;
            }
        }

        protected static List CreateGenericList(Web web, string title, string description)
        {
            using (Logger.MethodTraceLogger(title, description))
            {
                var creationInfo = new ListCreationInformation();
                creationInfo.Title = title;
                creationInfo.TemplateType = (int)ListTemplateType.GenericList;
                var list = web.Lists.Add(creationInfo);
                list.Description = description;
                list.Update();
                return list;
            }
        }

        protected static List CreateDocumentLibrary(Web web, string title, string description)
        {
            using (Logger.MethodTraceLogger(title, description))
            {
                var creationInfo = new ListCreationInformation();
                creationInfo.Title = title;
                creationInfo.TemplateType = (int)ListTemplateType.DocumentLibrary;
                var list = web.Lists.Add(creationInfo);
                list.Description = description;
                list.Update();
                return list;
            }
        }

        protected static void AddField(List list, string internalName, string displayName, string dataType, bool required, bool hidden)
        {
            var xml = $"<Field Name='{internalName}' DisplayName='{displayName}' Type='{dataType}' StaticName='{internalName}' />";
            var field = list.Fields.AddFieldAsXml(xml, true, AddFieldOptions.AddFieldInternalNameHint);
            field.Required = required;
            field.Hidden = hidden;
            field.Update();
        }

        protected static void AddHtmlNoteField(List list, string internalName, string displayName)
        {
            var xml = $"<Field Name='{internalName}' StaticName='{internalName}' DisplayName ='{displayName}' Type=\"Note\" RestrictedMode=\"TRUE\" RichText=\"TRUE\" RichTextMode=\"FullHtml\" RowOrdinal=\"0\" Required=\"FALSE\" EnforceUniqueValues=\"FALSE\" Indexed=\"FALSE\" NumLines=\"6\" IsolateStyles=\"TRUE\" AppendOnly=\"FALSE\" />";
            var field = list.Fields.AddFieldAsXml(xml, true, AddFieldOptions.AddFieldInternalNameHint);
            field.Update();
        }

        protected static void RenameField(List list, string internalName, string newDisplayName)
        {
            var field = list.Fields.GetByInternalNameOrTitle(internalName);
            field.Title = newDisplayName;
            field.Update();
        }
        protected static void MakeFieldUnique(List list, string internalName)
        {
            var field = list.Fields.GetByInternalNameOrTitle(internalName);
            field.EnforceUniqueValues = true;
            field.Update();
        }
        protected static void IndexField(List list, string internalName)
        {
            var field = list.Fields.GetByInternalNameOrTitle(internalName);
            field.Indexed = true;
            field.Update();
        }

        protected static void AddChoiceField(List list, string internalName, string displayName, string defaultState, IEnumerable<string> otherStates, bool required)
        {
            var xml = $"<Field Name='{internalName}' StaticName='{internalName}' DisplayName='{displayName}' Type='Choice' Format='Dropdown'><Default>{defaultState}</Default><CHOICES>"
                + string.Join(string.Empty, otherStates.Select(state => $"<CHOICE>{state}</CHOICE>"))
                + "</CHOICES></Field>";

            var field = list.Fields.AddFieldAsXml(xml, true, AddFieldOptions.AddFieldInternalNameHint);
            field.Required = required;
            field.Hidden = false;
            field.Update();
        }

        protected void AddSingleTaxonomyField(List list, string internalName, string displayName, Guid termSetId, bool required)
        {
            var xml = $"<Field Name='{internalName}' StaticName='{internalName}' DisplayName='{displayName}' Type='TaxonomyFieldType' ShowField='Term1033' />";
            var field = list.Fields.AddFieldAsXml(xml, true, AddFieldOptions.AddFieldInternalNameHint);
            var taxField = list.Context.CastTo<TaxonomyField>(field);

            taxField.SspId = GetTermStoreId();
            taxField.TermSetId = termSetId;
            taxField.AllowMultipleValues = false;
            taxField.Open = true;

            taxField.TargetTemplate = string.Empty;
            taxField.AnchorId = Guid.Empty;
            taxField.Required = required;
            taxField.Hidden = false;
            taxField.Update();

            list.Update();
        }

        public bool ListExists(string listName)
        {
            using (Logger.MethodTraceLogger(listName))
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                return ListExists(clientContext.Site.RootWeb, listName);
            }
        }

        protected static bool ListExists(Web web, string listName)
        {
            var list = web.Lists.GetByTitle(listName);
            web.Context.Load(list);

            try
            {
                web.Context.ExecuteQueryWithIncrementalRetry();
                return true;
            }
            catch (ServerException ex)
            {
                if (ex.ServerErrorTypeName != "System.ArgumentException") throw;
                return false;
            }
        }

        protected IEnumerable<ListItem> GetItemsSafe(List list, params Expression<Func<ListItemCollection, object>>[] retrievals)
        {
            var query = CamlQuery.CreateAllItemsQuery(2000);
            ListItemCollectionPosition listItemCollectionPosition = null;
            do
            {
                query.ListItemCollectionPosition = listItemCollectionPosition;
                var listItemCollection = list.GetItems(query);
                list.Context.Load(listItemCollection, items => items.ListItemCollectionPosition);
                list.Context.Load(listItemCollection, retrievals);
                list.Context.ExecuteQueryWithIncrementalRetry();
                foreach (var item in listItemCollection)
                {
                    yield return item;
                }
                listItemCollectionPosition = listItemCollection.ListItemCollectionPosition;
            } while (listItemCollectionPosition != null);
        }
    }
}
