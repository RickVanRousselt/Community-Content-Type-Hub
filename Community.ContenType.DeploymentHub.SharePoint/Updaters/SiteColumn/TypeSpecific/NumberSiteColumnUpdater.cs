using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Core;
using log4net;
using Microsoft.SharePoint.Client;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.SharePoint.Updaters.SiteColumn.TypeSpecific
{
    internal class NumberSiteColumnUpdater : TypeSpecificSiteColumnUpdater<FieldNumber>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(NumberSiteColumnUpdater));


        public NumberSiteColumnUpdater(FieldNumber siteColumn, PublishedSiteColumn publishedSiteColumn)
            : base(siteColumn, publishedSiteColumn) { }

        private May<int> Min => GetAttributeValue("Min").Select(int.Parse);
        private May<int> Max => GetAttributeValue("Max").Select(int.Parse);
        //private May<int> Decimals => GetAttributeValue("Decimals").Select(int.Parse);
        //private May<int> Percentage => GetAttributeValue("Percentage").Select(int.Parse);

        protected override void UpdateSpecificFields()
        {
            using (Logger.MethodTraceLogger())
            {
                Min.IfHasValueThenDo(x => SiteColumn.MinimumValue = x);
                Max.IfHasValueThenDo(x => SiteColumn.MaximumValue = x);
                //not supported via CSOM:
                //Decimals.IfHasValueThenDo(x => SiteColumn. = x);
                //Percentage.IfHasValueThenDo(x => SiteColumn. = x);
            }
        }
    }
}