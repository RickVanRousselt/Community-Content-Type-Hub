using System;
using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain;
using Community.ContenType.DeploymentHub.Domain.Core;
using log4net;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using Community.ContenType.DeploymentHub.Contracts.Repositories;

namespace Community.ContenType.DeploymentHub.SharePoint.Repositories
{
    public class ContentTypeGroupingListRepository : ListRepository, IContentTypeGroupingListRepository
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ContentTypeGroupingListRepository));

        private readonly IContentTypeRepository _contentTypeRepository;

        private const string DeploymentGroupInternalName = "Group";
        private const string DeploymentGroupDisplayName = "Deployment Group";
        private const string DescriptionInternalName = "Description";
        private const string DescriptionDisplayName = "Description";

        public ContentTypeGroupingListRepository(IClientContextProvider clientContextProvider, IContentTypeRepository contentTypeRepository)
            : base(clientContextProvider)
        {
            _contentTypeRepository = contentTypeRepository;
        }

        public void EnsureContentTypeGroupingList(Guid deploymentGroupsTermSetId)
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var web = clientContext.Site.RootWeb;

                if (!ListExists(web, Lists.ContentTypeGrouping))
                {
                    var list = CreateGenericList(web, Lists.ContentTypeGrouping, "Content Type Grouping List of Community Content Type Hub");
                    RenameField(list, "Title", "Content Type Name");
                    AddSingleTaxonomyField(list, DeploymentGroupInternalName, DeploymentGroupDisplayName, deploymentGroupsTermSetId, true);
                    AddField(list, DescriptionInternalName, DescriptionDisplayName, "Note", false, false);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
            }
        }

        public IEnumerable<KeyValuePair<DeploymentGroup, ContentTypeInfo>> GetContentTypeGroupPairs()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var web = clientContext.Site.RootWeb;
                var siteCollectionList = web.Lists.GetByTitle(Lists.ContentTypeGrouping);
                var allItems = GetItemsSafe(siteCollectionList, items => items.Include(x => x.Client_Title, x => x[DeploymentGroupInternalName]));
                var allContentTypes = _contentTypeRepository.GetContentTypeInfos();

                foreach (var item in allItems)
                {
                    var deploymentGroupValues = item[DeploymentGroupInternalName] as TaxonomyFieldValue;
                    var contentTypeId = allContentTypes.FirstOrDefault(x => x.Title == item.Client_Title.Trim())?.Id;
                    if (deploymentGroupValues != null && !string.IsNullOrWhiteSpace(contentTypeId))
                    {
                        var deploymentGroup = new DeploymentGroup(deploymentGroupValues.Label, new Guid(deploymentGroupValues.TermGuid));
                        var ct = new ContentTypeInfo(item.Client_Title.Trim(), contentTypeId);
                        yield return new KeyValuePair<DeploymentGroup, ContentTypeInfo>(deploymentGroup, ct);
                    }
                }
            }
        }
    }
}