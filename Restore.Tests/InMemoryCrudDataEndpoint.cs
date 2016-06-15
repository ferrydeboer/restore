using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Restore.Channel.Configuration;

namespace Restore.Tests
{
    public class InMemoryCrudDataEndpoint<T, TId> : ICrudEndpoint<T, TId>
        where TId : IEquatable<TId>
    {
        [NotNull] private readonly IEqualityComparer<T> _comparer;
        [NotNull] private IDictionary<TId, T> _items = new Dictionary<TId, T>();

        [NotNull]
        public TypeConfiguration<T, TId> TypeConfig { get; }

        // InMemoryEndpoint specific since this class is only used for testing.
        // This is used as a preferable alternative over Mocks.
        public event EventHandler<DataReadEventArgs<T, TId>> ItemRead;

        public event EventHandler<DataChangeEventArgs<T>> ItemCreated;

        public event EventHandler<DataChangeEventArgs<T>> ItemUpdated;

        public event EventHandler<DataChangeEventArgs<T>> ItemDeleted;

        public InMemoryCrudDataEndpoint(
            [NotNull] TypeConfiguration<T, TId> typeConfig,
            IEqualityComparer<T> comparer,
            [NotNull] IDictionary<TId, T> items)
            : this(typeConfig, comparer)
        {
            if (items == null) { throw new ArgumentNullException(nameof(items)); }

            _items = items;
        }

        public InMemoryCrudDataEndpoint(
            [NotNull] TypeConfiguration<T, TId> typeConfig,
            [NotNull] IDictionary<TId, T> items)
            : this(typeConfig, null, items)
        {
        }

        public InMemoryCrudDataEndpoint(TypeConfiguration<T, TId> typeConfig, IEnumerable<T> items)
            : this(typeConfig, items.ToDictionary(item => typeConfig.IdExtractor(item)))
        {
        }

        public InMemoryCrudDataEndpoint([NotNull] TypeConfiguration<T, TId> typeConfig, IEqualityComparer<T> comparer)
        {
            if (typeConfig == null) { throw new ArgumentNullException(nameof(typeConfig)); }

            TypeConfig = typeConfig;
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        public InMemoryCrudDataEndpoint([NotNull] TypeConfiguration<T, TId> typeConfig)
            : this(typeConfig, (IEqualityComparer<T>)null)
        {
        }

        public T Create([NotNull] T item)
        {
            if (item == null) { throw new ArgumentNullException(nameof(item)); }
            if (_items.ContainsKey(TypeConfig.IdExtractor(item))) { throw new ArgumentException("Item already exists"); }

            _items.Add(TypeConfig.IdExtractor(item), item);
            OnItemCreated(item);

            return item;
        }

        public T Read(TId id)
        {
            T result;
            var succes = _items.TryGetValue(id, out result);
            OnItemRead(new DataReadEventArgs<T, TId>(result, id));
            return succes ? result : default(T);
        }

        public IEnumerable<T> Read(params TId[] ids)
        {
            return ids.Select(Read).Where(result => !EqualityComparer<T>.Default.Equals(result, default(T)));
        }

        public T Update(T item)
        {
            // Just replace the instance if it exists.
            var itemId = TypeConfig.IdExtractor(item);
            var inlist = Read(itemId);
            if (!_comparer.Equals(inlist, default(T)))
            {
                // found, no default value atleast.
                _items[itemId] = item;
                OnItemUpdated(item);
            } else
            {
                throw new ArgumentException("Can not update unexisting item!");
            }

            return inlist;
        }

        public T Delete(T item)
        {
            if (_items.Remove(TypeConfig.IdExtractor(item)))
            {
                OnItemDeleted(item);

                // return item;
            }

            return default(T);
        }

        /// <summary>
        ///     For testing purposes, so far not part of general interface.
        /// </summary>
        public IEnumerable<T> ReadAll()
        {
            return _items.Values;
        }
        protected virtual void OnItemRead(DataReadEventArgs<T, TId> e)
        {
            ItemRead?.Invoke(this, e);
        }

        protected virtual void OnItemCreated(T obj)
        {
            ItemCreated?.Invoke(this, new DataChangeEventArgs<T>(obj, ChangeType.Create));
        }

        protected virtual void OnItemUpdated(T obj)
        {
            ItemUpdated?.Invoke(this, new DataChangeEventArgs<T>(obj, ChangeType.Update));
        }

        protected virtual void OnItemDeleted(T obj)
        {
            ItemDeleted?.Invoke(this, new DataChangeEventArgs<T>(obj, ChangeType.Delete));
        }

        public void Clear()
        {
            _items = new Dictionary<TId, T>();
        }
    }

    public class DataReadEventArgs<T, TId>
    {
        public T Item { get; }

        public TId Id { get; }

        public DataReadEventArgs(T item, TId id)
        {
            Item = item;
            Id = id;
        }
    }
}