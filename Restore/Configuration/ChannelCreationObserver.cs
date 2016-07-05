using System;
using Restore.Channel;

namespace Restore.Configuration
{
    public abstract class ChannelCreationObserver<T> : IChannelCreationObserver
        where T : ChannelObserver
    {
        private Func<Type, Type, bool> _when = (t1, t2) => true;
        public Func<Type, Type, bool> When
        {
            get { return _when; }
            set
            {
                if (value != null)
                {
                    _when = value;
                }
            }
        }

        public void Created(ISynchChannel channel)
        {
            if (_when(channel.Type1, channel.Type2))
            {
                channel.AddChannelObserver(Create());
            }
        }

        public abstract T Create();
    }
}