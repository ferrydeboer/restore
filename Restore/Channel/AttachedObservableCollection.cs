﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Restore.Extensions;

// TODO: Probably better of in another namespace since now UI concerns are coupling on lower level namespaces.
namespace Restore.Channel
{
    public class AttachedObservableCollection<T> : ObservableCollection<T>, IDisposable
    {
        private readonly IEqualityComparer<T> _changeComparer;
        private readonly Action<Action> _changeDispatcher;

        /// <summary>
        ///     It does not make sense to change a notifier once the collection is constructed. This will break assumptions made
        ///     on the notifier.
        /// </summary>
        private readonly IDataChangeNotifier<T> _contentChangeNotifier;

        private readonly Action<Action> _defaultChangeDispatcher = act => act();
        private Func<T, bool> _filterPredicate = _ => true;

        public AttachedObservableCollection([NotNull] IDataChangeNotifier<T> contentChangeNotifier)
            : this(contentChangeNotifier, null, null)
        {
        }

        public AttachedObservableCollection([NotNull] IDataChangeNotifier<T> contentChangeNotifier,
            IEqualityComparer<T> changeComparer)
            : this(contentChangeNotifier, changeComparer, null)
        {
        }

        public AttachedObservableCollection([NotNull] IDataChangeNotifier<T> contentChangeNotifier,
            IEqualityComparer<T> changeComparer, Action<Action> changeDispatcher)
        {
            _changeDispatcher = changeDispatcher ?? _defaultChangeDispatcher;
            _changeComparer = changeComparer ?? EqualityComparer<T>.Default;
            Attach(contentChangeNotifier);
        }

        public AttachedObservableCollection(IEnumerable<T> collection, IDataChangeNotifier<T> contentChangeNotifier)
            : base(collection)
        {
            if (contentChangeNotifier == null)
            {
                throw new ArgumentNullException(nameof(contentChangeNotifier));
            }
            _contentChangeNotifier = contentChangeNotifier;
            Attach(contentChangeNotifier);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ObservableCollection<T> Where([NotNull] Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            _filterPredicate = predicate;
            return this;
        }

        protected override void InsertItem(int index, T item)
        {
            Debug.WriteLine("InsertItem");
            if (_filterPredicate(item))
            {
                base.InsertItem(index, item);
            }
        }

        protected override void ClearItems()
        {
            Debug.WriteLine("ClearItems");
            base.ClearItems();
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            Debug.WriteLine("MoveItem");
            base.MoveItem(oldIndex, newIndex);
        }

        protected override void RemoveItem(int index)
        {
            Debug.WriteLine("RemoveItem");
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            Debug.WriteLine("SetItem");
            if (_filterPredicate(item))
            {
                base.SetItem(index, item);
            }
        }

        private void Attach([NotNull] IDataChangeNotifier<T> contentChangeNotifier)
        {
            contentChangeNotifier.ItemCreated += AddItem;
            contentChangeNotifier.ItemUpdated += UpdateItem;
            contentChangeNotifier.ItemDeleted += DeleteItem;
        }

        private void Detach()
        {
            _contentChangeNotifier.ItemCreated -= AddItem;
            _contentChangeNotifier.ItemUpdated -= UpdateItem;
            _contentChangeNotifier.ItemDeleted -= DeleteItem;
        }

        private void AddItem(object sender, DataChangeEventArgs<T> e)
        {
            if (_ordering == null)
            {
                _changeDispatcher(() => Add(e.Item));
            }
            else
            {
                _changeDispatcher(() => this.SortInsert(e.Item, _ordering.Comparer, !_ordering.Ascending));
            }
        }

        private void UpdateItem(object sender, DataChangeEventArgs<T> e)
        {
            // Does it really matter, yes, because I need to replace the item. After all, 
            // if the reference has changed there is no point/need to do this.
            var hasItem = this.FirstOrDefault(item => _changeComparer.Equals(item, e.Item));
            if (!_changeComparer.Equals(hasItem, default(T)))
            {
                // Has item, so replace.
                var indexOfExisting = IndexOf(hasItem);
                if (!_filterPredicate(hasItem))
                {
                    _changeDispatcher(() => RemoveItem(indexOfExisting));
                }
                else
                {
                    _changeDispatcher(() => SetItem(indexOfExisting, e.Item));
                }
            }
            else
            {
                AddItem(sender, e);
            }
        }

        private void DeleteItem(object sender, DataChangeEventArgs<T> e)
        {
            var hasItem = this.FirstOrDefault(item => _changeComparer.Equals(item, e.Item));
            if (!_changeComparer.Equals(hasItem, default(T)))
            {
                _changeDispatcher(() => Remove(hasItem));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Detach();
            }
        }

        private IOrder<T> _ordering;
        public IOrder<T> OrderBy(Expression<Func<T, IComparable>> orderExpression)
        {
            _ordering = new Order<T>(orderExpression);
            return _ordering;
        }
    }

    public class Order<T> : IOrder<T>
    {
        private readonly Expression<Func<T, IComparable>> _orderExpression;
        private readonly Func<T, T, int> _comparer;
        private int _direction = 0;
        private bool _ascending = true;

        public Order(Expression<Func<T, IComparable>> orderExpression)
        {
            _orderExpression = orderExpression;
            Func<T, IComparable> valueRetrieval = _orderExpression.Compile();
            _comparer = (first, second) =>
            {
                var value1 = valueRetrieval(first);
                var value2 = valueRetrieval(second);
                var result = value1.CompareTo(value2);
                Debug.WriteLine("{0} compared to {1} = {2}", value1, value2, result);
                return result;
            };
        }

        public IOrder<T> Asc()
        {
            _direction = 0;
            return this;
        }

        public IOrder<T> Desc()
        {
            _direction = -1;
            return this;
        }

        public bool Ascending => _direction == 0;

        public Func<T, T, int> Comparer
        {
            get { return _comparer; }
        }
    }

    public interface IOrder<T>
    {
        IOrder<T> Asc();
        IOrder<T> Desc();
        bool Ascending { get; }
        Func<T, T, int> Comparer { get; }
    }
}