using System;
using System.Collections.Generic;

namespace Restore.RxProto
{
    public abstract class DataEndpoint<T> : IDataEndpoint<T>
    {
        private readonly IList<ISynchronizationAction<T>> _synchActions = new List<ISynchronizationAction<T>>();

        public abstract void Update(T resource);

        public abstract void Create(T resource);

        public abstract void Delete(T resource);

        public abstract T Get(Identifier id);

        public abstract IObservable<T> GetListAsync();

        public abstract IEnumerable<T> GetList();

        public virtual Func<T, Identifier> IdentityResolver { get; protected set; }

        public virtual IObservable<T> ResourceChanged { get; protected set; }

        public IEnumerable<ISynchronizationAction<T>> SynchActions => _synchActions;

        public void AddSyncAction(Func<T, bool> applies, Action<IDataEndpoint<T>, T> execute, string name)
        {
            AddSyncAction(new OldSynchronizationAction<T>((e, r) => applies(r), execute, this, name));
        }

        public void AddSyncAction(Func<IDataEndpoint<T>, T, bool> applies, Action<IDataEndpoint<T>, T> execute, string name)
        {
            AddSyncAction(new OldSynchronizationAction<T>(applies, execute, this, name));
        }

        public void AddSyncAction(ISynchronizationAction<T> action)
        {
            _synchActions.Add(action);
        }
    }
}
