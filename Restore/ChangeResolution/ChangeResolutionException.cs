using System;

namespace Restore.ChangeResolution
{
    public class ChangeResolutionException : ItemSynchronizationException
    {
        public ChangeResolutionException()
        {
        }

        public ChangeResolutionException(object item)
            : base(item)
        {
        }

        public ChangeResolutionException(string message, object item)
            : base(message, item)
        {
        }

        public ChangeResolutionException(string message, Exception innerException, object item)
            : base(message, innerException, item)
        {
        }
    }
}