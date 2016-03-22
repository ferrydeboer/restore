using System;

namespace Restore.Channel
{
    public class SynchronizationError : SynchronizationEvent
    {
        public Exception Cause { get; }

        /// <summary>
        /// Gets or sets a value indicating if handled by a handler. If true after all handlers are called the Cause is not wrapped and thrown in a <see cref="SynchronizationException"/>
        /// </summary>
        public bool IsHandled { get; set; }

        public SynchronizationError(Type type1, Type type2, Exception cause)
            : base(type1, type2)
        {
            Cause = cause;
        }
    }
}