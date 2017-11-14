using System;
using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Domain
{
    public class PublishedSiteColumnList
    {
        private readonly ISet<PublishedSiteColumn> _publishedSiteColumns;

        public PublishedSiteColumnList(ISet<PublishedSiteColumn> publishedSiteColumns)
        {
            _publishedSiteColumns = publishedSiteColumns;
        }

        public ISet<PublishedSiteColumn> GetSiteColumns(ISet<Guid> siteColumnIds)
        {
            return new HashSet<PublishedSiteColumn>(_publishedSiteColumns.Where(publishedContentType => siteColumnIds.Contains(publishedContentType.Id)));
        }
    }
}