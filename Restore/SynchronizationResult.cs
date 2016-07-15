namespace Restore
{
    public class SynchronizationResult
    {
        public SynchronizationResult(bool success, string message = "")
        {
            Message = message;
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
        public string Message { get; }
    }
}