using System;
using JetBrains.Annotations;
using NUnit.Framework;
using Restore.Matching;

namespace Restore.Tests.Matching
{
    [TestFixture]
    public class SingleItemCompletionMatcherTest
    {
        [SetUp]
        public void SetUpTest()
        {
            var x = "test";
        }

        [Test]
        public void ShouldTryMatchFromGivenSourceType()
        {
            // I need a complete configuration in order for this object to work. :o
            var channelConfiguration = new TestConfiguration();
            var _matcherUnderTest = new SingleItemCompletionMatcher<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(
                channelConfiguration,
                typeof(LocalTestResource));

            // Add resource that should be retrieved by matcher
            channelConfiguration.Type1EndpointConfiguration.Endpoint.Create(new LocalTestResource(1, 10));

            var testMatch = new ItemMatch<LocalTestResource, RemoteTestResource>(
                null,
                new RemoteTestResource(1, "Missing"));
            _matcherUnderTest.Complete(testMatch);

            Assert.IsNotNull(testMatch.Result1);
        }
    }

    public class SingleItemCompletionMatcher<T1, T2, TId, TSynch>
        where TId : IEquatable<TId>
    {
        private readonly IChannelConfiguration<T1, T2, TId, TSynch> _channelConfiguration;
        private readonly Type _completionSourceType;

        public SingleItemCompletionMatcher(IChannelConfiguration<T1, T2, TId, TSynch> channelConfiguration, Type completionSourceType)
        {
            _channelConfiguration = channelConfiguration;
            _completionSourceType = completionSourceType;
        }

        public void Complete(ItemMatch<LocalTestResource, RemoteTestResource> itemMatch)
        {
            
        }
    }
}
