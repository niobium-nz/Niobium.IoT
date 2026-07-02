using System;

namespace Cod.IoT
{
    public class RebootAction : GenericAction<RebootCommand, DeviceCommandOutput>
    {
        protected override Type CommandType => typeof(RebootCommand);

        protected override DeviceCommandOutput ExecuteCore(RebootCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
