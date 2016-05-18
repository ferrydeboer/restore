using System;

namespace Restore
{
    public abstract class CrudEndpoint<T, TId> : ICrudEndpoint<T, TId>
        where TId : IEquatable<TId>
    {
        public event EventHandler<DataChangeEventArgs<T>> ItemCreated;
        public event EventHandler<DataChangeEventArgs<T>> ItemUpdated;
        public event EventHandler<DataChangeEventArgs<T>> ItemDeleted;

        public T Create(T item)
        {
            var result = DoCreate(item);
            OnItemCreated(DataChangeEventArgs<T>.Create(item));

            return result;
        }

        public virtual T Read(TId id)
        {
            return default(T);
        }

        public T Update(T item)
        {
            var result = DoUpdate(item);
            OnItemUpdated(DataChangeEventArgs<T>.Update(item));

            return result;
        }

        public T Delete(T item)
        {
            var result = DoDelete(item);
            OnItemDeleted(DataChangeEventArgs<T>.Delete(item));

            return result;
        }

        protected abstract T DoCreate(T item);
        protected abstract T DoUpdate(T item);
        protected abstract T DoDelete(T item);

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
