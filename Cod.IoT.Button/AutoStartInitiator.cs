namespace Cod.IoT.Button
{
    internal class AutoStartInitiator : GenericComponent, IComponent
    {
        protected override void Initialize()
        {
            IButtonService service = (IButtonService)GetService(Constants.ButtonServiceID);
            service.Start();
        }
    }
}
