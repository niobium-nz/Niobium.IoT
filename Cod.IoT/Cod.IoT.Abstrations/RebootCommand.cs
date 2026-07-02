namespace Cod.IoT
{
    public class RebootCommand : DeviceCommand
    {
        public int D { get; set; }
        public int Delay => D;
    }
}
