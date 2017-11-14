using System;
using System.Web;
using Community.ContenType.DeploymentHub.Domain.Core;
using Newtonsoft.Json;

namespace Community.ContenType.DeploymentHubWeb.Models
{
    public class PublishContentTypesSubTree
    {
        [JsonProperty(PropertyName = "text")]
        public string Title { get; private set; }

        public string Id { get; private set; }

        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; private set; }

        [JsonProperty(PropertyName = "data")]
        public Data ExtraData { get; private set; }

        public PublishContentTypesSubTree(string title, string id)
        {
            Title = title;
            Id = id;
            ExtraData = new Data(id, null);
            Icon = BuildDefaultImageUrl();
        }

        public PublishContentTypesSubTree(ContentTypeInfo ct)
            : this(HttpUtility.HtmlEncode(ct.Title), HttpUtility.HtmlEncode(ct.Id)) { }

        public PublishContentTypesSubTree(ContentTypeInfo ct, PublishedContentType pct)
        {
            Title = pct != null ? BuildTitleField(ct.Title, pct.Version, pct.LastPublished) : HttpUtility.HtmlEncode($"{ct.Title} - No Version - Never Published");

            Id = HttpUtility.HtmlEncode(ct.Id);
            ExtraData = new Data(HttpUtility.HtmlEncode(ct.Id), null);
            Icon = BuildDefaultImageUrl();
        }

        private static string BuildTitleField(string title, int version, DateTime lastPublished) => 
            HttpUtility.HtmlEncode($"{title} - Version: {version} - Last Published on: {lastPublished}");

        private static string BuildDefaultImageUrl() => "/Content/Images/ContentType_16x16.png";
    }
}
