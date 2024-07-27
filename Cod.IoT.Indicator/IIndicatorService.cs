namespace Cod.IoT.Indicator
{
    public interface IIndicatorService : IService
    {
        void TurnOn(int pin);

        void TurnOff(int pin);

        void StartBlink(int pin, int interval = Constants.DefaultBlinkInterval);

        void StopBlink(int pin);
    }
}
