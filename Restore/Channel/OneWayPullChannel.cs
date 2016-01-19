using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Restore.ChangeDispatching;
using Restore.ChangeResolution;
using Restore.Extensions;

namespace Restore.Channel
{
    public class OneWayPullChannel<T1, T2, TId, TSynch> : ISynchChannel<T1, T2, TSynch>, IDisposable where TId : IEquatable<TId>
    {
        [NotNull] private readonly IChannelConfiguration<T1, T2, TId, TSynch> _channelConfig;

        /// <summary>
        /// Didn't want to make this part of the more general configuration. It's not decided yet how to further work with data sources
        /// and possible replication.
        /// </summary>
        [NotNull] private readonly Func<Task<IEnumerable<T1>>> _t1DataSource;

        [NotNull] private readonly Func<Task<IEnumerable<T2>>> _t2DataSource;

        [NotNull][ItemNotNull]
        private readonly IList<Action<TSynch>> _synchItemListeners = new List<Action<TSynch>>();

        [NotNull]
        private readonly ChangeResolutionStep<TSynch, IChannelConfiguration<T1, T2, TId, TSynch>> _resolutionStep;
        [NotNull]
        private readonly ChangeDispatchingStep<TSynch> _dispatchStep;

        private event Action<SynchronizationStarted> SynchronizationStart;
        private event Action<SynchronizationFinished> SynchronizationFinished;

        SemaphoreSlim _lockSemaphore = new SemaphoreSlim(1);
        private bool _isSynchronizing = false;

        public OneWayPullChannel(
            [NotNull] IChannelConfiguration<T1, T2, TId, TSynch> channelConfig,
            [NotNull] Func<Task<IEnumerable<T1>>> t1DataSource, 
            [NotNull] Func<Task<IEnumerable<T2>>> t2DataSource)
        {
            if (channelConfig == null) throw new ArgumentNullException(nameof(channelConfig));
            if (t1DataSource == null) throw new ArgumentNullException(nameof(t1DataSource));
            if (t2DataSource == null) throw new ArgumentNullException(nameof(t2DataSource));

            _channelConfig = channelConfig;
            _t1DataSource = t1DataSource;
            _t2DataSource = t2DataSource;
            // Further development should further help determine the signatures steps.
            _resolutionStep = new ChangeResolutionStep<TSynch, IChannelConfiguration<T1, T2, TId, TSynch>>(channelConfig.SynchronizationResolvers.ToList(), channelConfig);
            _dispatchStep = new ChangeDispatchingStep<TSynch>();
        }

        public bool IsSynchronizing => _isSynchronizing;

        /// <summary>
        /// Synchronize will only run once. If being called by another thread at this stage of development 
        /// it will simply ignore the call.
        /// </summary>
        /// <returns></returns>
        public async Task Synchronize()
        {
            // In this case it makes more sense to acquire a lock here instead of the sync.
            // Now still two syncs could potentially fire at the same time, even when checking the variable.
            await LockSync(async () =>
            {
                var t1DataEnum = await _t1DataSource();
                var t1Data = t1DataEnum.ToList();
                await Synchronize(t1Data);
            });
        }

        /// <summary>
        /// <p>
        /// Drains data from the <typeparamref name="T1"/> data source in a responsive observable collection. Usually
        /// for displaying purposes.
        /// </p>
        /// <p>
        /// Since this is a one way channel you can only drain data of the T1 end.
        /// </p>
        /// </summary>
        /// <param name="condition">Delegate decision to actually refresh/synchronize. Should not
        /// be responsiblity of the channel, and since we only need this in a single scenario this
        /// suffices.</param>
        /// <returns></returns>
        public async Task<AttachedObservableCollection<T1>> Drain(bool condition)
        {
            var t1DataEnum = await _t1DataSource();
            var t1Data = t1DataEnum.ToList();
            var attachedObservableCollection = new AttachedObservableCollection<T1>(t1Data, _channelConfig.Type1EndpointConfiguration.Endpoint);
            await LockSync(async () =>
            {
                if (condition)
                {
                    // Set before starting on other thread because reading thread might be faster.
                    _isSynchronizing = true;
                    // Do synch on background! Including awaiting second data.
                    Fire(Synchronize(t1Data));
                }
                await Task.FromResult(new object());
            });
            
            return await Task.FromResult(attachedObservableCollection);
        }

