using System;
using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;

namespace Community.ContenType.DeploymentHub.SharePoint.Repositories
{
    public class SiteCollectionGroupingListRepository : ListRepository, ISiteCollectionGroupingListRepository
    {
        public SiteCollectionGroupingListRepository(IClientContextProvider clientContextProvider)
            : base(clientContextProvider) { }

        private static readonly ILog Logger = LogManager.GetLogger(typeof(SiteCollectionGroupingListRepository));

        private const string DeploymentGroupInternalName = "Group";
        private const string DeploymentGroupDisplayName = "Deployment Group";
        private const string DescriptionInternalName = "Description";
        private const string DescriptionDisplayName = "Description";

        public void EnsureSiteCollectionGroupingList(Guid deploymentGroupsTermSetId)
        {
            using (Logger.MethodTraceLogger(deploymentGroupsTermSetId))
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var web = clientContext.Site.RootWeb;

                if (!ListExists(web, Lists.SiteCollectionGrouping))
                {
                    var list = CreateGenericList(web, Lists.SiteCollectionGrouping,
                        "Site Collection Grouping List of Community Content Type Hub");
                    RenameField(list, "Title", "Site Collection URL");
                    IndexField(list, "Title");
                    MakeFieldUnique(list, "Title");
                    AddSingleTaxonomyField(list, DeploymentGroupInternalName, DeploymentGroupDisplayName,
                        deploymentGroupsTermSetId, true);
                    AddField(list, DescriptionInternalName, DescriptionDisplayName, "Note", false, false);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
            }

        }

        public IEnumerable<KeyValuePair<DeploymentGroup, SiteCollection>> GetSiteCollectionGroupPairs()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {

                var web = clientContext.Site.RootWeb;
                var siteCollectionList = web.Lists.GetByTitle(Lists.SiteCollectionGrouping);
                var allItems = GetItemsSafe(siteCollectionList, items => items.Include(x => x.Client_Title, x => x[DeploymentGroupInternalName]));

                foreach (var item in allItems)
                {
                    var deploymentGroupValues = item[DeploymentGroupInternalName] as TaxonomyFieldValue;
                    var siteCollectionUrl = item.Client_Title.Trim();
                    if (deploymentGroupValues != null && !string.IsNullOrWhiteSpace(siteCollectionUrl))
                    {
                        var deploymentGroup = new DeploymentGroup(deploymentGroupValues.Label, new Guid(deploymentGroupValues.TermGuid));
                        var url = UriExtensions
                            .MaybeCreateUri(siteCollectionUrl)
                            .Match(x => x, () => { throw new UriFormatException("Failed to parse URL " + siteCollectionUrl); });
                        yield return new KeyValuePair<DeploymentGroup, SiteCollection>(deploymentGroup, new SiteCollection(url));
                    }
                    else
                    {
                        Logger.Debug("Skipped a row in the Site Collection Grouping list because the site collection value was blank.");
                    }
                }
            }
        }

        public void AddSiteCollectionToDeploymentGroup(DeploymentGroup deploymentGroup, Uri siteCollectionUrl)
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var taxonomySession = TaxonomySession.GetTaxonomySession(clientContext).GetDefaultSiteCollectionTermStore();
                var term = taxonomySession.GetTerm(deploymentGroup.TermId);

                if (term != null)
                {
                    var web = clientContext.Site.RootWeb;
                    var siteCollectionList = web.Lists.GetByTitle(Lists.SiteCollectionGrouping);
                    var itemCreateInfo = new ListItemCreationInformation();
                    var newItem = siteCollectionList.AddItem(itemCreateInfo);
                    newItem["Title"] = siteCollectionUrl.AbsoluteUri;

                    var field = siteCollectionList.Fields.GetByInternalNameOrTitle(DeploymentGroupInternalName);
                    var txField = clientContext.CastTo<TaxonomyField>(field);
                    txField.SetFieldValueByTerm(newItem, term, 1033);

                    newItem.Update();

                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
                else
                {
                    throw new ApplicationException($"Deploymentgroup cannot be found. id={deploymentGroup.TermId}");
                }
            }
        }
    }
}