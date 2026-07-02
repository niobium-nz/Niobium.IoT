using System.Collections.Generic;

namespace Cod.IoT
{
    public static class IAppExtensions
    {
        public static IApp RegisterComponents(this IApp app, IEnumerable<IComponent> components)
        {
            foreach (var component in components)
            {
                app.RegisterComponent(component);
            }

            return app;
        }

        public static IApp RegisterAction(this IApp app, IAction action)
        {
            ICommandService commandService = (ICommandService)app.GetService(Constants.CommandServiceID);
            commandService.RegisterAction(action);

            return app;
        }

        public static IApp RegisterActions(this IApp app, IEnumerable<IAction> actions)
        {
            foreach (var action in actions)
            { 
                app.RegisterAction(action);
            }

            return app;
        }
    }
}
