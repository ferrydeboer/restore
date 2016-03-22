using System;

namespace Restore.Channel
{
    public abstract class SynchronizationEvent
    {
        public Type Type1 { get; }
        public Type Type2 { get; }

        public SynchronizationEvent(Type type1, Type type2)
        {
            Type1 = type1;
            Type2 = type2;
        }
    }
}