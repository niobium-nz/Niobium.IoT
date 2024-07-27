using System;
using System.Collections;
using System.Device.Gpio;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Cod.IoT.Button
{
    public class ButtonService : GenericService, IButtonService
    {
        private static readonly object syncroot = new();
        private int readValueInterval;
        private TimeSpan holdMinimumDownTime;
        private TimeSpan holdMaximumDownTime;
        private TimeSpan pressMinimumDownTime;
        private TimeSpan pressMaximumDownTime;
        private TimeSpan debounceTimeout;

        private bool stop;
        private Thread worker;
        private GpioController gpioController;

        private ArrayList pins = new();
        private Hashtable togglePressTriggerred = new();
        private Hashtable holdingEnabled = new();
        private Hashtable gpios = new();
        private Hashtable initialDownTime = new();
        private Hashtable pressPinValue = new();

        public override int ID => Constants.ButtonServiceID;

        public event ButtonEventHandler Pressed;

        public event ButtonEventHandler Held;

        public event ButtonEventHandler Released;

        public virtual void RegisterPress(int pin, bool isHoldingEnabled, byte pressPinValuePullUpMode)
        {
            Register(pin, pressPinValuePullUpMode, () => holdingEnabled.Add(pin, isHoldingEnabled));

            if (!holdingEnabled.Contains(pin))
            {
                holdingEnabled.Add(pin, isHoldingEnabled);
            }
            else
            {
                if (isHoldingEnabled && !(bool)holdingEnabled[pin])
                {
                    holdingEnabled[pin] = true;
                }
            }
        }

        public virtual void RegisterToggle(int pin, byte pressPinValuePullUpMode)
        {
            Register(pin, pressPinValuePullUpMode, () => togglePressTriggerred.Add(pin, false));
        }

        protected virtual void Register(int pin, byte pressPinValuePullUpMode, Action init)
        {
            gpioController ??= new GpioController();
            if (!pins.Contains(pin))
            {
                lock (syncroot)
                {
                    if (!pins.Contains(pin))
                    {
                        GpioPin gpioPin = gpioController.OpenPin(pin, PinMode.InputPullUp);
                        gpioPin.DebounceTimeout = debounceTimeout;
                        pins.Add(pin);
                        gpios.Add(pin, gpioPin);
                        pressPinValue.Add(pin, pressPinValuePullUpMode);
                        init();
                    }
                }
            }
        }

        public virtual void Unregister(int pin)
        {
            if (pins.Contains(pin))
            {
                lock (syncroot)
                {
                    if (pins.Contains(pin))
                    {
                        pins.Remove(pin);
                        pressPinValue.Remove(pin);
                        initialDownTime.Remove(pin);

                        if (holdingEnabled.Contains(pin))
                        {
                            holdingEnabled.Remove(pin);
                        }

                        if (togglePressTriggerred.Contains(pin))
                        {
                            togglePressTriggerred.Remove(pin);
                        }

                        gpioController?.ClosePin(pin);
                    }
                }
            }
        }

        protected virtual void DeterminePress(int pin, byte currentValue)
        {
            if (currentValue == (byte)pressPinValue[pin])
            {
                if (initialDownTime.Contains(pin))
                {
                    if (holdingEnabled.Contains(pin) && (bool)holdingEnabled[pin])
                    {
                        DateTime time = (DateTime)initialDownTime[pin];
                        TimeSpan delta = DateTime.UtcNow - time;
                        if (delta > holdMinimumDownTime && delta < holdMaximumDownTime)
                        {
                            // button has been pressed down for long enough so hold should be triggerred for a press button
                            OnHeld(pin, delta);
                        }
                    }
                }
                else
                {
                    // button has been pressed in down status
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
                        // button has been released in up status so pressed should be triggerred for a press button
                        OnPressed(pin);
                    }

                    initialDownTime.Remove(pin);
                }
            }
        }

        protected virtual void DetermineToggle(int pin, byte currentValue)
        {
            if (currentValue == (byte)pressPinValue[pin])
            {
                if (initialDownTime.Contains(pin))
                {
                    DateTime time = (DateTime)initialDownTime[pin];
                    TimeSpan delta = DateTime.UtcNow - time;
                    if (delta > pressMinimumDownTime && !(bool)togglePressTriggerred[pin])
                    {
                        // button has been pressed down for long enough so pressed should be triggerred for a toggle button
                        togglePressTriggerred[pin] = true;
                        OnPressed(pin);
                    }
                }
                else
                {
                    // button has been pressed in down status
                    initialDownTime.Add(pin, DateTime.UtcNow);
                }
            }
            else
            {
                if (initialDownTime.Contains(pin))
                {
                    if ((bool)togglePressTriggerred[pin])
                    {
                        // button has been released now in up status and because it had been pressed previously so released should be triggerred for a toggle button
                        togglePressTriggerred[pin] = false;
                        OnReleased(pin);
                    }

                    initialDownTime.Remove(pin);
                }
            }
        }

        protected virtual void ReadGPIOValue()
        {
            while (!stop)
            {
                lock (syncroot)
                {
                    foreach (int pin in pins)
                    {
                        GpioPin gpioPin = (GpioPin)gpios[pin];
                        byte v = (byte)gpioPin.Read();

                        if (togglePressTriggerred.Contains(pin))
                        {
                            DetermineToggle(pin, v);
                        }
                        else
                        {
                            DeterminePress(pin, v);
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

                togglePressTriggerred.Clear();
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
            Logger.LogDebug($"Pin {pin} is pressed");
            Pressed?.Invoke(pin);
        }

        protected virtual void OnReleased(int pin)
        {
            Logger.LogDebug($"Pin {pin} is released");
            Released?.Invoke(pin);
        }

        protected virtual void OnHeld(int pin, TimeSpan duration)
        {
            Logger.LogDebug($"Pin {pin} has been held for {duration}");
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
