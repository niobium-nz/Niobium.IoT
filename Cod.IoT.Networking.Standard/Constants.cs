namespace Cod.IoT.Networking
{
    public abstract class Constants : Cod.IoT.Constants
    {
        public const int NetworkManagerID = 5;
        public const int NetworkConnectionMaxRetry = 3;
        public const int NetworkWaitInterval = 30000;
        public const string NetworkProvinsioningRequestedFile = "/etc/aipro/netprorq.flg";
    }
}
