using System;

namespace Cod.IoT.Networking
{
    public interface INetworkManager : IService
    {
        bool AutoConnect { get; set; }

        bool IsEstablished { get; }

        byte[] PhysicalAddress { get; }

        event EventHandler Established;

        void Connect();
    }
}
