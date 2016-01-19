using System;

namespace Restore
{
    /// <summary>
    /// An object that notifies about changes on a certain data source. A few assumptions are made about the
    /// implementors of this object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataChangeNotifier<T>
    {
        /// <summary>
        /// Fired when an items is created/added to a data source.
        /// </summary>
        /// <remarks>
        /// It is assumed an item is only added once.
        /// </remarks>
        event EventHandler<DataChangeEventArgs<T>> ItemCreated;

        /// <summary>
        /// Fired when an items is updated in a data source.
        /// </summary>
        /// <remarks>
        /// <p>
        /// It is assumed that the updated item might be a different instance. Handlers should know how to deal
        /// with potentially identifying equality.
        /// </p>
        /// <p>
        /// It is assumed that an update could relate to a item for which no ItemCreated notification was sent out prior.
        /// </p>
        /// </remarks>
        event EventHandler<DataChangeEventArgs<T>> ItemUpdated;

        /// <summary>
        /// Fired when an items is deleted from a data source.
        /// </summary>
        event EventHandler<DataChangeEventArgs<T>> ItemDeleted;
    }
}