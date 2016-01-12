using System;

namespace Restore
{
    public class ItemSynchronizationException : Exception
    {
        /// <summary>
        /// The item that was being processed by a step and failed.
        /// </summary>
        public object Item { get; }

        public ItemSynchronizationException(object item)
        {
            Item = item;
        }

        public ItemSynchronizationException(string message, object item) : base(message)
        {
            Item = item;
        }

        public ItemSynchronizationException(string message, Exception innerException, object item) : base(message, innerException)
        {
            Item = item;
        }
    }
}