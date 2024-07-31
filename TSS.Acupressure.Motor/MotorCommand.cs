using Cod.IoT;

namespace TSS.Acupressure.Motor
{
    public class MotorCommand : DeviceCommand
    {
        public string S { get; set; }
        public string Sequence => S;

        public int I { get; set; }
        public int Interval => I;

        public int D { get; set; }
        public int Duration => D;

        public bool M { get; set; }
        public bool Mirror => M;

        public bool O { get; set; }
        public bool On => O;
    }
}
