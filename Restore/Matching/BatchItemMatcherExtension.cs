using System;
using System.Collections.Generic;
using System.Linq;

namespace Restore.Matching
{
    public static class BatchItemMatcherExtension
    {
        public static IEnumerable<ItemMatch<T1, T2>> BatchCompleteItems<T1, T2, TId>(this IEnumerable<ItemMatch<T1, T2>> original, ISynchSourcesConfig<T1, T2, TId> channelConfig, TargetType appendType)
            where TId : IEquatable<TId>
        {
            List<ItemMatch<T1,T2>> appendableMatches = new List<ItemMatch<T1, T2>>();

            foreach (var itemMatch in original)
            {
                if (itemMatch.IsComplete) { yield return itemMatch; }

                if (appendType == TargetType.T1 && !itemMatch.HasT1())
                {
                    appendableMatches.Add(itemMatch);
                }

                if (appendType == TargetType.T2 && !itemMatch.HasT2())
                {
                    appendableMatches.Add(itemMatch);
                }
            }

            if (appendableMatches.Count > 0)
            {
                // The logic to match two lists already exists, we can simply reuse it!
                var matcher = new ItemMatcher<T1, T2, TId, ItemMatch<T1, T2>>(channelConfig);

                IEnumerable<T1> append1 = null;
                IEnumerable<T2> append2 = null;
                if (appendType == TargetType.T1)
                {
                    var batchIds = appendableMatches.Select(
                        match => channelConfig.Type2EndpointConfiguration.TypeConfig.IdExtractor(match.Result2))
                        .ToArray();
                    append1 = channelConfig.Type1EndpointConfiguration.Endpoint.Read(batchIds);
                    append2 = appendableMatches.Select(match => match.Result2);
                }

                if (appendType == TargetType.T2)
                {
                    var batchIds = appendableMatches.Select(
                        match => channelConfig.Type1EndpointConfiguration.TypeConfig.IdExtractor(match.Result1))
                        .ToArray();
                    append1 = appendableMatches.Select(match => match.Result1);
                    append2 = channelConfig.Type2EndpointConfiguration.Endpoint.Read(batchIds);
                }

                if (append1 == null || append2 == null)
                {
                    throw new Exception("Having two empty lists is can not be matched! Something went wrong.");
                }

                foreach (var newMatch in matcher.Match(append1, append2))
                {
                    yield return newMatch;
                }
            }
        }
    }
}