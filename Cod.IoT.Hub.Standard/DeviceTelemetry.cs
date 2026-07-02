namespace Cod.IoT.Hub
{
    public class DeviceTelemetry : ITimestampable
    {
        public DateTimeOffset Timestamp { get; set; }
    }
}
