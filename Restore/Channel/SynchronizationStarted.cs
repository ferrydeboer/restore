using System;

namespace Restore.Channel
{
    public class SynchronizationStarted : SynchronizationEvent
    {
        public SynchronizationStarted(Type type1, Type type2)
            : base(type1, type2)
        {
        }
    }
}