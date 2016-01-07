using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Restore.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        /// Extracts the given item from the list based on the the predicate.
        /// </summary>
        /// <returns>The extracted item or it's default value.</returns>
        public static T Extract<T>([NotNull] this IList<T> list, Func<T, bool> predicate)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            var item = list.FirstOrDefault(predicate);
            if (item != null)
            {
                list.Remove(item);
            }
            return item;
        }
    }
}
