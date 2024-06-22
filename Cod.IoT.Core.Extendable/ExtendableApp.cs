using System;
using System.IO;
using System.IO.Hashing;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Cod.IoT
{
    public abstract class ExtendableApp : GenericApp
    {
        public override void Launch()
        {
            if (IsInitialized)
            {
                return;
            }

            var extensions = GetExtensions(Constants.ExtensionFolder);

            var invalidExtensions = false;
            if (extensions == null || extensions.Length == 0)
            {
                invalidExtensions = true;
            }

            if (!invalidExtensions)
            {
                foreach (var extension in extensions)
                {
                    if (extension == null)
                    {
                        continue;
                    }

                    try
                    {
                        var types = extension.GetTypes();
                        foreach (var type in types)
                        {
                            if (type.Name.EndsWith(Constants.ExtensionClassSuffix))
                            {
                                var constructor = type.GetConstructor(new Type[0]);
                                if (constructor != null)
                                {
                                    var instance = constructor.Invoke(new object[0]);
                                    if (instance != null && instance is IExtension ext)
                                    {
                                        ext.Use(this);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogCritical(e, $"Error loading extension {extension.FullName}");
                        invalidExtensions = true;
                        break;
                    }
                }
            }

            if (invalidExtensions)
            {
                try
                {
                    var files = Directory.GetFiles(Constants.ExtensionFolder);
                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            base.Launch();
        }

        private static Assembly[] GetExtensions(string baseDir)
        {
            try
            {
                string manifestSignatureFile = Path.Combine(baseDir, Constants.ExtensionManifestSignatureFileName);
                if (!File.Exists(manifestSignatureFile))
                {
                    return null;
                }

                string manifestSignature = File.ReadAllText(manifestSignatureFile);
                if (String.IsNullOrEmpty(manifestSignature) || !uint.TryParse(manifestSignature, out uint expectedManifestSignature))
                {
                    return null;
                }

                string manifestFile = Path.Combine(baseDir, Constants.ExtensionManifestFileName);
                if (!File.Exists(manifestFile))
                {
                    return null;
                }

                var manifestBuff = File.ReadAllBytes(manifestFile);
                Crc32 crc32 = new Crc32();
                crc32.Append(manifestBuff);
                var currentManifestSignature = crc32.GetCurrentHashAsUInt32();
                crc32.Reset();
                if (currentManifestSignature != expectedManifestSignature)
                {
                    return null;
                }

                var manifestContent = Encoding.UTF8.GetString(manifestBuff, 0, manifestBuff.Length);
                manifestBuff = null;
                var manifestLines = manifestContent.Split('\n');
                manifestContent = null;

                Assembly[] result = new Assembly[manifestLines.Length];

                for (int i = 0; i < manifestLines.Length; i++)
                {
                    if (String.IsNullOrEmpty(manifestLines[i]))
                    {
                        continue;
                    }

                    var segments = manifestLines[i].Split('=');
                    var fileFullPath = Path.Combine(baseDir, segments[0]);
                    if (!File.Exists(fileFullPath))
                    {
                        return null;
                    }

                    if (!uint.TryParse(segments[1], out uint fileSignature))
                    {
                        return null;
                    }

                    var buff = File.ReadAllBytes(fileFullPath);
                    crc32.Append(buff);
                    if (crc32.GetCurrentHashAsUInt32() != fileSignature)
                    {
                        return null;
                    }
                    crc32.Reset();

                    var assebly = Assembly.Load(buff);
                    result[i] = assebly;
                }

                return result;

            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
