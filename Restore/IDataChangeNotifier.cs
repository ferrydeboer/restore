using System;

namespace Restore
{
    public interface IDataChangeNotifier<T>
    {
        event Action<T> ItemCreated;
        event Action<T> ItemUpdated;
        event Action<T> ItemDeleted;
    }
}