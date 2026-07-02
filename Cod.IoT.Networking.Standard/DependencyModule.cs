using Microsoft.Extensions.DependencyInjection;

namespace Cod.IoT.Networking
{
    public static class DependencyModule
    {
        public static IServiceCollection AddNetwork(this IServiceCollection services)
        {
            services.AddCore();

            services.AddService<INetworkManager, FakeNetworkManager>();
           
            services.AddTransient<IComponent, AutoConnectInitiator>();
            return services;
        }
    }
}
