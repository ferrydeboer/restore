using System;
using JetBrains.Annotations;
using Restore.ChangeResolution;

namespace Restore
{
    /// <summary>
    /// These are the configuration parts that are primarily used as a part of
    /// Change Resolution/Dispatching.
    /// </summary>
    public interface ISynchSourcesConfig<T1, T2, TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// Gets translator used to move data from <typeparamref name="T1"/> onto <typeparamref name="T2"/>.
        /// </summary>
        [NotNull]
        ITranslator<T1, T2> TypeTranslator { get; }

        [NotNull]
        IEndpointConfiguration<T1, TId> Type1EndpointConfiguration { get; }

        [NotNull]
        IEndpointConfiguration<T2, TId> Type2EndpointConfiguration { get; }
    }
}