using System;
using System.Collections.Generic;

namespace Restore
{
    public class DataLoadedEventArgs<T> : EventArgs
    {
        public IEnumerable<T> LoadedData { get; private set; }

        public DataLoadedEventArgs(IEnumerable<T> loadedData)
        {
            LoadedData = loadedData;
        }
    }
}