namespace Cod.IoT.Button
{
    public delegate void ButtonEventHandler(int pin);

    public interface IButtonService : IService
    {
        event ButtonEventHandler Pressed;

        event ButtonEventHandler Held;

        void RegisterInterest(int pin, bool isHoldingEnabled);
        
        void UnregisterInterest(int pin);

        void Start();

        void Stop();
    }
}
