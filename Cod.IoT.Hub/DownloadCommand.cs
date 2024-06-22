using System;
using System.Collections;
using System.IO;
using System.IO.Hashing;
using System.Net.Http;
using System.Net.Security;
using Microsoft.Extensions.Logging;

namespace Cod.IoT.Hub
{
    public class DownloadCommand : GenericCommand
    {
        protected override string CommandName => "Download";

        protected override Hashtable ExecuteCore(Hashtable parameters)
        {
            if (!parameters.Contains("u") || parameters["u"] is not string inputURL
                || !parameters.Contains("s") || parameters["s"] is not string signature
                || !parameters.Contains("p") || parameters["p"] is not string outputPath
                || inputURL == null || signature == null || outputPath == null
                || string.IsNullOrEmpty(inputURL) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(outputPath)
                || !uint.TryParse(signature, out uint sig))
            {
                return new Hashtable { { "r", false } };
            }

            outputPath = outputPath.ReplaceSlashIntoBackSlash();
            var c = Download(inputURL, sig, outputPath);
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

                return new Hashtable { { "r", false }, { "c", c } };
            }

            return new Hashtable { { "r", true } };
        }

        protected virtual int Download(string inputURL, uint signature, string outputPath, int attempt = 1)
        {
            int statuCode = -1;
            try
            {
                Logger.LogDebug($"Downloading upgrade from {inputURL} on {attempt} attempt...");
                using var httpClient = new HttpClient { SslVerification = SslVerification.NoVerification, Timeout = TimeSpan.FromSeconds(30) };
                using var response = httpClient.Get(inputURL, HttpCompletionOption.ResponseHeadersRead);
                statuCode = (int)response.StatusCode;
                Logger.LogDebug($"Finished downloading {inputURL} with status {statuCode}");
                response.EnsureSuccessStatusCode();

                var buff = response.Content.ReadAsByteArray();
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

            return Download(inputURL, signature, outputPath, ++attempt);
        }
    }
}
