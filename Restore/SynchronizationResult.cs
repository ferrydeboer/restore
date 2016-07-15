namespace Restore
{
    public class SynchronizationResult
    {
        private readonly string _message;

        public SynchronizationResult(bool success, string message = "")
        {
            _message = message;
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