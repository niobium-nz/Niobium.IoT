namespace Cod.IoT.Hub
{
    public interface IHubService : IService
    {
        bool AutoConnect { get; set; }

        bool IsConnected { get; }

        event CommandArrivedEventHandler CommandArrived;

        void Connect();

        bool ReportTwins();
    }
}
