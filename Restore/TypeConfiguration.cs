using System;
using JetBrains.Annotations;

namespace Restore
{
    public class TypeConfiguration<T, TId>
    {
        [NotNull] public readonly Func<T, IEquatable<TId>> IdExtractor;

        public TypeConfiguration([NotNull] Func<T, IEquatable<TId>> idExtractor)
        {
            if (idExtractor == null) throw new ArgumentNullException(nameof(idExtractor));

            IdExtractor = idExtractor;
        }
    }
}