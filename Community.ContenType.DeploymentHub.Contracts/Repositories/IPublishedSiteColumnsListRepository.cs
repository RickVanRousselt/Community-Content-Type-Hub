using Community.ContenType.DeploymentHub.Domain;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Contracts.Repositories
{
    public interface IPublishedSiteColumnsListRepository : IPublishedListRepository<PublishedSiteColumn>
    {
        void EnsurePublishedSiteColumnsLibrary();
        void UpdateView();
        PublishedSiteColumn PublishSiteColumn(SiteColumn siteColumn);
        PublishedSiteColumnList GetList();
        bool CheckSiteColumnsForUpdate(SiteColumn siteColumn);
        bool CheckSiteColumnDataType(SiteColumn siteColumn);
    }
}