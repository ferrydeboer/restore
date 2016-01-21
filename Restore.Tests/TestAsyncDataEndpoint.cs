using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Restore.Tests
{
    public class TestAsyncDataEndpoint<T> : IAsyncDataEndpoint<T>
    {
        public event EventHandler<DataLoadedEventArgs<T>> DataLoaded;

        public TestAsyncDataEndpoint(string name)
        {
            Name = name;
        }

        public TestAsyncDataEndpoint(string name, List<T> data)
            : this(name)
        {
            Data = data;
        }

        public string Name { get; }

        [NotNull] public List<T> Data { get; }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            Debug.WriteLine($"Firing DataLoaded from {Name}");
            try
            {
                OnDataLoaded(new DataLoadedEventArgs<T>(Data));
                Assert.Fail("This should not run!");
            }
            catch (Exception)
            {
                Debug.WriteLine("This is not catched here");
            }

            // Best
            return Task.FromResult(Data.AsEnumerable());
        }

        protected virtual void OnDataLoaded(DataLoadedEventArgs<T> e)
        {
            DataLoaded?.Invoke(this, e);
        }
    }
}