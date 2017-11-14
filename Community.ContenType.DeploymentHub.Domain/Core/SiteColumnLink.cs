using System;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.Domain.Core
{
    public class SiteColumnLink : SiteColumnInfo
    {
        public May<bool> Required { get; }
        public May<bool> Hidden { get; }
        public May<bool> ReadOnly { get; }

        public SiteColumnLink(string title, Guid id, May<bool> required, May<bool> hidden, May<bool> readOnly) : base(title, id)
        {
            Required = required;
            Hidden = hidden;
            ReadOnly = readOnly;
        }
    }
}