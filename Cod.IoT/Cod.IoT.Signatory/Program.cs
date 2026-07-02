var files = new[] { @"C:\Users\Wen\source\repos\Cod.IoT\TSS.Acupressure.Extensions.Test\bin\Debug\TSS.Acupressure.Extensions.Test.pe" };
var outputFolder = @"C:\Users\Wen\source\repos\Cod.IoT\TSS.Acupressure.Extensions.Test\bin\Debug";
var downloadBaseUrl = "https://websitexiexie.z8.web.core.windows.net/ota/";
var extensionFolder = "I:/extensions/";
var extensionManifestFileName = "manifest.ini";
var extensionManifestSignatureFileName = "manifest.sig";

var crc32 = new System.IO.Hashing.Crc32();
var hashes = new Dictionary<string, uint>();

foreach (var file in files)
{
    var name = new FileInfo(file).Name;
    using var fs = File.OpenRead(file);
    await crc32.AppendAsync(fs);
    var hash = crc32.GetCurrentHashAsUInt32();
    hashes.Add(name, hash);
    crc32.Reset();
}

if (hashes.Count > 0)
{
    var manifestFile = $"{outputFolder}\\{extensionManifestFileName}";

    if (File.Exists(manifestFile))
    {
        File.Delete(manifestFile);
    }

    var names = hashes.Keys.ToArray();
    for (int i = 0; i < names.Length; i++)
    {
        var file = names[i];
        var line = $"{file}={hashes[file]}";
        if (i != names.Length - 1)
        {
            line += "\n";
        }
        await File.AppendAllTextAsync(manifestFile, line);
    }

    var signatureFile = $"{outputFolder}\\{extensionManifestSignatureFileName}";
    if (File.Exists(signatureFile))
    {
        File.Delete(signatureFile);
    }
    using var fs = File.OpenRead(manifestFile);
    await crc32.AppendAsync(fs);
    var hash = crc32.GetCurrentHashAsUInt32();
    crc32.Reset();
    await File.WriteAllTextAsync(signatureFile, hash.ToString());
    hashes.Add(extensionManifestFileName, hash);

    using var fs2 = File.OpenRead(signatureFile); 
    await crc32.AppendAsync(fs2);
    var hash2 = crc32.GetCurrentHashAsUInt32();
    crc32.Reset();
    hashes.Add(extensionManifestSignatureFileName, hash2);

    foreach (var name in names)
    {
        await File.WriteAllTextAsync($"{outputFolder}\\DownloadCommand-{name}.json", $"{{\"t\":\"Download\",\"u\":\"{downloadBaseUrl}{name}\",\"p\":\"{extensionFolder}{name}\",\"s\":\"{hashes[name]}\"}}");
    }

    await File.WriteAllTextAsync($"{outputFolder}\\DownloadCommand-{extensionManifestFileName}.json", $"{{\"t\":\"Download\",\"u\":\"{downloadBaseUrl}{extensionManifestFileName}\",\"p\":\"{extensionFolder}{extensionManifestFileName}\",\"s\":\"{hashes[extensionManifestFileName]}\"}}");
    await File.WriteAllTextAsync($"{outputFolder}\\DownloadCommand-{extensionManifestSignatureFileName}.json", $"{{\"t\":\"Download\",\"u\":\"{downloadBaseUrl}{extensionManifestSignatureFileName}\",\"p\":\"{extensionFolder}{extensionManifestSignatureFileName}\",\"s\":\"{hashes[extensionManifestSignatureFileName]}\"}}");
}