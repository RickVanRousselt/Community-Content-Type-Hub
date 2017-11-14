using System;
using System.Xml.Linq;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.Domain.Core
{
    public class PublishedSiteColumn : SiteColumnInfo
    {
        public DateTime LastPublished { get; }
        public int Version { get; }
        public XDocument Schema { get; }
        public XDocument SchemaWithoutVersion
        {
            get
            {
                var clone = XDocument.Parse(Schema.ToString());
                clone.Root?.Attribute("Version")?.Remove();
                return clone;
            }
        }

        public PublishedSiteColumn(string name, Guid id, DateTime lastPublished, XDocument schema, int version)
            : base(name, id)
        {
            LastPublished = lastPublished;
            Schema = schema;
            Version = version;
        }

        public override string ToString() => base.ToString() + $"with version={Version}";
    }

    public class PublishedTaxonomySiteColumn : PublishedSiteColumn
    {
        public TermPath Path { get; }
        public May<TermPath> DefaultValuePath { get; }

        public PublishedTaxonomySiteColumn(string name, Guid id, DateTime lastPublished, XDocument schema, int version, TermPath path, May<TermPath> defaultValuePath) 
            : base(name, id, lastPublished, schema, version)
        {
            Path = path;
            DefaultValuePath = defaultValuePath;
        }
    }
}