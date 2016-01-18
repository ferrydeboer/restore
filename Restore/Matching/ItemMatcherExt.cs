using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Restore.Matching
{
    public static class ItemMatcherExt
    {
        public static IEnumerable<T1> Match<T1, T2>(this IEnumerable<T1> first, Task<IEnumerable<T2>> secondProvider)
        {
            return null;
        }
    }
}
