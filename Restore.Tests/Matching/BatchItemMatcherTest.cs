using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Restore.Extensions;
using Restore.Matching;

namespace Restore.Tests.Matching
{
    [TestFixture]
    public class BatchItemMatcherTest
    {
        private IChannelConfiguration<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>> _channelConfig;
        private InMemoryCrudDataEndpoint<LocalTestResource, int> _localEndpoint;
        private InMemoryCrudDataEndpoint<RemoteTestResource, int> _remoteEndpoint;

        [SetUp]
        public void SetUpTest()
        {
            _channelConfig = Setup.TestChannelConfig();
            _localEndpoint = _channelConfig.Type1EndpointConfiguration.Endpoint as InMemoryCrudDataEndpoint<LocalTestResource, int>;
            _remoteEndpoint = _channelConfig.Type2EndpointConfiguration.Endpoint as InMemoryCrudDataEndpoint<RemoteTestResource, int>;
        }

        [Test]
        public void ShouldPassThroughCompleteMatches()
        {
            _localEndpoint.ItemRead += (sender, args) => Assert.Fail("Should not call local endpoint on complete matches");
            _remoteEndpoint.ItemRead += (sender, args) => Assert.Fail("Should not call remote endpoint on complete matches");
            var output = Enumerable.Range(1, 3)
                .Select(
                    i =>
                        new ItemMatch<LocalTestResource, RemoteTestResource>(
                            new LocalTestResource(i, i * 10),
                            new RemoteTestResource(i, $"test {i}")))
                .BatchCompleteItems(_channelConfig, TargetType.T1);

            Assert.AreEqual(3, output.Count());
        }

        [Test]
        public void ShouldQueryForAllMissingT1()
        {
            List<int> passed = new List<int>();
            var count = 3;
            var matches = Enumerable.Range(1, count)
                .Select(
                    i =>
                        new ItemMatch<LocalTestResource, RemoteTestResource>(
                            null,
                            new RemoteTestResource(i, $"test {i}")))
                .Do(match =>
                {
                    // Buffer the passed matches, this is exactly what the matcher should do as well!
                    // If the results are not fetched by batching the final outcoming matches
                    // should not all be complete
                    passed.Add(match.Result2.Id);
                    if (passed.Count == count)
                    {
                        foreach (var id in passed)
                        {
                            _localEndpoint.Create(new LocalTestResource(id, id * 10));
                        }
                    }
                })
                .BatchCompleteItems(_channelConfig, TargetType.T1);

            var itemMatches = matches as IList<ItemMatch<LocalTestResource, RemoteTestResource>> ?? matches.ToList();
            Assert.AreEqual(count, itemMatches.Count);
            Assert.IsTrue(itemMatches.All(match => match.IsComplete));
        }

        [Test]
        public void ShouldQueryForAllMissingT2()
        {
            List<int> passed = new List<int>();
            var count = 3;
            var matches = Enumerable.Range(1, count)
                .Select(
                    i =>
                        new ItemMatch<LocalTestResource, RemoteTestResource>(
                            new LocalTestResource(i, i * 10),
                            null))
                .Do(match =>
                {
                    // Buffer the passed matches, this is exactly what the matcher should do as well!
                    // If the results are not fetched by batching the final outcoming matches
                    // should not all be complete
                    Assert.IsNotNull(match.Result1.CorrelationId);
                    passed.Add(match.Result1.CorrelationId.Value);
                    if (passed.Count == count)
                    {
                        foreach (var id in passed)
                        {
                            _remoteEndpoint.Create(new RemoteTestResource(id, $"test {id}"));
                        }
                    }
                })
                .BatchCompleteItems(_channelConfig, TargetType.T2);

            var itemMatches = matches as IList<ItemMatch<LocalTestResource, RemoteTestResource>> ?? matches.ToList();
            Assert.AreEqual(count, itemMatches.Count);
            Assert.IsTrue(itemMatches.All(match => match.IsComplete));
        }

        [Test]
        public void ShouldReturnCompleteMatchesFirstDueToMatching()
        {
            var incompleteMatch = new ItemMatch<LocalTestResource, RemoteTestResource>(
                null,
                new RemoteTestResource(1, "complete"));
            var completeMatch = new ItemMatch<LocalTestResource, RemoteTestResource>(
                new LocalTestResource(2, 10),
                new RemoteTestResource(2, "complete"));

            var matches = new List<ItemMatch<LocalTestResource, RemoteTestResource>>
            {
                incompleteMatch,
                completeMatch
            };

            var localTestResource = new LocalTestResource(1, 10);
            _channelConfig.Type1EndpointConfiguration.Endpoint.Create(localTestResource);
            var resultMatches = matches.BatchCompleteItems(_channelConfig, TargetType.T1).ToList();

            Assert.AreEqual(2, resultMatches.Count);
            Assert.AreEqual(completeMatch, resultMatches[0]);
            Assert.AreEqual(incompleteMatch.Result2, resultMatches[1].Result2);
            Assert.AreEqual(localTestResource, resultMatches[1].Result1);
        }
    }
}