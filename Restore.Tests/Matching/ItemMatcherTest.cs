using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using Restore.Channel.Configuration;
using Restore.Matching;

namespace Restore.Tests.Matching
{
    [TestFixture]
    public class ItemMatcherTest
    {
        [NotNull] private readonly ItemMatcher<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>> _itemMatcherUnderTest;
        private List<LocalTestResource> _localResults;
        private List<RemoteTestResource> _remoteResults;

        public ItemMatcherTest()
        {
            _itemMatcherUnderTest = new ItemMatcher<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(
                new TypeConfiguration<LocalTestResource, int>(ltr => ltr.CorrelationId ?? -1, -1)
                , new TypeConfiguration<RemoteTestResource, int>(ltr => ltr.Id, 0));
        }

        [SetUp]
        public void SetUpTest()
        {
            _localResults = new List<LocalTestResource>
            {
                new LocalTestResource(1, 10) { Name = "Local 1" },
                new LocalTestResource(2) { Name = "Only Local 2" }
            };
            _remoteResults = new List<RemoteTestResource>
            {
                new RemoteTestResource(1, "Remote 1"),
                new RemoteTestResource(3, "Only Remote 2")
            };
        }

        // To big of a unit test, should be splitted up. Created to test first concept.
        [Test]
        public void ShouldMatch()
        {
            Assert.IsNotNull(_localResults, "_localResults != null");
            Assert.IsNotNull(_remoteResults, "_remoteResults != null");

            var matches = _itemMatcherUnderTest.Match(_localResults, _remoteResults).ToList();

            Assert.IsNotNull(matches);
            foreach (var match in matches)
            {
                Debug.WriteLine("{0} - {1}", match?.Result1?.Name, match?.Result2?.Name);
            }

            Assert.AreEqual(matches[0].Result1, _localResults[0]);
            Assert.AreEqual(matches.ElementAt(0).Result2, _remoteResults[0]);
        }

        [Test]
        public void ShouldOnlyReturnType2Matches()
        {
            Assert.IsNotNull(_localResults, "_localResults != null");
            Assert.IsNotNull(_remoteResults, "_remoteResults != null");

            var matches = _itemMatcherUnderTest.Match(new List<LocalTestResource>(), _remoteResults).ToList();

            Assert.IsNotNull(matches);
            Assert.AreEqual(2, matches.Count);
            for (int i = 0; i < 2; i++)
            {
                Assert.IsNull(matches[i].Result1);
                Assert.AreEqual(matches[i].Result2, _remoteResults[i]);
            }
        }

        [Test]
        public void ShouldOnlyReturnType1Matches()
        {
            Assert.IsNotNull(_localResults, "_localResults != null");
            Assert.IsNotNull(_remoteResults, "_remoteResults != null");

            var matches = _itemMatcherUnderTest.Match(_localResults, new List<RemoteTestResource>()).ToList();

            Assert.IsNotNull(matches);
            Assert.AreEqual(2, matches.Count);
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(matches[i].Result1, _localResults[i]);
                Assert.IsNull(matches[i].Result2);
            }
        }
    }
}
