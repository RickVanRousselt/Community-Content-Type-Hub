using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Core;
using log4net;
using Microsoft.SharePoint.Client;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.SharePoint.Updaters.SiteColumn.TypeSpecific
{
    internal class NoteSiteColumnUpdater : TypeSpecificSiteColumnUpdater<FieldMultiLineText>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(NoteSiteColumnUpdater));


        public NoteSiteColumnUpdater(FieldMultiLineText siteColumn, PublishedSiteColumn publishedSiteColumn) 
            : base(siteColumn, publishedSiteColumn) { }

        private May<int> NumLines => GetAttributeValue("NumLines").Select(int.Parse);
        private May<bool> AppendOnly => GetAttributeValue("AppendOnly").Select(ParseBool);
        private May<bool> RichText => GetAttributeValue("RichText").Select(ParseBool); //Rich Text != HTML?

        protected override void UpdateSpecificFields()
        {
            using (Logger.MethodTraceLogger())
            {
                NumLines.IfHasValueThenDo(x => SiteColumn.NumberOfLines = x);
                AppendOnly.IfHasValueThenDo(x => SiteColumn.AppendOnly = x);
                RichText.IfHasValueThenDo(x => SiteColumn.RichText = x);
            }
        }
    }
}