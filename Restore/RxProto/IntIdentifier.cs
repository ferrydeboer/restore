namespace Restore.RxProto
{
    public class IntIdentifier : Identifier
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

        public int Id { get; }

        protected bool Equals(IntIdentifier other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            if (obj.GetType() != GetType()) { return false; }
            return Equals((IntIdentifier)obj);
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