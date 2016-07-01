using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Restore.Extensions;

namespace Restore
{
    public abstract class SynchronizationStep<TInput, TOutput>
    {
        [NotNull] private readonly List<Action<TOutput>> _afterStepObservers = new List<Action<TOutput>>();

        public void AddOutputObserver([NotNull] Action<TOutput> observer)
        {
            if (observer == null) { throw new ArgumentNullException(nameof(observer)); }

            _afterStepObservers.Add(observer);
        }

        public abstract TOutput Process(TInput input);

        public IEnumerable<TOutput> Compose(IEnumerable<TInput> input)
        {
            return _afterStepObservers.Aggregate(input.Select(Process), (current, observer) => current.Do(observer));
        }
    }
}