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

    public class Test
    {
        public void Bla()
        {
            var builder = new PipelineBuilder();
            builder.StartWith(new TestStep<object>()).Then();
        }
    }

    public class TestStep<T> : IPipelineStep<T>
    {
        public IPipelineStep<TOut> Then<TOut>(IPipelineStep<TOut> next)
        {
            throw new NotImplementedException();
        }
    }

    public class PipelineBuilder
    {
        private List<IPipelineStep> steps = new List<IPipelineStep>();

        public IPipelineStep<TIn> StartWith<TIn>(IPipelineStep<TIn> start)
        {
            steps.Add(start);
            return start;
        }
    }

    public interface IPipelineStep
    {
    }

    public interface IPipelineStep<TIn> : IPipelineStep
    {
        IPipelineStep<TOut> Then<TOut>(IPipelineStep<TOut> next);
    }
}