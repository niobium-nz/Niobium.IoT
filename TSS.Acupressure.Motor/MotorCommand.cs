using System.Collections;
using Cod.IoT;

namespace TSS.Acupressure.Motor
{
    public class MotorCommand : GenericCommand
    {
        private IMotorDriver motorDriver;

        protected override string CommandName => "Motor";

        protected override void Initialize()
        {
            base.Initialize();
            motorDriver = (IMotorDriver)GetService(Constants.MotorDriverID);
        }

        protected override Hashtable ExecuteCore(Hashtable parameters)
        {
            bool result = false;

            if (parameters.Contains("o") && parameters["o"] is string o && !string.IsNullOrEmpty(o))
            {
                if (o.ToUpper() == Constants.BooleanStringTrue)
                {
                    if (parameters.Contains("i") && parameters["i"] is string i
                        && parameters.Contains("s") && parameters["s"] is string s
                        && parameters.Contains("d") && parameters["d"] is string d
                        && !string.IsNullOrEmpty(i) && !string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(d)
                        && int.TryParse(i, out int interval) && int.TryParse(d, out int duration))
                    {
                        result = motorDriver.Start(Helper.ParseMotorSequence(s), interval, duration);
                    }
                    else
                    {
                        result = motorDriver.Start();
                    }
                }
                else
                {
                    result = motorDriver.Stop();
                }
            }

            return new Hashtable { { "r", result } };
        }
    }
}
