using System;

namespace Restore.RxProto
{
    public interface IDataChanges<out T>
    {
        IObservable<T> ResourceChanged { get; }
    }
}