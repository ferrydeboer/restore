using System;
using System.Collections.Generic;

namespace Restore.Matching
{
    public class ItemMatch<T1, T2>
    {
        public T1 Result1 { get; }

        public T2 Result2 { get; }

        public bool IsComplete => HasT1() && HasT2();

        public ItemMatch(T1 result1, T2 result2)
        {
            Result1 = result1;
            Result2 = result2;

            // TODO: What to do if there is no default Equality comparer?
            if (!HasT1() && !HasT2())
            {
                throw new ArgumentException("A match can never contain two items that contain no value!");
            }
        }

        public bool HasT1()
        {
            return !EqualityComparer<T1>.Default.Equals(Result1, default(T1));
        }

        public bool HasT2()
        {
            return !EqualityComparer<T2>.Default.Equals(Result2, default(T2));
        }
    }

    public interface IItemMatch<out T1, out T2>
    {
        T1 Result1 { get; }
        T2 Result2 { get; }
    }
}