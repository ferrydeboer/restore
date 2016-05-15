using System;
using System.Collections.Generic;
using Restore.ChangeResolution;
using Restore.Channel.Configuration;
using Restore.Matching;
using Restore.Tests.ChangeResolution;

namespace Restore.Tests
{
    public class TestConfiguration : IChannelConfiguration<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>
    {
        private readonly InMemoryCrudDataEndpoint<LocalTestResource, int> _t1Endpoint =
            new InMemoryCrudDataEndpoint<LocalTestResource, int>(new TypeConfiguration<LocalTestResource, int>(ltr => ltr.CorrelationId ?? -1));

        public IEndpointConfiguration<LocalTestResource, int> Type1EndpointConfiguration { get; }

        private readonly InMemoryCrudDataEndpoint<RemoteTestResource, int> _t2Endpoint =
            new InMemoryCrudDataEndpoint<RemoteTestResource, int>(new TypeConfiguration<RemoteTestResource, int>(rtr => rtr.Id));

        public TestConfiguration()
        {
            Type2EndpointConfiguration = new EndpointConfiguration<RemoteTestResource, int>(_t2Endpoint.TypeConfig, _t2Endpoint);
            Type1EndpointConfiguration = new EndpointConfiguration<LocalTestResource, int>(_t1Endpoint.TypeConfig, _t1Endpoint);
            SynchronizationResolvers = new List<ISynchronizationResolver<ItemMatch<LocalTestResource, RemoteTestResource>>>();
            TypeTranslator = new TestResourceTranslator();
        }

        public IEndpointConfiguration<RemoteTestResource, int> Type2EndpointConfiguration { get; }


        public Func<IEnumerable<LocalTestResource>, IEnumerable<RemoteTestResource>, IEnumerable<ItemMatch<LocalTestResource, RemoteTestResource>>> ItemsPreprocessor
        { get; set; }

        public IEnumerable<ISynchronizationResolver<ItemMatch<LocalTestResource, RemoteTestResource>>> SynchronizationResolvers
        { get; }

        public ITranslator<LocalTestResource, RemoteTestResource> TypeTranslator { get; }
    }
}
