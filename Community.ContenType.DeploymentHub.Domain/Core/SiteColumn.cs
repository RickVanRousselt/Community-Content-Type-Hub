using System;
using System.Xml.Linq;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.Domain.Core
{
    public class SiteColumn
    {
        public enum FieldTypeKind
        {
            Invalid = 0,
            Integer = 1,
            Text = 2,
            Note = 3,
            DateTime = 4,
            Counter = 5,
            Choice = 6,
            Lookup = 7,
            Boolean = 8,
            Number = 9,
            Currency = 10,
            URL = 11,
            Computed = 12,
            Threading = 13,
            Guid = 14,
            MultiChoice = 15,
            GridChoice = 16,
            Calculated = 17,
            File = 18,
            Attachments = 19,
            User = 20,
            Recurrence = 21,
            CrossProjectLink = 22,
            ModStat = 23,
            Error = 24,
            ContentTypeId = 25,
            PageSeparator = 26,
            ThreadIndex = 27,
            WorkflowStatus = 28,
            AllDayEvent = 29,
            WorkflowEventType = 30,
            GeoLocation = 31,
            OutcomeChoice = 32,
            MaxItems = 33
        };

        public Guid Id { get; }
        public string Name { get; }
        public string InternalName { get; }
        public string StaticName { get; }
        public bool CanBeDeleted { get; }
        public string DefaultValue { get; }
        public string Description { get; }
        public bool EnforceUniqueValues { get; }
        public FieldTypeKind FldTypeKind { get; }
        public string TypeAsString { get; }
        public string Group { get; }
        public bool Hidden { get; }
        public bool Indexed { get; }
        public XDocument Schema { get; }
        public bool ReadOnly { get; }
        public bool Required { get; }


        public SiteColumn(Guid id, string name, string internalName, string staticName, bool canBeDeleted, string defaultValue, string description, bool enforceUniqueValues, FieldTypeKind fldTypeKind, string typeAsString, string @group, bool hidden, bool indexed, XDocument schema, bool readOnly, bool required)
        {
            Id = id;
            Name = name;
            InternalName = internalName;
            StaticName = staticName;
            CanBeDeleted = canBeDeleted;
            DefaultValue = defaultValue;
            Description = description;
            EnforceUniqueValues = enforceUniqueValues;
            FldTypeKind = fldTypeKind;
            TypeAsString = typeAsString;
            Group = group;
            Hidden = hidden;
            Indexed = indexed;
            Schema = schema;
            ReadOnly = readOnly;
            Required = required;
        }

        protected bool Equals(SiteColumn other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SiteColumn) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class TaxonomySiteColumn : SiteColumn
    {
        public TermPath Path { get; }
        public May<TermPath> DefaultValuePath { get; }

        public TaxonomySiteColumn(Guid id, string name, string internalName, string staticName, bool canBeDeleted, string defaultValue, string description, bool enforceUniqueValues, FieldTypeKind fldTypeKind, string typeAsString, string @group, bool hidden, bool indexed, XDocument schema, bool readOnly, bool required, TermPath path, May<TermPath> defaultValuePath) 
            : base(id, name, internalName, staticName, canBeDeleted, defaultValue, description, enforceUniqueValues, fldTypeKind, typeAsString, group, hidden, indexed, schema, readOnly, required)
        {
            Path = path;
            DefaultValuePath = defaultValuePath;
        }
    }
}

