using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Restore.ChangeResolution
{
    public static class ChangeResolverExt
    {
        public static IEnumerable<ISynchronizationAction<TSynch>> ResolveChange<TSynch>(this IEnumerable<TSynch> items, Func<TSynch, ISynchronizationAction<TSynch>> transformer)
        {
            // How to handle errors here? Probably need a way to catch them and dispatch them onto a handler?
            return items.Select(item => transformer(item));
        }

        public static IEnumerable<ISynchronizationAction<TSynch>> ResolveChange<TSynch, TCfg>(
            this IEnumerable<TSynch> items,
            [NotNull] ChangeResolutionStep<TSynch, TCfg> step)
        {
            if (step == null) { throw new ArgumentNullException(nameof(step)); }
            return step.Compose(items);
        }
    }
}