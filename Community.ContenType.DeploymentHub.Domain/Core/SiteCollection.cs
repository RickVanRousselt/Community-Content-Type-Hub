using System;
using System.Text.RegularExpressions;

namespace Community.ContenType.DeploymentHub.Domain.Core
{
    public class SiteCollection : IEquatable<SiteCollection>
    {
        public Uri Url { get; }

        public SiteCollection(Uri url)
        {
            Url = url;
        }

        public string SafeAlphaNumericName => Regex.Replace(Url.AbsolutePath, @"[^a-zA-Z0-9]+", "");
        public string SafeAlphaNumericNameFullUri => Regex.Replace(Url.AbsoluteUri, @"[^a-zA-Z0-9]+", "");

        public bool Equals(SiteCollection other) => UriIgnoreCaseEqualityComparer.Instance.Equals(Url, other?.Url);

        public override string ToString() => Url.AbsolutePath;

        public override bool Equals(object obj) => obj != null && Equals(obj as SiteCollection);

        public override int GetHashCode() => Url.GetHashCode();
    }
}
