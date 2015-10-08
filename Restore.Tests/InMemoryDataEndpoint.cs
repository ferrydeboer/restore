using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Restore.Tests
{
    /// <summary>
    /// Simple in memory endpoint that serves testing purposes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InMemoryDataEndpoint<T> : IDataEndpoint<T>
    {
        readonly IDictionary<IIdentifier, T> _items = new Dictionary<IIdentifier, T>();
        readonly Func<T, IIdentifier> _idResolver;
        readonly IList<IObserver<T>> _observers = new List<IObserver<T>>();
        readonly List<IObserver<T>> _deleteObservers = new List<IObserver<T>>();
        // Should be made a constructor argument or built using factory method.
        readonly IList<ISynchronizationAction<T>> _synchActions = new List<ISynchronizationAction<T>>();
        

        public InMemoryDataEndpoint(Func<T, IIdentifier> idResolver)
        {
            _idResolver = idResolver;
        }

        public void Update(T resource)
        {
            var id = _idResolver(resource);
            if (_items.ContainsKey(id))
            {
                _items[id] = resource;
            }
        }

        public void Create(T resource)
        {
            var id = _idResolver(resource);
            if (_items.ContainsKey(id))
            {
                _items[id] = resource;
            }
            else
            {
                _items.Add(id, resource);
            }
            OnResourceChanged(resource);
        }

        public void Delete(T resource)
        {
            var id = _idResolver(resource);
            if (_items.ContainsKey(id))
            {
                _items.Remove(id);
                OnDelete(resource);
            }
            OnResourceChanged(resource);
        }

        public T Get(IIdentifier id)
        {
            return _items.ContainsKey(id) ? _items[id] : default(T);
        }

        public Func<T, IIdentifier> IdentityResolver
        {
            get { return _idResolver; }
        }

        public void OnResourceChanged(T resource)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(resource);
            }
        }

        public IObservable<T> ResourceChanged
        {
            get
            {
                return Observable.Create<T>(o =>
                {
                    _observers.Add(o);
                    return () =>
                    {
                        _observers.Remove(o);
                    };
                });
            }
        }

        private void OnDelete(T resource)
        {
            foreach (var observer in _deleteObservers)
            {
                observer.OnNext(resource);
            }
        }

        public IObservable<T> ResourceDeleted
        {
            get
            {
                return Observable.Create<T>(o =>
                {
                    _deleteObservers.Add(o);
                    return () =>
                    {
                        _deleteObservers.Remove(o);
                    };
                });
            }
        }

        public IEnumerable<ISynchronizationAction<T>> SynchActions 
        {
            get
            {
                return _synchActions;
            }
        }

        public void AddSyncAction(Func<T, bool> applies, Action<IDataEndpoint<T>, T> execute, string name)
        {
            _synchActions.Add(new SynchronizationAction<T>(applies, execute, this, name));
        }
    }
}