using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Restore.Channel;

namespace Restore.Tests.Channel
{
    [TestFixture]
    public class OneWayPullChannelDrainTest : OneWayPullChannelTestBase
    {
        public Task<SynchronizationFinished> WaitForSynchFinish(
            Action<ObservableCollection<LocalTestResource>> resultAssertion = null)
        {
            var taskCompletion = new TaskCompletionSource<SynchronizationFinished>();
            ChannelUnderTest.AddSynchronizationFinishedObserver(finish => { taskCompletion.SetResult(finish); });

            var bogus = ChannelUnderTest.Drain(true);

            return taskCompletion.Task;
        }

        [Test]
        public async Task ShouldContainsSynchedDataInReturnedObservable()
        {
            var synchedResult = await ChannelUnderTest.Drain(true);

            // Don't know a better way of waiting till full synch completion.
            while (ChannelUnderTest.IsSynchronizing)
            {
                await Task.Delay(500);
            }

            var localTestResource =
                synchedResult.FirstOrDefault(ltr => ltr.CorrelationId.HasValue && ltr.CorrelationId == 3);
            Assert.AreEqual("Only Remote 3", localTestResource.Name);
            synchedResult.Dispose();
        }

        [Test]
        public async Task ShouldContainSynchedDataOnceFinished()
        {
            var finish = await WaitForSynchFinish();
            Assert.AreEqual(3, finish.ItemsProcessed);
            Assert.AreEqual(1, finish.ItemsSynchronized);
        }

        [Test]
        public async Task ShouldNoStartSynchronizationWhenConditionFalse()
        {
            var hasSynchronized = false;
            ChannelUnderTest.AddSynchronizationStartedObserver(_ => hasSynchronized = true);
            await ChannelUnderTest.Drain(false);

            Assert.IsFalse(hasSynchronized);
        }

        [Test]
        public async Task ShouldReturnOnlyLocalData()
        {
            var result = await ChannelUnderTest.Drain(false);

            Assert.AreEqual(TestData.LocalResults[0], result[0]);
            Assert.AreEqual(TestData.LocalResults[1], result[1]);
        }
    }
}