using Restore.Channel.Configuration;
using Restore.Matching;
using Restore.Tests.ChangeResolution;

namespace Restore.Tests.Matching
{
    [TestFixture]
    public class IndividualItemMatcherTest
    {
        [NotNull]
        private readonly IndividualItemMatcher<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>> _itemMatcherUnderTest;
        private List<LocalTestResource> _localResults;
        private List<RemoteTestResource> _remoteResults;
        private EndpointConfiguration<LocalTestResource, int> _t1EndpointCfg;
        private EndpointConfiguration<RemoteTestResource, int> _t2EndpointCfg;
        private IChannelConfiguration<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>> _channelConfig;

        public IndividualItemMatcherTest()
        {
            _t1EndpointCfg = CreateTestEndpointConfig<LocalTestResource, int>(ltr => ltr.CorrelationId ?? -1);
            _t2EndpointCfg = CreateTestEndpointConfig<RemoteTestResource, int>(ltr => ltr.Id);

            _channelConfig = new ChannelConfiguration<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(
                _t1EndpointCfg, _t2EndpointCfg, new TestResourceTranslator());

            _itemMatcherUnderTest =
                new IndividualItemMatcher<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(
                    _channelConfig, typeof(LocalTestResource));
        }

        [TearDown]
        public void TearDown()
        {
            ((InMemoryCrudDataEndpoint<LocalTestResource, int>)_t1EndpointCfg.Endpoint).Clear();
            ((InMemoryCrudDataEndpoint<RemoteTestResource, int>)_t2EndpointCfg.Endpoint).Clear();
        }

        public EndpointConfiguration<T, TId> CreateTestEndpointConfig<T, TId>(Func<T, TId> idResolver) where TId : IEquatable<TId>
        {
            var configuration = new TypeConfiguration<T, TId>(idResolver);
            var endpoint = new InMemoryCrudDataEndpoint<T, TId>(configuration);
            return new EndpointConfiguration<T, TId>(configuration, endpoint);
        }

        [Test]
        public void ShouldReturnAlreadyCompleteMatches()
        {
            var itemMatch = new ItemMatch<LocalTestResource, RemoteTestResource>(
                new LocalTestResource(1, 10),
                new RemoteTestResource(1, "test"));

            var result = _itemMatcherUnderTest.AppendIndividualItem(itemMatch);
            Assert.AreEqual(itemMatch, result);
        }

        [Test]
        public void ShouldAppendT1WhenDefaultButAvailable()
        {            
            var itemMatch = new ItemMatch<LocalTestResource, RemoteTestResource>(
                null,
                new RemoteTestResource(1, "test"));
            var item1 = new LocalTestResource(1, 10);
            _t1EndpointCfg.Endpoint.Create(item1);

            var result = _itemMatcherUnderTest.AppendIndividualItem(itemMatch);

            Assert.AreEqual(item1, result.Result1);
            Assert.AreEqual(itemMatch.Result2, result.Result2);
        }

        [Test]
        public void ShouldAppendT2WhenDefaultButAvailable()
        {
            
            var itemMatch = new ItemMatch<LocalTestResource, RemoteTestResource>(
                new LocalTestResource(1, 10),
                null);
            var item2 = new RemoteTestResource(1, "test");
            _t2EndpointCfg.Endpoint.Create(item2);

            var result = _itemMatcherUnderTest.AppendIndividualItem(itemMatch, typeof(RemoteTestResource));

            Assert.AreEqual(item2, result.Result2);
            Assert.AreEqual(itemMatch.Result1, result.Result1);
        }

        [Test]
        public void ShouldReturnOriginalMatchWhenT1NotAvailable()
        {
            var itemMatch = new ItemMatch<LocalTestResource, RemoteTestResource>(
                null,
                new RemoteTestResource(1, "test"));

            var result = _itemMatcherUnderTest.AppendIndividualItem(itemMatch);

            Assert.AreEqual(itemMatch, result);
        }

        [Test]
        public void ShouldReturnOriginalMatchWhenT2NotAvailable()
        {
            var itemMatch = new ItemMatch<LocalTestResource, RemoteTestResource>(
                new LocalTestResource(1, 10),
                null);

            var result = _itemMatcherUnderTest.AppendIndividualItem(itemMatch, typeof(RemoteTestResource));

            Assert.AreEqual(itemMatch, result);
        }
    }
}
