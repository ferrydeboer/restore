using System.Collections.Generic;

namespace Restore.Tests
{
    public class IntIdentifier : IIdentifier
    {
        public IntIdentifier(int id)
        {
            Id = id;
        }

        public static implicit operator int(IntIdentifier identifier)
        {
            return identifier.Id;
        }

        public static implicit operator IntIdentifier(int id)
        {
            return new IntIdentifier(id);
        }

        public int Id { get; set; }

        private sealed class IdEqualityComparer : IEqualityComparer<IntIdentifier>
        {
            public bool Equals(IntIdentifier x, IntIdentifier y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(IntIdentifier obj)
            {
                return obj.Id;
            }
        }

        protected bool Equals(IntIdentifier other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IntIdentifier) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}