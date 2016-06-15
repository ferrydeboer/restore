using System;
using JetBrains.Annotations;

namespace Restore.Channel.Configuration
{
    public class TypeConfiguration<T, TId>
        where TId : IEquatable<TId>
    {
        [NotNull]
        public Func<T, TId> IdExtractor { get; }

        public TypeConfiguration([NotNull] Func<T, TId> idExtractor)
        {
            if (idExtractor == null) { throw new ArgumentNullException(nameof(idExtractor)); }

            IdExtractor = idExtractor;
        }

        public TypeConfiguration(IIdResolver<T, TId> resolver)
        {
            IdExtractor = resolver.Resolve;
        }
    }
}