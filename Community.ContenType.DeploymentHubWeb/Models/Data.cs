using System;
using System.Collections.Generic;

namespace Community.ContenType.DeploymentHubWeb.Models
{
    public class Data
    {
        public string ContentTypeId { get; }

        public List<Uri> SiteCollectionUrls { get; }

        public Data(string contentTypeId, List<Uri> siteCollections)
        {
            SiteCollectionUrls = siteCollections;
            ContentTypeId = contentTypeId;
        }
     
    }
}