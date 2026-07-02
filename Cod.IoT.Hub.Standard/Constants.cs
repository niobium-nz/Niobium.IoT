namespace Cod.IoT.Hub
{
    public abstract class Constants : Cod.IoT.Networking.Constants
    {
        public const int HubServiceID = 6;

        public const string ConfigPrimaryCertificatePath = "PrimaryCert";
        public const string ConfigPrimaryCertificatePassword = "PrimaryCertPassword";
        public const string ConfigSecondaryCertificatePath = "SecondaryCert";
        public const string ConfigSecondaryCertificatePassword = "SecondaryCertPassword";
        public const string ConfigDatabaseFilePath = "HubDB";
        public const string ConfigHubHost = "HubHost";
        public const string ConfigLastTwinVersion = "TwinVersion";

        public const int HubConnectionRetryInterval = 10000;
        public const int DeviceTwinsReportMaxRetry = 10;
        public const int DeviceTwinsReportRetryInterval = 10000;
    }
}
