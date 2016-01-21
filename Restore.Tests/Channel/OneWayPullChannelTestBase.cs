using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Restore.ChangeResolution;
using Restore.Channel;
using Restore.Channel.Configuration;
using Restore.Matching;
using Restore.Tests.ChangeResolution;

namespace Restore.Tests.Channel
{
    public class OneWayPullChannelTestBase : IDisposable
    {
        protected OneWayPullChannel<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>> _channelUnderTest;
        protected InMemoryCrudDataEndpoint<LocalTestResource, int> _localEndpoint;
        protected InMemoryCrudDataEndpoint<RemoteTestResource, int> _remoteEndpoint;
        private ChannelConfiguration<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>> _channelConfig;

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
            _channelConfig = new ChannelConfiguration<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(endpoint1Config, endpoint2Config, new TestResourceTranslator());
            var itemsPreprocessor = new ItemMatcher<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(_channelConfig);
            _channelConfig.ItemsPreprocessor = itemsPreprocessor.Match;
            _channelConfig.AddSynchAction(new SynchronizationResolver<ItemMatch<LocalTestResource,RemoteTestResource>, ChannelConfiguration<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>>(
                _channelConfig,
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
                }));

            //var t2epConfig = new EndpointConfiguration()
            _channelUnderTest = new OneWayPullChannel<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(
                _channelConfig,
                () => Task.FromResult(Enumerable.AsEnumerable<LocalTestResource>(_localEndpoint.ReadAll())),
                () => Task.FromResult(_remoteEndpoint.ReadAll().AsEnumerable()));
        }

        public void Dispose()
        {
            _channelUnderTest.Dispose();
        }
    }
}