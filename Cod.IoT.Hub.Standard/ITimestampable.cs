namespace Cod.IoT.Hub
{
    public interface ITimestampable
    {
        DateTimeOffset Timestamp { get; set; }
    }
}
