namespace Cod.IoT
{
    public interface ICommandService : IService
    {
        void RegisterAction(IAction action);

        void UnregisterAction(IAction action);

        DeviceCommandOutput Execute(DeviceCommand command);

        DeviceCommandOutput Execute(string json);
    }
}
