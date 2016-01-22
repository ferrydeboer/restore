﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Restore.ChangeResolution;

namespace Restore.Channel.Configuration
{
    public class ChannelConfiguration<T1, T2, TId, TSynch> : IChannelConfiguration<T1, T2, TId, TSynch>
        where TId : IEquatable<TId>
    {
        [NotNull] public TypeConfiguration<T1, TId> Type1Configuration { get; }
        [NotNull] public TypeConfiguration<T2, TId> Type2Configuration { get; }
        public IEndpointConfiguration<T1, TId> Type1EndpointConfiguration { get; }
        public IEndpointConfiguration<T2, TId> Type2EndpointConfiguration { get; }
        public Func<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<TSynch>> ItemsPreprocessor { get; set; }

        public ITranslator<T1, T2> TypeTranslator { get; private set; }

        [NotNull] private readonly IList<ISynchronizationResolver<TSynch>> _synchronizationActions = new List<ISynchronizationResolver<TSynch>>();

        public IEnumerable<ISynchronizationResolver<TSynch>> SynchronizationResolvers => _synchronizationActions.AsEnumerable();

        public ChannelConfiguration(
            [NotNull] TypeConfiguration<T1, TId> type1Configuration,
            [NotNull] TypeConfiguration<T2, TId> type2Configuration)
        {
            if (type1Configuration == null) { throw new ArgumentNullException(nameof(type1Configuration)); }
            if (type2Configuration == null) { throw new ArgumentNullException(nameof(type2Configuration)); }

            Type1Configuration = type1Configuration;
            Type2Configuration = type2Configuration;
        }

        public ChannelConfiguration(
            [NotNull] Func<T1, TId> type1IdExtractor,
            [NotNull] Func<T2, TId> type2IdExtractor)
        {
            if (type1IdExtractor == null) { throw new ArgumentNullException(nameof(type1IdExtractor)); }
            if (type2IdExtractor == null) { throw new ArgumentNullException(nameof(type2IdExtractor)); }

            Type1Configuration = new TypeConfiguration<T1, TId>(type1IdExtractor);
            Type2Configuration = new TypeConfiguration<T2, TId>(type2IdExtractor);
        }

        public ChannelConfiguration(
            [NotNull] IEndpointConfiguration<T1, TId> type1EndpointConfiguration,
            [NotNull] IEndpointConfiguration<T2, TId> type2EndpointConfiguration,
            [NotNull] ITranslator<T1, T2> typeTranslator)
        {
            if (type1EndpointConfiguration == null) { throw new ArgumentNullException(nameof(type1EndpointConfiguration)); }
            if (type2EndpointConfiguration == null) { throw new ArgumentNullException(nameof(type2EndpointConfiguration)); }

            Type1EndpointConfiguration = type1EndpointConfiguration;
            Type1Configuration = type1EndpointConfiguration.TypeConfig;
            Type2EndpointConfiguration = type2EndpointConfiguration;
            TypeTranslator = typeTranslator;
            Type2Configuration = type2EndpointConfiguration.TypeConfig;
        }

        /*public void AddSynchAction(ISynchronizationAction<> matchItemSynchAction)
        {
            throw new NotImplementedException();
        }*/

        public void AddSynchAction([NotNull] ISynchronizationResolver<TSynch> action)
        {
            if (action == null) { throw new ArgumentNullException(nameof(action)); }

            _synchronizationActions.Add(action);
        }
    }
}