using System;
using System.Threading;

namespace Cod.IoT.Indicator
{
    public class ResetIndicator : GenericComponent
    {
        private const int DefaultBlinkDuration = 5000;
        private Thread blinkWorker;
        private IConfigurationProvider configurationProvider;
        private IIndicatorService indicatorService;

        protected int ResetLEDPIN { get; private set; }

        public ResetIndicator(int resetLEDPIN = 0)
        {
            this.ResetLEDPIN = resetLEDPIN;
        }

        protected override void Initialize()
        {
            if (ResetLEDPIN > 0)
            {
                indicatorService = (IIndicatorService)GetService(Constants.IndicatorServiceID);
                configurationProvider = (IConfigurationProvider)GetService(Constants.ConfigurationProviderID);
                configurationProvider.Cleared += ConfigurationProvider_Cleared;
            }
        }

        protected virtual void StartBlink()
        {
            StopBlink();

            blinkWorker = new Thread(BlinkCore);
            blinkWorker.Start();
        }

        protected virtual void StopBlink()
        {
            if (blinkWorker != null)
            {
                blinkWorker.Join(DefaultBlinkDuration);
                blinkWorker = null;
            }

            indicatorService?.StopBlink(ResetLEDPIN);
        }

        protected virtual void BlinkCore()
        {
            indicatorService?.StartBlink(ResetLEDPIN, 200);
            Thread.Sleep(DefaultBlinkDuration);
            indicatorService?.StopBlink(ResetLEDPIN);
            blinkWorker = null;
        }

        protected virtual void ConfigurationProvider_Cleared(object sender, EventArgs e)
        {
            StartBlink();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopBlink();
                configurationProvider.Cleared -= ConfigurationProvider_Cleared;
                configurationProvider = null;
                indicatorService = null;
            }
            base.Dispose(disposing);
        }
    }
}
