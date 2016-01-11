using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using Restore.Channel.Configuration;
using Restore.Matching;
using Restore.RxProto;
using Restore.Tests.ChangeResolution;
using Restore.Tests.RxProto;

namespace Restore.Tests.Channel
{
    /// <summary>
    /// Let's just start of with the simplest channel
    /// </summary>
    [TestFixture]
    public class OneWayPullChannelTest
    {
        // Create channel and trigger a pump data into an observable collection. (synch twice)
        // Create channel and trigger a pump/Open. (this is background synch)
        [Test]
        public async Task ShouldSynchNewDataFromRemote()
        {
            var type1Config = new TypeConfiguration<LocalTestResource, int>(ltr => ltr.CorrelationId.HasValue ? ltr.CorrelationId.Value : -1);
            var localEndpoint = new InMemoryCrudDataEndpoint<LocalTestResource, int>(type1Config, TestData.LocalResults);
            var endpoint1Config = new EndpointConfiguration<LocalTestResource, int>(
                type1Config, 
                localEndpoint);

            var type2Config = new TypeConfiguration<RemoteTestResource, int>(rtr => rtr.Id);
            var remoteEndpoint = new InMemoryCrudDataEndpoint<RemoteTestResource, int>(type2Config, TestData.RemoteResults);
            var endpoint2Config = new EndpointConfiguration<RemoteTestResource, int>(
                type2Config,
                remoteEndpoint);

            // This clearly requires a configuration API.
            var channelConfig = new ChannelConfiguration<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(endpoint1Config, endpoint2Config, new TestResourceTranslator());
            var itemsPreprocessor = new ItemMatcher<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(channelConfig);
            channelConfig.ItemsPreprocessor = itemsPreprocessor.Match;
            channelConfig.AddSynchAction(new ItemMatchSynchronizationAction<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(
                channelConfig,
                (item, cfg) =>
                {
                    return item.Result1 == null;
                },
                (item, cfg) =>
                {
                    var synchItem = item.Result1;
                    cfg.TypeTranslator.TranslateBackward(item.Result2, ref synchItem);
                    // Now the translate decides wether a new item has to be created, but the decision is there anyway because of the Create.
                    cfg.Type1EndpointConfiguration.Endpoint.Create(synchItem);
                    return new SynchronizationResult(true);
                }
            ));
            
            //var t2epConfig = new EndpointConfiguration()
            ISynchChannel<LocalTestResource, RemoteTestResource> channelUnderTest = new OneWayPullChannel<LocalTestResource, RemoteTestResource, int, ItemMatch<LocalTestResource, RemoteTestResource>>(
                channelConfig, 
                () => Task.FromResult(localEndpoint.ReadAll().AsEnumerable()),
                () => Task.FromResult(remoteEndpoint.ReadAll().AsEnumerable()));
            
            //channelUnderTest.SynchStarted 
            // First just make a channel that we can call synch on.
            await channelUnderTest.Synchronize();

/*            var synched1 = localEndpoint.Read(1);
            Assert.IsNotNull(synched1);
            Assert.AreEqual(TestData.RemoteResults[0].Name, synched1.Name);

            Assert.IsNull(localEndpoint.Read(2));*/

            var synched3 = localEndpoint.Read(3);
            Assert.IsNotNull(synched3);
            Assert.AreEqual(TestData.RemoteResults[1].Name, synched3.Name);
        }
    }

    public class ItemMatchSynchronizationAction<T1, T2, TId, TSynch> : ISynchronizationAction<ItemMatch<T1, T2>> where TId : IEquatable<TId>
    {
        private IChannelConfiguration<T1, T2, TId, TSynch> _channelConfig;
        private readonly Func<ItemMatch<T1, T2>, IChannelConfiguration<T1, T2, TId, TSynch>, bool> _decision;
        private readonly Func<ItemMatch<T1, T2>, IChannelConfiguration<T1, T2, TId, TSynch>, SynchronizationResult> _action;
        private ItemMatch<T1, T2> _applicant;
        private SynchronizationResult _synchronizationResult;

        public ItemMatchSynchronizationAction(
            IChannelConfiguration<T1, T2, TId, TSynch> channelConfig, 
            Func<ItemMatch<T1, T2>, IChannelConfiguration<T1, T2, TId, TSynch>, bool> decision, 
            Func<ItemMatch<T1, T2>, IChannelConfiguration<T1, T2, TId, TSynch>, SynchronizationResult> action)
        {
            _channelConfig = channelConfig;
            _decision = decision;
            _action = action;
        }


