using System;
using System.Collections.Generic;
using System.Linq;
using Restore.Matching;

namespace Restore.Tests.Matching
{
    public static class BatchItemMatcherExtension
    {
        public static IEnumerable<ItemMatch<T1, T2>> BatchMatchMissing<T1, T2, TId>(this IEnumerable<ItemMatch<T1, T2>> original, IChannelConfiguration<T1, T2, TId, ItemMatch<T1, T2>> channelConfig, Type appendType = null) where TId : IEquatable<TId>
        {
            if (appendType != null 
                && appendType != channelConfig.Type1EndpointConfiguration.EndpointType
                && appendType != channelConfig.Type2EndpointConfiguration.EndpointType)
            {
                throw new ArgumentException($"Non matched type {appendType.Name} supplied, expecting {channelConfig.Type1EndpointConfiguration.EndpointType.Name} or {channelConfig.Type2EndpointConfiguration.EndpointType.Name}");
            }

            List<ItemMatch<T1,T2>> appendableMatches = new List<ItemMatch<T1, T2>>();

            foreach (var itemMatch in original)
            {
                if (itemMatch.IsComplete) { yield return itemMatch; }

                if (appendType != null && appendType == channelConfig.Type1EndpointConfiguration.EndpointType && !itemMatch.HasT1())
                {
                    appendableMatches.Add(itemMatch);
                }

                if (appendType != null && appendType == channelConfig.Type2EndpointConfiguration.EndpointType && !itemMatch.HasT2())
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
                if (appendType != null && appendType == channelConfig.Type1EndpointConfiguration.EndpointType)
                {
                    var batchIds = appendableMatches.Select(
                        match => channelConfig.Type2EndpointConfiguration.TypeConfig.IdExtractor(match.Result2))
                        .ToArray();
                    append1 = channelConfig.Type1EndpointConfiguration.Endpoint.Read(batchIds);
                    append2 = appendableMatches.Select(match => match.Result2);
                }

                if (appendType != null && appendType == channelConfig.Type2EndpointConfiguration.EndpointType)
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