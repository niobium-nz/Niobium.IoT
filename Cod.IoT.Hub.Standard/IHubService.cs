namespace Cod.IoT.Hub
{
    public interface IHubService : IService
    {
        bool AutoConnect { get; set; }

        bool IsConnected { get; }

        void Connect();
    }
}
