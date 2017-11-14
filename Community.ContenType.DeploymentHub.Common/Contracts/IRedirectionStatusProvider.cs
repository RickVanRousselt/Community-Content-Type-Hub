using System;
using System.Web;

namespace Community.ContenType.DeploymentHub.Common.Contracts
{

    public interface IRedirectionStatusProvider
    {
        /// <summary>
        /// This method checks is it is needed to redirect to SharePoint to get a access token
        /// </summary>
        /// <param name="context">The http context of the current request</param>
        /// <param name="redirectUrl">This is the url where the application has to be redirected to, to get the token</param>
        /// <returns></returns>
        RedirectionStatusResult CheckRedirectionStatus(HttpContextBase context, out Uri redirectUrl);
    }
}
