using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Restore.Channel.Configuration
{
    /// <summary>
    /// Endpoint configuration that is based on a single data source callback. These opposite endpoint should
    /// essentially contain a callback that returns the same result on the other endpoint. For this same reason
    /// the write part of the endpoint is split up from the reading data source.
    /// </summary>
    /// <typeparam name="T">The data type for this endpoint.</typeparam>
    /// <typeparam name="TId">The id that <typeparamref name="T"/> is identified by.</typeparam>
    public class DataSourceEndpointConfiguration<T, TId> : IEndpointConfiguration<T, TId> where TId : IEquatable<TId>
    {
        public DataSourceEndpointConfiguration(TypeConfiguration<T, TId> typeConfig, Func<Task<IEnumerable<T>>> dataSource, ICrudEndpoint<T, TId> endpoint)
        {
            TypeConfig = typeConfig;
            DataSource = dataSource;
            Endpoint = endpoint;
        }

        public TypeConfiguration<T, TId> TypeConfig { get; }

        public Func<Task<IEnumerable<T>>> DataSource { get; }

        public ICrudEndpoint<T, TId> Endpoint { get; }
    }
}