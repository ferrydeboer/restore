using System;
using JetBrains.Annotations;

namespace Restore.Channel.Configuration
{
    /// <summary>
    ///     Endpoint configuration that is based on a single data source callback. These opposite endpoint should
    ///     essentially contain a callback that returns the same result on the other endpoint. For this same reason
    ///     the write part of the endpoint is split up from the reading data source.
    /// </summary>
    /// <typeparam name="T">The data type for this endpoint.</typeparam>
    /// <typeparam name="TId">The id that <typeparamref name="T" /> is identified by.</typeparam>
    public class EndpointConfiguration<T, TId> : IEndpointConfiguration<T, TId>
        where TId : IEquatable<TId>
    {
        public EndpointConfiguration(
            [NotNull] TypeConfiguration<T, TId> typeConfig,
            [NotNull] ICrudEndpoint<T, TId> endpoint)
        {
            if (typeConfig == null) { throw new ArgumentNullException(nameof(typeConfig)); }
            if (endpoint == null) { throw new ArgumentNullException(nameof(endpoint)); }
            TypeConfig = typeConfig;
            Endpoint = endpoint;
            EndpointType = typeof(T);
        }

        public Type EndpointType { get; }

        public TypeConfiguration<T, TId> TypeConfig { get; }

        public ICrudEndpoint<T, TId> Endpoint { get; }
        public bool IsNew(T item)
        {
            // If the default is null this doesn't work! However, I don't think that supports IEquatable!?
            return TypeConfig.IdExtractor(item).Equals(TypeConfig.DefaultExtractorValue);
        }
    }
}