using System;
using NUnit.Framework;

namespace Restore.Tests.Channel
{
    /// <summary>
    /// Let's just start of with the simplest channel
    /// </summary>
    [TestFixture]
    public class OneWayPullChannelTest
    {
        // Create channel and trigger a pump data into an observable collection. (synch twice)
        // Create channel and trigger a pump/Open. (this is background synch)

        [Test]
        public void ShouldSynchNewDataFromRemote()
        {
            //var localEndpoint = new InMemoryDataEndpoint<LocalTestResource>();
            //var channelConfig = new ChannelConfiguration<LocalTestResource, RemoteTestResource, int>();
            // config should contain: 
            // Type1 data source, endpoint, type and idExtractor
            // Type2 data source, endpoint, type and idExtractor
            
            //var t2epConfig = new DataSourceEndpointConfiguration()
            var channelUnderTest = new OneWayPullChannel<LocalTestResource, RemoteTestResource, int>();
        }
    }

    public class OneWayPullChannel<T1, T2, TId> where TId : IEquatable<TId>
    {
    }
}
