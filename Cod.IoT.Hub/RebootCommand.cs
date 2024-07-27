using System.Collections;
using nanoFramework.Runtime.Native;

namespace Cod.IoT.Hub
{
    internal class RebootCommand : GenericCommand
    {
        protected override string CommandName => "Reboot";

        protected override Hashtable ExecuteCore(Hashtable parameters)
        {
            int delay = 10000;
            if (parameters.Contains("d") && parameters["d"] is string d && int.TryParse(d, out var di))
            {
                delay = di;
            }

            Power.RebootDevice(delay);
            return new Hashtable { { "r", true } };
        }
    }
}
