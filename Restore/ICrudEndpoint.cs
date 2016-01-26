using System;

namespace Restore
{
    /// <summary>
    /// <P>
    /// Basic representation of CRUD functions that you always expect on a certain endpoint.
    /// </P>
    /// <P>
    /// Could evolve to operations
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
        void Create(T item);

        T Read(TId id);

        void Update(T item);

        void Delete(T item);
    }
}