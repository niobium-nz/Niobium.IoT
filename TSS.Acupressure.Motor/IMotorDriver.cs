using System;
using System.Collections;
using Cod.IoT;

namespace TSS.Acupressure.Motor
{
    public interface IMotorDriver : IService
    {
        bool IsCustomMode { get; set; }

        event EventHandler Started;

        event EventHandler Stopped;

        event EventHandler ModeChanged;

        bool Start();

        bool Start(ArrayList motorSequence, int interval, int duration);

        bool Stop();

        bool SetCustomParameter(ArrayList sequence, int interval, int duration);
    }
}
