using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Restore.ChangeResolution;
using Restore.Channel;
using Restore.Matching;

namespace Restore.Configuration
{
    public class ChannelSetup<T1, T2> : IChannelSetup<T1, T2>
    {
        private readonly IList<IChannelCreationObserver> _creationObservers = new List<IChannelCreationObserver>();

        public ChannelSetup(ITranslator<T1, T2> translator, Func<Task<IEnumerable<T1>>> listProvider1, Func<Task<IEnumerable<T2>>> listProvider2)
        {
            ListProvider1 = listProvider1;
            ListProvider2 = listProvider2;
            TypeTranslator = translator;
        }

        public Func<Task<IEnumerable<T1>>> ListProvider1 { get; }

        public Func<Task<IEnumerable<T2>>> ListProvider2 { get; }

        public ITranslator<T1, T2> TypeTranslator { get; }

        public IEnumerable<IChannelCreationObserver> CreationObservers => _creationObservers;

        public void AddCreationObserver(IChannelCreationObserver observer)
        {
            _creationObservers.Add(observer);
        }

        public ISynchChannel<T1, T2/*, ItemMatch<T1, T2>*/> Create<TId>(
            IChannelConfiguration<T1, T2, TId, ItemMatch<T1, T2>> config,
            IPlumber<T1, T2, TId> plumber)
            where TId : IEquatable<TId>
        {
            var channel = DoCreate(config, plumber);
            foreach (var channelCreationObserver in CreationObservers)
            {
                channelCreationObserver.Created(channel);
            }

            return channel;
        }

        protected virtual ISynchChannel<T1, T2/*, ItemMatch<T1, T2>*/> DoCreate<TId>(
            IChannelConfiguration<T1, T2, TId, ItemMatch<T1, T2>> config,
            IPlumber<T1, T2, TId> plumber)
            where TId : IEquatable<TId>
        {
            return new OneWayPullChannel<T1, T2, TId, ItemMatch<T1, T2>>(config, plumber, ListProvider1, ListProvider2);
        }
    }
}