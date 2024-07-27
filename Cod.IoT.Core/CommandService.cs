using System.Collections;

namespace Cod.IoT
{
    public class CommandService : GenericService, ICommandService
    {
        private ArrayList commands;

        public override int ID => Constants.CommandServiceID;

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

        public virtual object Execute(object payload)
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

        public virtual void RegisterCommand(ICommand command)
        {
            if (!IsInitialized)
            {
                commands ??= new ArrayList();
                if (!commands.Contains(commands))
                {
                    commands.Add(command);
                }
            }
        }

        public virtual void UnregisterCommand(ICommand command)
        {
            if (!IsInitialized)
            {
                if (commands.Contains(command))
                {
                    commands.Remove(command);
                }
            }
        }
    }
}
