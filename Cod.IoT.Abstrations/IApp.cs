using System;

namespace Cod.IoT
{
    public interface IApp : IDisposable
    {
        bool IsInitialized { get; }

        void RegisterService(int id, IService service);

        void RegisterComponent(IComponent component);

        IService GetService(int id);

        void Launch();

        string GetFullName();

        uint GarbageCollect(bool compactHeap);
    }
}
