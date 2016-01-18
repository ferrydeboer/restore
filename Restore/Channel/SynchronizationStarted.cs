using System;

namespace Restore.Channel
{
    public class SynchronizationStarted
    {
        public Type Type1 { get; }
        public Type Type2 { get; }

        public SynchronizationStarted(Type type1, Type type2)
        {
            Type1 = type1;
            Type2 = type2;
        }
    }
}