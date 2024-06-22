using Microsoft.Extensions.Logging;
using nanoFramework.Logging;
using System;

namespace Cod.IoT
{
    public abstract class GenericComponent : IComponent
    {
        private bool disposed;

        protected ILogger Logger { get; private set; }

        public bool IsInitialized { get; private set; }

        public IApp App { get; private set; }

        public void Initialize(IApp app)
        {
            Logger ??= LogDispatcher.LoggerFactory.CreateLogger(this.GetType().Name);
            if (!IsInitialized)
            {
                App = app;

                try
                {
                    Initialize();
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, $"An error occurred while initializing {GetType().Name}.");
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

        protected abstract void Initialize();

        protected object GetService(ushort id)
        {
            return App.GetService(id);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                App = null;
            }
        }
    }
}
