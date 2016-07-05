using System;
using Restore.Channel;

namespace Restore.Configuration
{
    public interface IChannelCreationObserver
    {
        void Created(ISynchChannel channel);
        Func<Type, Type, bool> When { get; set; }
    }
}