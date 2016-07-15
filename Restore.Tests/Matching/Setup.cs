using System;
using Restore.ChangeResolution;
using Restore.Channel.Configuration;
using Restore.Matching;
using Restore.Tests.ChangeResolution;

namespace Restore.Tests.Matching
{
    /// <summary>
    /// Contains a default configuration set that can be used throughout tests.
    /// </summary>
    public static class Setup
    {
        public static IChannelConfiguration<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>> TestChannelConfig()
        {
            return CreateChannelConfiguration(ltr => ltr.CorrelationId ?? -1, -1, rtr => rtr.Id, 0, new TestResourceTranslator());
        }

        public static IChannelConfiguration<T1, T2, TId, ItemMatch<T1, T2>> CreateChannelConfiguration<T1, T2, TId>(
            Func<T1, TId> t1IdResolver,
            TId t1DeafultResolverId,
            Func<T2, TId> t2IdResolver,
            TId t2DeafultResolverId,
            ITranslator<T1, T2> translator)
            where TId : IEquatable<TId>
        {
            var t1EndpointCfg = CreateTestEndpointConfig(t1IdResolver, t1DeafultResolverId);
            var t2EndpointCfg = CreateTestEndpointConfig(t2IdResolver, t2DeafultResolverId);

            var channelConfig = new ChannelConfiguration<T1, T2, TId, ItemMatch<T1, T2>>(
                t1EndpointCfg,
                t2EndpointCfg,
                translator);

            return channelConfig;
        }

        public static IEndpointConfiguration<T, TId> CreateTestEndpointConfig<T, TId>(Func<T, TId> idResolver, TId defaultIdValue)
            where TId : IEquatable<TId>
        {
            return CreateTestEndpointConfig(new TypeConfiguration<T, TId>(idResolver, defaultIdValue));
        }

        public static IEndpointConfiguration<T, TId> CreateTestEndpointConfig<T, TId>(TypeConfiguration<T, TId> configuration)
            where TId : IEquatable<TId>
        {
            var endpoint = new InMemoryCrudDataEndpoint<T, TId>(configuration);
            return new EndpointConfiguration<T, TId>(configuration, endpoint);
        }
    }
}
