namespace Restore.Channel
{
    /// <summary>
    /// Not in use yet. Was contemplating between attaching handlers using another component (lets call them appenders)
    /// under certain conditions or wether to inject a list of Observers. Now I think it's going to be a mix of both.
    /// So I still keep appenders, but they just add observers. The appenders should run in a specific order because
    /// the observers need to be attached in a specific order in certain cases.
    ///
    /// There's also the Individual item oberservers. For this I need something else probably. But cunstruction of the pipeline
    /// is going to change most probably anyway.
    /// </summary>
    /**
    Possible Channel improvements in order to register an ordered list of handlers on starting and finishing:
    * Inject a list somewhere else in the setup and let that register the the observers.
      + The advantage of this it further keeps the channel oblivious of this so it can be untouched.
      + Because it's done after construction is provide the possibility to do some more specific type inspection if the channel (it is more context aware) then just injecting a
        generic list of observer implementations.
      - If channels are transient this implies the handlers also have to be otherwise it block garbage disposal. So it requires unsubscribes/cleanups/lifecycle management.
      - For exception handling and.

    * Inject a list of different observer interface implementations into the channel (Which is right now using events, where everybody says you should not rely on the ordering there)
    */
    public abstract class ChannelObserver
    {
        public virtual void OnStarted(SynchronizationStarted started)
        {
        }

        public virtual void OnFinished(SynchronizationFinished finished)
        {
        }

        public virtual void OnError(SynchronizationError error)
        {
        }
    }
}
