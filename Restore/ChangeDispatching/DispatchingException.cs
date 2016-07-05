using System;

namespace Restore.ChangeDispatching
{
    public class DispatchingException : SynchronizationException
    {
        public DispatchingException()
        {
        }

        public DispatchingException(string message)
            : base(message)
        {
        }

        public DispatchingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}