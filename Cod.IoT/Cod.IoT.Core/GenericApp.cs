using System;
using System.Collections;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Cod.IoT
{
    public abstract class GenericApp : IApp
    {
        private bool disposed;

        protected ILogger Logger { get; private set; }
        protected ArrayList ServiceIDs { get; private set; }
        protected Hashtable Services { get; private set; }
        protected ArrayList Components { get; private set; }

        public bool IsInitialized { get; private set; }

        protected GenericApp()
        {
            ServiceIDs = new ArrayList();
            Services = new Hashtable();
            Components = new ArrayList();
        }

        public virtual void Launch()
        {
            // Delay that allows IO initialization
            Thread.Sleep(1000);

            Logger ??= LoggerFactory.CreateLogger(this.GetType().Name);
            if (!IsInitialized)
            {
                int[] serviceIds = new int[ServiceIDs.Count];
                ServiceIDs.CopyTo(serviceIds, 0);
                Helper.QuickSort(serviceIds);
                
                try
                { 
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
                catch (Exception e)
                {
                    Logger.LogCritical(e, "Error launching app.");
                    throw;
                }
            }

            IsInitialized = true;
        }

        public IApp RegisterService(IService service)
        {
            if (!IsInitialized && !disposed)
            {
                if (ServiceIDs.Contains(service.ID))
                {
                    Services[service.ID] = service;
                }
                else
                {
                    ServiceIDs.Add(service.ID);
                    Services.Add(service.ID, service);
                }
            }

            return this;
        }

        public IApp UnregisterService(int id)
        {
            if (!IsInitialized && !disposed && ServiceIDs.Contains(id))
            {
                Services.Remove(id);
                ServiceIDs.Remove(id);
            }

            return this;
        }

        public IApp RegisterComponent(IComponent component)
        {
            if (!IsInitialized && !disposed && !Components.Contains(component))
            {
                Components.Add(component);
            }

            return this;
        }

        public IApp UnregisterComponent(IComponent component)
        {
            if (!IsInitialized && !disposed && Components.Contains(component))
            {
                Components.Remove(component);
            }

            return this;
        }

        public IService GetService(int id)
        {
            if (ServiceIDs.Contains(id))
            {
                return (IService)Services[id];
            }

            return null;
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

                foreach (int key in ServiceIDs)
                {
                    object service = Services[key];
                    if (service != null)
                    {
                        ((IService)service).Dispose();
                    }
                }

                Services.Clear();
                ServiceIDs.Clear();
            }
        }

        public virtual string GetFullName() => GetType().Assembly.FullName;
    }
}
