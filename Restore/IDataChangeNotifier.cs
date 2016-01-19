using System;
using System.Dynamic;

namespace Restore
{
    public interface IDataChangeNotifier<T>
    {
        event EventHandler<DataChangeEventArgs<T>> ItemCreated;
        event EventHandler<DataChangeEventArgs<T>> ItemUpdated;
        event EventHandler<DataChangeEventArgs<T>> ItemDeleted;
    }

    public class DataChangeEventArgs<T> : EventArgs
    {
        public T Item { get; private set; }
        public ChangeType Change { get; private set; }

        public DataChangeEventArgs(T item, ChangeType change)
        {
            Item = item;
            Change = change;
        }
    }

    public enum ChangeType
    {
        Create,
        Update,
        Delete
    }
}