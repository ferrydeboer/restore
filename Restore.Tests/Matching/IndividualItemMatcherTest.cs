using NUnit.Framework;
using Restore.Matching;

namespace Restore.Tests.Matching
{
    [TestFixture]
    public class IndividualItemMatcherTest
    {
        private IndividualItemMatcher<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>> _itemMatcherUnderTest;
        private IChannelConfiguration<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>> _channelConfig;

        [SetUp]
        public void SetUpTest()
        {
            _channelConfig = Setup.TestChannelConfig();
            _itemMatcherUnderTest = new IndividualItemMatcher<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(
                    _channelConfig, TargetType.T1);
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
            _channelConfig.Type1EndpointConfiguration.Endpoint.Create(item1);

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
            _channelConfig.Type2EndpointConfiguration.Endpoint.Create(item2);

            var result = _itemMatcherUnderTest.AppendIndividualItem(itemMatch, TargetType.T2);

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

            var result = _itemMatcherUnderTest.AppendIndividualItem(itemMatch, TargetType.T2);

            Assert.AreEqual(itemMatch, result);
        }
    }
}
