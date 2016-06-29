using System;
using JetBrains.Annotations;

namespace Restore
{
    public class ItemChangeEventArgs<T> : EventArgs
    {
        public T Item { get; private set; }
        public ChangeType Change { get; private set; }

        public ItemChangeEventArgs([NotNull] T item, ChangeType change)
        {
            if (item == null) { throw new ArgumentNullException(nameof(item)); }

            Item = item;
            Change = change;
        }

        public static ItemChangeEventArgs<T> Create(T item)
        {
            return new ItemChangeEventArgs<T>(item, ChangeType.Create);
        }

        public static ItemChangeEventArgs<T> Update(T item)
        {
            return new ItemChangeEventArgs<T>(item, ChangeType.Update);
        }

        public static ItemChangeEventArgs<T> Delete(T item)
        {
            return new ItemChangeEventArgs<T>(item, ChangeType.Delete);
        }
    }

    public class ItemCreateEventArgs<T> : ItemChangeEventArgs<T>
    {
        public ItemCreateEventArgs([NotNull] T item)
            : base(item, ChangeType.Create)
        {
        }
    }

    public class ItemUpdateEventArgs<T> : ItemChangeEventArgs<T>
    {
        public ItemUpdateEventArgs([NotNull] T item)
            : base(item, ChangeType.Update)
        {
        }
    }

    public class ItemDeleteEventArgs<T> : ItemChangeEventArgs<T>
    {
        public ItemDeleteEventArgs([NotNull] T item)
            : base(item, ChangeType.Delete)
        {
        }
    }
}