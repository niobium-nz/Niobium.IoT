using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Device.Gpio;
using System.Threading;

namespace Cod.IoT.Button
{
    internal class ButtonService : GenericService, IButtonService
    {
        private DateTime _lastUpdateTime = DateTime.MinValue;

        private int readValueInterval;
        private TimeSpan holdMinimumDownTime;
        private TimeSpan holdMaximumDownTime;
        private TimeSpan pressMinimumDownTime;
        private TimeSpan pressMaximumDownTime;
        private TimeSpan debounceTimeout;

        private bool stop;
        private Thread worker;
        private GpioController gpioController;

        private Hashtable holdingEnabled;
        private Hashtable pins;
        private Hashtable initialDownTime;

        public override ushort ID => throw new NotImplementedException();

        public event ButtonEventHandler Pressed;

        public event ButtonEventHandler Held;

        public void RegisterInterest(int pin, bool isHoldingEnabled)
        {
            gpioController ??= new GpioController();

            pins ??= new Hashtable();
            initialDownTime ??= new Hashtable();
            if (isHoldingEnabled)
            {
                holdingEnabled ??= new Hashtable();
            }

            if (!pins.Contains(pin))
            {
                GpioPin gpioPin = gpioController.OpenPin(pin, PinMode.InputPullUp);
                gpioPin.DebounceTimeout = debounceTimeout;
                pins.Add(pin, gpioPin);
                holdingEnabled.Add(pin, isHoldingEnabled);
            }
        }

        public void UnregisterInterest(int pin)
        {
            if (pins != null && pins.Contains(pin))
            {
                pins.Remove(pin);
            }

            if (holdingEnabled != null && holdingEnabled.Contains(pin))
            {
                holdingEnabled.Remove(pin);
            }

            gpioController?.ClosePin(pin);
        }

        private void ReadGPIOValue()
        {
            while (!stop)
            {
                if (DateTime.UtcNow - _lastUpdateTime > TimeSpan.FromSeconds(30))
                {
                    Logger.LogInformation($"Current free memory left: {App.GarbageCollect(true)}.");
                    _lastUpdateTime = DateTime.UtcNow;
                }


                if (pins != null)
                {
                    foreach (object key in pins.Keys)
                    {
                        int pin = (int)key;
                        GpioPin gpioPin = (GpioPin)pins[pin];
                        byte v = (byte)gpioPin.Read();
                        if (v == Constants.ButtonDownGPIOValue)
                        {
                            if (initialDownTime.Contains(pin))
                            {
                                if (holdingEnabled.Contains(pin) && (bool)holdingEnabled[pin])
                                {
                                    DateTime time = (DateTime)initialDownTime[pin];
                                    TimeSpan delta = DateTime.UtcNow - time;
                                    if (delta > holdMinimumDownTime && delta < holdMaximumDownTime)
                                    {
                                        OnHeld(pin);
                                    }
                                }
                            }
                            else
                            {
                                initialDownTime.Add(pin, DateTime.UtcNow);
                            }
                        }
                        else
                        {
                            if (initialDownTime.Contains(pin))
                            {
                                DateTime time = (DateTime)initialDownTime[pin];
                                TimeSpan delta = DateTime.UtcNow - time;
                                if (delta > pressMinimumDownTime && delta < pressMaximumDownTime)
                                {
                                    OnPressed(pin);
                                }

                                initialDownTime.Remove(pin);
                            }
                        }
                    }
                }

                Thread.Sleep(readValueInterval);
            }
        }

        protected override void Initialize()
        {
            IConfigurationProvider config = (IConfigurationProvider)GetService(Constants.ConfigurationProviderID);
            object debounceTimeoutConfig = config.GetAsObject(Constants.ConfigDebounceTimeout);
            object holdMinimumDownTimeConfig = config.GetAsObject(Constants.ConfigHoldMinimumDownTime);
            object pressMinimumDownTimeConfig = config.GetAsObject(Constants.ConfigPressMinimumDownTime);
            object pressMaximumDownTimeConfig = config.GetAsObject(Constants.ConfigPressMaximumDownTime);

            debounceTimeout = debounceTimeoutConfig == null ? TimeSpan.FromMilliseconds(50) : TimeSpan.FromMilliseconds((int)debounceTimeoutConfig);
            holdMinimumDownTime = holdMinimumDownTimeConfig == null ? TimeSpan.FromMilliseconds(3000) : TimeSpan.FromMilliseconds((int)holdMinimumDownTimeConfig);
            pressMinimumDownTime = pressMinimumDownTimeConfig == null ? TimeSpan.FromMilliseconds(100) : TimeSpan.FromMilliseconds((int)pressMinimumDownTimeConfig);
            pressMaximumDownTime = pressMaximumDownTimeConfig == null ? TimeSpan.FromMilliseconds(2000) : TimeSpan.FromMilliseconds((int)pressMaximumDownTimeConfig);

            readValueInterval = Constants.DefaultGPIOReadValueInterval < pressMinimumDownTime.Milliseconds ? Constants.DefaultGPIOReadValueInterval : pressMinimumDownTime.Milliseconds;
            if (readValueInterval <= 0)
            {
                readValueInterval = Constants.MinimumGPIOReadValueInterval;
            }

            holdMaximumDownTime = holdMinimumDownTime + TimeSpan.FromMilliseconds(readValueInterval);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();

                gpioController.Dispose();

                holdingEnabled.Clear();
                pins.Clear();
                initialDownTime.Clear();

                holdingEnabled = null;
                pins = null;
                initialDownTime = null;
                gpioController = null;
            }

            base.Dispose(disposing);
        }

        protected virtual void OnPressed(int pin)
        {
            Pressed?.Invoke(pin);
        }

        protected virtual void OnHeld(int pin)
        {
            Held?.Invoke(pin);
        }

        public void Start()
        {
            if (worker == null)
            {
                stop = false;
                worker = new Thread(ReadGPIOValue);
                worker.Start();
            }
        }

        public void Stop()
        {
            if (worker != null)
            {
                stop = true;
                worker.Join(readValueInterval);
                worker = null;
            }
        }
    }
}
