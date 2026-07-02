using System;
using Microsoft.Extensions.Logging;

namespace Cod.IoT
{
    public abstract class GenericAction : IAction
    {
        private bool disposed;

        protected ILogger Logger { get; private set; }

        protected abstract Type CommandType { get; }

        public bool IsInitialized { get; private set; }

        public ICommandService Service { get; private set; }

        public void Initialize(ICommandService service)
        {
            Logger ??= LoggerFactory.CreateLogger(this.GetType().Name);

            if (!IsInitialized)
            {
                Service = service;

                try
                {
                    Initialize();
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, $"An error has occurred while initializing {GetType().FullName}.");
                }
            }

            IsInitialized = true;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
            }

            disposed = true;
            GC.SuppressFinalize(this);
        }

        public bool CanExecute(DeviceCommand command)
        {
            return command != null && command.GetType() == CommandType;
        }

        public bool CanExecute(string json)
        {
            return json.Contains($"\"t\":\"{CommandType.Name}\"");
        }

        public DeviceCommandOutput Execute(string json)
        {
            DeviceCommand command = (DeviceCommand)JSON.Instance.Deserialize(json, CommandType);
            return Execute(command);
        }

        public abstract DeviceCommandOutput Execute(DeviceCommand command);

        protected virtual void Initialize()
        {
        }

        protected object GetService(ushort id)
        {
            return Service.App.GetService(id);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Service = null;
            }
        }
    }
}
