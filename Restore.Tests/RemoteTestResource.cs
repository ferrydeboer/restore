namespace Restore.Tests
{
    public class RemoteTestResource
    {
        public RemoteTestResource(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }
    }
}