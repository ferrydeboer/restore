using System;
using NUnit.Framework;
using Restore.Matching;

namespace Restore.Tests.Matching
{
    [TestFixture]
    public class SingleItemCompletionMatcherTest
    {
        private TestConfiguration _channelConfiguration;
        private SingleItemCompletionMatcher<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>> _matcherUnderTest;
        private Type _completionSourceType;

        [SetUp]
        public void SetUpTest()
        {
            _channelConfiguration = new TestConfiguration();
            _completionSourceType = typeof(LocalTestResource);
            ConstructTestSubject();
        }

        private void ConstructTestSubject()
        {
            _matcherUnderTest = new SingleItemCompletionMatcher
                <LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(
                _channelConfiguration,
                _completionSourceType);
        }

        [Test]
        public void ShouldTryMatchFromGivenSourceType1()
        {
            // Add resource that should be retrieved by matcher
            var localTestResource = new LocalTestResource(1, 10);
            _channelConfiguration.Type1EndpointConfiguration.Endpoint.Create(localTestResource);

            var testMatch = new ItemMatch<LocalTestResource, RemoteTestResource>(
                null,
                new RemoteTestResource(1, "Missing"));
            var resultMatch = _matcherUnderTest.Complete(testMatch);

            Assert.IsNotNull(resultMatch.Result1);
            Assert.AreEqual(localTestResource, resultMatch.Result1);
        }

        [Test]
        public void ShouldTryMatchFromGivenSourceType2()
        {
            _completionSourceType = typeof(RemoteTestResource);
            ConstructTestSubject();
            var remoteTestResource = new RemoteTestResource(1, "test");
            _channelConfiguration.Type2EndpointConfiguration.Endpoint.Create(remoteTestResource);

            var testMatch = new ItemMatch<LocalTestResource, RemoteTestResource>(
                new LocalTestResource(1, 10), null);
            var resultMatch = _matcherUnderTest.Complete(testMatch);

            Assert.IsNotNull(resultMatch.Result2);
            Assert.AreEqual(remoteTestResource, resultMatch.Result2);
        }

        [Test]
        public void ShouldReturnSourceWhenReallyNoOppositeT1()
        {
            var testMatch = new ItemMatch<LocalTestResource, RemoteTestResource>(
                null,
                new RemoteTestResource(1, "Missing"));
            var resultMatch = _matcherUnderTest.Complete(testMatch);

            Assert.IsNull(resultMatch.Result1);
        }

        [Test]
        public void ShouldReturnSourceWhenReallyNoOppositeT2()
        {
            _completionSourceType = typeof(RemoteTestResource);
            ConstructTestSubject();
            var testMatch = new ItemMatch<LocalTestResource, RemoteTestResource>(
                new LocalTestResource(1),
                null);
            var resultMatch = _matcherUnderTest.Complete(testMatch);

            Assert.IsNull(resultMatch.Result2);
        }
    }
}
