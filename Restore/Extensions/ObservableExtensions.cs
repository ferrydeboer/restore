using System;
using System.Collections.ObjectModel;

namespace Restore.Extensions
{
    public static class ObservableExtensions
    {
        private static readonly Func<int, bool> AscendingSort = i => i > 0;
        private static readonly Func<int, bool> DescendingSort = i => i < 0;

        /// <summary>
        ///     Inserts item into the collection using the comparer assuming an already sorted list in the order
        ///     indicated by <paramref name="descending" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The colleciton to insert the item in.</param>
        /// <param name="item">The item to insert.</param>
        /// <param name="comparer">The comparer to use</param>
        /// <param name="descending">Assumes the collection is ascending by default. Set to true if the collection is descending.</param>
        public static void SortInsert<T>(this ObservableCollection<T> collection, T item, Func<T, T, int> comparer, bool descending = false)
        {
            // Can only modify the collection one at a time.
            lock (collection)
            {
                if (collection.Count == 0)
                {
                    collection.Add(item);
                    return;
                }

                var sort = descending ? DescendingSort : AscendingSort;
                var index = 0;

                while (index < collection.Count
                    /*call first to prevent expression from further evaluating and raise ArgumentOutOfRangeException*/
                       && sort(comparer(item, collection[index])))
                {
                    index++;
                }

                collection.Insert(index, item);
            }
        }

        public static void SortInsert<T>(this ObservableCollection<T> collection, T item, bool descending = false)
            where T : IComparable<T>
        {
            SortInsert(collection, item, (x, y) => x.CompareTo(y), descending);
        }
    }
}