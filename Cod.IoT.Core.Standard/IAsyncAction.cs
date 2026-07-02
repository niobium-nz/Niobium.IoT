using System.Threading.Tasks;

namespace Cod.IoT
{
    public interface IAsyncAction : IAction 
    {
        Task<DeviceCommandOutput> ExecuteAsync(string json);

        Task<DeviceCommandOutput> ExecuteAsync(DeviceCommand parameters);
    }
}
