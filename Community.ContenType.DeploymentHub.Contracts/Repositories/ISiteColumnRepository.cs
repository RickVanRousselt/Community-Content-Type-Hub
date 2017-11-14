using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Core;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.Contracts.Repositories
{
    public interface ISiteColumnRepository
    {
        IEnumerable<SiteColumn> GetColumnsFromContentType(ContentTypeInfo ctToPublish);
        May<SiteColumnInfo> MaybeGetSiteColumn(string title);
    }
}
