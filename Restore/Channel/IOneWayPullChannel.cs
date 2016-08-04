namespace Restore.Channel
{
    /// <summary>
    /// Simple marker interface to identify channels in such a way that they can be retrieved from the overall setup.
    /// This has preference over the complex generic interface of channels which is less relevant for consumers.
    /// </summary>
    /// <typeparam name="T">The end type, ussually the local type that is consumed within the app.</typeparam>
    public interface IOneWayPullChannel<T> : ISynchChannel // <T, T, ItemMatch<T, T>>s
    {
    }
}