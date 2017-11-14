using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Common;
using log4net;

namespace Community.ContenType.DeploymentHubWeb.ApiAuthentication
{
    public class ApiKeyRegistration
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ApiKeyRegistration));

        public bool AllowAllHubs { get; }
        public List<string> AllowedHubs { get; } = new List<string>();
        public List<string> AllowedSiteCollections { get; } = new List<string>();
        public List<string> AllowedDeploymentGroups { get; } = new List<string>();
        public bool AllowAllSiteCollections { get; }
        public bool AllowAllDeploymentGroups { get; }

        public ApiKeyRegistration(string registrationString)
        {
            using (Logger.MethodTraceLogger(registrationString))
            {
                var hubs = registrationString.Split(';').First().Split('=').Last().Split(',');
                AllowAllHubs = hubs.Length == 1 && hubs.FirstOrDefault() == "*";
                if (!AllowAllHubs)
                {
                    AllowedHubs.AddRange(hubs);
                }

                var siteCollections = registrationString.Split(';')[1].Split('=').Last().Split(',');
                AllowAllSiteCollections = siteCollections.Length == 1 && siteCollections.FirstOrDefault() == "*";
                if (!AllowAllSiteCollections)
                {
                    AllowedSiteCollections.AddRange(siteCollections);
                }

                var deploymentGroups = registrationString.Split(';').Last().Split('=').Last().Split(',');
                AllowAllDeploymentGroups = deploymentGroups.Length == 1 && deploymentGroups.FirstOrDefault() == "*";
                if (!AllowAllDeploymentGroups)
                {
                    AllowedDeploymentGroups.AddRange(deploymentGroups);
                }
            }
        }
    }
}