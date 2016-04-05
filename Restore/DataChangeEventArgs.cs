using System;
using JetBrains.Annotations;

namespace Restore
{
    public class DataChangeEventArgs<T> : EventArgs
    {
        public T Item { get; private set; }
        public ChangeType Change { get; private set; }

        public DataChangeEventArgs([NotNull] T item, ChangeType change)
        {
            if (item == null) { throw new ArgumentNullException(nameof(item)); }

            Item = item;
            Change = change;
        }

        public static DataChangeEventArgs<T> Create(T item)
        {
            return new DataChangeEventArgs<T>(item, ChangeType.Create);
        }

        public static DataChangeEventArgs<T> Update(T item)
        {
            return new DataChangeEventArgs<T>(item, ChangeType.Update);
        }

        public static DataChangeEventArgs<T> Delete(T item)
        {
            return new DataChangeEventArgs<T>(item, ChangeType.Delete);
        }
    }
}