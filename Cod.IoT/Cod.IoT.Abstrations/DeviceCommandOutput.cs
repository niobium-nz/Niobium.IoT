using System;

namespace Cod.IoT
{
    public class DeviceCommandOutput
    {
        public static readonly DeviceCommandOutput OK = new DeviceCommandOutput { S = 200 };
        public static readonly DeviceCommandOutput BadRequest = new DeviceCommandOutput { S = 400 };
        public static readonly DeviceCommandOutput NotFound = new DeviceCommandOutput { S = 404 };
        public static readonly DeviceCommandOutput InternalError = new DeviceCommandOutput { S = 500 };

        public int S { get; set; }
        public int Status
        {
            get { return S; }
        }

        public long E { get; set; }
        public long Executed
        {
            get { return E; }
        }

        public string M { get; set; }
        public string Message
        {
            get { return M; }
        }
    }
}
