using System;

namespace Cod.IoT
{
    public interface IAction : IDisposable
    {
        ICommandService Service { get; }

        void Initialize(ICommandService service);

        bool CanExecute(DeviceCommand command);

        bool CanExecute(string json);

        DeviceCommandOutput Execute(DeviceCommand command);

        DeviceCommandOutput Execute(string json);
    }
}
