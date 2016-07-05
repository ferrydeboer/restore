using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Restore.Channel;
using Restore.Channel.Configuration;
using Restore.Matching;

namespace Restore.Configuration
{
    public class RestoreConfiguration<TBase1, TBase2, TId> : IRestoreConfiguration
        where TId : IEquatable<TId>
    {
        public ISource<TBase1, TId> Source1 { get; set; }
        public ISource<TBase2, TId> Source2 { get; set; }

        public IPlumberFactory PlumberFactory { get; set; } = new PlumberFactory();

        private readonly RuleContainer<TBase1, TBase2, TId> _rules = new RuleContainer<TBase1, TBase2, TId>();
        private readonly IList<object> _channels = new List<object>();

        // Lacks observers, are now in the channel setup.

        /// <summary>
        /// In many cases the rules for synchronization of certain types are similar. Add predefined
        /// rules here. They will then be injected in the channel when that is being constructed.
        /// </summary>
        public void AddGenericRule<T>()
            where T : ISynchronizationRule<TBase1, TBase2, TId>, new()
        {
            _rules.AddGenericRule<T>();
        }

        // Sort of registration of factory, because the channel setup is merely a factory containing the data provider instances.
        public void CreateChannel<T1, T2>(IChannelSetup<T1, T2> setup)
            where T1 : TBase1
            where T2 : TBase2
        {
            // Constructing or expanding the channel setup model in such a way while still having type information
            // is a better strategy. Doing it afterwards requires a lot more effort using reflection.
            // - Find endpoints and build configuration using them.
            var t1Endpoint = Source1.GetEndpoint<T1>();
            var t1Extractor = Source1.CreateResolver<T1>();
            var t1TypeConfig = new TypeConfiguration<T1, TId>(t1Extractor);
            var t1EndpointConfig = new EndpointConfiguration<T1, TId>(t1TypeConfig, t1Endpoint);

            var t2Endpoint = Source2.GetEndpoint<T2>();
            var t2Extractor = Source2.CreateResolver<T2>();
            var t2TypeConfig = new TypeConfiguration<T2, TId>(t2Extractor);
            var t2EndpointConfig = new EndpointConfiguration<T2, TId>(t2TypeConfig, t2Endpoint);

            var channelConfig = new ChannelConfiguration<T1, T2, TId, ItemMatch<T1, T2>>(t1EndpointConfig, t2EndpointConfig, setup.TypeTranslator);

            var plumber = PlumberFactory.Create(channelConfig, _rules);
/*            // Everything below this is required for construction of the pipeline. If I would build an IPipelineFactory.
            // Questions if wether to make the pipeline factory then part of the config or not and how to deal with the rules.
            var itemMatcher = new ItemMatcher<T1, T2, TId, ItemMatch<T1, T2>>(channelConfig);
            channelConfig.ItemsPreprocessor = itemMatcher.Match;

            // - Instantiate Rules
            foreach (Type rule in _rules)
            {
                var genericTypeDef = rule.GetGenericTypeDefinition();
                var closedGenericTypeDef = genericTypeDef.MakeGenericType(typeof (T1), typeof (T2), typeof (TId));
                var ruleInstance = Activator.CreateInstance(closedGenericTypeDef) as SynchronizationRule<T1, T2, TId>;
                // - Create Rule instance
                if (ruleInstance != null)
                {
                    var resolver = ruleInstance.ResolverInstance(channelConfig);
                    channelConfig.AddSynchAction(resolver);
                }
            }*/

            // Could move towards a construction where factories are registered instead of the actual objects for preventing to many channels linguering
            // around in memory. However, right now the channels needs to be singletons and run in sequence due to sqlite limitations of having only one
            // thread writing!
            var channel = setup.Create(channelConfig, plumber);
            _channels.Add(channel);
        }

        public IEnumerable<T> GetChannels<T>()
            where T : ISynchChannel
        {
            var x = _channels.Where(c => c.GetType() == typeof(T)).Cast<T>();
            return x;
        }

        public IEnumerable<ISynchChannel> GetChannels(Type channelMarkerType)
        {
            var enumerable = _channels.Where(c =>
            {
                var channelType = c.GetType().GetTypeInfo();

                // The channel instance is a closed type while the interface provided is an open type.
                // So when the type provided is an open type, so if channelMarkerType is an open type, whe need to check every possible interface
                // to see it's a closed type and if so get it's generic definition.
                var isOfMarkedType = false;
                if (!channelMarkerType.IsConstructedGenericType)
                {
                    var interfaces = channelType.ImplementedInterfaces;
                    if ((from impIf in interfaces
                        where impIf.IsConstructedGenericType
                        select impIf.GetGenericTypeDefinition()).Any(openIf => openIf == channelMarkerType))
                    {
                        isOfMarkedType = true;
                    }
                }
                else
                {
                    isOfMarkedType = channelType.ImplementedInterfaces.Contains(channelMarkerType);
                }

                return isOfMarkedType;
            });
            return enumerable.Cast<ISynchChannel>();
        }

        // REFACTOR: There will be pull and push channels. Have not solved proper way of designing this.
        public ISynchChannel GetChannel<T>()
            where T : class, ISynchChannel
        {
            var x = _channels.First(c => c.GetType() == typeof(T)) as T;
            return x;
        }

        public ISynchChannel GetChannel(Type markerType)
        {
            return GetChannels(markerType).First();
        }

        public IEnumerable<ISynchChannel> Channels => _channels.Cast<ISynchChannel>();
    }

    public class PlumberFactory : IPlumberFactory
    {
        public IPreprocessorAppender Appender { get; set; }

        public virtual IPlumber<T1, T2, TId> Create<T1, T2, TId>(ISynchSourcesConfig<T1, T2, TId> source, IRuleContainer<TId> rules)
            where TId : IEquatable<TId>
        {
            var preprocessor = CreatePreprocessor(source, rules);
            var synchronizationResolvers = rules.GetTypedResolvers(source).ToList();
            var plumber = new ItemMatchPipelinePlumber<T1, T2, TId>(source, synchronizationResolvers, preprocessor);
            if (Appender != null)
            {
                plumber.Appender = Appender;
            }

            return plumber;
        }

        // Could also put this in another factory method.
        protected virtual Func<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<ItemMatch<T1, T2>>> CreatePreprocessor<T1, T2, TId>(ISynchSourcesConfig<T1, T2, TId> source, IRuleContainer<TId> rules)
            where TId : IEquatable<TId>
        {
            var matcher = new ItemMatcher<T1, T2, TId, ItemMatch<T1, T2>>(source);
            Func<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<ItemMatch<T1, T2>>> preprocessor = matcher.Match;
            return preprocessor;
        }
    }

    public class FullMatchAppender : IPreprocessorAppender
    {
        public IEnumerable<ItemMatch<T1, T2>> Append<T1, T2, TId>(ISynchSourcesConfig<T1, T2, TId> sourceConfig, IEnumerable<ItemMatch<T1, T2>> inlet)
            where TId : IEquatable<TId>
        {
            return inlet.CompleteSingleItems(sourceConfig, TargetType.T1)
                .BatchCompleteItems(sourceConfig, TargetType.T2);
        }
    }
}