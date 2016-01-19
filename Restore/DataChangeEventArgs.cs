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
            if (item == null) {  throw new ArgumentNullException(nameof(item)); }

            Item = item;
            Change = change;
        }
    }
}