using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Restore.ChangeDispatching
{
    public static class ChangeDispatchingExt
    {
        public static IEnumerable<SynchronizationResult> DispatchChange<TSynch>(
            this IEnumerable<ISynchronizationAction<TSynch>> items,
            [NotNull] ChangeDispatchingStep<TSynch> step)
        {
            if (step == null) { throw new ArgumentNullException(nameof(step)); }
            return step.Compose(items);
        }
    }
}
