namespace Cod.IoT.Button
{
    public class ResetTrigger : GenericComponent
    {
        protected IButtonService ButtonService { get; private set; }
        protected IConfigurationProvider ConfigurationProvider { get; private set; }

        protected int ResetButtonPin { get; private set; }
        protected byte ResetButtonPressPinValue { get; private set; }

        public ResetTrigger(int resetButtonPin, byte resetButtonPressPinValue)
        {
            this.ResetButtonPin = resetButtonPin;
            this.ResetButtonPressPinValue = resetButtonPressPinValue;
        }

        protected override void Initialize()
        {
            ConfigurationProvider = (IConfigurationProvider)GetService(Constants.ConfigurationProviderID);
            ButtonService = (IButtonService)GetService(Constants.ButtonServiceID);
            ButtonService.RegisterPress(ResetButtonPin, true, ResetButtonPressPinValue);
            ButtonService.Held += ButtonService_Held;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ButtonService != null)
                {
                    ButtonService.Held -= ButtonService_Held;
                }
                ButtonService = null;
                ConfigurationProvider = null;
            }

            base.Dispose(disposing);
        }

        private void ButtonService_Held(int pin)
        {
            if (pin == ResetButtonPin && ConfigurationProvider != null)
            {
                ConfigurationProvider.Clear();
            }
        }
    }
}
