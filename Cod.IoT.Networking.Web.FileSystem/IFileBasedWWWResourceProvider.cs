namespace Cod.IoT.Networking.Web.FileSystem
{
    public interface IFileBasedWWWResourceProvider
    {
        string[] GetAllResourcePath();

        byte[] GetResourceContent(string path);

        int GetResourceVersion(string path);
    }
}
