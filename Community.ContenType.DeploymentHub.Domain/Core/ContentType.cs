using System.Xml.Linq;

namespace Community.ContenType.DeploymentHub.Domain.Core
{
    public class ContentType : ContentTypeInfo
    {
        public string DocumentTemplate { get; }
        public string DocumentTemplateUrl { get; }
        public string SpContentTypeGroup { get; }
        public bool Hidden { get; }
        public bool ReadOnly { get; }
        public XDocument Schema { get; }

        public ContentType(string title, string id, string documentTemplate, string spContentTypeGroup, bool hidden, bool readOnly, XDocument schema, string documentTemplateUrl)
            : base(title, id)
        {
            DocumentTemplate = documentTemplate;
            SpContentTypeGroup = spContentTypeGroup;
            Hidden = hidden;
            ReadOnly = readOnly;
            Schema = schema;
            DocumentTemplateUrl = documentTemplateUrl;
        }
    }
}