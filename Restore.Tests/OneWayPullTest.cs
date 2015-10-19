using NUnit.Framework;

namespace Restore.Tests
{
    [TestFixture]
    public class OneWayPullTest
    {
        /**
         * Mimics scenario where a list of items is being pulled in from the source to synchronize them with the target.
         * Here it depends on the target to determine of something is deleted from the source.
         * Source: REST API
         * Targer: Local DB
         */

        // Scenarios:
        // - New item from source to target
        [Test]
        public void ShouldAddItemToTarget()
        {
            var testSource = new InMemoryDataEndpoint<TestResource>(r => r.Id);
            var testTarget = new InMemoryDataEndpoint<TestResource>(r => r.Id);
            var testResource = new TestResource(1) { Description = "Test" };
            testSource.Create(testResource);
            testTarget.AddSyncAction((e,r) => e.Get(e.IdentityResolver(r)) == null, (e, r) => e.Create(r), "Create");
            var testChannel = new SynchronizationChannel<TestResource>(testSource, testTarget, true);
            
            testChannel.Open();

            Assert.IsNotNull(testTarget.Get(1));
        }

        // - Change from source to target
        // - Delete from source to target
        [Test]
        public void ShouldDeleteItemFromTarget()
        {
            var testSource = new InMemoryDataEndpoint<TestResource>(r => r.Id);
            var testTarget = new BatchListCleanupEndpointDecorator<TestResource>(
                new InMemoryDataEndpoint<TestResource>(r => r.Id));
            var testResource = new TestResource(1) { Description = "Test" };
            testTarget.Create(testResource);
            testTarget.AddSyncAction((e, r) => e.Get(e.IdentityResolver(r)) == null, (e, r) => e.Create(r), "Create");
            var testChannel = new SynchronizationChannel<TestResource>(testSource, testTarget, true);
            testChannel.Opening += (s, e) => testTarget.Initialize();
            testChannel.Closing += (s, e) => testTarget.Finish();

            testChannel.Open();

            Assert.IsNull(testTarget.Get(1));
            Assert.IsFalse(testChannel.IsOpen);
        }
    }
}
