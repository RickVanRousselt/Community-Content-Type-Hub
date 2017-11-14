using System;
using System.Collections.Generic;
using System.Linq;

namespace Community.ContenType.DeploymentHub.Domain.Core
{
    public class TermPath : IEquatable<TermPath>
    {
        private readonly string[] _parts;

        public bool IsTermSet => _parts.Length == 2;
        public int Length => _parts.Length;
        public string TermGroup => _parts[0];
        public string TermSet => _parts[1];

        public Queue<string> Queue => new Queue<string>(_parts);

        public TermPath(params string[] parts)
        {
            if (parts.Length < 2)
            {
                throw new ArgumentException("Path must at least contain a group and a term set.");
            }
            if (parts.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Path must not contain empty parts.");
            }
            _parts = parts;
        }

        public TermPath(string path)
            : this(path.Split(';')) { }

        public override string ToString() => string.Join(";", _parts);

        public bool Equals(TermPath other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _parts.SequenceEqual(other._parts);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TermPath) obj);
        }

        public override int GetHashCode() => _parts?.GetHashCode() ?? 0;
    }
}
