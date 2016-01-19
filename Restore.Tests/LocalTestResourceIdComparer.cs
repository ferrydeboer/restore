using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Restore.Tests
{
    public class LocalTestResourceIdComparer : IEqualityComparer<LocalTestResource>
    {
        public bool Equals(LocalTestResource x, LocalTestResource y)
        {
            if (x == null && y != null) { return false; }
            if (x != null && y == null) { return false; }
            if (x == null) { return true; }
            if (ReferenceEquals(x, y))  { return true; }

            return x.LocalId.Equals(y.LocalId);
        }

        public int GetHashCode([NotNull] LocalTestResource obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return obj.LocalId.GetHashCode();
        }
    }
}
