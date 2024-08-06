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

        public static IAction MotorAction { get; private set; }
        public static IAction SetMotorAction { get; private set; }

        public static IApp UseMotor(this IApp app,
            int srclkPin, int rclkPin, int serPin, int bitLength,
            int switchButtonPin, byte switchButtonPressPinValue, int motorSwitchLEDPIN = -1,
            bool autoStartReadingGPIO = true, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                ButtonCoordinator = new ButtonCoordinator(switchButtonPin, switchButtonPressPinValue);
                MotorAction = new MotorAction();
                SetMotorAction = new SetMotorAction();

                app.UseCore(enableCommandSupport)
                   .UseButton(autoStartReadingGPIO: autoStartReadingGPIO, enableCommandSupport: enableCommandSupport)
                   .UseIndicator(enableCommandSupport)
                   .RegisterService(new MotorDriver(srclkPin, rclkPin, serPin, bitLength))
                   .RegisterAction(MotorAction)
                   .RegisterAction(SetMotorAction)
                   .RegisterComponent(ButtonCoordinator);

                if (motorSwitchLEDPIN > 0)
                {
                    LEDCoordinator = new LEDCoordinator(motorSwitchLEDPIN);
                    app.RegisterComponent(LEDCoordinator);
                }

                isLoaded = true;
            }

            return app;
        }
    }
}
