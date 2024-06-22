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

            if (parameters.Contains("i") && parameters["i"] is string i
                && parameters.Contains("s") && parameters["s"] is string s
                && parameters.Contains("d") && parameters["d"] is string d
                && !string.IsNullOrEmpty(i) && !string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(d)
                && int.TryParse(i, out int interval) && int.TryParse(d, out int duration))
            {
                s = s.Trim();
                result = driver.SetCustomParameter(Helper.ParseMotorSequence(s), interval, duration);
            }

            return new Hashtable { { "r", result } };
        }
    }
}
