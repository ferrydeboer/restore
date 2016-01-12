using Restore.RxProto;

namespace Restore.ChangeResolution
{
    public interface ISynchronizationResolver<T>
    {
        ISynchronizationAction<T> Resolve(T item);
    }
}