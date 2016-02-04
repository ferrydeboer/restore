using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Restore.Channel
{
    public interface ISynchChannel<T1, T2, TSynch> : ISynchChannel
    {
        void AddSynchItemObserver<T>(Action<TSynch> observer);
    }

    public interface ISynchChannel
    {
        // Synchronizes resources using given configuration.
        Task Synchronize();

        /// <summary>
        /// Event observer called when the synchronization of items starts on this channel.
        /// </summary>
        void AddSynchronizationStartedObserver([NotNull] Action<SynchronizationStarted> observer);

        /// <summary>
        /// Event observer called when the synchronization of items finished on this channel.
        /// </summary>
        void AddSynchronizationFinishedObserver([NotNull] Action<SynchronizationFinished> observer);

        // TODO: Error handling policies neccesary for a rollback.
    }
}