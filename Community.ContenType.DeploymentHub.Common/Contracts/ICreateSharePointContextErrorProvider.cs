using System.Collections.Generic;
using System.Web;

namespace Community.ContenType.DeploymentHub.Common.Contracts
{
    public interface ICreateSharePointContextErrorProvider
    {
        /// <summary>
        /// If a SharePoint context cannot be created, this method will return the reasons why.
        /// </summary>
        /// <param name="httpContext">The httpcontext of the current request</param>
        /// <returns></returns>
        List<string> GetErrors(HttpContext httpContext);
    }
}
