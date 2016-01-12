using System;

namespace Restore.ChangeDispatching
{
    public class DispatchingException : Exception
    {
        public DispatchingException()
        {
        }

        public DispatchingException(string message) : base(message)
        {
        }

        public DispatchingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}