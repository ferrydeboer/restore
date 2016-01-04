using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Restore.Tests
{
    /// <summary>
    /// Simple in memory endpoint that serves testing purposes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InMemoryDataEndpoint<T> : DataEndpoint<T>, IDataEndpoint<T>
    {
        readonly IDictionary<Identifier, T> _items = new Dictionary<Identifier, T>();
        readonly Func<T, Identifier> _idResolver;
        readonly IList<IObserver<T>> _observers = new List<IObserver<T>>();
        readonly List<IObserver<T>> _deleteObservers = new List<IObserver<T>>();
        // Should be made a constructor argument or built using factory method.


        public InMemoryDataEndpoint(Func<T, Identifier> idResolver)
        {
            _idResolver = idResolver;
        }

        public override void Update(T resource)
        {
            var id = _idResolver(resource);
            if (_items.ContainsKey(id))
            {
                _items[id] = resource;
            }
        }

        public override void Create(T resource)
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

        public override void Delete(T resource)
        {
            var id = _idResolver(resource);
            if (_items.ContainsKey(id))
            {
                _items.Remove(id);
                OnDelete(resource);
            }
            OnResourceChanged(resource);
        }

        public override T Get(Identifier id)
        {
            return _items.ContainsKey(id) ? _items[id] : default(T);
        }

        public override IObservable<T> GetListAsync()
        {
            return _items.Values.ToObservable(Scheduler.CurrentThread);
        }

        public override IEnumerable<T> GetList()
        {
            return _items.Values;
        }

        public override Func<T, Identifier> IdentityResolver
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

        public override IObservable<T> ResourceChanged
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
    }
}