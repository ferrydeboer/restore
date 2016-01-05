using System;

namespace Restore.RxProto
{
    /// <summary>
    /// Just there if nothing needs to be synchronized between. Should in the end probably just go, just not sure how to do that
    /// with Rx and not important yet.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NullSynchAction<T> : ISynchronizationAction<T>
    {
        public bool AppliesTo(T resource)
        {
            throw new Exception("Applies should never be called for this type of action!");
        }

        public void Execute()
        {
            // Do nothing
        }
    }
}