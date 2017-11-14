using System;
using System.Collections.Generic;

namespace Community.ContenType.DeploymentHub.Domain
{
    public sealed class UriIgnoreCaseEqualityComparer : EqualityComparer<Uri>
    {
        public static readonly UriIgnoreCaseEqualityComparer Instance = new UriIgnoreCaseEqualityComparer();

        private UriIgnoreCaseEqualityComparer() {}

        public override bool Equals(Uri x, Uri y) => Uri.Compare(x, y, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.InvariantCultureIgnoreCase) == 0;

        public override int GetHashCode(Uri obj) => obj?.GetHashCode() ?? 0;
    }
}