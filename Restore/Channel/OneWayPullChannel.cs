using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Restore.ChangeDispatching;
using Restore.ChangeResolution;
using Restore.Extensions;

namespace Restore.Channel
{
    public class OneWayPullChannel<T1, T2, TId, TSynch> : ISynchChannel<T1, T2, TSynch> where TId : IEquatable<TId>
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

        private event Action<SynchronizationStart> _synchronizationStart;
        private event Action<SynchronizationFinished> _synchronizationFinished;

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

        public async Task Synchronize()
        {
            // Here we have to be careful about thread safety. It's one instance. There is no
            // point in having two synchronizations of the same resource running simultaneously/overlapping.

            // TODO: We should either make sure this doesn't fail or otherwise wrap in a SynchronizationStartException...
            var t1Data = await _t1DataSource();
            var t2Data = await _t2DataSource();
            var pipeline =_channelConfig.ItemsPreprocessor(t1Data, t2Data);

            pipeline = _synchItemListeners.Aggregate(pipeline, (current, listener) => current.Select(item =>
            {
                listener(item);
                return item;
            }));

            int itemsProcessed = 0;
            var endPipeline = pipeline
                .Do(_ => itemsProcessed++)
                .ResolveChange(_resolutionStep)
                // Filter out NullSynchActions
                .Where(action => action.Applicant != null)
                .DispatchChange(_dispatchStep);

            OnSynchronizationStart(new SynchronizationStart(typeof(T1), typeof(T2)));
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

        public void AddSynchronizationStartedObserver([NotNull] Action<SynchronizationStart> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));
            _synchronizationStart += observer;
        }

        public void OnSynchronizationStart(SynchronizationStart eventArgs)
        {
            _synchronizationStart?.Invoke(eventArgs);
        }

        public void AddSynchronizationFinishedObserver(Action<SynchronizationFinished> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));
            _synchronizationFinished += observer;
        }

        protected virtual void OnSynchronizationFinished(SynchronizationFinished eventArgs)
        {
            _synchronizationFinished?.Invoke(eventArgs);
        }
    }

    
    public class SynchronizationFinished : SynchronizationStart
    {
        public SynchronizationFinished(Type type1, Type type2, int itemsProcessed, int itemsSynchronized) : base(type1, type2)
        {
            ItemsProcessed = itemsProcessed;
            ItemsSynchronized = itemsSynchronized;
        }

        public int ItemsProcessed { get; }

        public int ItemsSynchronized { get; }
    }
}