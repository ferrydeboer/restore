using System;
using Restore.Matching;

namespace Restore.Configuration
{
    public interface ISynchronizationRule<T1, T2, TId>
        where TId : IEquatable<TId>
    {
        bool When(ItemMatch<T1, T2> item, ISynchSourcesConfig<T1, T2, TId> cfg);

        SynchronizationResult Then(ItemMatch<T1, T2> item, ISynchSourcesConfig<T1, T2, TId> cfg);
    }
}