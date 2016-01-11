using System;
using System.Collections.Generic;
using Restore.ChangeResolution;
using Restore.Channel.Configuration;
using Restore.RxProto;

namespace Restore
{
    /// <summary>
    /// This is an interface with a rather complex set of generics. It is decided to facilitate a rather complex scenario
    /// and later on determine how to possibly simplify this or otherwise better compose different components.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TSynch"></typeparam>
    public interface IChannelConfiguration<T1, T2, TId, TSynch> where TId : IEquatable<TId>
    {
        [Obsolete("Part of EndpointConfiguration now")]
        TypeConfiguration<T1, TId> Type1Configuration { get; }
        [Obsolete("Part of EndpointConfiguration now")]
        TypeConfiguration<T2, TId> Type2Configuration { get; }

        IEndpointConfiguration<T1, TId> Type1EndpointConfiguration { get; }
        IEndpointConfiguration<T2, TId> Type2EndpointConfiguration { get; }

        Func<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<TSynch>> ItemsPreprocessor { get; set; }

        IEnumerable<ISynchronizationAction<TSynch>> SynchronizationActions { get; }

        /// <summary>
        /// Is primarily a part of Change Resolution. That's why the inteface is defined there.
        /// </summary>
        ITranslator<T1, T2> TypeTranslator { get; }
    }
}