        public async void Fire(Task synchTask)
        {
            try
            {
                await Task.Run(() => synchTask);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Caught exception with Fire");
                // TODO: Now this back ground scenario will require a better error handling strategy.
                // This goes into the void. But it's running on the back ground and the caller might have
                // had already returned, so there is not much that can be done anyway.
                throw ex;
            }
        }

        protected async Task Synchronize(IEnumerable<T1> input)
        {
            // Prevent synchronization of this channel to run on multiple threads.
/*            if (await _lockSemaphore.WaitAsync(0))
            {
                await Task.Delay(1000);
                //_isSynchronizing = true;
                try
                {*/
                    OnSynchronizationStart(new SynchronizationStarted(typeof(T1), typeof(T2)));

                    // TODO: We should either make sure this doesn't fail or otherwise wrap in a SynchronizationStartException...

                    // TODO: Awaiting the data source(s) should become part of the pipeline!
                    // This is hard however, and not neccesarily relevant since all should occur on another thread.
                    // Input    -> T1 Data Source.
                    // Output   -> SynchronizationResults.
                    // Refactor, do this first, only do remaining work in thread.
                    var t1Data = input;
                    var t2Data = await _t2DataSource().ConfigureAwait(false);
                    var pipeline = _channelConfig.ItemsPreprocessor(t1Data, t2Data);

                    pipeline = _synchItemListeners.Aggregate(pipeline, (current, listener) => current.Select(item =>
                    {
                        listener(item);
                        return item;
                    }));

                    int itemsProcessed = 0;
                    var endPipeline = pipeline
                        .Do(_ => itemsProcessed++)
                        .ResolveChange(_resolutionStep)
                        // Filter out NullSynchActions, which don't have an applicant instance.
                        .Where(action => action.Applicant != null)
                        .DispatchChange(_dispatchStep);

                    // Pump items out at the end of the sequence. In the end is probably responsibility of separate
                    // class.
                    int itemsSynchronized = 0;
                    try
                    {
                        foreach (SynchronizationResult result in endPipeline)
                        {
                            if (!result)
                            {
                                Debug.WriteLine("Failed executing an item.");
                            }
                            else
                            {
                                itemsSynchronized++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new ItemSynchronizationException("Synchronization of an item failed for an unknown reason.", ex, null);
                    }
                    OnSynchronizationFinished(new SynchronizationFinished(typeof(T1), typeof(T2), itemsProcessed, itemsSynchronized));
/*                }
                finally
                {
                    _isSynchronizing = false;
                    _lockSemaphore.Release();
                }
            }*/
        }

        private async Task LockSync(Func<Task> mechanism)
        {
            // Prevent synchronization of this channel to run on multiple threads.
            if (await _lockSemaphore.WaitAsync(0))
            {
                _isSynchronizing = true;
                await Task.Delay(1000);
                //_isSynchronizing = true;
                try
                {
                    await mechanism();
                }
                finally
                {
                    _isSynchronizing = false;
                    _lockSemaphore.Release();
                }
            }
        }

        // Sticking with same pattern for what you can call events. Though some can be part of the pipeline
                // while others are simply events on the channel. Under water events are simply used where appropriate.
        public void AddSynchItemObserver<T>([NotNull] Action<TSynch> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));
            _synchItemListeners.Add(observer);
        }

        public void AddSynchActionObserver([NotNull] Action<ISynchronizationAction<TSynch>> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));

            _resolutionStep.AddResultObserver(observer);
        }

        public void AddSynchronizationStartedObserver([NotNull] Action<SynchronizationStarted> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));
            SynchronizationStart += observer;
        }

        public void AddSynchronizationFinishedObserver(Action<SynchronizationFinished> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));
            SynchronizationFinished += observer;
        }

        protected virtual void OnSynchronizationStart(SynchronizationStarted eventArgs)
        {
            SynchronizationStart?.Invoke(eventArgs);
        }

        protected virtual void OnSynchronizationFinished(SynchronizationFinished eventArgs)
        {
            SynchronizationFinished?.Invoke(eventArgs);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _lockSemaphore.Dispose();
                _lockSemaphore = null;
            }
        }
    }
}