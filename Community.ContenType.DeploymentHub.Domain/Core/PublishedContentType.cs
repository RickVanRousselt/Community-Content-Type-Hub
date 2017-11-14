using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.Domain.Core
{
    public class PublishedContentType : ContentTypeInfo
    {
        public string Filename => Title + ".xml";
        public DateTime LastPublished { get; }
        public XDocument Schema { get; }
        public int Version { get; }

        public PublishedContentType(string name, string id, DateTime lastPublished, XDocument schema, int version)
            : base(name, id)
        {
            LastPublished = lastPublished;
            Schema = schema;
            Version = version;
        }

        public ISet<SiteColumnInfo> ExtractSiteColumnInfos()
        {
            var idElements = Schema.XPathSelectElements("/ContentType/FieldRefs/FieldRef");
            // ReSharper disable PossibleNullReferenceException -> Name and ID are required fields
            return new HashSet<SiteColumnInfo>(idElements.Select(el => new SiteColumnInfo(el.Attribute("Name").Value, new Guid(el.Attribute("ID").Value))));
            // ReSharper restore PossibleNullReferenceException
        }

        public ISet<SiteColumnLink> ExtractSiteColumnLinks()
        {
            var idElements = Schema.XPathSelectElements("/ContentType/FieldRefs/FieldRef");
            return new HashSet<SiteColumnLink>(idElements.Select(el => new SiteColumnLink(
                // ReSharper disable PossibleNullReferenceException - Name and ID are required attributes
                el.Attribute("Name").Value, 
                new Guid(el.Attribute("ID").Value),
                // ReSharper restore PossibleNullReferenceException
                MaybeParseBool(el.Attribute("Required")?.Value),
                MaybeParseBool(el.Attribute("Hidden")?.Value),
                MaybeParseBool(el.Attribute("ReadOnly")?.Value)
            )));
        }

        public override string ToString() => base.ToString() + $"with version={Version}";

        private static May<bool> MaybeParseBool(string b)
        {
            bool result;
            return bool.TryParse(b, out result)
                ? result.Maybe()
                : May.NoValue;
        }
    }
}
