using System;

namespace Community.ContenType.DeploymentHubWeb.ApiAuthentication
{
    public interface IApiKeyValidator
    {
        ApiKeyValidationResult Validate(Uri hubUrl, Uri siteCollectionUrl, string deploymentGroup, string apiKey);
    }
}
