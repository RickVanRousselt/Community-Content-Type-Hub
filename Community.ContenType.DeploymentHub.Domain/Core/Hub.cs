using System;

namespace Community.ContenType.DeploymentHub.Domain.Core
{
    public class Hub : SiteCollection
    {
        public Hub(Uri url) : base(url) { }

        public string SafeName => Url.AbsolutePath.Replace('/', '-').ToLowerInvariant();
        public string NoStartSlashName => Url.AbsolutePath.TrimStart('/').ToLowerInvariant();
    }
}