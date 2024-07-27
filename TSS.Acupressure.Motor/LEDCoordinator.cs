using System;
using Cod.IoT;
using Cod.IoT.Indicator;

namespace TSS.Acupressure.Motor
{
    public class LEDCoordinator : GenericComponent
    {
        private IMotorDriver driver;
        private IIndicatorService indicatorService;
        protected int motorSwitchLEDPIN;

        public LEDCoordinator(int motorSwitchLEDPIN)
        {
            this.motorSwitchLEDPIN = motorSwitchLEDPIN;
        }

        protected override void Initialize()
        {
            indicatorService = (IIndicatorService)GetService(Cod.IoT.Indicator.Constants.IndicatorServiceID);
            indicatorService.TurnOff(motorSwitchLEDPIN);

            driver = (IMotorDriver)GetService(Constants.MotorDriverID);
            driver.Started += Driver_Started;
            driver.Stopped += Driver_Stopped;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                driver.Started -= Driver_Started;
                driver.Stopped -= Driver_Stopped;
                indicatorService = null;
                driver = null;
            }
            base.Dispose(disposing);
        }

        protected virtual void Driver_Stopped(object sender, EventArgs e)
        {
            if (motorSwitchLEDPIN > 0)
            {
                indicatorService.TurnOff(motorSwitchLEDPIN);
            }
        }

        protected virtual void Driver_Started(object sender, EventArgs e)
        {
            if (motorSwitchLEDPIN > 0)
            {
                indicatorService.TurnOn(motorSwitchLEDPIN);
            }
        }
    }
}
