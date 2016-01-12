using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;
using NUnit.Framework;
using Restore.ChangeResolution;
using Restore.Channel;
using Restore.Channel.Configuration;
using Restore.Matching;
using Restore.RxProto;
using Restore.Tests.ChangeResolution;
using Restore.Tests.RxProto;

namespace Restore.Tests.Channel
{
    /// <summary>
    /// Let's just start of with the simplest channel
    /// </summary>
    [TestFixture]
    public class OneWayPullChannelTest
    {
        private ISynchChannel<LocalTestResource, RemoteTestResource, ItemMatch<LocalTestResource, RemoteTestResource>> _channelUnderTest;
        private InMemoryCrudDataEndpoint<LocalTestResource, int> _localEndpoint;
        private InMemoryCrudDataEndpoint<RemoteTestResource, int> _remoteEndpoint;

        [SetUp]
        public void SetUpTest()
        {
            var type1Config = new TypeConfiguration<LocalTestResource, int>(ltr => ltr.CorrelationId.HasValue ? ltr.CorrelationId.Value : -1);
            _localEndpoint = new InMemoryCrudDataEndpoint<LocalTestResource, int>(type1Config, TestData.LocalResults);
            var endpoint1Config = new EndpointConfiguration<LocalTestResource, int>(
                type1Config,
                _localEndpoint);

            var type2Config = new TypeConfiguration<RemoteTestResource, int>(rtr => rtr.Id);
            _remoteEndpoint = new InMemoryCrudDataEndpoint<RemoteTestResource, int>(type2Config, TestData.RemoteResults);
            var endpoint2Config = new EndpointConfiguration<RemoteTestResource, int>(
                type2Config,
                _remoteEndpoint);

            // This clearly requires a configuration API.
            var channelConfig = new ChannelConfiguration<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(endpoint1Config, endpoint2Config, new TestResourceTranslator());
            var itemsPreprocessor = new ItemMatcher<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(channelConfig);
            channelConfig.ItemsPreprocessor = itemsPreprocessor.Match;
            channelConfig.AddSynchAction(new SynchronizationResolver<ItemMatch<LocalTestResource,RemoteTestResource>, ChannelConfiguration<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>>(
                channelConfig,
                (item, cfg) =>
                {
                    return item.Result1 == null;
                },
                (item, cfg) =>
                {
                    var synchItem = item.Result1;
                    cfg.TypeTranslator.TranslateBackward(item.Result2, ref synchItem);
                    // Now the translate decides wether a new item has to be created, but the decision is there anyway because of the Create.
                    cfg.Type1EndpointConfiguration.Endpoint.Create(synchItem);
                    return new SynchronizationResult(true);
                }
            ));

            //var t2epConfig = new EndpointConfiguration()
            _channelUnderTest = new OneWayPullChannel<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(
                channelConfig,
                () => Task.FromResult(_localEndpoint.ReadAll().AsEnumerable()),
                () => Task.FromResult(_remoteEndpoint.ReadAll().AsEnumerable()));
        }
        // Create channel and trigger a pump data into an observable collection. (synch twice)
        // Create channel and trigger a pump/Open. (this is background synch)
        [Test]
        public async Task ShouldSynchNewDataFromRemote()
        {
            //channelUnderTest.SynchStarted 
            // First just make a channel that we can call synch on.
            await _channelUnderTest.Synchronize();

/*            var synched1 = localEndpoint.Read(1);
            Assert.IsNotNull(synched1);
            Assert.AreEqual(TestData.RemoteResults[0].Name, synched1.Name);

            Assert.IsNull(localEndpoint.Read(2));*/

            var synched3 = _localEndpoint.Read(3);
            Assert.IsNotNull(synched3);
            Assert.AreEqual(TestData.RemoteResults[1].Name, synched3.Name);
        }

        [Test]
        public async Task ShouldCallSideEffectListenerOnMatchedData()
        {
            var matched = false;
            _channelUnderTest.AddSynchItemListener<ItemMatch<LocalTestResource, RemoteTestResource>>(match =>
            {
                matched = true;
            });

            await _channelUnderTest.Synchronize();

            Assert.IsTrue(matched);
        }
    }

    //public ExecuteableSynchronizationAction

    // Since I'm not fully sure what design is going to look like in terms of different channel types
    // I just name the channel to what I intend it to do.

    /*
    public static class ChangeDispatcher
    {
        public static IEnumerable<SynchronizationResult> Dispatch<TSynch>(this IEnumerable<ISynchronizationAction<TSynch>> ) 
    }
    */
}
