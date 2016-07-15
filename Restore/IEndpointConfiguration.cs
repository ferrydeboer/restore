using System;
using JetBrains.Annotations;
using Restore.Channel.Configuration;

namespace Restore
{
    public interface IEndpointConfiguration<T, TId>
        where TId : IEquatable<TId>
    {
        [NotNull] Type EndpointType { get; }

        [NotNull] TypeConfiguration<T, TId> TypeConfig { get; }

        [NotNull] ICrudEndpoint<T, TId> Endpoint { get; }

        bool IsNew(T item);
    }
}