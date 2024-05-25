using System;

namespace Cod.IoT
{
    public interface ICommand : IDisposable
    {
        ICommandService Service { get; }

        void Initialize(ICommandService service);

        bool CanExecute(object parameters);

        object Execute(object parameters);
    }
}
