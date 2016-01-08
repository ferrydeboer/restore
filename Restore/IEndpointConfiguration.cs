using System;
using JetBrains.Annotations;
using Restore.Channel.Configuration;

namespace Restore
{
    public interface IEndpointConfiguration<T, TId> where TId : IEquatable<TId>
    {
        [NotNull] TypeConfiguration<T, TId> TypeConfig { get; }

        [NotNull] ICrudEndpoint<T, TId> Endpoint { get; }
    }
}