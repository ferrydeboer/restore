using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Restore.Channel.Configuration;

namespace Restore
{
    /// <summary>
    /// Simplest implementation of an endpoint which can be used for experimental
    /// testing purposes.
    /// </summary>
    public class InMemoryCrudDataEndpoint<T, TId> : CrudEndpoint<T, TId>
        where TId : IEquatable<TId>
    {
        [NotNull] private readonly IEqualityComparer<T> _comparer;
        [NotNull] private IDictionary<TId, T> _items = new Dictionary<TId, T>();

        [NotNull]
        public TypeConfiguration<T, TId> TypeConfig { get; }

        /// <summary>
        /// InMemoryEndpoint specific to enable exposure of slightly more
        /// information on how the endpoint is being used from synchronization
        /// actions.
        /// </summary>
        public event EventHandler<DataReadEventArgs<T, TId>> ItemRead;


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

        protected override T DoCreate([NotNull] T item)
        {
            if (item == null) { throw new ArgumentNullException(nameof(item)); }

            OnItemCreate(new ItemCreateEventArgs<T>(item));

            var itemId = TypeConfig.IdExtractor(item);
            if (_items.ContainsKey(itemId)) { throw new ArgumentException("Item already exists"); }

            _items.Add(itemId, item);
            OnItemCreated(item);

            return item;
        }

        public override T Read(TId id)
        {
            T result;
            var succes = _items.TryGetValue(id, out result);
            OnItemRead(new DataReadEventArgs<T, TId>(result, id));
            return succes ? result : default(T);
        }

        public override IEnumerable<T> Read(params TId[] ids)
        {
            return ids.Select(Read).Where(result => !EqualityComparer<T>.Default.Equals(result, default(T)));
        }

        protected override T DoUpdate(T item)
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

        protected override T DoDelete(T item)
        {
            var idExtractor = TypeConfig.IdExtractor(item);
            if (_items.Remove(idExtractor))
            {
                OnItemDeleted(item);
                return default(T);
            }

            return item;
        }

        /// <summary>
        ///     For testing purposes, so far not part of general interface.
        /// </summary>
        public IEnumerable<T> ReadAll()
        {
            return _items.Values;
        }

        public void Clear()
        {
            _items = new Dictionary<TId, T>();
        }

        protected virtual void OnItemRead(DataReadEventArgs<T, TId> e)
        {
            ItemRead?.Invoke(this, e);
        }

        protected virtual void OnItemCreated(T obj)
        {
            OnItemCreated(new ItemCreateEventArgs<T>(obj));
        }

        protected virtual void OnItemUpdated(T obj)
        {
            OnItemUpdated(new ItemUpdateEventArgs<T>(obj));
        }

        protected virtual void OnItemDeleted(T obj)
        {
            OnItemDeleted(new ItemDeleteEventArgs<T>(obj));
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