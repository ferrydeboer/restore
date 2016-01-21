namespace Restore.Tests
{
    public class RemoteTestResource
    {
        public RemoteTestResource(string name)
        {
            Name = name;
        }

        public RemoteTestResource(int id, string name)
            : this(name)
        {
            Id = id;
        }

        public int Id { get; }

        public string Name { get; }
    }
}