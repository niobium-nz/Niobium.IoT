using System;

namespace Cod.IoT.Hub
{
    public interface IHubService : IService
    {
        bool AutoConnect { get; set; }

        bool IsConnected { get; }

        event CommandArrivedEventHandler CommandArrived;

        event EventHandler ConnectionChanged;

        void Connect();

        bool ReportTwins();
    }
}
