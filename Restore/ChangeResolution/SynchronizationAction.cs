using System;
using JetBrains.Annotations;

namespace Restore.ChangeResolution
{
    public class SynchronizationAction<T, TCfg> : ISynchronizationAction<T>
    {
        [NotNull] private readonly TCfg _config;
        [NotNull] private readonly Func<T, TCfg, SynchronizationResult> _action;
        [NotNull] private readonly T _applicant;

        public SynchronizationAction(
            [NotNull] TCfg config,
            [NotNull] Func<T, TCfg, SynchronizationResult> action,
            T applicant,
            string name = "Undefined")
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }
            if (action == null) { throw new ArgumentNullException(nameof(action)); }

            _config = config;
            _action = action;
            _applicant = applicant;
            Name = name;
        }

        public bool AppliesTo(T item)
        {
            throw new NotImplementedException();
        }

        public SynchronizationResult Execute()
        {
            return _action(_applicant, _config);
        }

        public T Applicant => _applicant;

        public string Name { get; }
    }
}