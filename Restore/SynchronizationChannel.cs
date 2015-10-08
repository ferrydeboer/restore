using System;
using System.Diagnostics;
using System.Reactive.Linq;

namespace Restore
{
    /// <summary>
    /// Links between two data endpoints. Primary purpose is coordinate the change detection for certain objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SynchronizationChannel<T> : IDisposable
    {
        private readonly IDataEndpoint<T> _source;
        private readonly IDataEndpoint<T> _target;
        private IObservable<T> _dispatcher;
        private IDisposable _sourceSubscription;

        public SynchronizationChannel(IDataEndpoint<T> source, IDataEndpoint<T> target)
        {
            _source = source;
            _target = target;
            CreateDispatcher();
        }

        void CreateDispatcher()
        {
            // TODO: Doesn't really add much.
            _dispatcher = _source.ResourceChanged.Select(t => t);
        }

        public void Open()
        {
            var _synchActions = _dispatcher
                .Select(ToSynchAction)
                .Catch<ISynchronizationAction<T>, Exception>(ex => Observable.Return(new NullSynchAction<T>()));
            _sourceSubscription = _synchActions.Subscribe(action =>
            {
                try
                {
                    action.Execute();
                    // Could also added interceptor on this.
                    Debug.WriteLine(action);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(action.ToString() + " Failed with message " + ex.Message);
                }
                
            });
        }

        public ISynchronizationAction<T> ToSynchAction(T resource)
        {
            foreach (ISynchronizationAction<T> action in _target.SynchActions)
            {
                if (action.AppliesTo(resource))
                {
                    return action;
                }
            }
            // It's better to filter out elements that don't have an applicable action in the end. HAve to figure out how.
            return new NullSynchAction<T>();
        }
        
        public void Dispose()
        {
            if (_sourceSubscription != null)
            {
                _sourceSubscription.Dispose();   
            }
        }

        public void AddDispatchObserver(Func<T, T> interceptor)
        {
            _dispatcher = _dispatcher.Select(interceptor);
        }
    }
}