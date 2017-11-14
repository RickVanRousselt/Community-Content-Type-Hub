using System.Xml.Linq;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.SharePoint.Updaters
{
    internal abstract class UpdaterBase
    {
        protected readonly XDocument Schema;

        protected UpdaterBase(XDocument schema)
        {
            Schema = schema;
        }
        protected static May<string> GetAttributeValue(XDocument document, string name) => document.Root?.Attribute(name)?.Value.Maybe() ?? May.NoValue;
        protected May<string> GetAttributeValue(string name) => GetAttributeValue(Schema, name);
        protected static bool ParseBool(string b) => b == "TRUE";
    }
}