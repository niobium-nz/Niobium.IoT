using Microsoft.Extensions.Logging;
using System;
using System.Collections;

namespace Cod.IoT
{
    public abstract class GenericApp : IApp
    {
        private bool disposed;

        protected ILogger Logger { get; private set; }
        protected Hashtable Services { get; private set; }
        protected ArrayList Components { get; private set; }

        public bool IsInitialized { get; private set; }

        protected GenericApp()
            : this(Cod.IoT.Logger.Instance)
        {
            Services = new Hashtable();
            Components = new ArrayList();
        }

        protected GenericApp(ILogger logger)
        {
            Services = new Hashtable();
            Components = new ArrayList();
            Logger = logger;
        }

        public virtual void Launch()
        {
            if (!IsInitialized)
            {
                int[] serviceIds = new int[Services.Count];
                Services.Keys.CopyTo(serviceIds, 0);
                Helper.QuickSort(serviceIds);

                foreach (int serviceId in serviceIds)
                {
                    IService service = (IService)Services[serviceId];
                    service.Initialize(this);
                }

                foreach (IComponent component in Components)
                {
                    component.Initialize(this);
                }
            }

            IsInitialized = true;
            Logger.LogInformation($"{GetFullName()} initialized with free memory left: {GarbageCollect(true)}");
        }

        public void RegisterService(int id, IService service)
        {
            if (!IsInitialized && !disposed && !Services.Contains(id))
            {
                Services.Add(id, service);
            }
        }

        public void RegisterComponent(IComponent component)
        {
            if (!IsInitialized && !disposed && !Components.Contains(component))
            {
                Components.Add(component);
            }
        }

        public IService GetService(int id)
        {
            return (IService)Services[id];
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                foreach (IComponent component in Components)
                {
                    component?.Dispose();
                }

                Components.Clear();

                foreach (object key in Services.Keys)
                {
                    object service = Services[key];
                    if (service != null)
                    {
                        ((IService)service).Dispose();
                    }
                }

                Services.Clear();
            }
        }

        public abstract string GetFullName();

        public uint GarbageCollect(bool compactHeap)
        {
            return nanoFramework.Runtime.Native.GC.Run(compactHeap);
        }
    }
}
