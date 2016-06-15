using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Restore.Matching
{
    /// <summary>
    /// Used to append initial matchresults that is based on two lists. When the list does not contain
    /// the items it can still exist (somewhere else) since the list is only the basis for the synchronization
    /// this matcher queries the endpoint to possible append the match.
    /// Could be written as a SynchronizationStep. But since it's added outside of channel
    /// generation and doesn't transform types it's more in line with the matchers.
    /// </summary>
    public class IndividualItemMatcher<T1, T2, TId, TSynch>
        where TId : IEquatable<TId>
    {
        private readonly Type _appendType;
        public IChannelConfiguration<T1, T2, TId, TSynch> ChannelConfig { get; }

        public IndividualItemMatcher([NotNull] IChannelConfiguration<T1, T2, TId, TSynch> channelConfig, Type appendType)
        {
            _appendType = appendType;
            if (channelConfig == null) { throw new ArgumentNullException(nameof(channelConfig)); }
            ChannelConfig = channelConfig;
        }

        public ItemMatch<T1, T2> AppendIndividualItem([NotNull] ItemMatch<T1, T2> initial, Type appendType = null)
        {
            if (appendType == null)
            {
                appendType = _appendType;
            }

            if (initial == null) { throw new ArgumentNullException(nameof(initial)); }

            if(initial.IsComplete) { return initial; }

            if (appendType == typeof(T1))
            {
                return Append<T1>(initial);
            }
            return Append<T2>(initial);
        }

        private ItemMatch<T1,T2> Append<T>(ItemMatch<T1, T2> match)
        {
            // Given the generics it very difficult to reduce duplication further here.
            if (typeof(T) == typeof(T1) && EqualityComparer<T1>.Default.Equals(match.Result1, default(T1)))
            {
                var item1Id = ChannelConfig.Type2EndpointConfiguration.TypeConfig.IdExtractor(match.Result2);
                var result1 = ChannelConfig.Type1EndpointConfiguration.Endpoint.Read(item1Id);
                if (!EqualityComparer<T1>.Default.Equals(result1, default(T1)))
                {
                    return new ItemMatch<T1, T2>(result1, match.Result2);
                }
            }
            if (typeof(T) == typeof(T2) && EqualityComparer<T2>.Default.Equals(match.Result2, default(T2)))
            {
                var itemId = ChannelConfig.Type1EndpointConfiguration.TypeConfig.IdExtractor(match.Result1);
                var result2 = ChannelConfig.Type2EndpointConfiguration.Endpoint.Read(itemId);
                if (!EqualityComparer<T2>.Default.Equals(result2, default(T2)))
                {
                    return new ItemMatch<T1, T2>(match.Result1, result2);
                }
            }

            return match;
        }

        /*
        public ItemMatch<T1, T2> AppendT1(ItemMatch<T1, T2> match, 
            Func<ItemMatch<T1, T2>, T1> itemReader, 
            Func<ItemMatch<T1, T2>, TId> idReader,
            Func<ItemMatch<T1, T2>, T1> itemEndpointReader)
        {
            if (EqualityComparer<T1>.Default.Equals(itemReader(match), default(T1)))
            {
                var item1Id = idReader(match);
                var result1 = itemEndpointReader(match);
                if (EqualityComparer<T1>.Default.Equals(result1, default(T1)))
                {
                    return new ItemMatch<T1, T2>(result1, match.Result2);
                }
            }

            return null;
        }*/
    }
}
