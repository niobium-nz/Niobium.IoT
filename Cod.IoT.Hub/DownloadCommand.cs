namespace Cod.IoT.Hub
{
    public class DownloadCommand : DeviceCommand
    {
        public uint S { get; set; }
        public uint Signature => S;

        public string I { get; set; }
        public string InputURL => I;

        public string O { get; set; }
        public string OutputPath => O;
    }
}
