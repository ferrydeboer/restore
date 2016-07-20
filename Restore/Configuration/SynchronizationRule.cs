using System;
using Restore.ChangeResolution;
using Restore.Matching;

namespace Restore.Configuration
{
    /// <summary>
    /// Basically a class that provides mapping of strong typed rules to resolver/actions types that channels use internally.
    /// </summary>
    public abstract class SynchronizationRule<TBase1, TBase2, TId> : ISynchronizationRule<TBase1, TBase2, TId>
        where TId : IEquatable<TId>
    {
        protected SynchronizationRule(string name)
        {
            Name = name;
        }

        public string Name { get; }

        // Experiment to make type specific rule out of generic rule.
        public SynchronizationResolver<ItemMatch<TBase1, TBase2>, ISynchSourcesConfig<TBase1, TBase2, TId>> ResolverInstance(ISynchSourcesConfig<TBase1, TBase2, TId> cfg)
            // where TCfg : ISynchSourcesConfig<TBase1, TBase2, TId>
        {
            // Create instance of self with derived types.
            // Then again, passing the methods as a resolver will still result in calls on the same instance, which we might not want in the future.
            var derivedInstance = Activator.CreateInstance(GetType());
            var castedInstance = (SynchronizationRule<TBase1, TBase2, TId>)derivedInstance;

            return new SynchronizationResolver<ItemMatch<TBase1, TBase2>, ISynchSourcesConfig<TBase1, TBase2, TId>>(cfg, castedInstance.When, castedInstance.Then, Name);
        }

        public abstract bool When(ItemMatch<TBase1, TBase2> item, ISynchSourcesConfig<TBase1, TBase2, TId> cfg);

        public abstract SynchronizationResult Then(ItemMatch<TBase1, TBase2> item, ISynchSourcesConfig<TBase1, TBase2, TId> cfg);

        protected SynchronizationResult CreateResult(bool succes, string message)
        {
            return new SynchronizationResult(succes, message, Name);
        }
    }
}