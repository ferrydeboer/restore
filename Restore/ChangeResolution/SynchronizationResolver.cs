using System;
using JetBrains.Annotations;

namespace Restore.ChangeResolution
{
    public class SynchronizationResolver<T, TCfg> : ISynchronizationResolver<T>
    {
        [NotNull] private readonly TCfg _config;
        [NotNull] private readonly Func<T, TCfg, bool> _decision;
        [NotNull] private readonly Func<T, TCfg, SynchronizationResult> _action;
        [NotNull] private readonly string _name;

        public SynchronizationResolver(
            TCfg config,
            [NotNull] Func<T, TCfg, bool> decision,
            [NotNull] Func<T, TCfg, SynchronizationResult> action,
            string name = "Undefined")
        {
            if (decision == null) { throw new ArgumentNullException(nameof(decision)); }
            if (action == null) { throw new ArgumentNullException(nameof(action)); }
            _config = config;
            _decision = decision;
            _action = action;
            _name = name;
        }

        public ISynchronizationAction<T> Resolve(T item)
        {
            if (_decision(item, _config))
            {
                return new SynchronizationAction<T, TCfg>(_config, _action, item, _name);
            }

            return null;
        }
    }
}