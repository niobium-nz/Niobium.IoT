using Cod.IoT.Networking;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.DependencyInjection;

namespace Cod.IoT.Hub
{
    public static class DependencyModule
    {
        public static IServiceCollection AddHub(this IServiceCollection services)
        {
            services.AddNetwork();

            ExceptionExtensions.NetworkExceptions.Add(typeof(ClosedChannelException));

            services.AddService<IHubService, HubService>();
            services.AddTransient<IComponent, AutoConnectInitiator>();
            services.AddTransient<IAction, DownloadAction>();
            services.AddTransient<IDevice, X509IoTHubDevice>();
            return services;
        }
    }
}
