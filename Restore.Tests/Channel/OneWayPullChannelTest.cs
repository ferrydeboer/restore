using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using Restore.ChangeResolution;
using Restore.Channel;
using Restore.Channel.Configuration;
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
            // First just make a channel that we can call synch on.
            await ChannelUnderTest.Synchronize();

/*            var synched1 = localEndpoint.Read(1);
            Assert.IsNotNull(synched1);
            Assert.AreEqual(TestData.RemoteResults[0].Name, synched1.Name);

            Assert.IsNull(localEndpoint.Read(2));*/

            var synched3 = LocalEndpoint.Read(3);
            Assert.IsNotNull(synched3);
            Assert.AreEqual(TestData.RemoteResults[1].Name, synched3.Name);
        }

        [Test]
        public async Task ShouldCallSideEffectListenerOnMatchedData()
        {
            var matched = false;
            ChannelUnderTest.AddSynchItemObserver<ItemMatch<LocalTestResource, RemoteTestResource>>(match =>
            {
                matched = true;
            });

            await ChannelUnderTest.Synchronize();

            Assert.IsTrue(matched);
        }

        [Test]
        public async Task ShouldWrapItemExceptionIntoSynchronizationException()
        {
            var expectedException = new Exception("An observer has broken the item");
            ChannelUnderTest.AddSynchActionObserver(action =>
            {
                throw expectedException;
            });

            try
            {
                await ChannelUnderTest.Synchronize();
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
            ChannelUnderTest.AddSynchronizationStartedObserver(start =>
            {
                Assert.AreEqual(typeof(LocalTestResource), start.Type1);
                Assert.AreEqual(typeof(RemoteTestResource), start.Type2);
                isCalled = true;
            });

            await ChannelUnderTest.Synchronize();

            Assert.IsTrue(isCalled);
        }

        [Test]
        public async Task ShouldCallSynchronizationFinishedObserver()
        {
            bool isCalled = false;
            ChannelUnderTest.AddSynchronizationFinishedObserver(finish =>
            {
                Assert.AreEqual(typeof(LocalTestResource), finish.Type1);
                Assert.AreEqual(typeof(RemoteTestResource), finish.Type2);
                Assert.AreEqual(3, finish.ItemsProcessed);
                Assert.AreEqual(1, finish.ItemsSynchronized);
                isCalled = true;
            });

            await ChannelUnderTest.Synchronize();

            Assert.IsTrue(isCalled);
        }

        [Test]
        public void ShouldIgnoreSecondSynchCallIfASynchIsAlreadyRunning()
        {
            int startCallCount = 0;
            ChannelUnderTest.AddSynchronizationStartedObserver(_ =>
            {
                // put in small delay to make test deterministic and have thread at least hold on for little longer.
                Task.Delay(50);
                startCallCount++;
            });

            // Start first in new thread that should immediately stop second from running.
            var task1 = Task.Run(() => ChannelUnderTest.Synchronize());

            // This one should never start since it starts immediately after, don't expect first to finish.
            var task2 = ChannelUnderTest.Synchronize();
            Task.WaitAll(task1, task2);

            Assert.AreEqual(1, startCallCount);
        }

        [Test]
        [ExpectedException(typeof(SynchronizationException), ExpectedMessage = "Data source 2 delivered a null result!")]
        public async Task ShouldBreakOutOfSynchronizationWhenDataSource2NullData()
        {
            // Should I throw an exception if one of the data providers returns null? I simply can not proceed. 
            // Silently simply stepping out of the execution is not really informative.
            ChannelUnderTest = new OneWayPullChannel<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(
                _channelConfig,
                () => Task.FromResult(LocalEndpoint.ReadAll().AsEnumerable()),
                () => Task.FromResult((IEnumerable<RemoteTestResource>)null));

            await ChannelUnderTest.Synchronize();
        }

        [Test]
        [ExpectedException(typeof(SynchronizationException), ExpectedMessage = "Data source 1 delivered a null result!")]
        public async Task ShouldBreakOutOfSynchronizationWhenDataSource1NullData()
        {
            // Should I throw an exception if one of the data providers returns null? I simply can not proceed. 
            // Silently simply stepping out of the execution is not really informative.
            ChannelUnderTest = new OneWayPullChannel
                <LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(
                _channelConfig,
                () => Task.FromResult((IEnumerable<LocalTestResource>) null),
                () => Task.FromResult((IEnumerable<RemoteTestResource>) null));

            await ChannelUnderTest.Synchronize();
        }

        [Test]
        public async Task ShouldThrowSynchronizationExceptionWhenPreprocessingFails()
        {
            var exception = new Exception("Test");
            _channelConfig.ItemsPreprocessor = (resources, enumerable) =>
            {
                throw exception;
            };

            try
            {
                await ChannelUnderTest.Synchronize();
            }
            catch (SynchronizationException ex)
            {
                Assert.AreEqual("Provided items preprocessor failed with message: \"Test\"", ex.Message);
            }
            catch (Exception)
            {
                Assert.Fail("Expecting exception to be wrapper in a SynchronizationException");
            }
        }

        [Test]
        public async Task ShouldNotWrapSynchExceptionInAnotherException()
        {
            var exception = new Exception("Test");

            _channelConfig.AddSynchAction(new SynchronizationResolver<ItemMatch<LocalTestResource, RemoteTestResource>, ChannelConfiguration<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>>(
                _channelConfig,
                (item, cfg) =>
                {
                    throw exception;
                },
                (item, cfg) => new SynchronizationResult(true)));

            ConstructTestSubject();
            try
            {
                await ChannelUnderTest.Synchronize();
            }
            catch (ItemSynchronizationException ex)
            {
                Assert.AreEqual(exception, ex.InnerException);
            }
        }

        [Test]
        public void ShouldNotThrowExceptionIfHandledByObserver()
        {
            var exception = new Exception("Test");
            _channelConfig.AddSynchAction(new SynchronizationResolver<ItemMatch<LocalTestResource, RemoteTestResource>, ChannelConfiguration<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>>(
                _channelConfig,
                (item, cfg) =>
                {
                    throw exception;
                },
                (item, cfg) => new SynchronizationResult(true)));

            ConstructTestSubject();
            ChannelUnderTest.AddSynchronizationErrorObserver(error =>
            {
                Assert.AreEqual(exception, error.Cause);
                error.IsHandled = true;
            });
        }
    }
}
