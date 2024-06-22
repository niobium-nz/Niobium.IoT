using System;

namespace Cod.IoT
{
    public interface ITaskService : IService
    {
        void Schedule(Action task);
    }
}
