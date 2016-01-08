using System;
using JetBrains.Annotations;

namespace Restore.Channel.Configuration
{
    public class ChannelConfiguration<T1, T2, TId> : IChannelConfiguration<T1, T2, TId> where TId : IEquatable<TId>
    {
        [NotNull] public TypeConfiguration<T1, TId> Type1Configuration { get; }
        [NotNull] public TypeConfiguration<T2, TId> Type2Configuration { get; }
        public IEndpointConfiguration<T1, TId> Type1EndpointConfiguration { get; }
        public IEndpointConfiguration<T2, TId> Type2EndpointConfiguration { get; }

        public ChannelConfiguration(
            [NotNull] TypeConfiguration<T1, TId> type1Configuration,
            [NotNull] TypeConfiguration<T2, TId> type2Configuration)
        {
            if (type1Configuration == null) throw new ArgumentNullException(nameof(type1Configuration));
            if (type2Configuration == null) throw new ArgumentNullException(nameof(type2Configuration));

            Type1Configuration = type1Configuration;
            Type2Configuration = type2Configuration;
        }

        public ChannelConfiguration(
            [NotNull] Func<T1, TId> type1IdExtractor,
            [NotNull] Func<T2, TId> type2IdExtractor)
        {
            if (type1IdExtractor == null) throw new ArgumentNullException(nameof(type1IdExtractor));
            if (type2IdExtractor == null) throw new ArgumentNullException(nameof(type2IdExtractor));

            Type1Configuration = new TypeConfiguration<T1, TId>(type1IdExtractor);
            Type2Configuration = new TypeConfiguration<T2, TId>(type2IdExtractor);
        }

        public ChannelConfiguration(
            [NotNull] IEndpointConfiguration<T1, TId> type1EndpointConfiguration,
            [NotNull] IEndpointConfiguration<T2, TId> type2EndpointConfiguration)
        {
            if (type1EndpointConfiguration == null) throw new ArgumentNullException(nameof(type1EndpointConfiguration));
            if (type2EndpointConfiguration == null) throw new ArgumentNullException(nameof(type2EndpointConfiguration));

            Type1EndpointConfiguration = type1EndpointConfiguration;
            Type1Configuration = type1EndpointConfiguration.TypeConfig;
            Type2EndpointConfiguration = type2EndpointConfiguration;
            Type2Configuration = type2EndpointConfiguration.TypeConfig;
        }
    }
}