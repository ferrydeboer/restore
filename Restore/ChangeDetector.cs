using System;
using System.Threading;

namespace Restore
{
    public class ChangeDetector<T> : IDataChanges<T>
    {
        public ChangeDetector(IDataEndpoint<T> testSource, IDataEndpoint<T> target)
        {
            testSource.GetList().Subscribe(DetermineChangeType, OnError, OnComplete, new CancellationToken());
        }

        private void OnError(Exception obj)
        {
            throw new NotImplementedException();
        }

        private void OnComplete()
        {
            throw new NotImplementedException();
        }

        public void DetermineChangeType(T resource)
        {
            
        }

        public IObservable<T> ResourceChanged { get; private set; }
    }
}