namespace Restore.RxProto
{
    /// <summary>
    /// A Synchronization action is a possible action that schould occur given the information available in <typeparam name="T">item</typeparam>.
    /// Depending on the setup of the channel this is very flexible. This might be pre matched entities but it can just as well be a type that
    /// is used on a specific <see cref="ICrudEndpoint{T,TId}"/>
    /// </summary>
    /// <typeparam name="T">The type this action operates on</typeparam>
    public interface ISynchronizationAction<in T>
    {
        bool AppliesTo(T item);
        SynchronizationResult Execute();
    }

    public interface ISynchronizationAction
    {
        bool AppliesTo(object item);

        void Execute();
    }

    public class SynchronizationResult
    {

        public SynchronizationResult(bool succes)
        {
            Succes = succes;
        }

        public SynchronizationResult()
        {
            Succes = true;
        }

        public static implicit operator bool(SynchronizationResult result)
        {
            return result.Succes;
        }
        public virtual bool Succes { get; private set; }
    }
}