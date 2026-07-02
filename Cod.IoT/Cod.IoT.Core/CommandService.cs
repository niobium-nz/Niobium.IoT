using System.Collections;

namespace Cod.IoT
{
    public class CommandService : GenericService, ICommandService
    {
        protected ArrayList Actions { get; private set; }

        public override int ID => Constants.CommandServiceID;

        protected override void Initialize()
        {
            if (Actions == null)
            {
                return;
            }

            foreach (IAction action in Actions)
            {
                action.Initialize(this);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Actions != null)
                {
                    foreach (IAction action in Actions)
                    {
                        action.Dispose();
                    }

                    Actions.Clear();
                    Actions = null;
                }
            }

            base.Dispose(disposing);
        }

        public virtual DeviceCommandOutput Execute(DeviceCommand command)
        {
            if (command == null)
            {
                return DeviceCommandOutput.BadRequest;
            }

            foreach (IAction action in Actions)
            {
                if (action.CanExecute(command))
                {
                    return action.Execute(command);
                }
            }

            return DeviceCommandOutput.NotFound;
        }

        public virtual DeviceCommandOutput Execute(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return DeviceCommandOutput.BadRequest;
            }

            foreach (IAction action in Actions)
            {
                if (action.CanExecute(json))
                {
                    return action.Execute(json);
                }
            }

            return DeviceCommandOutput.NotFound;
        }

        public virtual void RegisterAction(IAction action)
        {
            if (!IsInitialized)
            {
                Actions ??= new ArrayList();
                if (!Actions.Contains(Actions))
                {
                    Actions.Add(action);
                }
            }
        }

        public virtual void UnregisterAction(IAction action)
        {
            if (!IsInitialized)
            {
                if (Actions.Contains(action))
                {
                    Actions.Remove(action);
                }
            }
        }
    }
}
