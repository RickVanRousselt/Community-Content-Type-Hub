using System;

namespace Community.ContenType.DeploymentHub.Domain.Core
{
    public class SiteColumnInfo : IEquatable<SiteColumnInfo>
    {
        public string Title { get; }
        public Guid Id { get; }

        public SiteColumnInfo(string title, Guid id)
        {
            Title = title;
            Id = id;
        }

        public bool Equals(SiteColumnInfo other) => other != null && other.Id == Id;

        public override int GetHashCode() => Id.GetHashCode();

        public override bool Equals(object obj) => obj != null && Equals(obj as SiteColumnInfo);

        public override string ToString() => $"[{Id}]{GetType().Name} '{Title}'";
    }
}