using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Restore.Matching;

namespace Restore.Tests.Matching
{
    [TestFixture]
    public class PredefinedQueryReplicatorTest
    {
        [Test]
        public async Task ShouldSimplyMapToPredefinedQuery()
        {
            var localDataEndpoint = new TestAsyncDataEndpoint<LocalTestResource>("Local", new List<LocalTestResource>
                {
                    new LocalTestResource(1, 10),
                    new LocalTestResource(2, 20)
                });
            var remoteDataEndpoint = new TestAsyncDataEndpoint<RemoteTestResource>("Remote", new List<RemoteTestResource>
                {
                    new RemoteTestResource(1, "Remote 1"),
                    new RemoteTestResource(2, "Remote 2"),
                    new RemoteTestResource(3, "Remote 3")
                });
            var replicator = new PredefinedQueryReplicatorAsync<LocalTestResource, RemoteTestResource>(localDataEndpoint, remoteDataEndpoint,
                async rde =>
                {
                    Debug.WriteLine("Loading remote data!");
                    return await rde?.GetAllAsync();
                });

            replicator.QueryReplicated += (sender, args) =>
            {
                Assert.AreEqual(remoteDataEndpoint.Data, args?.ReplicationResults);
            };

            // Trigger a query event on the local data endpoint.
            var replicated = await replicator.Replicate(new DataLoadedEventArgs<LocalTestResource>(localDataEndpoint.Data.AsEnumerable()));
        }

        [Test]
        public void ShouldWrapExceptionOnReplicationFailure()
        {
            
        }
    }
}
