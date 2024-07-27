using System;

namespace Cod.IoT
{
    public interface IService : IDisposable
    {
        int ID { get; }

        bool IsInitialized { get; }

        IApp App { get; }

        void Initialize(IApp app);
    }
}
