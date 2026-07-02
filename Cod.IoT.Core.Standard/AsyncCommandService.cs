using System.Threading.Tasks;

namespace Cod.IoT
{
    internal class AsyncCommandService : CommandService, IAsyncCommandService
    {
        public async Task<DeviceCommandOutput> ExecuteAsync(DeviceCommand command)
        {
            if (command == null)
            {
                return DeviceCommandOutput.BadRequest;
            }

            foreach (IAction action in Actions)
            {
                if (action.CanExecute(command))
                {
                    if (action is IAsyncAction asyncAction)
                    {
                        return await asyncAction.ExecuteAsync(command);
                    }
                    else
                    {
                        return action.Execute(command);
                    }
                }
            }

            return DeviceCommandOutput.NotFound;
        }

        public async Task<DeviceCommandOutput> ExecuteAsync(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return DeviceCommandOutput.BadRequest;
            }

            foreach (IAction action in Actions)
            {
                if (action.CanExecute(json))
                {
                    if (action is IAsyncAction asyncAction)
                    {
                        return await asyncAction.ExecuteAsync(json);
                    }
                    else
                    {
                        return action.Execute(json);
                    }
                }
            }

            return DeviceCommandOutput.NotFound;
        }
    }
}
