using Microsoft.Extensions.DependencyInjection;

namespace Cod.IoT
{
    public static class DependencyModule
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddTransient<LoggerFactoryAdaptor>();

            services.AddService<IConfigurationProvider, ConfigurationProvider>();
            services.AddService<ICommandService, IAsyncCommandService, AsyncCommandService>();
            services.AddService<ITaskService, IAsyncTaskService, AsyncTaskService>();

            services.AddTransient<IAction, RebootAction>();
            services.AddTransient<IAction, PingAction>();
            return services;
        }
    }
}
