using System;
using System.Collections.Generic;
using JetBrains.Annotations;

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
    /// <typeparam name="TId">The type of the identifier that identifies <typeparamref name="T"/></typeparam>
    public interface ICrudEndpoint<T, TId> : IDataChangeNotifier<T>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// Create an item once. Calling this method twice with the same object instance should result
        /// in a second instance of the same item with a new identifier.
        /// </summary>
        /// <param name="item">The item to save.</param>
        /// <returns>The resulting T which could possibly differ from <paramref name="item"/> due to
        /// side effects of the operation implementation.</returns>
        T Create(T item);

        T Read(TId id);

        [NotNull]
        IEnumerable<T> Read(params TId[] ids);

        /// <summary>
        /// Updates <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The item to update.</param>
        /// <returns>The resulting T which could possibly differ from <paramref name="item"/> due to
        /// side effects of the operation implementation.</returns>
        T Update(T item);

        /// <summary>
        /// Deletes the <paramref name="item"/>
        /// </summary>
        /// <param name="item">The item to delete</param>
        /// <returns>The resulting T which could possibly differ from <paramref name="item"/> due to
        /// side effects of the operation implementation.</returns>
        T Delete(T item);
    }
}