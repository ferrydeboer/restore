using System;
using JetBrains.Annotations;

namespace Restore.Matching
{
    public class SingleItemCompletionMatcher<T1, T2, TId>
        where TId : IEquatable<TId>
    {
        private readonly ISynchSourcesConfig<T1, T2, TId> _channelConfiguration;
        private readonly TargetType _completionSourceType;

        public SingleItemCompletionMatcher(
            [NotNull] ISynchSourcesConfig<T1, T2, TId> channelConfiguration,
            TargetType completionSourceType)
        {
            if (channelConfiguration == null) { throw new ArgumentNullException(nameof(channelConfiguration)); }

            _channelConfiguration = channelConfiguration;
            _completionSourceType = completionSourceType;
        }

        public ItemMatch<T1, T2> Complete(ItemMatch<T1, T2> itemMatch)
        {
            if (itemMatch.IsComplete) { return itemMatch; }

            if (_completionSourceType == TargetType.T1 && itemMatch.Result1 == null)
            {
                var result2Id =
                    _channelConfiguration.Type2EndpointConfiguration.TypeConfig.IdExtractor(itemMatch.Result2);
                var oppositeItem = _channelConfiguration.Type1EndpointConfiguration.Endpoint.Read(result2Id);
                return new ItemMatch<T1, T2>(oppositeItem, itemMatch.Result2);
            }

            if (_completionSourceType == TargetType.T2 && itemMatch.Result2 == null)
            {
                var result1Id =
                    _channelConfiguration.Type1EndpointConfiguration.TypeConfig.IdExtractor(itemMatch.Result1);
                var oppositeItem = _channelConfiguration.Type2EndpointConfiguration.Endpoint.Read(result1Id);
                return new ItemMatch<T1, T2>(itemMatch.Result1, oppositeItem);
            }

            // Expecting not to even this this.
            return itemMatch;
        }
    }
}