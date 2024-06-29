using System.Collections;

namespace TSS.Acupressure.Motor
{
    public abstract class Constants : Cod.IoT.Constants
    {
        public const int MotorDriverID = 101;
        public const string ConfigMotorSequence = "MotorSequence";
        public const string ConfigMotorInterval = "MotorInterval";
        public const string ConfigMotorDuration = "MotorDuration";
        public const string ConfigMotorMirror = "MotorMirror";

        public static ArrayList DefaultMotorSequence = new() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        public static int DefaultMotorInterval = 0;
        public static int DefaultMotorDuration = 500;
        public static bool DefaultMotorMirror = false;
        public const int MinimumMotorDuration = 10;
    }
}
