using System;
using System.Diagnostics;
using System.Reactive.Linq;

namespace Restore.RxProto
{
    /// <summary>
    /// Links between two data endpoints. Primary purpose is coordinate the change detection for certain objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SynchronizationChannel<T> : IDisposable
    {
        readonly IDataEndpoint<T> _source;
        readonly IDataEndpoint<T> _target;
        readonly bool _isBatchChannel;
        IObservable<T> _dispatcher;
        IDisposable _sourceSubscription;
        

        public SynchronizationChannel(IDataEndpoint<T> source, IDataEndpoint<T> target, bool isBatchChannel)
        {
            _source = source;
            _target = target;
            _isBatchChannel = isBatchChannel;
            CreateDispatcher();
        }

        public SynchronizationChannel(IDataEndpoint<T> source, IDataEndpoint<T> target) : this(source, target, false)
        {
        }

        void CreateDispatcher()
        {
            // TODO: Doesn't really add much.
            _dispatcher = _source.ResourceChanged.Select(t => t);
        }

        public void Open()
        {
            // The thing is the event should always come before any data coming in.
            OnOpening();
            if (!_isBatchChannel)
            {
                var synchActions = _dispatcher
                       .Select(ToSynchAction)
                       .Catch<ISynchronizationAction<T>, Exception>(ex => Observable.Return(new NullSynchAction<T>()));
                _sourceSubscription = synchActions.Subscribe(OnNext, OnClosing);
            }else
            {
                // TODO: Dispatcher is not used.
                _sourceSubscription = _source.GetListAsync().Select(t => t).Select(ToSynchAction)
                    .Catch<ISynchronizationAction<T>, Exception>(ex => Observable.Return(new NullSynchAction<T>()))
                    .Subscribe(OnNext, OnClosing);
            }
        }

        private static void OnNext(ISynchronizationAction<T> action)
        {
            try
            {
                action.Execute();
                // Could also added interceptor on this.
                Debug.WriteLine(action);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(action + " Failed with message " + ex.Message);
            }
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
            Dispose(true);
        }

        protected virtual void Dispose(bool all)
        {
            _sourceSubscription?.Dispose();
        }


        public void AddDispatchObserver(Func<T, T> interceptor)
        {
            _dispatcher = _dispatcher.Select(interceptor);
        }

        public event EventHandler<EventArgs> Opening;

        protected virtual void OnOpening()
        {
            IsOpen = true;
            var handler = Opening;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> Closing;

        protected virtual void OnClosing()
        {
            var handler = Closing;
            handler?.Invoke(this, EventArgs.Empty);
            IsOpen = false;
        }

        public bool IsOpen { get; set; }
    }
}