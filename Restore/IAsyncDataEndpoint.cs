using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Restore
{
    /// <summary>
    /// <p>
    /// Data Endpoint interface that is being used as a facade and add's notification mechanisms to which one can subscribe for various purposes.
    /// </p>
    /// <p>
    /// Given the current asynchronous nature of a lot of application it is chosed to solely work with asynchronous definitions. Because it is fairly trivial
    /// to wrap a synchronous method in a Task within a facade implementation.
    /// </p>
    /// </summary>
    /// <typeparam name="T">The data type this endpoint is persisting and retrieving.</typeparam>
    public interface IAsyncDataEndpoint<T>
    {
        /// <summary>
        /// Gets the name of the endpoint.
        /// </summary>
        /// <value>
        /// The name of the endpoint which can be used to give it a clearer distinction than what the
        /// channel simply uses.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Thrown when a list of data is being loaded in a data endpoint by a query or some other event. This totally depends on the
        /// endpoint implementation.
        /// </summary>
        event EventHandler<DataLoadedEventArgs<T>> DataLoaded;

        /// <summary>
        /// Method that should return ALL resources of type T.
        /// </summary>
        /// <returns>All resources of type T.</returns>
        /// <remarks>
        /// Obvisouly task can not be null, but Enumerable should not be null but empty in case of zero results.
        /// </remarks>
        [NotNull]
        Task<IEnumerable<T>> GetAllAsync();
    }
}