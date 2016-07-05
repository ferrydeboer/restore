using System;
using JetBrains.Annotations;
using Restore.ChangeResolution;

namespace Restore.Channel.Configuration
{
    public class SynchSourcesConfiguration<T1, T2, TId> : ISynchSourcesConfig<T1, T2, TId>
        where TId : IEquatable<TId>
    {
        public ITranslator<T1, T2> TypeTranslator { get; }
        public IEndpointConfiguration<T1, TId> Type1EndpointConfiguration { get; }
        public IEndpointConfiguration<T2, TId> Type2EndpointConfiguration { get; }

        public SynchSourcesConfiguration(
            [NotNull] IEndpointConfiguration<T1, TId> type1EndpointConfiguration,
            [NotNull] IEndpointConfiguration<T2, TId> type2EndpointConfiguration,
            [NotNull] ITranslator<T1, T2> typeTranslator)
        {
            if (type1EndpointConfiguration == null) { throw new ArgumentNullException(nameof(type1EndpointConfiguration)); }
            if (type2EndpointConfiguration == null) { throw new ArgumentNullException(nameof(type2EndpointConfiguration)); }

            Type1EndpointConfiguration = type1EndpointConfiguration;
            Type2EndpointConfiguration = type2EndpointConfiguration;
            TypeTranslator = typeTranslator;
        }
    }
}