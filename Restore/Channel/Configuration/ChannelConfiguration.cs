using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Restore.ChangeResolution;

namespace Restore.Channel.Configuration
{
    public class ChannelConfiguration<T1, T2, TId, TSynch> : IChannelConfiguration<T1, T2, TId, TSynch>
        where TId : IEquatable<TId>
    {
        public IEndpointConfiguration<T1, TId> Type1EndpointConfiguration { get; }
        public IEndpointConfiguration<T2, TId> Type2EndpointConfiguration { get; }
        public Func<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<TSynch>> ItemsPreprocessor { get; set; }

        public ITranslator<T1, T2> TypeTranslator { get; private set; }

        [NotNull] private readonly IList<ISynchronizationResolver<TSynch>> _synchronizationActions = new List<ISynchronizationResolver<TSynch>>();

        public IEnumerable<ISynchronizationResolver<TSynch>> SynchronizationResolvers => _synchronizationActions.AsEnumerable();

        public ChannelConfiguration(
            [NotNull] IEndpointConfiguration<T1, TId> type1EndpointConfiguration,
            [NotNull] IEndpointConfiguration<T2, TId> type2EndpointConfiguration,
            [NotNull] ITranslator<T1, T2> typeTranslator)
        {
            if (type1EndpointConfiguration == null) { throw new ArgumentNullException(nameof(type1EndpointConfiguration)); }
            if (type2EndpointConfiguration == null) { throw new ArgumentNullException(nameof(type2EndpointConfiguration)); }

            Type1EndpointConfiguration = type1EndpointConfiguration;
            Type2EndpointConfiguration = type2EndpointConfiguration;
            TypeTranslator = typeTranslator;
        }

        public void AddSynchAction([NotNull] ISynchronizationResolver<TSynch> action)
        {
            if (action == null) { throw new ArgumentNullException(nameof(action)); }

            _synchronizationActions.Add(action);
        }
    }
}