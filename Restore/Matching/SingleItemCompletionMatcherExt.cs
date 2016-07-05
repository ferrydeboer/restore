using System;
using System.Collections.Generic;
using System.Linq;

namespace Restore.Matching
{
    public static class SingleItemCompletionMatcherExt
    {
        public static IEnumerable<ItemMatch<T1, T2>> CompleteSingleItems<T1, T2, TId>(
            this IEnumerable<ItemMatch<T1, T2>> original,
            ISynchSourcesConfig<T1, T2, TId> channelConfig,
            TargetType appendType)
            where TId : IEquatable<TId>
        {
            var singleItemMatcher = new SingleItemCompletionMatcher<T1, T2, TId>(channelConfig, appendType);
            return original.Select(match => singleItemMatcher.Complete(match));
        }
    }
}
