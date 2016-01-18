using System;

namespace Restore.Channel
{
    public class SynchronizationFinished : SynchronizationStarted
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