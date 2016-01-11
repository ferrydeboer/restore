using System.Collections.Generic;

namespace Restore.Tests
{
    /// <summary>
    /// Some trivial test data that covers most synchronization scenario's.
    /// </summary>
    public static class TestData
    {
        // When doing two way synch this misses the locally available resource that does have a Correlation Id.

        public static List<LocalTestResource> LocalResults { get; } = new List<LocalTestResource>
        {
            new LocalTestResource(1, 10) { Name = "Local 1" },
            new LocalTestResource(2) { Name = "Only Local 2" }
        };

        public static List<RemoteTestResource> RemoteResults { get; } = new List<RemoteTestResource>
        {
            new RemoteTestResource(1, "Remote 1"),
            new RemoteTestResource(3, "Only Remote 3")
        };
    }
}
