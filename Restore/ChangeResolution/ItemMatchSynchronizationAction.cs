using System;
using Restore.Matching;
using Restore.RxProto;

namespace Restore.ChangeResolution
{
    public class ItemMatchSynchronizationAction<T1, T2, TId, TSynch> : ISynchronizationAction<ItemMatch<T1, T2>> where TId : IEquatable<TId>
    {
        private IChannelConfiguration<T1, T2, TId, TSynch> _channelConfig;
        private readonly Func<ItemMatch<T1, T2>, IChannelConfiguration<T1, T2, TId, TSynch>, bool> _decision;
        private readonly Func<ItemMatch<T1, T2>, IChannelConfiguration<T1, T2, TId, TSynch>, SynchronizationResult> _action;
        private ItemMatch<T1, T2> _applicant;
        private SynchronizationResult _synchronizationResult;

        public ItemMatchSynchronizationAction(
            IChannelConfiguration<T1, T2, TId, TSynch> channelConfig, 
            Func<ItemMatch<T1, T2>, IChannelConfiguration<T1, T2, TId, TSynch>, bool> decision, 
            Func<ItemMatch<T1, T2>, IChannelConfiguration<T1, T2, TId, TSynch>, SynchronizationResult> action,
            string name = "Undefined")
        {
            _channelConfig = channelConfig;
            _decision = decision;
            _action = action;
            Name = name;
        }


        public bool AppliesTo(ItemMatch<T1, T2> item)
        {
            var appliesTo = _decision(item, _channelConfig);
            if (appliesTo)
            {
                _applicant = item;
            }
            return appliesTo;
        }

        // Possible currying variant of this. The only issue with this is that it looses most of it's context
        // and thus makes it harder to later on determine potential execution rules. For now let's stick with
        // the interface variant though more error prone.
        // An alternative better way to solve this is in the end change this is such a way that we don't return
        // a Func but just another object with the execute action and the state. Simple as that.
        public Func<SynchronizationResult> Applies(ItemMatch<T1, T2> item)
        {
            var appliesTo = _decision(item, _channelConfig);
            if (appliesTo)
            {
                return () => _action(item, _channelConfig);
            }
            return null;
        }

        public SynchronizationResult Execute()
        {
            if (_applicant == null)
            {
                throw new InvalidOperationException("Can't execute this synchronization action since there is no applicant.");
            }
            _synchronizationResult = _action(_applicant, _channelConfig);
            // Because the action contains state it can not be executed twice. Only other option is actually
            // returning a curried function from the AppliesTo.
            _applicant = null;
            return _synchronizationResult;
        }

        public ItemMatch<T1, T2> Applicant => _applicant;

        public string Name { get; }
    }
}