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
        protected int motorModeLEDPIN;

        public LEDCoordinator(int motorSwitchLEDPIN, int motorModeLEDPIN)
        {
            this.motorSwitchLEDPIN = motorSwitchLEDPIN;
            this.motorModeLEDPIN = motorModeLEDPIN;
        }

        protected override void Initialize()
        {
            indicatorService = (IIndicatorService)GetService(Cod.IoT.Indicator.Constants.IndicatorServiceID);
            indicatorService.TurnOff(motorSwitchLEDPIN);
            indicatorService.TurnOn(motorModeLEDPIN);

            driver = (IMotorDriver)GetService(Constants.MotorDriverID);
            driver.Started += Driver_Started;
            driver.Stopped += Driver_Stopped;
            driver.ModeChanged += Driver_ModeChanged;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                driver.ModeChanged -= Driver_ModeChanged;
                driver.Started -= Driver_Started;
                driver.Stopped -= Driver_Stopped;
                indicatorService = null;
                driver = null;
            }
            base.Dispose(disposing);
        }

        protected virtual void Driver_ModeChanged(object sender, EventArgs e)
        {
            if (motorModeLEDPIN > 0)
            {
                if (driver.IsCustomMode)
                {
                    indicatorService.TurnOff(motorModeLEDPIN);
                }
                else
                {
                    indicatorService.TurnOn(motorModeLEDPIN);
                }
            }
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
