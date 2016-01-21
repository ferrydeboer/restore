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
            int startCallCount = 0;
            _channelUnderTest.AddSynchronizationStartedObserver(_ =>
            {
                // put in small delay to make test deterministic and have thread at least hold on for little longer.
                Task.Delay(50);
                startCallCount++;
            });
            // Start first in new thread that should immediately stop second from running.
            var task1 = Task.Run(() => _channelUnderTest.Synchronize());
            // This one should never start since it starts immediately after, don't expect first to finish.
            var task2 = _channelUnderTest.Synchronize();
            Task.WaitAll(task1, task2);

            Assert.AreEqual(1, startCallCount);
        }
    }
}
