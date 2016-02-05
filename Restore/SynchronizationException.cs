﻿using System;

namespace Restore
{
    public class SynchronizationException : Exception
    {
        public SynchronizationException()
        {
        }

        public SynchronizationException(string message)
            : base(message)
        {
        }

        public SynchronizationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
