namespace Cod.IoT.Hub
{
    public interface IDevice : IAsyncDisposable
    {
        DeviceConnectionStatus Status { get; }

        Task ConnectAsync();

        void Send(ITimestampable data);

        Task ReportTwinsAsync(CancellationToken cancellationToken);
    }
}
