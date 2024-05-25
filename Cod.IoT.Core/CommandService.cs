using System.Collections;

namespace Cod.IoT
{
    internal class CommandService : GenericService, ICommandService
    {
        private ArrayList commands;

        public override ushort ID => Constants.CommandServiceID;

        protected override void Initialize()
        {
            if (commands == null)
            {
                return;
            }

            foreach (ICommand command in commands)
            {
                command.Initialize(this);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (commands != null)
                {
                    foreach (ICommand command in commands)
                    {
                        command.Dispose();
                    }

                    commands.Clear();
                    commands = null;
                }
            }

            base.Dispose(disposing);
        }

        public object Execute(object payload)
        {
            if (payload == null)
            {
                return "400";
            }

            foreach (ICommand command in commands)
            {
                if (command.CanExecute(payload))
                {
                    return command.Execute(payload);
                }
            }

            return "404";
        }

        public void RegisterCommand(ICommand command)
        {
            commands ??= new ArrayList();
            commands.Add(command);
        }
    }
}
