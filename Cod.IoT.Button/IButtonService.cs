namespace Cod.IoT.Button
{
    public delegate void ButtonEventHandler(int pin);

    public interface IButtonService : IService
    {
        event ButtonEventHandler Pressed;

        event ButtonEventHandler Released;

        event ButtonEventHandler Held;

        void RegisterPress(int pin, bool isHoldingEnabled, byte pressPinValuePullUpMode);

        void RegisterToggle(int pin, byte pressPinValuePullUpMode);
        
        void Unregister(int pin);

        void Start();

        void Stop();
    }
}
