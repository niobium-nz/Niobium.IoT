using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Cod.IoT
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IEnumerable<IService> Services { get; private set; } = [];
        public static IEnumerable<IComponent> Components { get; private set; } = [];
        public static IEnumerable<IAction> Actions { get; private set; } = [];

        public static bool IsCommandSupportEnabled { get; private set; }

        public static IApp UseCore(this IApp app, IServiceProvider serviceProvider)
        {
            if (!isLoaded)
            {
                LoggerFactory.Initialize(serviceProvider.GetRequiredService<LoggerFactoryAdaptor>());
                JSON.Instance = new SystemJsonSerializer();

                Services = serviceProvider.GetRequiredService<IEnumerable<IService>>();
                foreach (var service in Services)
                {
                    app.RegisterService(service);
                }

                Components = serviceProvider.GetRequiredService<IEnumerable<IComponent>>();
                foreach (var component in Components)
                {
                    app.RegisterComponent(component);
                }

                Actions = serviceProvider.GetRequiredService<IEnumerable<IAction>>();
                foreach (var action in Actions)
                {
                    app.RegisterAction(action);
                }

                isLoaded = true;
            }

            return app;
        }
    }
}
