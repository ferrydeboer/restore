using System;
using System.Collections.ObjectModel;

namespace Restore.Extensions
{
    public static class ObservableCollectionExtensions
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
        public static void SortInsert<T>(this ObservableCollection<T> collection, T item, Func<T, T, int> comparer, bool descending)
        {
            var finalComparer = comparer;
            if (descending)
            {
                finalComparer = (arg1, arg2) => comparer(arg1, arg2) * -1;
            }

            collection.SortInsert(item, finalComparer);
        }

        /// <summary>
        ///     Inserts item into the collection using the comparer assuming an already sorted list.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="collection">The collection to insert the item in.</param>
        /// <param name="item">The item to insert.</param>
        /// <param name="comparer">The comparer to use</param>
        public static void SortInsert<T>(this ObservableCollection<T> collection, T item, Func<T, T, int> comparer)
        {
            // Can only modify the collection one at a time.
            lock (collection)
            {
                if (collection.Count == 0)
                {
                    collection.Add(item);
                    return;
                }

                var index = collection.FindItemIndex(item, comparer);
                collection.Insert(index, item);
            }
        }

        /// <summary>
        /// Returns the index the given <paramref name="item"/> should be at given the <paramref name="comparer"/>
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="collection">The collection to find the index in.</param>
        /// <param name="item">The item to find the index for.</param>
        /// <param name="comparer">The comparer to use to determine the item.</param>
        /// <returns>The index withing the <paramref name="collection"/> where <paramref name="item"/> belongs</returns>
        public static int FindItemIndex<T>(this ObservableCollection<T> collection, T item, Func<T, T, int> comparer)
        {
            var index = 0;

            while (index < collection.Count /*call first to prevent expression from further evaluating and raise ArgumentOutOfRangeException*/
                   && comparer(item, collection[index]) > 0)
            {
                index++;
            }

            return index;
        }

        public static void SortInsert<T>(this ObservableCollection<T> collection, T item, bool descending = false)
            where T : IComparable<T>
        {
            SortInsert(collection, item, (x, y) => x.CompareTo(y), descending);
        }
    }
}