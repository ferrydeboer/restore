using System;
using System.Collections.Generic;
using Restore.ChangeResolution;
using Restore.Matching;

namespace Restore.Configuration
{
    public class RuleContainer<TBase1, TBase2, TId> : IRuleContainer<TId>
        where TId : IEquatable<TId>
    {
        private readonly IList<Type> _rules = new List<Type>();

        public void AddGenericRule<T>()
            where T : ISynchronizationRule<TBase1, TBase2, TId>, new()
        {
            _rules.Add(typeof(T));
        }

        public IEnumerable<ISynchronizationResolver<ItemMatch<T1, T2>>>
            GetTypedResolvers<T1, T2>(ISynchSourcesConfig<T1, T2, TId> sourcesConfig)
/*            where T1 : TBase1
            where T2 : TBase2*/
        {
            foreach (Type rule in _rules)
            {
                var genericTypeDef = rule.GetGenericTypeDefinition();
                var closedGenericTypeDef = genericTypeDef.MakeGenericType(typeof(T1), typeof(T2), typeof(TId));
                var ruleInstance = Activator.CreateInstance(closedGenericTypeDef) as SynchronizationRule<T1, T2, TId>;
                if (ruleInstance != null)
                {
                    yield return ruleInstance.ResolverInstance(sourcesConfig);
                }
            }
        }
    }
}