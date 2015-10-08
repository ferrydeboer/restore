using System;
using System.Collections.Generic;

namespace Restore
{
    public interface IDataEndpoint<T>
    {
        void Update(T testResource);

        void Create(T resource);

        void Delete(T resource);

        T Get(IIdentifier id);

        Func<T, IIdentifier> IdentityResolver { get; }

        IObservable<T> ResourceChanged { get; }

        IEnumerable<ISynchronizationAction<T>> SynchActions{ get; }

        void AddSyncAction(Func<T, bool> applies, Action<IDataEndpoint<T>, T> execution, string name = null);
    }
}