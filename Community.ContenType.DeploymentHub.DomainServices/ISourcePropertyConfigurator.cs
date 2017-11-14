using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.DomainServices
{
    public interface ISourcePropertyConfigurator
    {
        void ConfigureSources(ISet<SiteCollection> siteCollections, Hub sourceHub);
    }
}