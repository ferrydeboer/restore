using System;
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
    /// <summary>
    /// <p>
    /// Observable collection that updates based on incoming changes from an <see cref="IDataChangeNotifier{T}"/> given
    /// defined filtering and sorting behaviour. For this reason it implements IDisposable so it allows detaching from
    /// the notifier once not needed any longer.
    /// </p>
    /// <p>
    /// Public change methods are however not overridable. This currently implies
    /// that sorting does not work when using public API. Collection is currently intended only to be used a read only connection
    /// thus it should be exposed through an interface.
    /// </p>
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
        private Order<T> _ordering;

        /// <summary>
        /// Main constructor.
        /// </summary>
        /// <param name="contentChangeNotifier">The notifier updating this collection.</param>
        /// <param name="changeComparer">Possible equality comparer used to determine updated items already exist in the list or not.</param>
        /// <param name="changeDispatcher">Possible callback used to dispatch content changes to ensure they are executed on the UI thread.</param>
        public AttachedObservableCollection(
            [NotNull] IDataChangeNotifier<T> contentChangeNotifier,
            IEqualityComparer<T> changeComparer,
            Action<Action> changeDispatcher)
        {
            if (contentChangeNotifier == null)
            {
                throw new ArgumentNullException(nameof(contentChangeNotifier));
            }
            _contentChangeNotifier = contentChangeNotifier;
            Attach(contentChangeNotifier);
            _changeDispatcher = changeDispatcher ?? _defaultChangeDispatcher;
            _changeComparer = changeComparer ?? EqualityComparer<T>.Default;
        }

        public AttachedObservableCollection(
            IEnumerable<T> collection,
            [NotNull] IDataChangeNotifier<T> contentChangeNotifier,
            IEqualityComparer<T> changeComparer,
            Action<Action> changeDispatcher) : base(collection)
        {
            if (contentChangeNotifier == null)
            {
                throw new ArgumentNullException(nameof(contentChangeNotifier));
            }
            _contentChangeNotifier = contentChangeNotifier;
            Attach(_contentChangeNotifier);
            _changeDispatcher = changeDispatcher ?? _defaultChangeDispatcher;
            _changeComparer = changeComparer ?? EqualityComparer<T>.Default;
        }

        public AttachedObservableCollection([NotNull] IDataChangeNotifier<T> contentChangeNotifier)
            : this(contentChangeNotifier, null, null)
        {
        }

        public AttachedObservableCollection([NotNull] IDataChangeNotifier<T> contentChangeNotifier,
            IEqualityComparer<T> changeComparer)
            : this(contentChangeNotifier, changeComparer, null)
        {
        }

        public AttachedObservableCollection(IEnumerable<T> collection, IDataChangeNotifier<T> contentChangeNotifier)
            : this(collection, contentChangeNotifier, null, null)
        {
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
            Debug.WriteLine("InsertItem at {0}", index);
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
            Debug.WriteLine("MoveItem from {0} to {1}", oldIndex, newIndex);
            base.MoveItem(oldIndex, newIndex);
        }

        protected override void RemoveItem(int index)
        {
            Debug.WriteLine("RemoveItem as {0}", index);
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            Debug.WriteLine("SetItem at {0}", index);
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
                _changeDispatcher(() => this.SortInsert(e.Item, _ordering.Comparer));
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
                    if (_ordering != null)
                    {
                        // Simply remove the item and add it again at the right location. This is far simpler
                        // because with reference equals you're always comparing the item with itself, which
                        // then requires code to deal with as well.
                        _changeDispatcher(() => RemoveAt(indexOfExisting));
                        AddItem(sender, e);
                    }
                    else
                    {
                        // This replaces where as MoveItem doesn't!
                        _changeDispatcher(() => SetItem(indexOfExisting, e.Item));
                    }
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

        public IOrder<T> OrderBy(Expression<Func<T, IComparable>> orderExpression)
        {
            _ordering = new Order<T>(orderExpression);
            return _ordering;
        }

        private class Order<U> : IOrder<U>
        {
            private readonly Expression<Func<U, IComparable>> _orderExpression;
            private int _direction;
            private bool _ascending = true;

            public Order(Expression<Func<U, IComparable>> orderExpression)
            {
                _orderExpression = orderExpression;
                Func<U, IComparable> valueRetrieval = _orderExpression.Compile();
                Comparer = (first, second) =>
                {
                    var value1 = valueRetrieval(first);
                    var value2 = valueRetrieval(second);
                    var result = value1.CompareTo(value2) * _direction;
                    // Introduce different behaviour based on order direction.
                    if (result == 0)
                    {
                        result = _direction;
                    }
                    return result;
                };
            }

            public IOrder<U> Asc()
            {
                _direction = 1;
                return this;
            }

            public IOrder<U> Desc()
            {
                _direction = -1;
                return this;
            }

            public bool Ascending => _direction == 1;

            public Func<U, U, int> Comparer { get; }
        }
    }

    public interface IOrder<T>
    {
        IOrder<T> Asc();
        IOrder<T> Desc();
    }
}