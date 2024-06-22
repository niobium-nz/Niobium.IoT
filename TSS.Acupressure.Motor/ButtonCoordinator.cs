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
        protected int modeButtonPin;
        protected byte modeButtonPressPinValue;

        public ButtonCoordinator(int switchButtonPin, byte switchButtonPressPinValue, int modeButtonPin, byte modeButtonPressPinValue)
        {
            this.switchButtonPin = switchButtonPin;
            this.switchButtonPressPinValue = switchButtonPressPinValue;
            this.modeButtonPin = modeButtonPin;
            this.modeButtonPressPinValue = modeButtonPressPinValue;
        }

        protected override void Initialize()
        {
            driver = (IMotorDriver)GetService(Constants.MotorDriverID);
            buttonService = (IButtonService)GetService(Cod.IoT.Button.Constants.ButtonServiceID);
            buttonService.RegisterToggle(switchButtonPin, switchButtonPressPinValue);
            buttonService.RegisterPress(modeButtonPin, false, modeButtonPressPinValue);
            buttonService.Pressed += ButtonService_Pressed;
            buttonService.Released += ButtonService_Released;
        }

        protected virtual void ButtonService_Pressed(int pin)
        {
            if (pin == switchButtonPin)
            {
                driver.Start();
            }
            else if (pin == modeButtonPin)
            {
                driver.IsCustomMode = !driver.IsCustomMode;
            }
        }

        protected virtual void ButtonService_Released(int pin)
        {
            if (pin == switchButtonPin)
            {
                driver.Stop();
            }
        }
    }
}
