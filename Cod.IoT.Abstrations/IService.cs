using System;

namespace Cod.IoT
{
    public interface IService : IDisposable
    {
        ushort ID { get; }

        bool IsInitialized { get; }

        bool IsStarted { get; }

        IApp App { get; }

        void Initialize(IApp app);
    }
}
