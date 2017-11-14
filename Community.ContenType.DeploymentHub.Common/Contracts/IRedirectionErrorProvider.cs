using System.Collections.Generic;
using System.Web;

namespace Community.ContenType.DeploymentHub.Common.Contracts
{

    public interface IRedirectionErrorProvider
    {
        /// <summary>
        /// If redirection fails, this method will return a list of reasons why.
        /// </summary>
        /// <param name="httpContext">The httpcontext of the current request</param>
        List<string> GetRedirectionErrors(HttpContextBase httpContext);
    }
}
