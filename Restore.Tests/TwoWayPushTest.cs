using System;
using NUnit.Framework;

namespace Restore.Tests
{
    /// <summary>
    /// Mimics the behaviour where items could be synchronized two ways. This is where certain conflict handling
    /// comes in. The intented implementation is using date times so we'll use that for now.
    /// </summary>
    [TestFixture]
    public class TwoWayPushTest
    {
        [Test]
        public void ShouldOverrideSourceChangeBecauseTargetUpdateWins()
        {
            var _testSource = new InMemoryDataEndpoint<TestResource>(t => t.Id);
            var _testTarget = new InMemoryDataEndpoint<TestResource>(t => t.Id);
            var testTargetItem = new TestResource(1)
            {
                Description = "New Server Description",
                ServerModifiedAt = new DateTime(2015, 10, 19, 16, 26, 28)
            };
            _testTarget.Create(testTargetItem);

            var _testChannel = new SynchronizationChannel<TestResource>(_testSource, _testTarget);

            var testSourceItem = new TestResource(1)
            {
                Description = "Conflicting Client Description",
                ServerModifiedAt = new DateTime(2015, 10, 19, 12, 00, 00)
            };

            _testSource.Create(testSourceItem);
            // So now starting and finishing concepts are becoming part of the data endpoint?
            // Or is this for testing purposes only>
            _testSource.Finish();

            // The current implementation of the SynchronizationAction only know about the data source they 
            // belong to. The problem now becomes that a synchronization action at the target endpoint
            // actually requires an update at the source endpoint.
            // * Giving synchronization actions two endpoints 
            //   (would actions actully still require to be part of the endpoint then? This is only because they need to call them)
            //   - 
            // * Have the endpoint trigger/action simply trigger a data change that will then lead to an
            //   update on the source endpoint. (need two channels for that then)
            //   + Keeps existing mechanisms on channels intacts using the changes.
            //   ? Stronger coupling between actions and endpoint, actions wil the work on the implementation instead of the interface.
        }
    }
}
