using System;
using System.Collections.Generic;
using Restore.Channel;

namespace Restore.Configuration
{
    public interface IRestoreConfiguration
    {
        // Doubt this is going to work!
        IEnumerable<T> GetChannels<T>()
            where T : ISynchChannel;

        /// <summary>
        /// Returns all channel instanced of this configuration that are of the given channelMarkerType.
        /// </summary>
        /// <param name="channelMarkerType">The marker interface that identifies this channel.</param>
        /// <returns>The enumerable of channels.</returns>
        IEnumerable<ISynchChannel> GetChannels(Type channelMarkerType);

        ISynchChannel GetChannel<T>()
            where T : class, ISynchChannel;

        ISynchChannel GetChannel(Type type);

        IEnumerable<ISynchChannel> Channels { get; }
    }

    public class NewConfiguratorSyntaxTest
    {
        public void Bla()
        {
            var config = new ItemMatchChannelConfiguration();
        }
    }

    public class ItemMatchChannelConfiguration
    {
        // ItemMatchingFactoryComposition factories;

        /*
        public static ISourceSetup Between<T, TId>(Action<ISource<T, TId>> source1Configurator)
            where TId : IEquatable<TId>
        {
            return null;
        }
        */
    }

    public class SourceSetup<T, TId>
    {
        IRuleSetup<T1, T2, TId> And<T1, T2, TId>(Action<ISource<T2, TId>> source2Configurator)
            where TId : IEquatable<TId>
        {
            return null;
        }
    }

    internal interface IRuleSetup<T1, T2, TId>
    {
    }

    public interface ISourceSetup
    {
    }

/*
    public class ItemMatchingFactoryComposition
    {
        IEndpointFactory endpointFactory;

        IPlumberFactory plumberFactory;

        IChannelFactory channelFactory;
    }*/

    internal interface IChannelFactory
    {
        ISynchChannel<T1, T2, TId> Create<T1, T2, TId>(ISynchSourcesConfig<T1, T2, TId> sourceCfg, IPlumber<T1, T2, TId> plumber)
            where TId : IEquatable<TId>;
    }

    internal interface IEndpointFactory
    {
        ICrudEndpoint<T, TId> Create<T, TId>(ISource<T, TId> source)
            where TId : IEquatable<TId>;
    }
}