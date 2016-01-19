using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Restore.Channel
{
    public class AttachedObservableCollection<T> : ObservableCollection<T>, IDisposable
    {
        private IDataChangeNotifier<T> _contentChangeNotifier;

        public AttachedObservableCollection(IDataChangeNotifier<T> contentChangeNotifier)
        {
            Attach(contentChangeNotifier);
        }

        public AttachedObservableCollection(IEnumerable<T> collection, IDataChangeNotifier<T> contentChangeNotifier) : base(collection)
        {
            Attach(contentChangeNotifier);
        }

        private void Attach(IDataChangeNotifier<T> contentChangeNotifier)
        {
            _contentChangeNotifier = contentChangeNotifier;
            _contentChangeNotifier.ItemCreated += AddItem;
            _contentChangeNotifier.ItemUpdated += UpdateItem;
            _contentChangeNotifier.ItemDeleted += DeleteItem;
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
            throw new NotImplementedException();
        }

        private void DeleteItem(object sender, DataChangeEventArgs<T> e)
        {
            throw new NotImplementedException();
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
