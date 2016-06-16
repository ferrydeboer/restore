using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Restore.ChangeResolution;

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
    /// <remarks>
    /// TODO: Figure out a way to get rid of this insanely large generic signature. Because the config is passed around everywhere angle brackets explode.
    /// </remarks>
    public interface IChannelConfiguration<T1, T2, TId, TSynch> : ISynchSourcesConfig<T1, T2, TId>
        where TId : IEquatable<TId>
    {
        Func<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<TSynch>> ItemsPreprocessor { get; set; }

        /// <summary>
        /// Gets the <see cref="ISynchronizationResolver{T}"/> that will be used to determine the applicable <see cref="SynchronizationAction{T,TCfg}"/>
        /// </summary>
        /// <remarks>
        /// Is primarily a part of Change Resolution. That's why the inteface is defined there.
        /// So this together with the SynchronizationResolvers is config only required in the Resolution step.
        /// </remarks>
        [NotNull] IEnumerable<ISynchronizationResolver<TSynch>> SynchronizationResolvers { get; }
    }
}
