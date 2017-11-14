using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.SharePoint.Updaters.SiteColumn
{
    public interface ISiteColumnUpdater
    {
        void Update(PublishedSiteColumn publishedSiteColumn);
    }
}