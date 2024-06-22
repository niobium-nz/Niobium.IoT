using System;
using System.Collections;
using System.Threading;
using Cod.IoT;
using Iot.Device.Multiplexing;

namespace TSS.Acupressure.Motor
{
    public class MotorDriver : GenericService, IMotorDriver
    {
        private bool stop;
        private bool isCustomMode;
        private Thread worker;
        private IConfigurationProvider configuration;

        protected ArrayList activeMotorSequence;
        protected int activeInterval;
        protected int activeDuration;

        protected readonly ShiftRegister controller;

        public bool IsCustomMode
        {
            get => isCustomMode;
            set
            {
                if (isCustomMode != value)
                {
                    isCustomMode = value;
                    OnModeChanged();
                }
            }
        }

        public event EventHandler Started;

        public event EventHandler Stopped;

        public event EventHandler ModeChanged;

        public override int ID => Constants.MotorDriverID;

        public MotorDriver(int srclkPin, int rclkPin, int serPin, int bitLength)
        {
            var mapping = new Sn74hc595PinMapping(serPin, srclkPin, rclkPin);
            controller = new Sn74hc595(mapping, bitLength: bitLength);
        }

        protected override void Initialize()
        {
            base.Initialize();
            configuration = (IConfigurationProvider)GetService(Constants.ConfigurationProviderID);
            Reset();
        }

        public bool SetCustomParameter(ArrayList sequence, int interval, int duration)
        {
            if (sequence.Count > 0 && interval >= 0 && duration > Constants.MinimumMotorDuration)
            {
                var s = Helper.JoinMotorSequence(sequence);
                if (!String.IsNullOrEmpty(s))
                {
                    configuration.Set(Constants.ConfigMotorSequence, s);
                    configuration.Set(Constants.ConfigMotorInterval, interval);
                    configuration.Set(Constants.ConfigMotorDuration, duration);
                    return true;
                }
            }

            return false;
        }

        public virtual bool Start()
        {
            if (IsCustomMode)
            {
                return Start(Helper.ParseMotorSequence(
                    configuration.GetAsString(Constants.ConfigMotorSequence)),
                    configuration.GetAsNumber(Constants.ConfigMotorInterval),
                    configuration.GetAsNumber(Constants.ConfigMotorDuration));
            }
            else
            {
                return Start(Constants.DefaultMotorSequence, Constants.DefaultMotorInterval, Constants.DefaultMotorDuration);
            }
        }

        public virtual bool Start(ArrayList motorSequence, int interval, int duration)
        {
            if (motorSequence == null || motorSequence.Count <= 0 || interval < 0 || duration <= Constants.MinimumMotorDuration)
            {
                return false;
            }

            if (worker == null)
            {
                this.activeMotorSequence = motorSequence;
                this.activeInterval = interval;
                this.activeDuration = duration;

                Reset();
                stop = false;
                worker = new Thread(Drive);
                worker.Start();
                OnStarted();
                return true;
            }

            return false;
        }

        public virtual bool Stop()
        {
            stop = true;
            if (worker != null)
            {
                worker.Join(1000);
                worker = null;
                Reset();
                OnStopped();
                return true;
            }

            return false;
        }

        protected virtual void Reset()
        {
            controller.ShiftByte(0b_0000_0000, false);
            controller.ShiftByte(0b_0000_0000, true);
        }

        protected virtual void Drive()
        {
            while (!stop)
            {
                foreach (int motor in activeMotorSequence)
                {
                    DriveMotor(motor);
                    Thread.Sleep(activeDuration);
                    if (activeInterval > 0)
                    {
                        Thread.Sleep(activeInterval);
                    }

                    if (stop)
                    {
                        Reset();
                        break;
                    }
                }
            }
        }

        protected virtual void DriveMotor(int id)
        {
            switch (id)
            {
                case 1:
                    controller.ShiftByte(0b_0000_0000, false);
                    controller.ShiftByte(0b_0000_0001, true);
                    break;
                case 2:
                    controller.ShiftByte(0b_0000_0000, false);
                    controller.ShiftByte(0b_0000_0100, true);
                    break;
                case 3:
                    controller.ShiftByte(0b_0000_0000, false);
                    controller.ShiftByte(0b_0001_0000, true);
                    break;
                case 4:
                    controller.ShiftByte(0b_0000_0000, false);
                    controller.ShiftByte(0b_0100_0000, true);
                    break;
                case 5:
                    controller.ShiftByte(0b_0000_0001, false);
                    controller.ShiftByte(0b_0000_0000, true);
                    break;
                case 6:
                    controller.ShiftByte(0b_0000_0100, false);
                    controller.ShiftByte(0b_0000_0000, true);
                    break;
                case 7:
                    controller.ShiftByte(0b_0001_0000, false);
                    controller.ShiftByte(0b_0000_0000, true);
                    break;
                case 8:
                    controller.ShiftByte(0b_0100_0000, false);
                    controller.ShiftByte(0b_0000_0000, true);
                    break;
                default:
                    break;
            }
        }

        protected virtual void OnStarted()
        {
            Started?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnStopped()
        {
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnModeChanged()
        {
            ModeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
