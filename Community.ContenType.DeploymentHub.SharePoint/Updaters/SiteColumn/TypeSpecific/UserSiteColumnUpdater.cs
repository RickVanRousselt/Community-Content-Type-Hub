using System;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Core;
using log4net;
using Microsoft.SharePoint.Client;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.SharePoint.Updaters.SiteColumn.TypeSpecific
{
    internal class UserSiteColumnUpdater : TypeSpecificSiteColumnUpdater<FieldUser>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UserSiteColumnUpdater));

        public UserSiteColumnUpdater(FieldUser siteColumn, PublishedSiteColumn publishedSiteColumn)
            : base(siteColumn, publishedSiteColumn) { }

        private May<bool> AllowMultipleValues => GetAttributeValue("Mult").Select(ParseBool);
        private May<string> UserSelectionMode => GetAttributeValue("UserSelectionMode");
        private May<int> UserSelectionScope => GetAttributeValue("UserSelectionScope").Select(int.Parse);
        private May<bool> Presence => GetAttributeValue("Presence").Select(ParseBool);

        protected override void UpdateSpecificFields()
        {
            using (Logger.MethodTraceLogger())
            {
                UserSelectionMode.IfHasValueThenDo(
                    x =>
                        SiteColumn.SelectionMode =
                            (FieldUserSelectionMode)Enum.Parse(typeof(FieldUserSelectionMode), x));
                UserSelectionScope.IfHasValueThenDo(x => SiteColumn.SelectionGroup = x);
                Presence.IfHasValueThenDo(x => SiteColumn.Presence = x);
                AllowMultipleValues.IfHasValueThenDo(x => SiteColumn.AllowMultipleValues = x);
                //Show options are not available in CSOM
            }
        }
    }
}