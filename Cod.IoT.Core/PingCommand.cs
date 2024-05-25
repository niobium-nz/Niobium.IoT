using System.Collections;

namespace Cod.IoT
{
    internal class PingCommand : GenericCommand
    {
        protected override string CommandName => "Ping";

        protected override Hashtable ExecuteCore(Hashtable parameters)
        {
            return new Hashtable
            {
                { "c", 200 },
                { "r", "Pong" }
            };
        }
    }
}
