using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Security;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Microsoft.SharePoint.Client;

namespace Community.ContenType.DeploymentHub.Common.ClientContextProviders
{
    /// <summary>
    /// This provider can be used to create a clientcontext for O365, using a user and password
    /// </summary>
    public class UserPasswordOnlineClientContextProvider : IClientContextProvider
    {
        private readonly Uri _siteUri;
        private readonly MailAddress _userName;
        private readonly SecureString _password;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="siteUri"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// /// <exception cref="ArgumentNullException">
        /// All parameters are required
        /// </exception>
        public UserPasswordOnlineClientContextProvider(Uri siteUri, MailAddress userName, SecureString password)
        {
            var missingArguments = new List<string>();

            if (siteUri == null)
            {
                throw new ArgumentNullException(nameof(siteUri));
            }
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            
            _siteUri = siteUri;
            _userName = userName;
            _password = password;
        }

        public ClientContext CreateClientContext()
        {
            return new ClientContext(_siteUri)
            {
                Credentials = new SharePointOnlineCredentials(_userName.Address, _password)
            };

        }
    }
}
