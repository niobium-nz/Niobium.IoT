namespace Cod.IoT.Indicator
{
    public interface IIndicatorService : IService
    {
        bool IsOnOff(int pin);

        void Switch(int pin, bool isOn);

        void StartBlink(int pin, int interval = Constants.DefaultBlinkInterval);

        void StopBlink(int pin);
    }
}
