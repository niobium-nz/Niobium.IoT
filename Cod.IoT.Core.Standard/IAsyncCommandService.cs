using System.Threading.Tasks;

namespace Cod.IoT
{
    public interface IAsyncCommandService : ICommandService
    {
        Task<DeviceCommandOutput> ExecuteAsync(DeviceCommand command);

        Task<DeviceCommandOutput> ExecuteAsync(string json);
    }
}
