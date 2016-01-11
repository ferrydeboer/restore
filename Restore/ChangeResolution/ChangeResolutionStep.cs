using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Restore.RxProto;

namespace Restore.ChangeResolution
{
    public class ChangeResolutionStep<TItem, TCfg>
    {
        [NotNull] private readonly IList<IChangeResolver<TItem>> _resolvers;
        [NotNull] private readonly TCfg _configuration;

        public ChangeResolutionStep([NotNull] IList<IChangeResolver<TItem>> resolvers, [NotNull] TCfg configuration)
        {
            if (resolvers == null) throw new ArgumentNullException(nameof(resolvers));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _resolvers = resolvers;
            _configuration = configuration;
        }

        public ISynchronizationAction<TItem> Resolve(TItem item)
        {
            foreach (var changeResolver in _resolvers)
            {
                var synchronizationAction = changeResolver.Resolve(item);
                if (synchronizationAction != null)
                {
                    return synchronizationAction;
                }
            }

            return new NullSynchAction<TItem>();
        }
    }
}