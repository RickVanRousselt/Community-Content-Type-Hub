using System;
using System.Configuration;
using System.Linq;
using Community.ContenType.DeploymentHub.Common;
using log4net;

namespace Community.ContenType.DeploymentHubWeb.ApiAuthentication
{
    public class ApiKeyValidator : IApiKeyValidator
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ApiKeyValidator));

        public ApiKeyValidationResult Validate(Uri hubUrl, Uri siteCollectionUrl, string deploymentGroup, string apiKey)
        {
            using (Logger.MethodTraceLogger(hubUrl, siteCollectionUrl, deploymentGroup, apiKey))
            {
                try
                {
                    var apiKeyRegistrationString = ConfigurationManager.AppSettings.Get($"api:{apiKey}");
                    if (string.IsNullOrEmpty(apiKeyRegistrationString))
                    {
                        return new ApiKeyValidationResult(false, "No registration found for that apiKey");
                    }

                    var apiKeyRegistration = new ApiKeyRegistration(apiKeyRegistrationString);

                    var hub = hubUrl.AbsoluteUri.ToLower().TrimEnd('/').Split('/').Last();
                    if (!apiKeyRegistration.AllowAllHubs && apiKeyRegistration.AllowedHubs.All(h => h != hub))
                    {
                        return new ApiKeyValidationResult(false, "Hub is not allowed for that apiKey");
                    }

                    var siteCollection = siteCollectionUrl.AbsoluteUri.ToLower().TrimEnd('/').Split('/').Last();
                    if (!apiKeyRegistration.AllowAllSiteCollections && apiKeyRegistration.AllowedSiteCollections.All(s => s.ToLower() != siteCollection))
                    {
                        return new ApiKeyValidationResult(false, "SiteCollection is not allowed for that apiKey");
                    }

                    if (!apiKeyRegistration.AllowAllDeploymentGroups && apiKeyRegistration.AllowedDeploymentGroups.All(d => d != deploymentGroup.ToLower()))
                    {
                        return new ApiKeyValidationResult(false, "DeploymentGroup is not allowed for that apiKey");
                    }

                    return new ApiKeyValidationResult(true, string.Empty);
                }
                catch (Exception ex)
                {
                    Logger.Error("Unexpected error while validating apiKey", ex);
                    return new ApiKeyValidationResult(false, "Unexpected error while validating apiKey");
                }
            }
        }
    }
}