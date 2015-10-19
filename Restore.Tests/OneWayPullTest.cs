using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace Restore.Tests
{
    [TestFixture]
    public class OneWayPullTest
    {
        // Simplest scenario is to pass in functions on both channel data sources that return the list.
        // This way I don't even have to change the implementation of the GetAll()
        // Will source push/return an observable?
        /**
         * Source: REST API
         * Targer: Local DB
         */

        // Big difference: Change detection depends on both sources. This deviates from the one way push mechanism.
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


    }
}
