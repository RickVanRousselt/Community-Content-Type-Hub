using System;
using System.Collections.Generic;
using System.Web;
using Community.ContenType.DeploymentHub.Common.Contracts;

namespace Community.ContenType.DeploymentHub.Common.Redirection
{
    public class RedirectionStatusProvider : IRedirectionStatusProvider
    {
        private readonly IRedirectionErrorProvider _redirectionChecker;

        public RedirectionStatusProvider(IRedirectionErrorProvider redirectionChecker)
        {
            _redirectionChecker = redirectionChecker;
        }

        public RedirectionStatusProvider() : this(new RedirectionErrorProvider())
        {
        }

        public RedirectionStatusResult CheckRedirectionStatus(HttpContextBase context, out Uri redirectUrl)
        {
            var spRedirectionStatus = GeneratedSharePointArtifacts.SharePointContextProvider.CheckRedirectionStatus(context, out redirectUrl);

            List<string> redirectionErrors = null;
            Contracts.RedirectionStatus redirectionStatus = Contracts.RedirectionStatus.CanNotRedirect;
            switch (spRedirectionStatus)
            {
                case Community.ContenType.DeploymentHub.Common.GeneratedSharePointArtifacts.RedirectionStatus.Ok:
                    redirectionStatus = Contracts.RedirectionStatus.Ok;
                    break;
                case Community.ContenType.DeploymentHub.Common.GeneratedSharePointArtifacts.RedirectionStatus.ShouldRedirect:
                    redirectionStatus = Contracts.RedirectionStatus.ShouldRedirect;
                    break;
                case Community.ContenType.DeploymentHub.Common.GeneratedSharePointArtifacts.RedirectionStatus.CanNotRedirect:
                    redirectionStatus = Contracts.RedirectionStatus.CanNotRedirect;
                    redirectionErrors = _redirectionChecker.GetRedirectionErrors(context);
                    break;
                default:
                    throw new NotSupportedException("Invalid redirection status");
            }

            return new RedirectionStatusResult(redirectionStatus, redirectionErrors);
        }
    }
}
