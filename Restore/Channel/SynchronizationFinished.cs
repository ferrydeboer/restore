using System;
using System.Collections.Generic;

namespace Restore.Channel
{
    public class SynchronizationFinished : SynchronizationEvent
    {
        public SynchronizationFinished(Type type1, Type type2, int itemsProcessed, int itemsSynchronized)
            : base(type1, type2)
        {
            ItemsProcessed = itemsProcessed;
            ItemsSynchronized = itemsSynchronized;
        }

        internal SynchronizationFinished(Type type1, Type type2, ISynchPipeline pipeline, IList<SynchronizationResult> results)
            : base(type1, type2)
        {
            ItemsProcessed = pipeline.ItemsProcessed;
            ItemsSynchronized = pipeline.ItemsSynchronized;
            ItemsFailed = pipeline.ItemsFailed;
            Results = results;
        }

        public int ItemsProcessed { get; }

        public int ItemsSynchronized { get; }

        public int ItemsFailed { get; }

        public IList<SynchronizationResult> Results { get; }
    }
}