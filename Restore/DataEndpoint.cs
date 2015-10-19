using System;
using System.Collections.Generic;

namespace Restore
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

        public virtual Func<T, Identifier> IdentityResolver { get; private set; }

        public virtual IObservable<T> ResourceChanged { get; private set; }

        public IEnumerable<ISynchronizationAction<T>> SynchActions 
        {
            get
            {
                return _synchActions;
            }
        }

        public void AddSyncAction(Func<T, bool> applies, Action<IDataEndpoint<T>, T> execute, string name)
        {
            AddSyncAction(new SynchronizationAction<T>((e, r) => applies(r), execute, this, name));
        }

        public void AddSyncAction(Func<IDataEndpoint<T>, T, bool> applies, Action<IDataEndpoint<T>, T> execute, string name)
        {
            AddSyncAction(new SynchronizationAction<T>(applies, execute, this, name));
        }

        public void AddSyncAction(ISynchronizationAction<T> action)
        {
            _synchActions.Add(action);
        }
    }
}
