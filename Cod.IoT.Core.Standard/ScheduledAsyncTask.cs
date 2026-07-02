using System;
using System.Threading.Tasks;

namespace Cod.IoT
{
    internal class ScheduledAsyncTask
    {
        public Func<Task> Task { get; set; }

        public DateTimeOffset Due { get; set; }

        public bool IsPastDue => DateTimeOffset.UtcNow > Due;

        public ScheduledAsyncTask(Func<Task> task)
            : this(task, DateTimeOffset.MinValue)
        {
        }

        public ScheduledAsyncTask(Func<Task> task, DateTimeOffset due)
        {
            this.Task = task;
            this.Due = due;
        }
    }
}
