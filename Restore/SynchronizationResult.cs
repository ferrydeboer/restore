namespace Restore
{
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