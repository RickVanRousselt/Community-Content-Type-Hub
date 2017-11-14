using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Core;
using log4net;
using Microsoft.SharePoint.Client;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.SharePoint.Updaters.SiteColumn.TypeSpecific
{
    internal class MultiChoiceSiteColumnUpdater : TypeSpecificSiteColumnUpdater<FieldMultiChoice>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MultiChoiceSiteColumnUpdater));


        public MultiChoiceSiteColumnUpdater(FieldMultiChoice siteColumn, PublishedSiteColumn publishedSiteColumn)
            : base(siteColumn, publishedSiteColumn) { }

        private May<bool> FillInChoice => GetAttributeValue("FillInChoice").Select(ParseBool);
        private May<IEnumerable<string>> Choices => Schema.Root?
                                                        .XPathSelectElements("/CHOICES/CHOICE")
                                                        .Select(e => e.Attribute("Value")?.Value ?? "Missing Value")
                                                        .Maybe() ?? May.NoValue;

        protected override void UpdateSpecificFields()
        {
            using (Logger.MethodTraceLogger())
            {
                Choices.IfHasValueThenDo(x => SiteColumn.Choices = x.ToArray());
                FillInChoice.IfHasValueThenDo(x => SiteColumn.FillInChoice = x);
            }
        }
    }
}