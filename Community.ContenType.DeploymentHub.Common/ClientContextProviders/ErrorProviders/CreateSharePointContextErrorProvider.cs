using System;
using System.Collections.Generic;
using System.Web;
using Community.ContenType.DeploymentHub.Common.Contracts;

namespace Community.ContenType.DeploymentHub.Common.ClientContextProviders.ErrorProviders
{
    public class CreateSharePointContextErrorProvider : ICreateSharePointContextErrorProvider
    {
        /// <summary>
        /// If a SharePoint context cannot be created. this method will return the reasons why.
        /// </summary>
        /// <param name="httpContext">The httpcontext of the current request</param>
        /// <returns></returns>
        public List<string> GetErrors(HttpContext httpContext)
        {
            var httpContextBase = new HttpContextWrapper(httpContext);
            var createSharePointContextErrors = new List<string>();

            try
            {
                CheckHttpContextNotNull(httpContextBase, createSharePointContextErrors);
                CheckSpHostUrlPresent(httpContextBase, createSharePointContextErrors);

                var spContext = LoadSharePointContextAndValidateType(httpContextBase, createSharePointContextErrors);

                var isValid = ValidateSharePointContext(httpContextBase, spContext, createSharePointContextErrors);

                if (spContext == null || !isValid)
                {
                    CheckSpLanguage(httpContext, createSharePointContextErrors);
                    CheckSpClientTag(httpContext, createSharePointContextErrors);
                    CheckSpProductNumber(httpContext, createSharePointContextErrors);
                    CheckSpAppWeb(httpContext, createSharePointContextErrors);
                    string contextTokenString = CheckContextTokenString(httpContext, createSharePointContextErrors);
                    CheckContextToken(httpContext, contextTokenString, createSharePointContextErrors);
                }
            }
            catch (Exception ex)
            {
                createSharePointContextErrors.Add("ERROR in CreateClientContextErrorProvider. Exception " + ex.ToString());
            }

            return createSharePointContextErrors;
        }


        private void CheckHttpContextNotNull(HttpContextBase httpContext, List<string> createSharePointContextErrors)
        {
            if (httpContext == null)
            {
                createSharePointContextErrors.Add("HttpContext is null");
            }
        }

        private void CheckSpHostUrlPresent(HttpContextWrapper httpContext, List<string> createSharePointContextErrors)
        {
            if (httpContext != null)
            {
                Uri spHostUrl = GeneratedSharePointArtifacts.SharePointContext.GetSPHostUrl(httpContext.Request);
                if (spHostUrl == null)
                {
                    createSharePointContextErrors.Add("SPHostUrl is not present in the querystring");
                }
            }
        }

        private GeneratedSharePointArtifacts.SharePointAcsContext LoadSharePointContextAndValidateType(HttpContextWrapper httpContext, List<string> createSharePointContextErrors)
        {
            if (httpContext != null)
            {
                object spContextObject = httpContext.Session["SPContext"];
                var spContext = spContextObject as GeneratedSharePointArtifacts.SharePointAcsContext;

                if (httpContext.Session["SPContext"] == null)
                {
                    createSharePointContextErrors.Add("SharePoint context not present in Session");
                }

                if (spContextObject != null && spContext == null)
                {
                    createSharePointContextErrors.Add("The SPContext value in Session does not contain a object of expected type SharePointAcsContext");
                }

                return spContext;
            }
            else
            {
                return null;
            }
        }

