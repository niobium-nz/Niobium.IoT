using System;

namespace Cod.IoT
{
    internal class ScheduledTask
    {
        public Action Action { get; set; }

        public DateTime Due { get; set; }

        public bool IsPastDue => DateTime.UtcNow > Due;

        public ScheduledTask(Action task)
            : this(task, DateTime.MinValue)
        {
        }

        public ScheduledTask(Action action, DateTime due)
        {
            this.Action = action;
            this.Due = due;
        }
    }
}
