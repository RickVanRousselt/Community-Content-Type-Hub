using System;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;

namespace Community.ContenType.DeploymentHub.DomainServices.Provision
{
    public class MainProvisioner : MainProvisionerBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MainProvisioner));

        public MainProvisioner(IWebPropertyRepository webPropertyRepository, IStatusListRepository statusListRepository, ITermStoreRepository termStoreRepository, IConfigurationListRepository configurationListRepository, IContentTypeGroupingListRepository contentTypeGroupingListRepository, ISiteCollectionGroupingListRepository siteCollectionGroupingListRepository, IPublishedSiteColumnsListRepository publishedSiteColumnsListRepository, IPublishedContentTypesListRepository publishedContentTypesListRepository, IUserRepository userRepository, IProvisionEventHub eventHub, IPublishedDocTemplateListRepository docTemplateListRepository) 
            : base(webPropertyRepository)
        {
            using (Logger.MethodTraceLogger())
            {
                SetNextVersionProvisioner(new VersionProvisionerV1_0_0_0(statusListRepository, configurationListRepository, siteCollectionGroupingListRepository, contentTypeGroupingListRepository, publishedSiteColumnsListRepository, publishedContentTypesListRepository, termStoreRepository, userRepository, eventHub));
                SetNextVersionProvisioner(new VersionProvisionerV1_0_0_1(statusListRepository, userRepository, publishedSiteColumnsListRepository, publishedContentTypesListRepository, eventHub));
                SetNextVersionProvisioner(new VersionProvisionerV1_0_0_3(statusListRepository, eventHub, userRepository));
                SetNextVersionProvisioner(new VersionProvisionerV1_0_0_4(docTemplateListRepository, eventHub, userRepository));
            }
        }

        protected override Version GetCurrentVersion()
        {
            using (Logger.MethodTraceLogger())
            {
                Version version = null;

                try
                {
                    version = base.GetCurrentVersion();
                    Logger.Debug("GetCurrentVersion -> " + version);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error getting version", ex);
                }
                return version;
            }
        }
    }
}
