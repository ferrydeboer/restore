using System;

namespace Restore
{
    public interface IDataChanges<T>
    {
        IObservable<T> ResourceChanged { get; }
    }
}