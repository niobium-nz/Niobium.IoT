using Microsoft.Extensions.Logging;
using nanoFramework.Json;
using System;
using System.Collections;

namespace Cod.IoT
{
    public abstract class GenericCommand : ICommand
    {
        private bool disposed;

        protected ILogger Logger { get; private set; }

        protected abstract string CommandName { get; }

        public bool IsInitialized { get; private set; }

        public ICommandService Service { get; private set; }

        protected GenericCommand()
            : this(Cod.IoT.Logger.Instance)
        {
        }

        protected GenericCommand(ILogger logger)
        {
            Logger = logger;
        }

        public void Initialize(ICommandService service)
        {
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

        public bool CanExecute(object parameters)
        {
            return parameters != null && parameters is string payload && payload.Contains(CommandName);
        }

        public object Execute(object parameters)
        {
            object param = JsonConvert.DeserializeObject((string)parameters, typeof(Hashtable));
            if (param == null)
            {
                return null;
            }

            Hashtable result = ExecuteCore((Hashtable)param);
            return JsonConvert.SerializeObject(result);
        }

        protected virtual void Initialize()
        {
        }

        protected abstract Hashtable ExecuteCore(Hashtable parameters);

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
