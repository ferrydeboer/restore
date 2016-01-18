using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using Restore.Channel;

namespace Restore.Tests.Channel
{
    [TestFixture]
    public class OneWayPullChannelDrainTest : OneWayPullChannelTestBase
    {
        [Test]
        public async Task ShouldNoStartSynchronizationWhenConditionFalse()
        {
            bool hasSynchronized = false;
            _channelUnderTest.AddSynchronizationStartedObserver(_ => hasSynchronized = true);
            await _channelUnderTest.Drain(false);

            Assert.IsFalse(hasSynchronized);
        }
        
        [Test]
        public async Task ShouldReturnOnlyLocalData()
        {
            var result = await _channelUnderTest.Drain(false);

            Assert.AreEqual(TestData.LocalResults[0], result[0]);
            Assert.AreEqual(TestData.LocalResults[1], result[1]);
        }

        [Test]
        public async Task ShouldContainSynchedDataOnceFinished()
        {
            Debug.WriteLine("Calling async");
            _channelUnderTest.AddSynchronizationStartedObserver(_ => Debug.WriteLine("Synch started"));
            _channelUnderTest.AddSynchronizationFinishedObserver(_ => Debug.WriteLine("Synch finished"));
            var finish = await WaitForSynchFinish();
            Assert.AreEqual(3, finish.ItemsProcessed);
            Assert.AreEqual(1, finish.ItemsSynchronized);
            Debug.WriteLine("Finished async");
        }


        public Task<SynchronizationFinished> WaitForSynchFinish()
        {
            TaskCompletionSource<SynchronizationFinished> taskCompletion = new TaskCompletionSource<SynchronizationFinished>();
            Task<ObservableCollection<LocalTestResource>> test = null;
            _channelUnderTest.AddSynchronizationFinishedObserver(finish =>
            {
                var x = test.Result;
                taskCompletion.SetResult(finish);
            });
            try
            {
                test = _channelUnderTest.Drain(true);
            }
            // This is not catching since the method is not awaited!
            // Making method async changes expectations of TaskCompletionSource though.
            catch (Exception ex)
            {
                taskCompletion.SetException(ex);
            }

            return taskCompletion.Task;
        }
    }
}
