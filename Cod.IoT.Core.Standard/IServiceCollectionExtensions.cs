using Microsoft.Extensions.DependencyInjection;

namespace Cod.IoT
{
    public static class IServiceCollectionExtensions
    {
        public static void AddService<TInterface, TImplementation>(this IServiceCollection services)
            where TImplementation : class, TInterface, IService
            where TInterface : class
        {
            services.AddSingleton<TImplementation>();
            services.AddSingleton<TInterface>(sp => sp.GetService<TImplementation>());
            services.AddSingleton<IService>(sp => sp.GetService<TImplementation>());
        }

        public static void AddService<TInterface, TInterface2, TImplementation>(this IServiceCollection services)
            where TImplementation : class, TInterface, TInterface2, IService
            where TInterface : class
            where TInterface2 : class
        {
            services.AddSingleton<TImplementation>();
            services.AddSingleton<TInterface>(sp => sp.GetService<TImplementation>());
            services.AddSingleton<TInterface2>(sp => sp.GetService<TImplementation>());
            services.AddSingleton<IService>(sp => sp.GetService<TImplementation>());
        }
    }
}
