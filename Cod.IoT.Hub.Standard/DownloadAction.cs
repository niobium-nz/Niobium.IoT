using System.IO.Hashing;
using Microsoft.Extensions.Logging;

namespace Cod.IoT.Hub
{
    public class DownloadAction : GenericAsyncAction<DownloadCommand, DeviceCommandOutput>
    {
        protected override Type CommandType => typeof(DownloadCommand);

        protected override async Task<DeviceCommandOutput> ExecuteCoreAsync(DownloadCommand command)
        {
            if (string.IsNullOrEmpty(command.URL) || string.IsNullOrEmpty(command.Output))
            {
                return DeviceCommandOutput.BadRequest;
            }

            var outputPath = command.Output.ReplaceSlashIntoBackSlash();
            var c = await DownloadAsync(command.URL, (uint)command.Signature, outputPath);
            if (c != 0)
            {
                try
                {
                    if (File.Exists(outputPath))
                    {
                        File.Delete(outputPath);
                    }
                }
                catch (Exception)
                {
                }

                return new DeviceCommandOutput { S = c };
            }

            return DeviceCommandOutput.OK;
        }

        protected virtual async Task<int> DownloadAsync(string inputURL, uint signature, string outputPath, int attempt = 1)
        {
            int statuCode = -1;
            try
            {
                Logger.LogDebug($"Downloading upgrade from {inputURL} on {attempt} attempt...");
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                using var response = await httpClient.GetAsync(inputURL, HttpCompletionOption.ResponseHeadersRead);
                statuCode = (int)response.StatusCode;
                Logger.LogDebug($"Finished downloading {inputURL} with status {statuCode}");
                response.EnsureSuccessStatusCode();

                var buff = await response.Content.ReadAsByteArrayAsync();
                Crc32 crc32 = new Crc32();
                crc32.Append(buff);
                if (crc32.GetCurrentHashAsUInt32() != signature)
                {
                    return -2;
                }

                File.WriteAllBytes(outputPath, buff);
                Logger.LogDebug($"Successfully saved upgrade to {outputPath}");
                return 0;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error {statuCode} downloading upgrade from {inputURL}");
            }

            if (attempt >= 5)
            {
                return statuCode;
            }

            return await DownloadAsync(inputURL, signature, outputPath, ++attempt);
        }

        public override DeviceCommandOutput Execute(DeviceCommand command) => throw new NotImplementedException();
    }
}
