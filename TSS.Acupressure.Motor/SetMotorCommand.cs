using Cod.IoT;

namespace TSS.Acupressure.Motor
{
    public class SetMotorCommand : DeviceCommand
    {
        public string S { get; set; }
        public string Sequence => S;

        public int I { get; set; }
        public int Interval => I;

        public int D { get; set; }
        public int Duration => D;

        public bool M { get; set; }
        public bool Mirror => M;
    }
}
