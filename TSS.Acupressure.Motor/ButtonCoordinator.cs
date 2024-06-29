using Cod.IoT;
using Cod.IoT.Button;

namespace TSS.Acupressure.Motor
{
    public class ButtonCoordinator : GenericComponent
    {
        private IButtonService buttonService;
        private IMotorDriver driver;

        protected int switchButtonPin;
        protected byte switchButtonPressPinValue;

        public ButtonCoordinator(int switchButtonPin, byte switchButtonPressPinValue)
        {
            this.switchButtonPin = switchButtonPin;
            this.switchButtonPressPinValue = switchButtonPressPinValue;
        }

        protected override void Initialize()
        {
            driver = (IMotorDriver)GetService(Constants.MotorDriverID);
            buttonService = (IButtonService)GetService(Cod.IoT.Button.Constants.ButtonServiceID);
            buttonService.RegisterPress(switchButtonPin, false, switchButtonPressPinValue);
            buttonService.Pressed += ButtonService_Pressed;
        }

        protected virtual void ButtonService_Pressed(int pin)
        {
            if (pin == switchButtonPin)
            {
                if (driver.IsStarted)
                {
                    driver.Stop();
                }
                else
                {
                    driver.Start();
                }
            }
        }
    }
}
