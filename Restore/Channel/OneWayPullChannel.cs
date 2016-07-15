using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Restore.ChangeDispatching;
using Restore.ChangeResolution;
using Restore.Extensions;
using Restore.Matching;

namespace Restore.Channel
{
    public class OneWayPullChannel<T1, T2, TId, TSynch>
        : ISynchChannel<T1, T2>, IOneWayPullChannel<T1>, IDisposable
        where TId : IEquatable<TId>
    {
        public Type Type1 { get; } = typeof(T1);

        public Type Type2 { get; } = typeof(T2);

        [NotNull]
        public ISynchSourcesConfig<T1, T2, TId> ChannelConfig { get; }

        /// <summary>
        /// Didn't want to make this part of the more general configuration. It's not decided yet how to further work with data sources
        /// and possible replication.
        /// </summary>
        [NotNull] private readonly Func<Task<IEnumerable<T1>>> _t1DataSource;

        [NotNull] private readonly Func<Task<IEnumerable<T2>>> _t2DataSource;

        private event Action<SynchronizationStarted> SynchronizationStart;
        private event Action<SynchronizationFinished> SynchronizationFinished;
        private event Action<SynchronizationError> SynchronizationError;

        private SemaphoreSlim _lockSemaphore = new SemaphoreSlim(1);
        private bool _isSynchronizing;

        public OneWayPullChannel(
            [NotNull] ISynchSourcesConfig<T1, T2, TId> channelConfig,
            [NotNull] IPlumber<T1, T2, TId> plumber,
            [NotNull] Func<Task<IEnumerable<T1>>> t1DataSource,
            [NotNull] Func<Task<IEnumerable<T2>>> t2DataSource)
        {
            if (plumber == null) { throw new ArgumentNullException(nameof(plumber)); }
            if (t1DataSource == null) { throw new ArgumentNullException(nameof(t1DataSource)); }
            if (t2DataSource == null) { throw new ArgumentNullException(nameof(t2DataSource)); }

            ChannelConfig = channelConfig;
            Plumber = plumber;
            _t1DataSource = t1DataSource;
            _t2DataSource = t2DataSource;
        }

        public bool IsSynchronizing => _isSynchronizing;

        public IPlumber<T1, T2, TId> Plumber { get; protected set; }

        public void AddChannelObserver([NotNull] ChannelObserver observer)
        {
            if (observer == null) { throw new ArgumentNullException(nameof(observer)); }

            AddSynchronizationStartedObserver(observer.OnStarted);
            AddSynchronizationFinishedObserver(observer.OnFinished);
            AddSynchronizationErrorObserver(observer.OnError);
        }

        /// <summary>
        /// Synchronize will only run once. If being called by another thread at this stage of development
        /// it will simply ignore the call.
        /// </summary>
        public async Task Synchronize()
        {
            // In this case it makes more sense to acquire a lock here instead of the sync.
            // Now still two syncs could potentially fire at the same time, even when checking the variable.
            await LockSync(async () =>
            {
                var t1DataEnum = await _t1DataSource();
                await Synchronize(t1DataEnum);
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
        public async Task<AttachedObservableCollection<T1>> Drain(bool condition)
        {
            var t1DataEnum = await _t1DataSource();
            var t1Data = t1DataEnum.ToList();
            var attachedObservableCollection = new AttachedObservableCollection<T1>(t1Data, ChannelConfig.Type1EndpointConfiguration.Endpoint);
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

        /// <summary>
        /// Runs the given task asynchronously.
        /// </summary>
        public async void Fire(Task synchTask)
        {
            try
            {
                await Task.Run(() => synchTask);
            }
            catch (Exception ex)
            {
                if (!OnError(ex))
                {
                    throw ex; // This never arrives anywhere given this method is async void!
                }
            }
        }

        /// <summary>
        /// Called when an error occurs in the synchronization process. Returns true if the error is handled.
        /// </summary>
        /// <param name="exception">The exception that was thrown.</param>
        /// <returns>True if the error is handled.</returns>
        protected virtual bool OnError(Exception exception)
        {
            // First pass to handlers
            var synchronizationError = new SynchronizationError(typeof(T1), typeof(T2), exception);
            OnSynchronizationError(synchronizationError);
            return synchronizationError.IsHandled;
        }

        protected async Task Synchronize(IEnumerable<T1> input)
        {
            await Synchronize(BuildSynchPipeline(input));
/*            OnSynchronizationStart(new SynchronizationStarted(typeof(T1), typeof(T2)));

            // The problem with this type of error handling is that is does not allow us ignore errors and continue enumeration.
            // However, it should be up to the implementor to distinguish expected failures like validation from actual exceptions
            // that imply there is something really wrong.
            try
            {
                var pipeline = await BuildSynchPipeline(input);

                // Pump items out at the end of the sequence. In the end is probably responsibility of separate
                // class.
                foreach (SynchronizationResult result in pipeline)
                {
                    // Only once synchroniazation is completely done we can actually 100 % sure
                    // evaluate succes on the SynchronizationResults. For instance, deleting expenses is supported as a batch/bulk operation.
                    // This is endpoint specific. This results can essentially be inconclusive.
                    if (!result)
                    {
                        Debug.WriteLine("Failed executing an item.");
                    }
                    else
                    {
                        pipeline.ItemsSynchronized++;
                    }
                }

                // This can fail because it for instance runs a transaction. Then what?
                // Should we instead always raise a synchronization finished regardless of the outcome?
                // Only when this is really relevant we can further decide on this.
                OnSynchronizationFinished(new SynchronizationFinished(typeof(T1), typeof(T2), pipeline.ItemsProcessed, pipeline.ItemsSynchronized));
            }
            catch (SynchronizationException ex)
            {
                if (!OnError(ex))
                {
                    // Let already wrapped exception through!
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (!OnError(ex))
                {
                    // This simply end up in a void of another thread. Besides, It can wrap an already existing SynchronizationException
                    throw new ItemSynchronizationException("Synchronization of an item failed for an unknown reason.", ex, null);
                }
            }*/
        }

        protected async Task Synchronize(Task<SynchPipeline> pipelineTask)
        {
            OnSynchronizationStart(new SynchronizationStarted(typeof(T1), typeof(T2)));

            // The problem with this type of error handling is that is does not allow us ignore errors and continue enumeration.
            // However, it should be up to the implementor to distinguish expected failures like validation from actual exceptions
            // that imply there is something really wrong.
            try
            {
                var pipeline = await pipelineTask;
                // Pump items out at the end of the sequence. In the end is probably responsibility of separate
                // class.
                foreach (SynchronizationResult result in pipeline)
                {
                    // Only once synchroniazation is completely done we can actually 100 % sure
                    // evaluate succes on the SynchronizationResults. For instance, deleting expenses is supported as a batch/bulk operation.
                    // This is endpoint specific. This results can essentially be inconclusive.
                    if (!result)
                    {
                        Debug.WriteLine("Failed executing an item.");
                    }
                    else
                    {
                        pipeline.ItemsSynchronized++;
                    }
                }

                // This can fail because it for instance runs a transaction. Then what?
                // Should we instead always raise a synchronization finished regardless of the outcome?
                // Only when this is really relevant we can further decide on this.
                OnSynchronizationFinished(new SynchronizationFinished(typeof(T1), typeof(T2), pipeline.ItemsProcessed, pipeline.ItemsSynchronized));
            }
            catch (SynchronizationException ex)
            {
                if (!OnError(ex))
                {
                    // Let already wrapped exception through!
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (!OnError(ex))
                {
                    // This simply end up in a void of another thread. Besides, It can wrap an already existing SynchronizationException
                    throw new ItemSynchronizationException("Synchronization of an item failed for an unknown reason.", ex, null);
                }
            }
        }

        private async Task<SynchPipeline> BuildSynchPipeline(IEnumerable<T1> t1Data)
        {
            if (t1Data == null)
            {
                throw new SynchronizationException("Data source 1 delivered a null result!");
            }

            // TODO: Should awaiting the data source(s) become part of the pipeline?
            IEnumerable<T2> t2Data;
            try
            {
                t2Data = await _t2DataSource().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new SynchronizationException($"Retrieving data from Data Source 2 failed with message: \"{ex.Message}\"", ex);
            }

            if (t2Data == null)
            {
                throw new SynchronizationException("Data source 2 delivered a null result!");
            }

            // return synchPipeline;
            return Plumber.CreatePipeline(t1Data, t2Data, ChannelConfig);
        }

        private SynchPipeline BuildPushPipeline(IEnumerable<T2> t2Data)
        {
            if (t2Data == null)
            {
                throw new SynchronizationException("Data source 2 delivered a null result!");
            }

            IEnumerable<T1> t1Data = new List<T1>();

            // return synchPipeline;
            return Plumber.CreatePipeline(t1Data, t2Data, ChannelConfig);
        }

        private async Task LockSync(Func<Task> mechanism)
        {
            // Prevent synchronization of this channel to run on multiple threads.
            if (await _lockSemaphore.WaitAsync(0))
            {
                _isSynchronizing = true;
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

        public void AddSynchronizationStartedObserver(Action<SynchronizationStarted> observer)
        {
            if (observer == null) { throw new ArgumentNullException(nameof(observer)); }
            SynchronizationStart += observer;
        }

        public void AddSynchronizationFinishedObserver(Action<SynchronizationFinished> observer)
        {
            if (observer == null) { throw new ArgumentNullException(nameof(observer)); }
            SynchronizationFinished += observer;
        }

        public void AddSynchronizationErrorObserver(Action<SynchronizationError> observer)
        {
            if (observer == null) { throw new ArgumentNullException(nameof(observer)); }
            SynchronizationError += observer;
        }

        protected virtual void OnSynchronizationStart(SynchronizationStarted eventArgs)
        {
            SynchronizationStart?.Invoke(eventArgs);
        }

        protected virtual void OnSynchronizationFinished(SynchronizationFinished eventArgs)
        {
            SynchronizationFinished?.Invoke(eventArgs);
        }

        protected virtual void OnSynchronizationError(SynchronizationError eventArgs)
        {
            SynchronizationError?.Invoke(eventArgs);
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

        public void Push(IEnumerable<T2> items)
        {
            // Assume pipeline knows how to complete!
            Synchronize(Task.FromResult(BuildPushPipeline(items))).Wait();
        }

        public void Push(IEnumerable<T1> items)
        {
            throw new NotImplementedException();
        }
    }

    public class ItemMatchPipelinePlumber<T1, T2, TId> : IPlumber<T1, T2, TId>
        where TId : IEquatable<TId>
    {
        private readonly Func<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<ItemMatch<T1, T2>>> _preprocessor;

        [NotNull]
        [ItemNotNull]
        private readonly IList<Action<ItemMatch<T1, T2>>> _synchItemListeners = new List<Action<ItemMatch<T1, T2>>>();

        [NotNull]
        private readonly ChangeResolutionStep<ItemMatch<T1, T2>, ISynchSourcesConfig<T1, T2, TId>> _resolutionStep;

        [NotNull]
        private readonly ChangeDispatchingStep<ItemMatch<T1, T2>> _dispatchStep;

        public ItemMatchPipelinePlumber(
            ISynchSourcesConfig<T1, T2, TId> sourceConfig,
            IList<ISynchronizationResolver<ItemMatch<T1, T2>>> synchronizationResolvers,
            Func<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<ItemMatch<T1, T2>>> preprocessor)
        {
            _preprocessor = preprocessor;
            _resolutionStep = new ChangeResolutionStep<ItemMatch<T1, T2>, ISynchSourcesConfig<T1, T2, TId>>(synchronizationResolvers, sourceConfig);
            _dispatchStep = new ChangeDispatchingStep<ItemMatch<T1, T2>>();
        }

        public IPreprocessorAppender Appender { get; set; }

        public SynchPipeline CreatePipeline(IEnumerable<T1> source1, IEnumerable<T2> source2, ISynchSourcesConfig<T1, T2, TId> sourcesConfig)
        {
            IEnumerable<ItemMatch<T1, T2>> pipeline = CreateInlet(source1, source2, sourcesConfig);

            if (Appender != null)
            {
                pipeline = Appender.Append(sourcesConfig, pipeline);
            }

            pipeline = _synchItemListeners.Aggregate(pipeline, (current, listener) => current.Select(item =>
            {
                listener(item);
                return item;
            }));

            // This is an odd construct where the pipeline has to be constructed because it needs a counter in the do.
            var synchPipeline = new SynchPipeline();
            var endPipeline = pipeline
                .Do(_ => synchPipeline.ItemsProcessed++)
                .ResolveChange(_resolutionStep)
                .Where(action => action.Applicant != null) // Filter out NullSynchActions, which don't have an applicant instance, why not put it in the step itself?
                .DispatchChange(_dispatchStep);
            synchPipeline.Pipeline = endPipeline;

            return synchPipeline;
        }

        private IEnumerable<ItemMatch<T1, T2>> CreateInlet(IEnumerable<T1> source1, IEnumerable<T2> source2, ISynchSourcesConfig<T1, T2, TId> sourcesConfig)
        {
            IEnumerable<ItemMatch<T1, T2>> inlet;
            try
            {
                inlet = _preprocessor(source1, source2);
            }
            catch (Exception ex)
            {
                throw new SynchronizationException($"Provided items preprocessor failed with message: \"{ex.Message}\"", ex);
            }

            return inlet;
        }

        public void AddSynchActionObserver([NotNull] Action<ISynchronizationAction<ItemMatch<T1, T2>>> observer)
        {
            if (observer == null) { throw new ArgumentNullException(nameof(observer)); }

            _resolutionStep.AddOutputObserver(observer);
        }

        public void AddSynchResultObserver([NotNull] Action<SynchronizationResult> observer)
        {
            if (observer == null) { throw new ArgumentNullException(nameof(observer)); }

            _dispatchStep.AddOutputObserver(observer);
        }

        public void AddSynchItemObserver(Action<ItemMatch<T1, T2>> action)
        {
            _synchItemListeners.Add(action);
        }
    }

    public interface ISynchPipeline : IEnumerable<SynchronizationResult>
    {
        int ItemsProcessed { get; set; }

        int ItemsSynchronized { get; set; }
    }

    public class SynchPipeline : ISynchPipeline
    {
        private int _itemsSynchronized;
        public int ItemsProcessed { get; set; }

        public int ItemsSynchronized
        {
            get { return _itemsSynchronized; }
            set
            {
                Debug.WriteLine($"{GetHashCode()} - Raising ItemsSycnhronized from {_itemsSynchronized} to {value}");
                _itemsSynchronized = value;
            }
        }

        public IEnumerable<SynchronizationResult> Pipeline { private get; set; }

        public IEnumerator<SynchronizationResult> GetEnumerator()
        {
            return Pipeline.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}