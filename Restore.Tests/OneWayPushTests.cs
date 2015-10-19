using System;
using NUnit.Framework;

namespace Restore.Tests
{
    [TestFixture]
    public class OneWayPushTests
    {
        private TestResource _testResource;
        InMemoryDataEndpoint<TestResource> _testSource;
        IDataEndpoint<TestResource> _testTarget;
        private SynchronizationChannel<TestResource> _testChannel;
            
        [SetUp]
        public void SetUpTest()
        {
            _testSource = new InMemoryDataEndpoint<TestResource>(t => t.Id);
            _testSource.ResourceDeleted.Subscribe(t => t.Deleted = true);
            _testTarget = new InMemoryDataEndpoint<TestResource>(t => t.Id);
            _testTarget.AddSyncAction(t => t.Deleted, (ds, t) => ds.Delete(t), "Delete");
            _testTarget.AddSyncAction(t => string.IsNullOrEmpty(t.CorrelationId), 
                (ds, r) => ds.Create(r), "Create");
            
            _testTarget.AddSyncAction(
                t => !string.IsNullOrEmpty(t.CorrelationId), (ds, r) =>
                {
                    var resourceToUpdate = ds.Get(r.Id);
                    if (resourceToUpdate != null)
                    {
                        resourceToUpdate.Update(r);
                        ds.Update(resourceToUpdate);
                    }
                }, "Update");

            _testChannel = new SynchronizationChannel<TestResource>(_testSource, _testTarget);
        }

        [TearDown]
        public void TearDownTest()
        {
            _testChannel.Dispose();
        }

        [Test]
        public void ShouldCreateResourceInTarget()
        {
            using (var channel = new SynchronizationChannel<TestResource>(_testSource, _testTarget))
            {
                channel.Open();
                _testResource = new TestResource(1);
                _testSource.Create(_testResource);

                Assert.AreEqual(_testResource, _testTarget.Get(1));
            }
        }

        [Test]
        public void ShouldUpdateResourceInTargetWhenUpdatedLocal()
        {
            using (var channel = new SynchronizationChannel<TestResource>(_testSource, _testTarget))
            {
                // Arrange
                _testResource = new TestResource(1) { CorrelationId = "1030" };
                _testTarget.Create(_testResource);
                var changedResource = _testResource.Copy();
                 changedResource.Description = "Changed";

                // Act
                channel.Open();
                _testSource.Create(changedResource);

                // Assert
                var actualtTestResource = _testTarget.Get(1);
                Assert.AreEqual(_testResource, actualtTestResource);
                Assert.AreEqual("Changed", actualtTestResource.Description);
            }
        }

        [Test]
        public void ShouldDeleteResourceInTargetWhenDeletedAtTarget()
        {
            using (var channel = new SynchronizationChannel<TestResource>(_testSource, _testTarget))
            {
                // Arrange
                _testResource = new TestResource(1);
                channel.Open();
                _testSource.Create(_testResource);
                
                // Act
                _testSource.Delete(_testResource);

                // Assert
                Assert.IsNull(_testTarget.Get(1));
            }
        }

        [Test]
        public void ShouldNotBreakPublishing()
        {
            _testTarget = new InMemoryDataEndpoint<TestResource>(t => t.Id);
            _testTarget.AddSyncAction(t => t.Deleted, (ds, t) =>
            {
                throw new Exception("Ooops");
            }, "Delete");
            _testTarget.AddSyncAction(t => string.IsNullOrEmpty(t.CorrelationId),
                (ds, r) => ds.Create(r), "Create");
            using (var channel = new SynchronizationChannel<TestResource>(_testSource, _testTarget))
            {
                channel.Open();

                _testResource = new TestResource(1);
                _testSource.Create(_testResource);
                _testSource.Delete(_testResource);
                var testResource2 = new TestResource(2);
                _testSource.Create(testResource2);

                Assert.AreEqual(testResource2, _testTarget.Get(2));
            }
        }
    }
}
