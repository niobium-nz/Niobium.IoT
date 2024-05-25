namespace Cod.IoT
{
    public interface ICommandService : IService
    {
        void RegisterCommand(ICommand command);

        object Execute(object payload);
    }
}
