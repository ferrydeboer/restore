﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Restore.ChangeResolution
{
    public class ChangeResolutionStepOld<TItem, TCfg> // : SynchronizationStep<TItem, ISynchronizationAction<TItem>>
    {
        [NotNull] [ItemNotNull]
        private readonly IList<ISynchronizationResolver<TItem>> _resolvers;

        [NotNull] private readonly IList<Action<ISynchronizationAction<TItem>>> _observers = new List<Action<ISynchronizationAction<TItem>>>();
        [NotNull] private readonly TCfg _configuration;

        public ChangeResolutionStepOld([NotNull] IList<ISynchronizationResolver<TItem>> resolvers, [NotNull] TCfg configuration)
        {
            if (resolvers == null) { throw new ArgumentNullException(nameof(resolvers)); }
            if (configuration == null) { throw new ArgumentNullException(nameof(configuration)); }

            _resolvers = resolvers;
            _configuration = configuration;
        }

        public ISynchronizationAction<TItem> Resolve(TItem item)
        {
            try
            {
                foreach (var changeResolver in _resolvers)
                {
                    var synchronizationAction = changeResolver.Resolve(item);

                    // Could implement some debugginglogic that will tell if there are other applicable steps
                    // That might indicate a configuration error.
                    if (synchronizationAction != null)
                    {
                        return synchronizationAction;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ChangeResolutionException(
                    $"Failed to resolve change for {item}",
                    ex,
                    item);
            }

            // No Resolution/Synch is required.
            return new NullSynchAction<TItem>();
        }

        public void AddResultObserver([NotNull] Action<ISynchronizationAction<TItem>> action)
        {
            if (action == null) { throw new ArgumentNullException(nameof(action)); }

            _observers.Add(action);
        }

        // TODO This should be generalizable by creating a step with TInput & TOutput
        public IEnumerable<ISynchronizationAction<TItem>> Compose(IEnumerable<TItem> input)
        {
            var pipeline = input.Select(Resolve);
            pipeline = _observers.Aggregate(pipeline, (current, observer) => current.Select(item =>
            {
                // TODO: Maybe we should prevent observers from braking the chain?
                // We might still need a logging mechanism to at least notify about this.
                observer(item);
                return item;
            }));

            return pipeline;
        }
    }

    public class ChangeResolutionStep<TItem, TCfg> : SynchronizationStep<TItem, ISynchronizationAction<TItem>>
    {
        [NotNull]
        [ItemNotNull]
        private readonly IList<ISynchronizationResolver<TItem>> _resolvers;

        [NotNull]
        private readonly TCfg _configuration;

        public ChangeResolutionStep([NotNull] IList<ISynchronizationResolver<TItem>> resolvers, [NotNull] TCfg configuration)
        {
            if (resolvers == null) { throw new ArgumentNullException(nameof(resolvers)); }
            if (configuration == null) { throw new ArgumentNullException(nameof(configuration)); }

            _resolvers = resolvers;
            _configuration = configuration;
        }

        protected ISynchronizationAction<TItem> Resolve(TItem item)
        {
            try
            {
                foreach (var changeResolver in _resolvers)
                {
                    var synchronizationAction = changeResolver.Resolve(item);

                    // Could implement some debugginglogic that will tell if there are other applicable steps
                    // That might indicate a configuration error.
                    if (synchronizationAction != null)
                    {
                        return synchronizationAction;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ChangeResolutionException(
                    $"Failed to resolve change for {item}",
                    ex,
                    item);
            }

            // No Resolution/Synch is required.
            return new NullSynchAction<TItem>();
        }

        public override ISynchronizationAction<TItem> Process(TItem input)
        {
            return Resolve(input);
        }
    }
}