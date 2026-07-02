namespace Cod.IoT.Hub
{
    public class DownloadCommand : DeviceCommand
    {
        public required string URL { get; set; }

        public long Signature { get; set; }

        public required string Output { get; set; }

    }
}
