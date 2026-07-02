namespace Cod.IoT
{
    public class DeviceCommand
    {
        public string T { get; set; }
        public string Type
        {
            get { return T; }
        }

        public long E { get; set; }
        public long Expires
        {
            get { return E; }
        }

        public long V { get; set; }
        public long Valids
        {
            get { return V; }
        }
    }
}
