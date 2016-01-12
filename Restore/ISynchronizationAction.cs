namespace Restore
{
    /// <summary>
    /// A Synchronization action is a possible action that schould occur given the information available in <typeparam name="T">item</typeparam>.
    /// Depending on the setup of the channel this is very flexible. This might be pre matched entities but it can just as well be a type that
    /// is used on a specific <see cref="ICrudEndpoint{T,TId}"/>
    /// </summary>
    /// <typeparam name="T">The type this action operates on</typeparam>
    public interface ISynchronizationAction<T>
    {
        bool AppliesTo(T item);
        SynchronizationResult Execute();

        /// <summary>
        /// Facilitates any further introspection on what this action is operating on.
        /// </summary>
        T Applicant { get; }

        /// <summary>
        /// A possible name to identify what this action is doing exactly. This is primarily for Exceptions and
        /// Logging.
        /// </summary>
        string Name { get; }
    }

    public interface ISynchronizationAction
    {
        bool AppliesTo(object item);

        void Execute();
    }
}