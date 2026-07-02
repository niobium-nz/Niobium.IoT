using System;
using System.Threading.Tasks;

namespace Cod.IoT
{
    public interface IAsyncTaskService : ITaskService
    {
        void Schedule(Func<Task> task);

        void Schedule(Func<Task> task, DateTimeOffset due);
    }
}
