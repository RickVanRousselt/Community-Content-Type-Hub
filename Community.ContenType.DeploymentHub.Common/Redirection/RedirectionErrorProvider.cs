using System;
using System.Collections.Generic;
using System.Web;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Microsoft.IdentityModel.Tokens;

namespace Community.ContenType.DeploymentHub.Common.Redirection
{
    public class RedirectionErrorProvider : IRedirectionErrorProvider
    {
        /// <summary>
        /// test stijn3
        /// </summary>
        /// <param name="httpContext"></param>
        public List<string> GetRedirectionErrors(HttpContextBase httpContext)
        {
            List<string> redirectionErrors = new List<string>();

            try
            {
                CheckHttpContextNotNull(httpContext, redirectionErrors);
                var sharePointContextExists = CheckSecurityTokenExpired(httpContext, redirectionErrors);
                if (!sharePointContextExists)
                {
                    CheckSpHostUrlPresent(httpContext, redirectionErrors);
                    CheckRequestPost(httpContext, redirectionErrors);
                }
            }
            catch (Exception exception)
            {
                redirectionErrors.Add($"Unexpected error in RedirectionChecker. Exeption: {exception.ToString()}");
            }

            return redirectionErrors;
        }

        protected virtual void CheckHttpContextNotNull(HttpContextBase httpContext, List<string> redirectionErrors)
        {
            if (httpContext == null)
            {
                redirectionErrors.Add("httpContext is null");
            }
        }

        protected virtual bool CheckSecurityTokenExpired(HttpContextBase httpContext, List<string> redirectionErrors)
        {
            bool contextTokenExpired = false;

            GeneratedSharePointArtifacts.SharePointContext sharePointContext = null;
            try
            {
                sharePointContext = GeneratedSharePointArtifacts.SharePointContextProvider.Current.GetSharePointContext(httpContext);
            }
            catch (SecurityTokenExpiredException)
            {
                contextTokenExpired = true;
            }

            const string SPHasRedirectedToSharePointKey = "SPHasRedirectedToSharePoint";

            if (!string.IsNullOrEmpty(httpContext.Request.QueryString[SPHasRedirectedToSharePointKey]) && !contextTokenExpired)
            {
                redirectionErrors.Add($"Context token expired & the querystring contains {SPHasRedirectedToSharePointKey} parameter");
            }

            return sharePointContext != null;
        }

        protected virtual void CheckSpHostUrlPresent(HttpContextBase httpContext, List<string> redirectionErrors)
        {
            Uri spHostUrl = GeneratedSharePointArtifacts.SharePointContext.GetSPHostUrl(httpContext.Request);

            if (spHostUrl == null)
            {
                redirectionErrors.Add("SPHostUrl is not present in the querystring");
            }
        }

        protected virtual void CheckRequestPost(HttpContextBase httpContext, List<string> redirectionErrors)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(httpContext.Request.HttpMethod, "POST"))
            {
                redirectionErrors.Add("The call was made using POST instead of GET. Redirect to SharePoint is not possible.Use GET instead.");
            }
        }
    }
}
