using System;
using nanoFramework.Runtime.Native;

namespace Cod.IoT
{
    public class RebootAction : GenericAction
    {
        protected override Type CommandType => typeof(RebootCommand);

        public override DeviceCommandOutput Execute(DeviceCommand command)
        {
            if (command is RebootCommand reboot)
            {
                Power.RebootDevice(reboot.Delay);
                return DeviceCommandOutput.OK;
            }

            return DeviceCommandOutput.BadRequest;
        }
    }
}
