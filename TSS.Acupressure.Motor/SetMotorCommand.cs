using System.Collections;
using Cod.IoT;

namespace TSS.Acupressure.Motor
{
    public class SetMotorCommand : GenericCommand
    {
        private IMotorDriver driver;

        protected override string CommandName => "SetMotor";

        protected override void Initialize()
        {
            base.Initialize();
            driver = (IMotorDriver)GetService(Constants.MotorDriverID);
        }

        protected override Hashtable ExecuteCore(Hashtable parameters)
        {
            bool result = false;

            if (parameters.Contains("i") && parameters["i"] is string i && !string.IsNullOrEmpty(i)
                && parameters.Contains("s") && parameters["s"] is string s && !string.IsNullOrEmpty(s)
                && parameters.Contains("d") && parameters["d"] is string d && !string.IsNullOrEmpty(d)
                && parameters.Contains("m") && parameters["m"] is string m && !string.IsNullOrEmpty(m)
                && int.TryParse(i, out int interval) && int.TryParse(d, out int duration))
            {
                s = s.Trim();
                var mirror = m.Trim().ToUpper() == Constants.BooleanStringTrue;
                result = driver.SetCustomParameter(Helper.ParseMotorSequence(s), interval, duration, mirror);
            }

            return new Hashtable { { "r", result } };
        }
    }
}
