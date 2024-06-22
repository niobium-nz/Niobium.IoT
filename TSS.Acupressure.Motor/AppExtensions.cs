using Cod.IoT;
using Cod.IoT.Button;
using Cod.IoT.Indicator;

namespace TSS.Acupressure.Motor
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IComponent LEDCoordinator { get; private set; }
        public static IComponent ButtonCoordinator { get; private set; }

        public static ICommand MotorCommand { get; private set; }
        public static ICommand SetMotorCommand { get; private set; }

        public static IApp AddMotor(this IApp app,
            int srclkPin, int rclkPin, int serPin, int bitLength,
            int switchButtonPin, byte switchButtonPressPinValue, int modeButtonPin, byte modeButtonPressPinValue,
            int motorSwitchLEDPIN = -1, int motorModeLEDPIN = -1,
            bool autoStartReadingGPIO = true, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                ButtonCoordinator = new ButtonCoordinator(switchButtonPin, switchButtonPressPinValue, modeButtonPin, modeButtonPressPinValue);

                app.AddCore(enableCommandSupport)
                   .AddButton(autoStartReadingGPIO: autoStartReadingGPIO, enableCommandSupport: enableCommandSupport)
                   .AddIndicator(enableCommandSupport)
                   .RegisterService(new MotorDriver(srclkPin, rclkPin, serPin, bitLength))
                   .RegisterComponent(ButtonCoordinator);

                if (motorSwitchLEDPIN > 0 || motorModeLEDPIN > 0)
                {
                    LEDCoordinator = new LEDCoordinator(motorSwitchLEDPIN, motorModeLEDPIN);
                    app.RegisterComponent(LEDCoordinator);
                }

                if (enableCommandSupport)
                {
                    MotorCommand = new MotorCommand();
                    SetMotorCommand = new SetMotorCommand();

                    app.RegisterCommand(MotorCommand)
                       .RegisterCommand(SetMotorCommand);
                }

                isLoaded = true;
            }

            return app;
        }
    }
}
