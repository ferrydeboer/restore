using System;

namespace Restore
{
    public abstract class CrudEndpoint<T, TId> : ICrudEndpoint<T, TId>
        where TId : IEquatable<TId>
    {
        public event EventHandler<DataChangeEventArgs<T>> ItemCreated;
        public event EventHandler<DataChangeEventArgs<T>> ItemUpdated;
        public event EventHandler<DataChangeEventArgs<T>> ItemDeleted;

        public void Create(T item)
        {
            DoCreate(item);
            OnItemCreated(DataChangeEventArgs<T>.Create(item));
        }

        public virtual T Read(TId id)
        {
            return default(T);
        }

        public void Update(T item)
        {
            DoUpdate(item);
            OnItemUpdated(DataChangeEventArgs<T>.Update(item));
        }

        public void Delete(T item)
        {
            DoDelete(item);
            OnItemDeleted(DataChangeEventArgs<T>.Delete(item));
        }

        protected abstract void DoCreate(T item);
        protected abstract void DoUpdate(T item);
        protected abstract void DoDelete(T item);

        protected virtual void OnItemCreated(DataChangeEventArgs<T> e)
        {
            ItemCreated?.Invoke(this, e);
        }

        protected virtual void OnItemUpdated(DataChangeEventArgs<T> e)
        {
            ItemUpdated?.Invoke(this, e);
        }

        protected virtual void OnItemDeleted(DataChangeEventArgs<T> e)
        {
            ItemDeleted?.Invoke(this, e);
        }
    }
}
