namespace Cod.Platform.Devices.Provisioning
{
    public class DeviceProvisioningResponse
    {
        public string DeviceID { get; set; }

        public string PrimaryKey { get; set; }

        public string SecondaryKey { get; set; }

        public string AssignedHub { get; set; }
    }
}
