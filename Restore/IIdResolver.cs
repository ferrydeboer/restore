using System;

namespace Restore
{
    public interface IIdResolver<T, TId>
        where TId : IEquatable<TId>
    {
        TId Resolve(T item);
    }
}