using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Restore.ChangeResolution;
using Restore.Channel;
using Restore.Matching;

namespace Restore.Configuration
{
    public interface IChannelSetup<T1, T2> : IChannelSetupCreationEvent
    {
        ITranslator<T1, T2> TypeTranslator { get; }

        Func<Task<IEnumerable<T1>>> ListProvider1 { get; }
        Func<Task<IEnumerable<T2>>> ListProvider2 { get; }

        /// <summary>
        /// Factory Method that created the actual channel implementation. Allow for the use of inherited channels with provide a set of standard behaviour.
        /// </summary>
        /// <typeparam name="TId">The Id type of the channel to create.</typeparam>
        /// <param name="config">The configuration that the channel will work on.</param>
        /// <returns>an Plumber instance for the given types.</returns>
        ISynchChannel<T1, T2, ItemMatch<T1, T2>> Create<TId>(
            IChannelConfiguration<T1, T2, TId, ItemMatch<T1, T2>> config,
            IPlumber<T1, T2, TId> plumber)
            where TId : IEquatable<TId>;
    }

    public interface IChannelSetupCreationEvent
    {
        void AddCreationObserver(IChannelCreationObserver observer);
    }
}