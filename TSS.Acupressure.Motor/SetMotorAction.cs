using System;
using Cod.IoT;

namespace TSS.Acupressure.Motor
{
    public class SetMotorAction : GenericAction
    {
        private IMotorDriver driver;

        protected override Type CommandType => typeof(SetMotorCommand);

        protected override void Initialize()
        {
            base.Initialize();
            driver = (IMotorDriver)GetService(Constants.MotorDriverID);
        }

        public override DeviceCommandOutput Execute(DeviceCommand command)
        {
            if (command is SetMotorCommand setMotor)
            {
                var result = driver.SetCustomParameter(
                    Helper.ParseMotorSequence(setMotor.Sequence),
                    setMotor.Interval,
                    setMotor.Duration,
                    setMotor.Mirror);

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
