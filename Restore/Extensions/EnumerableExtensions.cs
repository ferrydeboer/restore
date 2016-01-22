using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Restore.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Side effect extension method copied from Rx Do method. Simple returns a new IEnumerable
        /// that calls the observer action.
        /// </summary>
        /// <typeparam name="T">The type the enumerable contains.</typeparam>
        /// <param name="enumerable">The enumerables to add to.</param>
        /// <param name="observer">The action to be call for each item on the enumerable.</param
        public static IEnumerable<T> Do<T>(this IEnumerable<T> enumerable, [NotNull] Action<T> observer)
        {
            if (observer == null) { throw new ArgumentNullException(nameof(observer)); }
            return enumerable.Select<T, T>(item =>
            {
                observer(item);
                return item;
            });
        }
    }
}
