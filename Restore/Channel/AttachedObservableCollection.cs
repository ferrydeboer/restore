using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;

namespace Restore.Channel
{
    public class AttachedObservableCollection<T> : ObservableCollection<T>, IDisposable
    {
        private readonly IEqualityComparer<T> _changeComparer;

        /// <summary>
        /// It does not make sense to change a notifier once the collection is constructed. This will break assumptions made
        /// on the notifier.
        /// </summary>
        private readonly IDataChangeNotifier<T> _contentChangeNotifier;

        public AttachedObservableCollection([NotNull] IDataChangeNotifier<T> contentChangeNotifier) 
            : this(contentChangeNotifier, null)
        {
        }

        public AttachedObservableCollection([NotNull] IDataChangeNotifier<T> contentChangeNotifier, IEqualityComparer<T> changeComparer)
        {
            _changeComparer = changeComparer ?? EqualityComparer<T>.Default;
            Attach(contentChangeNotifier);
        }

        public AttachedObservableCollection(IEnumerable<T> collection, IDataChangeNotifier<T> contentChangeNotifier) : base(collection)
        {
            if (contentChangeNotifier == null) { throw new ArgumentNullException(nameof(contentChangeNotifier)); }
            _contentChangeNotifier = contentChangeNotifier;
            Attach(contentChangeNotifier);
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
            Add(e.Item);
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
                SetItem(indexOfExisting, e.Item);
            }
        }

        private void DeleteItem(object sender, DataChangeEventArgs<T> e)
        {
            var hasItem = this.FirstOrDefault(item => _changeComparer.Equals(item, e.Item));
            if (!_changeComparer.Equals(hasItem, default(T)))
            {
                Remove(hasItem);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Detach();
            }
        }
    }
}
