using System.IO;
using System.Net;

namespace Cod.IoT.Networking.Web.FileSystem
{
    public class FileSystemHttpHandler : GenericHttpHandler
    {
        private readonly string wwwroot;
        public IFileBasedWWWResourceProvider Provider { get; set; }

        protected override string Method => "GET";

        public FileSystemHttpHandler(string wwwroot, IFileBasedWWWResourceProvider provider)
        {
            this.wwwroot = wwwroot;
            this.Provider = provider;
        }

        protected override void Initialize()
        {
            if (Provider != null)
            {
                var resources = Provider.GetAllResourcePath();
                if (resources != null && resources.Length > 0)
                {
                    foreach (var resource in resources)
                    {
                        var content = Provider.GetResourceContent(resource);
                        if (content != null && content.Length > 0)
                        {
                            try
                            {
                                if (!File.Exists(resource))
                                {
                                    File.WriteAllText(resource, content);
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
        }

        protected override bool IsSupported(string path) => File.Exists(RegulateFilePath(path));

        protected override void Handle(HttpListenerRequest request, HttpListenerResponse response)
        {
            var path = request.GetPath();
            var file = RegulateFilePath(path);
            using var fs = File.OpenRead(file);
            response.SendResponse(fs);
        }

        protected string RegulateFilePath(string path)
        {
            if (path == "/")
            {
                path = Constants.WWWDefaultDocument;
            }

            if (path[0] == '/')
            {
                path = path.Substring(1);
            }

            return $"{wwwroot}\\{path}".ReplaceSlashIntoBackSlash();
        }
    }
}
