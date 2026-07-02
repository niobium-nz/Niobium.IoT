namespace Cod.IoT.Hub
{
    public class CloudToDeviceMessage
    {
        public required string CorrelationID { get; set; }

        public DateTimeOffset Enqueued { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset Valids { get; set; }

        public DateTimeOffset Expires { get; set; }

        public uint DeliveryCount { get; set; }

        public required string JSONBody { get; set; }
    }
}
