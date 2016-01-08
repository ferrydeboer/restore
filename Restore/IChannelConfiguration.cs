using System;
using Restore.Channel.Configuration;

namespace Restore
{
    public interface IChannelConfiguration<T1, T2, TId> where TId : IEquatable<TId>
    {
        TypeConfiguration<T1, TId> Type1Configuration { get; }
        TypeConfiguration<T2, TId> Type2Configuration { get; }

        IEndpointConfiguration<T1, TId> Type1EndpointConfiguration { get; }
        IEndpointConfiguration<T2, TId> Type2EndpointConfiguration { get; }
    }
}
