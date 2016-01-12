using System;
using System.Collections.Generic;
using System.Linq;
using Restore.RxProto;

namespace Restore.ChangeResolution
{
    public static class ChangeResolverExt
    {
        public static IEnumerable<ISynchronizationAction<TSynch>> ResolveChange<TSynch>(this IEnumerable<TSynch> items, Func<TSynch, ISynchronizationAction<TSynch>> transformer)
        {
            // How to handle errors here? Probably need a way to catch them and dispatch them onto a handler?
            return items.Select(item => transformer(item));
        }

        public static IEnumerable<ISynchronizationAction<TSynch>> ResolveChange<TSynch, TCfg>(this IEnumerable<TSynch> items, ChangeResolutionStep<TSynch, TCfg> step)
        {
            return step.Compose(items);
        }
    }
}