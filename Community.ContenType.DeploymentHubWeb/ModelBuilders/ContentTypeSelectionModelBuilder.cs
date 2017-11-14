using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Domain;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHubWeb.Models;

namespace Community.ContenType.DeploymentHubWeb.ModelBuilders
{
    public class ContentTypeSelectionModelBuilder
    {
        private readonly Groupings _groupings;

        public ContentTypeSelectionModelBuilder(Groupings groupings)
        {
            _groupings = groupings;
        }

        public IEnumerable<DeploymentGroupSubTree> GetDeploymentGroupSubTreesIgnoreSiteCollections(List<PublishedContentType> publishedContentTypes)
        {
            foreach (var grouping in _groupings.GetContentTypesPerDeploymentGroup())
            {
                var deploymentGroup = grouping.Key;
                var contentTypes = grouping.Select(ct => new PublishContentTypesSubTree(ct, publishedContentTypes.FirstOrDefault(x => x.Title == ct.Title))).ToList();
                yield return new DeploymentGroupSubTree(
                    deploymentGroup,
                    new StateTree(false, true),
                    contentTypes,
                    Enumerable.Empty<SiteCollection>());
            }
        }

        public IEnumerable<DeploymentGroupSubTree> GetDeploymentGroupSubTrees(List<PublishedContentType> publishedContentTypes)
        {
            foreach (var grouping in _groupings.GetContentTypesPerDeploymentGroup())
            {
                var deploymentGroup = grouping.Key;
                var contentTypeSubTrees = grouping.Select(ct => new PublishContentTypesSubTree(ct, publishedContentTypes.FirstOrDefault(x => x.Title == ct.Title))).ToList();
                yield return new DeploymentGroupSubTree(
                    deploymentGroup,
                    new StateTree(false, true),
                    contentTypeSubTrees,
                    new List<SiteCollection>());
            }
        }
    }
}