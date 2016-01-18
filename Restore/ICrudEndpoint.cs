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
    public interface ICrudEndpoint<T, TId> : IDataChangeNotifier<T> where TId : IEquatable<TId>
    {
        T Create(T item);
        T Read(TId id);
        T Update(T item);
        T Delete(T item);
    }
}