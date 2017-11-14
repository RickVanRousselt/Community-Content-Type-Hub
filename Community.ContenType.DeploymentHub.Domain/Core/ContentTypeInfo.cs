using System;

namespace Community.ContenType.DeploymentHub.Domain.Core
{
    public class ContentTypeInfo : IEquatable<ContentTypeInfo>
    {
        public string Title { get; }
        public string Id { get; }
    
        public ContentTypeInfo(string title, string id)
        {
            Title = title;
            Id = id;
        }

        public bool Equals(ContentTypeInfo other) => other != null && other.Id == Id;

        public override int GetHashCode() => Id.GetHashCode();

        public override bool Equals(object obj) => obj != null && Equals(obj as ContentTypeInfo);

        public override string ToString() => $"[{Id}]{GetType().Name} '{Title}'";
    }
}
