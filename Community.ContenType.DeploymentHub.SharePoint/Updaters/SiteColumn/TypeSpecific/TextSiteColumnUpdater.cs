using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Core;
using log4net;
using Microsoft.SharePoint.Client;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.SharePoint.Updaters.SiteColumn.TypeSpecific
{
    internal class TextSiteColumnUpdater : TypeSpecificSiteColumnUpdater<FieldText>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TextSiteColumnUpdater));


        public TextSiteColumnUpdater(FieldText siteColumn, PublishedSiteColumn publishedSiteColumn) 
            : base(siteColumn, publishedSiteColumn) { }

        public May<int> MaxLength => GetAttributeValue("MaxLength").Select(int.Parse);

        protected override void UpdateSpecificFields()
        {
            using (Logger.MethodTraceLogger())
            {
                MaxLength.IfHasValueThenDo(x => SiteColumn.MaxLength = x);
            }
        }
    }
}