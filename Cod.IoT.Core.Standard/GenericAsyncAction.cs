using System.Threading.Tasks;

namespace Cod.IoT
{
    public abstract class GenericAction<TIn, TOut> : GenericAction
        where TIn : DeviceCommand
        where TOut : DeviceCommandOutput
    {
        public override DeviceCommandOutput Execute(DeviceCommand command)
        {
            if (command is TIn cmd)
            {
                return ExecuteCore(cmd);
            }

            return DeviceCommandOutput.BadRequest;
        }

        protected abstract TOut ExecuteCore(TIn command);
    }

    public abstract class GenericAsyncAction<TIn, TOut> : GenericAction, IAsyncAction
        where TIn : DeviceCommand
        where TOut : DeviceCommandOutput
    {
        public async Task<DeviceCommandOutput> ExecuteAsync(DeviceCommand command)
        {
            if (command is TIn cmd)
            {
                return await ExecuteCoreAsync(cmd);
            }

            return DeviceCommandOutput.BadRequest;
        }

        public async Task<DeviceCommandOutput> ExecuteAsync(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return DeviceCommandOutput.BadRequest;
            }

            var cmd = JSON.Instance.Deserialize<TIn>(json);
            if (cmd == null)
            {
                return DeviceCommandOutput.BadRequest;
            }

            return await ExecuteCoreAsync(cmd);
        }

        protected abstract Task<TOut> ExecuteCoreAsync(TIn command);
    }
}
