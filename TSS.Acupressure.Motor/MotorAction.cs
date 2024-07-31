using System;
using Cod.IoT;

namespace TSS.Acupressure.Motor
{
    public class MotorAction : GenericAction
    {
        private IMotorDriver driver;

        protected override Type CommandType => typeof(MotorCommand);

        protected override void Initialize()
        {
            base.Initialize();
            driver = (IMotorDriver)GetService(Constants.MotorDriverID);
        }

        public override DeviceCommandOutput Execute(DeviceCommand command)
        {
            if (command is MotorCommand motor)
            {
                bool result;
                if (motor.On)
                {
                    if (string.IsNullOrEmpty(motor.Sequence))
                    {
                        result = driver.Start();
                    }
                    else
                    {
                        result = driver.Start(
                            Helper.ParseMotorSequence(motor.Sequence),
                            motor.Interval,
                            motor.Duration,
                            motor.Mirror);
                    }
                }
                else
                {
                    result = driver.Stop();
                }

                if (result)
                {
                    return DeviceCommandOutput.OK;
                }
                else
                {
                    return DeviceCommandOutput.InternalError;
                }
            }

            return DeviceCommandOutput.BadRequest;
        }
    }
}
