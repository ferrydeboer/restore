using System;
using JetBrains.Annotations;
using Restore.RxProto;

namespace Restore.ChangeResolution
{
    public class ChangeResolver<T, TCfg> : IChangeResolver<T>
    {
        [NotNull] private readonly TCfg _config;
        [NotNull] private readonly Func<T, TCfg, bool> _decision;
        [NotNull] private readonly Func<T, TCfg, SynchronizationResult> _action;

        public ChangeResolver(
            TCfg config,
            [NotNull] Func<T, TCfg, bool> decision, 
            [NotNull] Func<T, TCfg, SynchronizationResult> action)
        {
            if (decision == null) throw new ArgumentNullException(nameof(decision));
            if (action == null) throw new ArgumentNullException(nameof(action));
            _config = config;
            _decision = decision;
            _action = action;
        }

        public ISynchronizationAction<T> Resolve(T item)
        {
            if (_decision(item, _config))
            {
                return new SynchronizationAction<T, TCfg>(_config, _action, item);
            }
            return null;
        }
    }
}