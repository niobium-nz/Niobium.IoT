using System;

namespace Cod.IoT
{
    public interface IApp : IDisposable
    {
        bool IsInitialized { get; }

        IApp RegisterService(IService service);

        IApp RegisterComponent(IComponent component);

        IApp UnregisterService(int id);

        IApp UnregisterComponent(IComponent component);

        IService GetService(int id);

        void Launch();

        string GetFullName();
    }
}
