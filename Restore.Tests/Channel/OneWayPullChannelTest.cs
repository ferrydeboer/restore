using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using Restore.Matching;

namespace Restore.Tests.Channel
{
    /// <summary>
    /// Let's just start of with the simplest channel
    /// </summary>
    [TestFixture]
    public class OneWayPullChannelTest : OneWayPullChannelTestBase
    {
        // Create channel and trigger a pump data into an observable collection. (synch twice)
        // Create channel and trigger a pump/Open. (this is background synch)
        [Test]
        public async Task ShouldSynchNewDataFromRemote()
        {
            //channelUnderTest.SynchStarted 
            // First just make a channel that we can call synch on.
            await _channelUnderTest.Synchronize();

/*            var synched1 = localEndpoint.Read(1);
            Assert.IsNotNull(synched1);
            Assert.AreEqual(TestData.RemoteResults[0].Name, synched1.Name);

            Assert.IsNull(localEndpoint.Read(2));*/

            var synched3 = _localEndpoint.Read(3);
            Assert.IsNotNull(synched3);
            Assert.AreEqual(TestData.RemoteResults[1].Name, synched3.Name);
        }

        [Test]
        public async Task ShouldCallSideEffectListenerOnMatchedData()
        {
            var matched = false;
            _channelUnderTest.AddSynchItemObserver<ItemMatch<LocalTestResource, RemoteTestResource>>(match =>
            {
                matched = true;
            });

            await _channelUnderTest.Synchronize();

            Assert.IsTrue(matched);
        }

        [Test]
        public async Task ShouldWrapItemExceptionIntoSynchronizationException()
        {
            var expectedException = new Exception("An observer has broken the item");
            _channelUnderTest.AddSynchActionObserver(action =>
            {
                throw expectedException;
            });

            try
            {
                await _channelUnderTest.Synchronize();
            }
            catch (ItemSynchronizationException ex)
            {
                // For step specific errors the steps will assign the item. Whenever some kind of exception
                // occurs where it's not catched by a step, the actual item in the pipeline is unknown. 
                // So for now we accept it's null.
                Assert.IsNull(ex.Item);
                Assert.AreEqual("Synchronization of an item failed for an unknown reason.", ex.Message);
                Assert.AreEqual(expectedException, ex.InnerException);
            }
        }

        [Test]
        public async Task ShouldCallSynchronizationStartedObserver()
        {
            bool isCalled = false;
            _channelUnderTest.AddSynchronizationStartedObserver(start =>
            {
                Assert.AreEqual(typeof(LocalTestResource), start.Type1);
                Assert.AreEqual(typeof(RemoteTestResource), start.Type2);
                isCalled = true;
            });

            await _channelUnderTest.Synchronize();

            Assert.IsTrue(isCalled);
        }

        [Test]
        public async Task ShouldCallSynchronizationFinishedObserver()
        {
            bool isCalled = false;
            _channelUnderTest.AddSynchronizationFinishedObserver(finish =>
            {
                Assert.AreEqual(typeof(LocalTestResource), finish.Type1);
                Assert.AreEqual(typeof(RemoteTestResource), finish.Type2);
                Assert.AreEqual(3, finish.ItemsProcessed);
                Assert.AreEqual(1, finish.ItemsSynchronized);
                isCalled = true;
            });

            await _channelUnderTest.Synchronize();

            Assert.IsTrue(isCalled);
        }

        [Test]
        public void ShouldIgnoreSecondSynchCallIfASynchIsAlreadyRunning()
        {
            // Make two threads
            // Have first thread run synch
            // - Cause this first thread to sleep using OnSynchronizationStartedHandler
            // Have second thread run synch.
            // I guess the only way to test this is to simply look what the timestamp was the thread finished.
            // The second thread should be finished prior to the first. I could also have some sort of 
            // event that is called when multiple synch calls are done.

            int startCallCount = 0;
            _channelUnderTest.AddSynchronizationStartedObserver(_ =>
            {
                Debug.WriteLine("Synch start " + startCallCount);
                Task.Delay(1000);
                startCallCount++;
            });
            var task1 = Task.Factory.StartNew(async () =>
            {
                await _channelUnderTest.Synchronize();
            });
            var task2 = Task.Factory.StartNew(async () =>
            {
                await _channelUnderTest.Synchronize();
            });
            Task.WaitAll(task1, task2);

            Assert.AreEqual(1, startCallCount);
        }
    }
}
