using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restore.ChangeResolution
{
    /// <summary>
    /// Just there if nothing needs to be synchronized between. Should in the end probably just go, just not sure how to do that
    /// with Rx and not important yet.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class NullSynchAction<T> : ISynchronizationAction<T>
    {
        public bool AppliesTo(T resource)
        {
            throw new Exception("Applies should never be called for this type of action!");
        }

        SynchronizationResult ISynchronizationAction<T>.Execute()
        {
            return new SynchronizationResult(true);
        }

        public T Applicant { get; } = default(T);
        public string Name { get; } = "Null Synch Action";
    }
}
