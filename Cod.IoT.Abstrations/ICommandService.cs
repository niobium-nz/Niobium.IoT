namespace Cod.IoT
{
    public interface ICommandService : IService
    {
        void RegisterCommand(ICommand command);

        void UnregisterCommand(ICommand command);

        object Execute(object payload);
    }
}
