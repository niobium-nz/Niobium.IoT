namespace Cod.IoT.Button
{
    public abstract class Constants : Cod.IoT.Constants
    {
        public const int ButtonServiceID = 4;

        public const string ConfigDebounceTimeout = "DebounceTimeout";
        public const string ConfigHoldMinimumDownTime = "HoldMinimumDownTime";
        public const string ConfigPressMinimumDownTime = "PressMinimumDownTime";
        public const string ConfigPressMaximumDownTime = "PressMaximumDownTime";

        public const byte LowPinValue = 0x00;
        public const byte HighPinValue = 0x01;
        public const int DefaultGPIOReadValueInterval = 100;
        public const int MinimumGPIOReadValueInterval = 10;
    }
}
