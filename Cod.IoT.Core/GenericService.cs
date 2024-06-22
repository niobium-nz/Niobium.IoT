using Microsoft.Extensions.Logging;
using nanoFramework.Logging;
using System;

namespace Cod.IoT
{
    public abstract class GenericService : IService
    {
        private bool disposed;
        protected ILogger Logger { get; private set; }

        public bool IsInitialized { get; private set; }

        public abstract int ID { get; }

        public IApp App { get; private set; }

        public bool IsStarted { get; private set; }

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
                    Rollback();
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

        protected virtual void Initialize()
        { }

        protected virtual void Rollback()
        { }

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
