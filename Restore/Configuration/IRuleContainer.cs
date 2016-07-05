using System;
using System.Collections.Generic;
using Restore.ChangeResolution;
using Restore.Matching;

namespace Restore.Configuration
{
    public interface IRuleContainer<TId>
        where TId : IEquatable<TId>
    {
        IEnumerable<ISynchronizationResolver<ItemMatch<T1, T2>>>
            GetTypedResolvers<T1, T2>(ISynchSourcesConfig<T1, T2, TId> sourcesConfig);

/*            where T1 : TBase1
            where T2 : TBase2;*/

        // void AddGenericRule<T>() where T : ISynchronizationRule<TBase1, TBase2, TId>, new();
    }
}