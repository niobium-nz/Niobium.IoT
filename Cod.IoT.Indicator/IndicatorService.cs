using System;
using System.Collections;
using System.Device.Gpio;
using System.Threading;

namespace Cod.IoT.Indicator
{
    public class IndicatorService : GenericService, IIndicatorService
    {
        protected static readonly object syncroot = new();
        protected bool stop;
        protected Thread worker;
        protected GpioController gpioController;
        protected Hashtable pins = new();
        protected Hashtable onOff = new();
        protected ArrayList blink = new();
        protected Hashtable blinkLastTime = new();
        protected Hashtable blinkInterval = new();

        public override int ID => Constants.IndicatorServiceID;

        public virtual void StartBlink(int pin, int interval = Constants.DefaultBlinkInterval)
        {
            if (!blink.Contains(pin))
            {
                lock (syncroot)
                {
                    if (!blink.Contains(pin))
                    {
                        blink.Add(pin);
                        blinkLastTime.Add(pin, DateTime.MinValue);
                        blinkInterval.Add(pin, TimeSpan.FromMilliseconds(interval));
                        Start();
                    }
                }
            }
        }

        public virtual void StopBlink(int pin)
        {
            if (blink.Contains(pin))
            {
                lock (syncroot)
                {
                    if (blink.Contains(pin))
                    {
                        blinkLastTime.Remove(pin);
                        blinkInterval.Remove(pin);
                        blink.Remove(pin);

                        if (blink.Count == 0)
                        {
                            Stop();
                        }
                    }
                }
            }
        }

        public virtual void TurnOff(int pin)
        {
            GetPin(pin).Write(PinValue.Low);
            if (!onOff.Contains(pin))
            {
                onOff.Add(pin, false);
            }
            else
            {
                onOff[pin] = false;
            }
        }

        public virtual void TurnOn(int pin)
        {
            GetPin(pin).Write(PinValue.High);
            if (!onOff.Contains(pin))
            {
                onOff.Add(pin, true);
            }
            else
            {
                onOff[pin] = true;
            }
        }

        public bool IsOnOff(int pin)
        {
            if (onOff.Contains(pin))
            {
                return (bool)onOff[pin];
            }

            return false;
        }

        public void Switch(int pin, bool isOn)
        {
            if (isOn)
            {
                TurnOn(pin);
            }
            else
            {
                TurnOff(pin);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
                onOff.Clear();
                pins.Clear();
                blink.Clear();
                blinkLastTime.Clear();
                blinkInterval.Clear();
                gpioController?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected GpioPin GetPin(int pin)
        {
            if (pins.Contains(pin))
            {
                return (GpioPin)pins[pin];
            }

            gpioController ??= new GpioController();
            var gpio = gpioController.OpenPin(pin, PinMode.Output);
            pins.Add(pin, gpio);
            return gpio;
        }

        protected virtual void Blink()
        {
            while (!stop)
            {
                lock (syncroot)
                {
                    var now = DateTime.UtcNow;
                    foreach (int pin in blink)
                    {
                        var lastTime = (DateTime)blinkLastTime[pin];
                        var interval = (TimeSpan)blinkInterval[pin];
                        if (now - lastTime > interval)
                        {
                            try
                            {
                                GetPin(pin).Toggle();
                                blinkLastTime[pin] = now;
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                Thread.Sleep(100);
            }
        }

        private void Start()
        {
            if (worker == null)
            {
                stop = false;
                worker = new Thread(Blink);
                worker.Start();
            }
        }

        private void Stop()
        {
            if (worker != null)
            {
                stop = true;
                worker.Join(500);
                worker = null;
            }
        }
    }
}
