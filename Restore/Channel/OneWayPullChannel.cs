using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Restore.ChangeDispatching;
using Restore.ChangeResolution;

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
            var t1Data = await _t1DataSource();
            var t2Data = await _t2DataSource();
            var pipeline =_channelConfig.ItemsPreprocessor(t1Data, t2Data);

            pipeline = _synchItemListeners.Aggregate(pipeline, (current, listener) => current.Select(item =>
            {
                listener(item);
                return item;
            }));

            var endPipeline = pipeline
                .ResolveChange(_resolutionStep)
                .DispatchChange(_dispatchStep);

            foreach (SynchronizationResult result in endPipeline)
            {
                if (!result)
                {
                    Debug.WriteLine("Failed executing an item.");
                }
            }
        }

        public void AddSynchItemListener<T>([NotNull] Action<TSynch> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            _synchItemListeners.Add(action);
        }

        public void AddSynchActionListener([NotNull] Action<ISynchronizationAction<TSynch>> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _resolutionStep.AddResultObserver(action);
        }
    }
}