        public bool AppliesTo(ItemMatch<T1, T2> item)
        {
            var appliesTo = _decision(item, _channelConfig);
            if (appliesTo)
            {
                _applicant = item;
            }
            return appliesTo;
        }

        // Possible currying variant of this. The only issue with this is that it looses most of it's context
        // and thus makes it harder to later on determine potential execution rules. For now let's stick with
        // the interface variant though more error prone.
        // An alternative better way to solve this is in the end change this is such a way that we don't return
        // a Func but just another object with the execute action and the state. Simple as that.
        public Func<SynchronizationResult> Applies(ItemMatch<T1, T2> item)
        {
            var appliesTo = _decision(item, _channelConfig);
            if (appliesTo)
            {
                return () => _action(item, _channelConfig);
            }
            return null;
        }

        public SynchronizationResult Execute()
        {
            if (_applicant == null)
            {
                throw new InvalidOperationException("Can't execute this synchronization action since there is no applicant.");
            }
            _synchronizationResult = _action(_applicant, _channelConfig);
            // Because the action contains state it can not be executed twice. Only other option is actually
            // returning a curried function from the AppliesTo.
            _applicant = null;
            return _synchronizationResult;
        }
    }

    //public ExecuteableSynchronizationAction

    public interface ISynchChannel<T1, T2>
    {
        Task Synchronize();
    }

    // Since I'm not fully sure what design is going to look like in terms of different channel types
    // I just name the channel to what I intend it to do.
    public class OneWayPullChannel<T1, T2, TId, TSynch> : ISynchChannel<T1, T2> where TId : IEquatable<TId>
    {
        [NotNull] private readonly IChannelConfiguration<T1, T2, TId, TSynch> _channelConfig;

        /// <summary>
        /// Didn't wan't to make this part of the more general configuration. It's not decided yet how to further work with data sources
        /// and possible replication.
        /// </summary>
        [NotNull] private readonly Func<Task<IEnumerable<T1>>> _t1DataSource;

        [NotNull] private readonly Func<Task<IEnumerable<T2>>> _t2DataSource;

        public OneWayPullChannel(
            [NotNull] IChannelConfiguration<T1, T2, TId, TSynch> channelConfig,
            [NotNull] Func<Task<IEnumerable<T1>>> t1DataSource, 
            [NotNull] Func<Task<IEnumerable<T2>>> t2DataSource)
        {
            if (channelConfig == null) throw new ArgumentNullException(nameof(channelConfig));
            if (t1DataSource == null) throw new ArgumentNullException(nameof(t1DataSource));
            if (t2DataSource == null) throw new ArgumentNullException(nameof(t2DataSource));

            _channelConfig = channelConfig;
            _t1DataSource = t1DataSource;
            _t2DataSource = t2DataSource;
        }

        public async Task Synchronize()
        {
            var t1Data = await _t1DataSource();
            var t2Data = await _t2DataSource();
            var pipeline = _channelConfig.ItemsPreprocessor(t1Data, t2Data)
                .ResolveChange(ChangeResolver)
                .Select(Dispatcher);

            foreach (SynchronizationResult result in pipeline)
            {
                if (!result)
                {
                    Debug.WriteLine("Failed executing an item.");
                }
            }
        }

        public ISynchronizationAction<TSynch> ChangeResolver
            ([NotNull] TSynch item)
        {
            if (item == null) {  throw new ArgumentNullException(nameof(item)); }

            // TODO: Error handling?
            return _channelConfig.SynchronizationActions.FirstOrDefault(action => action.AppliesTo(item)) ?? new NullSynchAction<TSynch>();
            // Injecting NullSynchActions provides means of logging
        }

        public SynchronizationResult Dispatcher(ISynchronizationAction<TSynch> synchAction)
        {
            // TODO: Error handling
            return synchAction.Execute();
        }
    }

    public static class ChangeResolver
    {
        public static IEnumerable<ISynchronizationAction<TSynch>> ResolveChange<TSynch>(this IEnumerable<TSynch> items, Func<TSynch, ISynchronizationAction<TSynch>> transformer)
        {
            // How to handle errors here? Probably need a way to catch them and dispatch them onto a handler?
            return items.Select(item => transformer(item));
        }
    }

    /*
    public static class ChangeDispatcher
    {
        public static IEnumerable<SynchronizationResult> Dispatch<TSynch>(this IEnumerable<ISynchronizationAction<TSynch>> ) 
    }
    */
}
