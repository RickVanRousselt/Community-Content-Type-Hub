using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.SharePoint.Repositories
{
    public class SiteColumnRepository : ISiteColumnRepository
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SiteColumnRepository));

        private readonly IClientContextProvider _clientContextProvider;
        private readonly ITermStoreRepository _termStoreRepository;

        public SiteColumnRepository(IClientContextProvider clientContextProvider, ITermStoreRepository termStoreRepository)
        {
            _clientContextProvider = clientContextProvider;
            _termStoreRepository = termStoreRepository;
        }

        public IEnumerable<SiteColumn> GetColumnsFromContentType(ContentTypeInfo ctToPublish)
        {
            using (Logger.MethodTraceLogger(ctToPublish))
            using (var clientContext = _clientContextProvider.CreateClientContext())
            {
                var contentType = clientContext.Site.RootWeb.ContentTypes.GetById(ctToPublish.Id);
                clientContext.Load(contentType, ct => ct.Fields.Include(f => f.Id,
                    f => f.Title,
                    f => f.InternalName,
                    f => f.StaticName,
                    f => f.CanBeDeleted,
                    f => f.DefaultValue,
                    f => f.Description,
                    f => f.EnforceUniqueValues,
                    f => f.FieldTypeKind,
                    f => f.TypeAsString,
                    f => f.Group,
                    f => f.Hidden,
                    f => f.Indexed,
                    f => f.SchemaXml,
                    f => f.ReadOnlyField,
                    f => f.Required));
                clientContext.ExecuteQueryWithIncrementalRetry();

                var siteColumsToProcess = contentType.Fields.Where(f => !CanSiteColumnBeIgnored(f));
                foreach (var field in siteColumsToProcess)
                {
                    var taxonomyField = field as TaxonomyField;
                    if (taxonomyField != null)
                    {
                        clientContext.Load(taxonomyField, tx => tx.TermSetId, tx => tx.AnchorId, tx => tx.SspId);
                        clientContext.ExecuteQueryWithIncrementalRetry();

                        var defaultValuePath = GetDefaultValue(field, taxonomyField);

                        var termIdentifier = TermIdentifier.FromTaxonomyField(taxonomyField.TermSetId, taxonomyField.AnchorId);
                        var maybePath = _termStoreRepository.MaybeGetTermPathToTermIdentifier(termIdentifier);
                        var path = maybePath.Match(x => x, () => { throw new InvalidOperationException($"Path of {termIdentifier} could not be found. Term linked to deleted term\termset"); });
                        
                        var fieldSchema = XDocument.Parse(field.SchemaXml);
                        fieldSchema.Root?.Attribute("WebId")?.Remove();
                        fieldSchema.Root?.Attribute("List")?.Remove();
                        fieldSchema.Root?.Attribute("SourceID")?.Remove();

                        yield return new TaxonomySiteColumn(field.Id, field.Title, field.InternalName, field.StaticName,
                            field.CanBeDeleted, field.DefaultValue, field.Description, field.EnforceUniqueValues,
                            (SiteColumn.FieldTypeKind) field.FieldTypeKind, field.TypeAsString, field.Group,
                            field.Hidden, field.Indexed, fieldSchema, field.ReadOnlyField,
                            field.Required, path, defaultValuePath);
                    }
                    else
                    {
                        yield return new SiteColumn(field.Id, field.Title, field.InternalName, field.StaticName,
                            field.CanBeDeleted, field.DefaultValue, field.Description, field.EnforceUniqueValues,
                            (SiteColumn.FieldTypeKind) field.FieldTypeKind, field.TypeAsString, field.Group,
                            field.Hidden, field.Indexed, XDocument.Parse(field.SchemaXml), field.ReadOnlyField,
                            field.Required);
                    }
                }
            }
        }

        private static bool CanSiteColumnBeIgnored(Field field) => field.InternalName.StartsWith("_") ||
                                                                                            field.InternalName.StartsWith("ows") ||
                                                                                            field.Group.StartsWith("_Hidden") ||
                                                                                            field.InternalName == "TaxCatchAll" ||
                                                                                            field.InternalName == "TaxCatchAllLabel" ||
                                                                                            field.Title.EndsWith("_0");

        private May<TermPath> GetDefaultValue(Field field, TaxonomyField taxonomyField)
        {
            if (!string.IsNullOrWhiteSpace(field.DefaultValue))
            {
                var defaultTermId = new Guid(field.DefaultValue.Split('|')[1]);
                var defaultTermIdentifier = TermIdentifier.FromTerm(taxonomyField.TermSetId, defaultTermId);
                var maybeDefaultValuePath = _termStoreRepository.MaybeGetTermPathToTermIdentifier(defaultTermIdentifier);
                if(!maybeDefaultValuePath.HasValue) Logger.Debug("Taxonomy field default value could not be found.");
                return maybeDefaultValuePath;
            }
            return May.NoValue;
        }

        public May<SiteColumnInfo> MaybeGetSiteColumn(string title)
        {
            using (Logger.MethodTraceLogger(title))
            using (var clientContext = _clientContextProvider.CreateClientContext())
            {
                try
                {
                    var siteColumns = clientContext.Site.RootWeb.Fields;
                    var returnedCol = siteColumns.GetByInternalNameOrTitle(title);
                    clientContext.Load(returnedCol, col => col.Id);
                    clientContext.ExecuteQueryWithIncrementalRetry();

                    return new SiteColumnInfo(title, returnedCol.Id);
                }
                catch (ServerException e)
                {
                    if (e.ServerErrorTypeName == "System.ArgumentException")
                    {
                        Logger.Debug($"Requested Site Column '{title}' does not exist in {clientContext.Url} : {e}");
                        
                    }
                    Logger.Error(e);
                    return May.NoValue;
                }
                catch (Exception ex)
                {
                    Logger.Debug($"Requested Site Column '{title}' does not exist in {clientContext.Url} : {ex}");
                    return May.NoValue;
                }
            }
        }
    }
}