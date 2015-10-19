using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Restore.Tests
{
    /// <summary>
    /// Simple in memory endpoint that serves testing purposes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InMemoryDataEndpoint<T> : IDataEndpoint<T>
    {
        readonly IDictionary<Identifier, T> _items = new Dictionary<Identifier, T>();
        readonly Func<T, Identifier> _idResolver;
        readonly IList<IObserver<T>> _observers = new List<IObserver<T>>();
        readonly List<IObserver<T>> _deleteObservers = new List<IObserver<T>>();
        // Should be made a constructor argument or built using factory method.
        readonly IList<ISynchronizationAction<T>> _synchActions = new List<ISynchronizationAction<T>>();
        

        public InMemoryDataEndpoint(Func<T, Identifier> idResolver)
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

        public T Get(Identifier id)
        {
            return _items.ContainsKey(id) ? _items[id] : default(T);
        }

        public Func<T, Identifier> IdentityResolver
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
            _synchActions.Add(new SynchronizationAction<T>((e, r) => applies(r), execute, this, name));
        }

        public IObservable<T> GetList()
        {
            return _items.Values.ToObservable(Scheduler.CurrentThread);
        }

        public void AddSyncAction(Func<IDataEndpoint<T>, T, bool> applies, Action<IDataEndpoint<T>, T> execute, string name)
        {
            _synchActions.Add(new SynchronizationAction<T>(applies, execute, this, name));
        }
    }
}