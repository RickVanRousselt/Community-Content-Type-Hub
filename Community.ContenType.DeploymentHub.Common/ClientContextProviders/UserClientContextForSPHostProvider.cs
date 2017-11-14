using System.Web;
using Community.ContenType.DeploymentHub.Common.ClientContextProviders.ErrorProviders;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Common.Contracts.Exceptions;
using Microsoft.SharePoint.Client;

namespace Community.ContenType.DeploymentHub.Common.ClientContextProviders
{
    /// <summary>
    /// This provider can be used to create a user clientcontext for the hostweb. The provider can only be used in webapplications
    /// </summary>
    public class UserClientContextForSPHostProvider : IClientContextProvider
    {
        private ICreateSharePointContextErrorProvider _createClientContextErrorProvider;

        public UserClientContextForSPHostProvider()
        {
            _createClientContextErrorProvider = new CreateSharePointContextErrorProvider();
        }

        public UserClientContextForSPHostProvider(ICreateSharePointContextErrorProvider createClientContextErrorProvider)
        {
            _createClientContextErrorProvider = createClientContextErrorProvider;
        }

        /// <summary>
        /// This method creates a clientcontext to communicate to SharePoint.
        /// </summary>
        /// <exception cref="CreateSharePointContextException">
        /// When the SharePoint context cannot be created, this exception will be raised. In the message you will find the reasons why it cannot be created
        /// </exception>
        /// <returns></returns>
        public ClientContext CreateClientContext()
        {
            var spContext = GeneratedSharePointArtifacts.SharePointContextProvider.Current.GetSharePointContext(HttpContext.Current);

            if (spContext == null)
            {
                var createSharePointContextErrors = _createClientContextErrorProvider.GetErrors(HttpContext.Current);
                throw new CreateSharePointContextException(createSharePointContextErrors);
            }

            return spContext.CreateUserClientContextForSPHost();
        }
    }
}
