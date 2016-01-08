using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Restore.Channel.Configuration;

namespace Restore.Tests
{
    public class InMemoryCrudDataEndpoint<T, TId> : ICrudEndpoint<T, TId> where TId : IEquatable<TId>
    {
        [NotNull] private readonly TypeConfiguration<T, TId> _typeConfig;
        [NotNull] private readonly IDictionary<TId, T> _items;

        public InMemoryCrudDataEndpoint([NotNull] TypeConfiguration<T, TId> typeConfig, [NotNull] IDictionary<TId, T> items) : this(typeConfig)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            _items = items;
        }

        public InMemoryCrudDataEndpoint([NotNull] TypeConfiguration<T, TId> typeConfig)
        {
            if (typeConfig == null) throw new ArgumentNullException(nameof(typeConfig));
            _typeConfig = typeConfig;
            if (_items == null)
            {
                _items = new Dictionary<TId, T>();
            }
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
            return default(T);
        }

        public T Update(T item)
        {
            throw new NotImplementedException();
        }

        public T Delete(T item)
        {
            throw new NotImplementedException();
        }
    }
}
