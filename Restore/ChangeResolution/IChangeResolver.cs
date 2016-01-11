using Restore.RxProto;

namespace Restore.ChangeResolution
{
    public interface IChangeResolver<T>
    {
        ISynchronizationAction<T> Resolve(T item);
    }
}