using System;

namespace Cod.IoT
{
    internal class PingAction : GenericAction
    {
        protected override Type CommandType => typeof(PingCommand);

        public override DeviceCommandOutput Execute(DeviceCommand command)
        {
            return DeviceCommandOutput.OK;
        }
    }
}
