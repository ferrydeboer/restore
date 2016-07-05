using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Restore.Matching;

namespace Restore.Channel
{
    public interface IPlumber<T1, T2, TId>
        where TId : IEquatable<TId>
    {
        IPreprocessorAppender Appender { get; set; }

        SynchPipeline CreatePipeline(IEnumerable<T1> source1, IEnumerable<T2> source2, ISynchSourcesConfig<T1, T2, TId> sourcesConfig);
    }

    public interface IPreprocessorAppender
    {
        IEnumerable<ItemMatch<T1, T2>> Append<T1, T2, TId>([NotNull] ISynchSourcesConfig<T1, T2, TId> sourceConfig, IEnumerable<ItemMatch<T1, T2>> inlet)
            where TId : IEquatable<TId>;
    }
}
