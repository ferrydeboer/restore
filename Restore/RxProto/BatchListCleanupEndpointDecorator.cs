using System;
using System.Collections.Generic;
using System.Linq;

namespace Restore.RxProto
{
    /// <summary>
    /// Endpoint decorator that triggers deletes items that are not synchronized from a source list. 
    /// There is a problem with this approach since it now by passes the general channel feedback mechanism!
    /// If I simply have this endpoint already containing an action for registration what is then still
    /// needed is something to feed back into the result channel from the channel (that is currently not there yet).
    /// </summary>
    public class BatchListCleanupEndpointDecorator<T> : IDataEndpoint<T>
    {
        private readonly IDataEndpoint<T> _decorated;
        private IList<T> _unsynchedItems;

        public BatchListCleanupEndpointDecorator(IDataEndpoint<T> decorated)
        {
            if (decorated == null) { throw new ArgumentNullException(nameof(decorated)); }
            _decorated = decorated;
        }

        public void Initialize()
        {
            _unsynchedItems = GetList().ToList();
        }

        public void Finish()
        {
            foreach (var unsynchedItem in _unsynchedItems)
            {
                Delete(unsynchedItem);
            }
        }

        public IObservable<T> ResourceChanged { get; protected set; }

        public void Update(T resource)
        {
            _decorated.Update(resource);
        }

        public void Create(T resource)
        {
            _decorated.Create(resource);
        }

        public void Delete(T resource)
        {
            _decorated.Delete(resource);
        }

        public T Get(Identifier id)
        {
            //var synchedItem = _decorated.Get(id);
            var unsynchedItem = _unsynchedItems.FirstOrDefault(item => IdentityResolver(item) == id);
            if (unsynchedItem != null)
            {
                _unsynchedItems.Remove(unsynchedItem);
            }
            return unsynchedItem;
        }

        public IObservable<T> GetListAsync()
        {
            return _decorated.GetListAsync();
        }

        public IEnumerable<T> GetList()
        {
            return _decorated.GetList();
        }

        public Func<T, Identifier> IdentityResolver => _decorated.IdentityResolver;

        public IEnumerable<ISynchronizationAction<T>> SynchActions => _decorated.SynchActions;

        public void AddSyncAction(Func<T, bool> applies, Action<IDataEndpoint<T>, T> execution, string name = null)
        {
            _decorated.AddSyncAction(applies, execution, name);
        }

        public void AddSyncAction(Func<IDataEndpoint<T>, T, bool> applies, Action<IDataEndpoint<T>, T> execute, string name = null)
        {
            _decorated.AddSyncAction(applies, execute, name);
        }

        public void AddSyncAction(ISynchronizationAction<T> action)
        {
            _decorated.AddSyncAction(action);
        }
    }
}
