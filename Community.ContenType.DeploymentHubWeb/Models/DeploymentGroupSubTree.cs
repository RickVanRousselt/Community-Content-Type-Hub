using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Community.ContenType.DeploymentHub.Domain.Core;
using Newtonsoft.Json;

namespace Community.ContenType.DeploymentHubWeb.Models
{
    public class DeploymentGroupSubTree
    {
        [JsonProperty(PropertyName = "text")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "id")]
        public Guid TermId { get; private set; }

        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; private set; }

        [JsonProperty(PropertyName = "state")]
        public StateTree State { get; private set; }

        [JsonProperty(PropertyName = "data")]
        public Data ExtraData { get; private set; }

        [JsonProperty(PropertyName = "children")]
        public List<PublishContentTypesSubTree> ContentTypes { get; private set; }

        public DeploymentGroupSubTree(string name, Guid termId, StateTree state, List<PublishContentTypesSubTree> contentTypes, List<Uri> siteCollectionUrls)
        {
            Icon = BuildDefaultImageUrl();
            Name = name;
            TermId = termId;
            ContentTypes = contentTypes;
            ExtraData = new Data(null,siteCollectionUrls);
            State = state;
        }

        public DeploymentGroupSubTree(DeploymentGroup deploymentGroup, StateTree state, List<PublishContentTypesSubTree> contentTypes, IEnumerable<SiteCollection> siteCollections)
            : this(HttpUtility.HtmlEncode(deploymentGroup.Name), deploymentGroup.TermId, state, contentTypes, siteCollections.Select(x => x.Url).ToList()) { }

        public string BuildDefaultImageUrl()
        {
            return "/Content/Images/DeploymentGroup_16x16.png";
        }
    }
}