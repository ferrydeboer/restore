using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Restore.ChangeResolution;
using Restore.Channel;
using Restore.Channel.Configuration;
using Restore.Matching;
using Restore.Tests.ChangeResolution;

namespace Restore.Tests.Channel
{
    public class OneWayPullChannelTestBase : IDisposable
    {
        protected OneWayPullChannel<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>> ChannelUnderTest { get; set; }

        protected InMemoryCrudDataEndpoint<LocalTestResource, int> LocalEndpoint { get; set; }
        protected InMemoryCrudDataEndpoint<RemoteTestResource, int> RemoteEndpoint { get; set; }
        protected ChannelConfiguration<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>> ChannelConfig { get; set; }

        protected ItemMatchPipelinePlumber<LocalTestResource, RemoteTestResource, int> Plumber { get; set; }

        [SetUp]
        public void SetUpTest()
        {
            var type1Config = new TypeConfiguration<LocalTestResource, int>(
                ltr => ltr.CorrelationId.HasValue ? ltr.CorrelationId.Value : -1,
                -1);
            LocalEndpoint = new InMemoryCrudDataEndpoint<LocalTestResource, int>(type1Config, TestData.LocalResults);
            var endpoint1Config = new EndpointConfiguration<LocalTestResource, int>(
                type1Config,
                LocalEndpoint);

            var type2Config = new TypeConfiguration<RemoteTestResource, int>(rtr => rtr.Id, 0);
            RemoteEndpoint = new InMemoryCrudDataEndpoint<RemoteTestResource, int>(type2Config, TestData.RemoteResults);
            var endpoint2Config = new EndpointConfiguration<RemoteTestResource, int>(
                type2Config,
                RemoteEndpoint);

            // This clearly requires a configuration API.
            ChannelConfig = new ChannelConfiguration<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(endpoint1Config, endpoint2Config, new TestResourceTranslator());
            var itemsPreprocessor = new ItemMatcher<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(ChannelConfig);

            ChannelConfig.ItemsPreprocessor = itemsPreprocessor.Match;

            var resolvers = new List<ISynchronizationResolver<ItemMatch<LocalTestResource, RemoteTestResource>>>
            {
                new SynchronizationResolver<ItemMatch<LocalTestResource,RemoteTestResource>, ChannelConfiguration<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>>(
                ChannelConfig,
                (item, cfg) => item.Result1 == null,
                (item, cfg) =>
                {
                    var synchItem = item.Result1;
                    cfg.TypeTranslator.TranslateBackward(item.Result2, ref synchItem);

                    // Now the translate decides wether a new item has to be created, but the decision is there anyway because of the Create.
                    cfg.Type1EndpointConfiguration.Endpoint.Create(synchItem);
                    return new SynchronizationResult(true);
                })
            };
            Plumber = new ItemMatchPipelinePlumber<LocalTestResource, RemoteTestResource, int>(ChannelConfig, resolvers, itemsPreprocessor.Match);
            ConstructTestSubject();
        }

        protected void ConstructTestSubject()
        {
            ChannelUnderTest = new OneWayPullChannel
                <LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(
                ChannelConfig,
                Plumber,
                () => Task.FromResult(LocalEndpoint.ReadAll().AsEnumerable()),
                () => Task.FromResult(RemoteEndpoint.ReadAll().AsEnumerable()));
        }

        public void Dispose()
        {
            ChannelUnderTest.Dispose();
        }
    }
}