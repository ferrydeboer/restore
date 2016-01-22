using System;

namespace Restore
{
    /// <summary>
    /// <P>
    /// Basic representation of CRUD functions that you always expect on a certain endpoint.
    /// </P>
    /// <P>
    /// The reason these write functions return the type is because the operation might fail.
    /// For instance for concurrency reasons. Then the implementation can return the actual
    /// value for possible further processing.
    /// </P>
    /// </summary>
    /// <typeparam name="T">The type this endpoint stores.</typeparam>
    public interface ICrudEndpoint<T, TId> : IDataChangeNotifier<T>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// Create an item once. Calling this method twice with the same object instance should result
        /// in a second instance of the same item with a new identifier.
        /// </summary>
        /// <param name="item">The item to save.</param>
        /// <returns>The item resulting from the operation.</returns>
        T Create(T item);
        T Read(TId id);
        T Update(T item);
        T Delete(T item);
    }
}