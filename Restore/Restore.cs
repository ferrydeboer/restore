using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Restore.Channel;
using Restore.Configuration;

namespace Restore
{
    public class Restore : IRestoreConfiguration
    {
        private readonly List<IRestoreConfiguration> _configurations = new List<IRestoreConfiguration>();

        public IEnumerable<ISynchChannel> Channels
        {
            get
            {
                return _configurations.Select(cfg => cfg.Channels).SelectMany(cs => cs);
            }
        }

        public IEnumerable<T> GetChannels<T>() where T : ISynchChannel
        {
            var x = _configurations.Select(cfg => cfg.GetChannels<T>()).SelectMany(c => c);
            return x;
        }

        // With multiple configurations this can cause naming conflicts!
        public ISynchChannel GetChannel(string name)
        {
            return _configurations.Select(cfg => cfg.Channels).SelectMany(c => c).FirstOrDefault();
        }

        public void AddConfiguration([NotNull] IRestoreConfiguration configuration)
        {
            if (configuration == null) { throw new ArgumentNullException(nameof(configuration)); }

            _configurations.Add(configuration);
        }

        public IEnumerable<ISynchChannel> GetChannels(Type type)
        {
            var enumerable = _configurations.Select(cfg => cfg.GetChannels(type));
            return enumerable.SelectMany(c =>
            {
                var synchChannels = c as ISynchChannel[] ?? c.ToArray();
                return synchChannels;
            });
        }

        public ISynchChannel GetChannel<T>() where T : class, ISynchChannel
        {
            return GetChannel(typeof (T));
        }

        public ISynchChannel GetChannel(Type type)
        {
            var channels = GetChannels(type).ToList();
            if (channels.Count() > 1)
            {
                throw new ArgumentException($"There are multiple channels for type {type.Name}");
            }
            return channels.First();
        }
    }
}