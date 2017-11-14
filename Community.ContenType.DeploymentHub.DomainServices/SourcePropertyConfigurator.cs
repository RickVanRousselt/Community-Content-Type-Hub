using System;
using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;

namespace Community.ContenType.DeploymentHub.DomainServices
{
    public class SourcePropertyConfigurator : ISourcePropertyConfigurator
    {
        private readonly Func<SiteCollection, IWebPropertyRepository> _webPropertyRepositoryGenerator;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SourcePropertyConfigurator));

        public SourcePropertyConfigurator(Func<SiteCollection, IWebPropertyRepository> webPropertyRepositoryGenerator)
        {
            _webPropertyRepositoryGenerator = webPropertyRepositoryGenerator;
        }

        public void ConfigureSources(ISet<SiteCollection> siteCollections, Hub sourceHub)
        {
            //verifiers will make sure the old value was either empty or the current hub, so no need to check twice
            foreach (var siteCollection in siteCollections)
            {
                try
                {
                    var webPropertyRepository = _webPropertyRepositoryGenerator(siteCollection);
                    webPropertyRepository.SetProperty(Configs.PushSource, sourceHub.Url.AbsoluteUri);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error setting source on target site collection {siteCollection.Url}", ex);
                }
            }
        }
    }
}
