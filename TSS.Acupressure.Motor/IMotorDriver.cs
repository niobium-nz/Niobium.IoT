using System;
using System.Collections;
using Cod.IoT;

namespace TSS.Acupressure.Motor
{
    public interface IMotorDriver : IService
    {
        bool IsStarted { get; }

        event EventHandler Started;

        event EventHandler Stopped;

        bool Start();

        bool Start(ArrayList motorSequence, int interval, int duration, bool mirror);

        bool Stop();

        bool SetCustomParameter(ArrayList sequence, int interval, int duration, bool mirror);
    }
}