        private bool ValidateSharePointContext(HttpContextWrapper httpContext, GeneratedSharePointArtifacts.SharePointAcsContext spContext,List<string> createSharePointContextErrors)
        {
            var isValid = true;
            var validationErrors = "The SharePoint context is NOT valid: VALIDATION ERRORS(";

            if (httpContext != null && spContext != null)
            {
                Uri spHostUrl = GeneratedSharePointArtifacts.SharePointContext.GetSPHostUrl(httpContext.Request);
                string contextToken = GeneratedSharePointArtifacts.TokenHelper.GetContextTokenFromRequest(httpContext.Request);
                HttpCookie spCacheKeyCookie = httpContext.Request.Cookies["SPCacheKey"];
                string spCacheKey = spCacheKeyCookie != null ? spCacheKeyCookie.Value : null;

                bool spHostUrlMatches = spHostUrl == spContext.SPHostUrl;
                bool cacheKeyNotNull = !string.IsNullOrEmpty(spContext.CacheKey);
                bool cacheKeyMatches = cacheKeyNotNull && spCacheKey == spContext.CacheKey;
                bool contextTokenNotNull = !string.IsNullOrEmpty(spContext.ContextToken);
                bool contextTokenMatches = contextTokenNotNull && (string.IsNullOrEmpty(contextToken) || contextToken == spContext.ContextToken);

                if (!spHostUrlMatches)
                {
                    isValid = false;
                    validationErrors += $"SpHostUrl does not match! {spHostUrl} <> {spContext.SPHostUrl};";
                }

                if (!cacheKeyNotNull)
                {
                    isValid = false;
                    validationErrors += $"Cache key is null;";
                }

                if (!cacheKeyMatches)
                {
                    isValid = false;
                    validationErrors += $"Cache keys do not match! {spCacheKey} <> {spContext.CacheKey};";
                }

                if (!contextTokenNotNull)
                {
                    isValid = false;
                    validationErrors += $"Context token is null;";
                }

                if (!contextTokenMatches)
                {
                    isValid = false;
                    validationErrors += $"Context tokens do not match {contextToken} <> {spContext.ContextToken};";
                }

                if (!isValid)
                {
                    validationErrors = validationErrors.TrimEnd(';');
                    validationErrors = ")";
                    createSharePointContextErrors.Add(validationErrors);
                }
            }

            return isValid;
        }

        private void CheckSpLanguage(HttpContext httpContext, List<string> createSharePointContextErrors)
        {
            if (httpContext != null)
            { 
                string spLanguage = httpContext.Request.QueryString[GeneratedSharePointArtifacts.SharePointContext.SPLanguageKey];
                if (string.IsNullOrEmpty(spLanguage))
                {
                    createSharePointContextErrors.Add($"{GeneratedSharePointArtifacts.SharePointContext.SPLanguageKey} could not be found in the querystring");
                }
            }
        }

        private void CheckSpClientTag(HttpContext httpContext, List<string> createSharePointContextErrors)
        {
            if (httpContext == null)
            {
                string spClientTag = httpContext.Request.QueryString[GeneratedSharePointArtifacts.SharePointContext.SPClientTagKey];
                if (string.IsNullOrEmpty(spClientTag))
                {
                    createSharePointContextErrors.Add($"{GeneratedSharePointArtifacts.SharePointContext.SPClientTagKey} could not be found in the querystring");
                }
            }
        }

        private void CheckSpProductNumber(HttpContext httpContext, List<string> createSharePointContextErrors)
        {
            if (httpContext == null)
            {
                string spProductNumber = httpContext.Request.QueryString[GeneratedSharePointArtifacts.SharePointContext.SPProductNumberKey];
                if (string.IsNullOrEmpty(spProductNumber))
                {
                    createSharePointContextErrors.Add($"{GeneratedSharePointArtifacts.SharePointContext.SPProductNumberKey} could not be found in the querystring");
                }
            }
        }

        private void CheckSpAppWeb(HttpContext httpContext, List<string> createSharePointContextErrors)
        {
            if (httpContext == null)
            {
                string spProductNumber = httpContext.Request.QueryString[GeneratedSharePointArtifacts.SharePointContext.SPProductNumberKey];
                if (string.IsNullOrEmpty(spProductNumber))
                {
                    createSharePointContextErrors.Add($"{GeneratedSharePointArtifacts.SharePointContext.SPAppWebUrlKey} could not be found in the querystring");
                }
            }
        }

        private string CheckContextTokenString(HttpContext httpContext, List<string> createSharePointContextErrors)
        {
            if (httpContext !=null)
            {
                string contextTokenString = GeneratedSharePointArtifacts.TokenHelper.GetContextTokenFromRequest(httpContext.Request);
                if (string.IsNullOrEmpty(contextTokenString))
                {
                    createSharePointContextErrors.Add("The context token could not be retrieved from the request");
                }
                return contextTokenString;
            }
            return string.Empty;
        }

        private void CheckContextToken(HttpContext httpContext, string contextTokenString, List<string> createSharePointContextErrors)
        {
            if (httpContext != null && !string.IsNullOrEmpty(contextTokenString))
            {
                var contextToken = GeneratedSharePointArtifacts.TokenHelper.ReadAndValidateContextToken(contextTokenString, httpContext.Request.Url.Authority);
                if (contextToken == null)
                {
                    createSharePointContextErrors.Add("We did not recieve a context token from the tokenservice");
                }
            }
        }
    }
}
