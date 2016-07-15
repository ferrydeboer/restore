using System;
using JetBrains.Annotations;

namespace Restore.Channel.Configuration
{
    public class TypeConfiguration<T, TId>
        where TId : IEquatable<TId>
    {
        [NotNull]
        public Func<T, TId> IdExtractor { get; }

        public TId DefaultExtractorValue { get; set; }

        public TypeConfiguration([NotNull] Func<T, TId> idExtractor, TId defaultExtractorValue)
        {
            if (idExtractor == null) { throw new ArgumentNullException(nameof(idExtractor)); }

            IdExtractor = idExtractor;
            DefaultExtractorValue = defaultExtractorValue;
        }

        public TypeConfiguration(IIdResolver<T, TId> resolver, TId defaultExtractorValue)
        {
            DefaultExtractorValue = defaultExtractorValue;
            IdExtractor = resolver.Resolve;
        }
    }
}