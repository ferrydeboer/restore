using System;

namespace Restore.ChangeResolution
{
    public class ChangeResolutionException : Exception
    {
        public object Item { get; private set; }

        public ChangeResolutionException(object item)
        {
            Item = item;
        }

        public ChangeResolutionException(string message, object item) : base(message)
        {
            Item = item;
        }

        public ChangeResolutionException(string message, Exception innerException, object item) : base(message, innerException)
        {
            Item = item;
        }
    }
}