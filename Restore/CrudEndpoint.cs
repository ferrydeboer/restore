using System;
using System.Collections.Generic;

namespace Restore
{
    /// <summary>
    /// <p>
    /// Event exposing endpoint using template methods. Allows for interception of
    /// items before and after they are being changed.
    /// </p>
    /// <p>
    /// Decided to use explicit events and event args for evey change instead
    /// of ItemChange & ItemChange since the latter is slightly more error prone from
    /// the consuming end since you have to dispatch yourself. Having a base type event
    /// args still allow for one handler on multiple events. Having event specific AventArgs
    /// on the other hand makes the API more stable on the other hand.
    /// </p>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public abstract class CrudEndpoint<T, TId> : ICrudEndpoint<T, TId>
        where TId : IEquatable<TId>
    {
        public event EventHandler<ItemCreateEventArgs<T>> ItemCreate;
        public event EventHandler<ItemCreateEventArgs<T>> ItemCreated;
        public event EventHandler<ItemUpdateEventArgs<T>> ItemUpdate;
        public event EventHandler<ItemUpdateEventArgs<T>> ItemUpdated;
        public event EventHandler<ItemDeleteEventArgs<T>> ItemDelete;
        public event EventHandler<ItemDeleteEventArgs<T>> ItemDeleted;

        public T Create(T item)
        {
            OnItemCreate(new ItemCreateEventArgs<T>(item));
            var result = DoCreate(item);
            OnItemCreated(new ItemCreateEventArgs<T>(item));

            return result;
        }

        public abstract T Read(TId id);

        public abstract IEnumerable<T> Read(params TId[] ids);

        public T Update(T item)
        {
            OnItemUpdate(new ItemUpdateEventArgs<T>(item));
            var result = DoUpdate(item);
            OnItemUpdated(new ItemUpdateEventArgs<T>(item));

            return result;
        }

        public T Delete(T item)
        {
            OnItemDelete(new ItemDeleteEventArgs<T>(item));
            var result = DoDelete(item);
            OnItemDeleted(new ItemDeleteEventArgs<T>(item));

            return result;
        }

        protected abstract T DoCreate(T item);
        protected abstract T DoUpdate(T item);
        protected abstract T DoDelete(T item);

        protected virtual void OnItemCreate(ItemCreateEventArgs<T> e)
        {
            ItemCreate?.Invoke(this, e);
        }

        protected virtual void OnItemCreated(ItemCreateEventArgs<T> e)
        {
            ItemCreated?.Invoke(this, e);
        }

        protected virtual void OnItemUpdate(ItemUpdateEventArgs<T> e)
        {
            ItemUpdate?.Invoke(this, e);
        }

        protected virtual void OnItemUpdated(ItemUpdateEventArgs<T> e)
        {
            ItemUpdated?.Invoke(this, e);
        }

        protected virtual void OnItemDelete(ItemDeleteEventArgs<T> e)
        {
            ItemDelete?.Invoke(this, e);
        }

        protected virtual void OnItemDeleted(ItemDeleteEventArgs<T> e)
        {
            ItemDeleted?.Invoke(this, e);
        }
    }
}
