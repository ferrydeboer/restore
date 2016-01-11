using System;
using System.Threading.Tasks;

namespace Restore.Channel
{
    public interface ISynchChannel<T1, T2, TSynch>
    {
        Task Synchronize();
        void AddSynchItemListener<T>(Action<TSynch> action);
    }
}