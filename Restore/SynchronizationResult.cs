namespace Restore
{
    public class SynchronizationResult
    {
        public SynchronizationResult(bool success)
        {
            Success = success;
        }

        public SynchronizationResult()
        {
            Success = true;
        }

        public static implicit operator bool(SynchronizationResult result)
        {
            return result.Success;
        }

        public virtual bool Success { get; }
    }
}