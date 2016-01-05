namespace Restore.RxProto
{
    /// <summary>
    /// A Synchronization action is a possible scenario that can occur given the <typeparam name="T">resource</typeparam> and the
    /// <see cref="IDataEndpoint{T}"/> that this resources is being synchronized to.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISynchronizationAction<in T>
    {
        bool AppliesTo(T resource);
        void Execute();
    }
}