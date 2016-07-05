using System;
using Restore.Configuration;

namespace Restore.Channel
{
    public interface IPlumberFactory/*<TBase1, TBase2, TId>
        where TId : IEquatable<TId>*/
    {
        IPlumber<T1, T2, TId> Create<T1, T2, TId>(ISynchSourcesConfig<T1, T2, TId> source, IRuleContainer<TId> rules)
            where TId : IEquatable<TId>;
    }
}