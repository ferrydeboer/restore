using System;
using JetBrains.Annotations;

namespace Restore.Channel.Configuration
{
    public class TypeConfiguration<T, TId> where TId : IEquatable<TId>
    {
        [NotNull] public readonly Func<T, TId> IdExtractor;

        public TypeConfiguration([NotNull] Func<T, TId> idExtractor)
        {
            if (idExtractor == null) throw new ArgumentNullException(nameof(idExtractor));

            IdExtractor = idExtractor;
        }
    }
}