using System;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.Domain
{
    public static class UriExtensions
    {
        public static May<Uri> MaybeCreateUri(string url)
        {
            try
            {
                return new Uri(url);
            }
            catch (UriFormatException)
            {
                return May.NoValue;
            }
        }
    }
}
