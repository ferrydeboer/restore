using System;
using System.Collections.Generic;

namespace Restore.RxProto
{
    public interface IDataEndpoint<T> : IDataChanges<T>
    {
        void Update(T resource);

        void Create(T resource);

        void Delete(T resource);

        T Get(Identifier id);

        IObservable<T> GetListAsync();

        IEnumerable<T> GetList();

        /// <summary>
        /// Resolves the 
        /// </summary>
        Func<T, Identifier> IdentityResolver { get; }

        IEnumerable<ISynchronizationAction<T>> SynchActions{ get; }

        void AddSyncAction(Func<T, bool> applies, Action<IDataEndpoint<T>, T> execution, string name = null);

        void AddSyncAction(Func<IDataEndpoint<T>, T, bool> applies, Action<IDataEndpoint<T>, T> execute, string name = null);

        void AddSyncAction(ISynchronizationAction<T> action);
    }
}