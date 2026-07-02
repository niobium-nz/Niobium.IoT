using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace Cod.Platform.Devices.Provisioning.Functions
{
    public class Provisioning(ILogger<Provisioning> logger, IConfiguration configuration)
    {
        private static readonly TokenCredential credential = new DefaultAzureCredential(true);

        [Function(nameof(Provisioning))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = $"v1/{nameof(Provisioning)}")]
            [FromBody]
            DeviceProvisioningRequest req,
            CancellationToken cancellationToken)
        {
            var deviceIDSecret = configuration["DEVICE_ID_SECRET"] ?? throw new ArgumentException("DEVICE_ID_SECRET");
            var devicePINSecret = configuration["DEVICE_PIN_SECRET"] ?? throw new ArgumentException("DEVICE_PIN_SECRET");
            var hostname = configuration["IOT_HUB_HOSTNAME"] ?? throw new ArgumentException("IOT_HUB_HOSTNAME");
            if (deviceIDSecret.Length != 64)
            {
                throw new ArgumentException("DEVICE_ID_SECRET must be 64 bytes.");
            }
            if (devicePINSecret.Length != 64)
            {
                throw new ArgumentException("DEVICE_PIN_SECRET must be 64 bytes.");
            }

            if (string.IsNullOrWhiteSpace(req.PIN) || !PINHelper.ValidateDevicePIN(req.PIN, devicePINSecret))
            {
                return new StatusCodeResult((int)HttpStatusCode.Forbidden);
            }

            var deviceID = PINHelper.GenerateDeviceIDFromDeviceUID(req.UID, deviceIDSecret).ToString();
            var device = await GetOrAddDevice(hostname, deviceID, req.UID, req.PIN, cancellationToken);
            var key = device.Authentication.SymmetricKey;
            key.IsValid(true);

            return new OkObjectResult(new DeviceProvisioningResponse
            {
                AssignedHub = hostname,
                DeviceID = deviceID,
                PrimaryKey = key.PrimaryKey,
                SecondaryKey = key.SecondaryKey,
            });
        }

        private async Task<Device> GetOrAddDevice(string hostname, string deviceID, string uid, string pin, CancellationToken cancellationToken)
        {
            using RegistryManager registryManager = RegistryManager.Create(hostname, credential);

            var existingDevice = await registryManager.GetDeviceAsync(deviceID, cancellationToken);
            if (existingDevice != null)
            {
                var existingTwin = await registryManager.GetTwinAsync(deviceID, cancellationToken);
                if (existingTwin != null)
                {
                    var reported = existingTwin.Properties.Reported;
                    if (reported.Contains(nameof(DeviceProvisioningRequest.UID))
                        && reported[nameof(DeviceProvisioningRequest.UID)] == uid
                        && reported.Contains(nameof(DeviceProvisioningRequest.PIN))
                        && reported[nameof(DeviceProvisioningRequest.PIN)] == pin)
                    {
                        logger.LogInformation($"Existing device {deviceID} found and assigned to hub {hostname} based on UID={uid} and PIN={pin}");
                        return existingDevice;
                    }
                }

                logger.LogInformation($"Existing device {deviceID} removed based on UID={uid} and PIN={pin}");
                await registryManager.RemoveDeviceAsync(deviceID, cancellationToken);
            }

            var defaultTwin = new TwinCollection();
            defaultTwin[nameof(DeviceProvisioningRequest.UID)] = uid;
            defaultTwin[nameof(DeviceProvisioningRequest.PIN)] = pin;

            var operation = await registryManager.AddDeviceWithTwinAsync(new Device(deviceID), new Twin(new TwinProperties { Reported = defaultTwin }), cancellationToken);
            if (!operation.IsSuccessful)
            {
                foreach (var error in operation.Errors)
                {
                    logger.LogError($"Provisioning failed on device {deviceID} with code {error.ErrorCode} and status {error.ErrorStatus}.");
                }
                throw new ApplicationException($"Error(s) occurred while creating device {deviceID} with UID={uid} and PIN={pin}");
            }

            logger.LogInformation($"New device {deviceID} created and assigned to hub {hostname} based on UID={uid} and PIN={pin}");
            return await registryManager.GetDeviceAsync(deviceID, cancellationToken);
        }
    }
}
