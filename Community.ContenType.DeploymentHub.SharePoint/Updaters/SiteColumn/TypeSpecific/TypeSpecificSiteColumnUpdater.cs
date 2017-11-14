using System;
using Community.ContenType.DeploymentHub.Domain.Core;
using Microsoft.SharePoint.Client;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.SharePoint.Updaters.SiteColumn.TypeSpecific
{
    //https://msdn.microsoft.com/en-us/library/office/aa979575.aspx
    internal abstract class TypeSpecificSiteColumnUpdater<TField> : UpdaterBase, ITypeSpecificSiteColumnUpdater where TField : Field
    {
        protected readonly TField SiteColumn;

        private May<string> Description => GetAttributeValue("Description");
        private May<string> Group => GetAttributeValue("Group");
        private May<bool> EnforceUniqueValues => GetAttributeValue("EnforceUniqueValues").Select(ParseBool);
        protected May<string> DefaultValue => Schema.Root?.Elements("Default").MayFirst().Select(n => n.Value) ?? May.NoValue;

        protected TypeSpecificSiteColumnUpdater(TField siteColumn, PublishedSiteColumn publishedSiteColumn)
            : base(publishedSiteColumn.Schema)
        {
            if (siteColumn == null)
            {
                throw new ArgumentException($"Type '{GetType(publishedSiteColumn)}' of {publishedSiteColumn} does not match type on target Site Collection.");
            }
            SiteColumn = siteColumn;
        }

        internal static FieldType GetType(PublishedSiteColumn siteColumn)
        {
            // Note: this method will return FieldType.Error for PublishedTaxonomySiteColumn
            // because real type "TaxonomyFieldType(Multi)" is not present in the FieldType enum
            var type = GetAttributeValue(siteColumn.Schema, "Type").Else("Error");
            FieldType result;
            return Enum.TryParse(type, out result)
                ? result
                : FieldType.Error; //fieldtype 'Error' is not in supported list, so will simply not be updated
        }

        public void Update(bool taxonomyOnly)
        {
            if (!taxonomyOnly)
            {
                UpdateBaseFields();
            }
            UpdateDefaultValue();
            UpdateSpecificFields();
            SiteColumn.UpdateAndPushChanges(true);
            SiteColumn.Context.ExecuteQueryWithIncrementalRetry(); //make sure Update() is called before the context is disposed!
        }

        private void UpdateBaseFields()
        {
            Group.IfHasValueThenDo(b => SiteColumn.Group = b);
            Description.IfHasValueThenDo(b => SiteColumn.Description = b);
            EnforceUniqueValues.IfHasValueThenDo(b => SiteColumn.EnforceUniqueValues = b);
        }

        protected virtual void UpdateDefaultValue()
        {
            DefaultValue.IfHasValueThenDo(b => SiteColumn.DefaultValue = b);
        }

        protected abstract void UpdateSpecificFields();
    }
}