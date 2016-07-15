using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Restore.Channel
{
    public interface ISynchChannel<T1, T2> : ISynchChannel
    {
        void Push(IEnumerable<T2> items);
        void Push(IEnumerable<T1> items);
    }

    public interface ISynchChannel
    {
        [NotNull]
        Type Type1 { get; }

        [NotNull]
        Type Type2 { get; }

        // Synchronizes resources using given configuration.
        Task Synchronize();

        void AddChannelObserver(ChannelObserver observer);

        /// <summary>
        /// Event observer called when the synchronization of items starts on the channel.
        /// </summary>
        void AddSynchronizationStartedObserver([NotNull] Action<SynchronizationStarted> observer);

        /// <summary>
        /// Event observer called when the synchronization of items finished on the channel.
        /// </summary>
        void AddSynchronizationFinishedObserver([NotNull] Action<SynchronizationFinished> observer);

        /// <summary>
        /// Event observer called when the synchronization of items fails on the channel.
        /// </summary>
        void AddSynchronizationErrorObserver([NotNull] Action<SynchronizationError> observer);

        bool IsSynchronizing { get; }
    }
}