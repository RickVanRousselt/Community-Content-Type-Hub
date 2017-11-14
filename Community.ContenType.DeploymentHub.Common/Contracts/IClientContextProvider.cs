using Community.ContenType.DeploymentHub.Common.Contracts.Exceptions;
using Microsoft.SharePoint.Client;

namespace Community.ContenType.DeploymentHub.Common.Contracts
{
    public interface IClientContextProvider
    {
        /// <summary>
        /// This method creates a ClientContext to communicate with SharePoint 
        /// </summary>
        /// <exception cref="CreateSharePointContextException">
        /// When the SharePoint context cannot be created, this exception will be raised. In the message you will find the reasons why it cannot be created
        /// </exception>
        /// <returns>ClientContext object for SharePoint</returns>
        ClientContext CreateClientContext();
    }
}
