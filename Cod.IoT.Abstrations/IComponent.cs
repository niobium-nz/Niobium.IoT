using System;

namespace Cod.IoT
{
    public interface IComponent : IDisposable
    {
        bool IsInitialized { get; }

        IApp App { get; }

        void Initialize(IApp app);
    }
}
