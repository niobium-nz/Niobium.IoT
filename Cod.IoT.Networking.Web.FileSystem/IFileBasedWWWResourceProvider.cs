namespace Cod.IoT.Networking.Web.FileSystem
{
    public interface IFileBasedWWWResourceProvider
    {
        string[] GetAllResourcePath();

        string GetResourceContent(string path);
    }
}
