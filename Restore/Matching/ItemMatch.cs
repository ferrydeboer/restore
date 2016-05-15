using System;
using System.Collections.Generic;

namespace Restore.Matching
{
    public class ItemMatch<T1, T2>
    {
        public T1 Result1 { get; }

        public T2 Result2 { get; }

        public bool IsComplete =>
            !EqualityComparer<T1>.Default.Equals(Result1, default(T1))
            && !EqualityComparer<T2>.Default.Equals(Result2, default(T2));

        public ItemMatch(T1 result1, T2 result2)
        {
            // TODO: What to do if there is no default Equality comparer?
            if (EqualityComparer<T1>.Default.Equals(result1, default(T1))
                && EqualityComparer<T2>.Default.Equals(result2, default(T2)))
            {
                throw new ArgumentException("A match can never contain two items that contain no value!");
            }

            Result1 = result1;
            Result2 = result2;
        }
    }

    public interface IItemMatch<out T1, out T2>
    {
        T1 Result1 { get; }
        T2 Result2 { get; }
    }
}