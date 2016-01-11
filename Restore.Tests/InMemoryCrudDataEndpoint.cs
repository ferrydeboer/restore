using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Restore.Channel.Configuration;

namespace Restore.Tests
{
    public class InMemoryCrudDataEndpoint<T, TId> : ICrudEndpoint<T, TId> where TId : IEquatable<TId>
    {
        [NotNull] private readonly TypeConfiguration<T, TId> _typeConfig;
        [NotNull] private readonly IDictionary<TId, T> _items = new Dictionary<TId, T>();

        public InMemoryCrudDataEndpoint([NotNull] TypeConfiguration<T, TId> typeConfig, [NotNull] IDictionary<TId, T> items) : this(typeConfig)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            _items = items;
        }

        public InMemoryCrudDataEndpoint(TypeConfiguration<T, TId> typeConfig, IEnumerable<T> items) 
            : this(typeConfig, items.ToDictionary(item => typeConfig.IdExtractor(item)))
        {
        }

        public InMemoryCrudDataEndpoint([NotNull] TypeConfiguration<T, TId> typeConfig)
        {
            if (typeConfig == null) throw new ArgumentNullException(nameof(typeConfig));
            _typeConfig = typeConfig;
        }

        public T Create(T item)
        {
            if (_items.ContainsKey(_typeConfig.IdExtractor(item)))
            {
                throw new ArgumentException("Item already exists");
            }
            _items.Add(_typeConfig.IdExtractor(item), item);
            return item;
        }

        public T Read(TId id)
        {
            T result;
            var succes = _items.TryGetValue(id, out result);
            return succes ? result : default(T);
        }

        /// <summary>
        /// For testing purposes, so far not part of general interface.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> ReadAll()
        {
            return _items.Values;
        } 

        public T Update(T item)
        {
            // Just replace the instance if it exists.
            var itemId = _typeConfig.IdExtractor(item);
            var inlist = Read(itemId);
            if (EqualityComparer<T>.Default.Equals(inlist, default(T)))
            {
                // found, no default value atleast.
                _items[itemId] = item;
            }
            else
            {
                throw new ArgumentException("Can not update unexisting item!");
            }
            return inlist;
        }

        public T Delete(T item)
        {
            if (_items.Remove(_typeConfig.IdExtractor(item)))
            {
                return item;
            }
            return default(T);

        }
    }
}
