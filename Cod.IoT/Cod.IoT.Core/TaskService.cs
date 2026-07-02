using System;
using System.Collections;
using System.Threading;

namespace Cod.IoT
{
    public class TaskService : GenericService, ITaskService
    {
        protected bool StopRequested { get; private set; }
        private Thread worker;
        
        protected Queue Tasks { get; private set; }

        public override int ID => Constants.TaskServiceID;

        public TaskService()
        {
            Tasks = new Queue();
        }

        public virtual void Schedule(Action task)
        {
            Tasks.Enqueue(new ScheduledTask(task));
        }

        public void Schedule(Action task, DateTime due)
        {
            Tasks.Enqueue(new ScheduledTask(task, due));
        }

        protected override void Initialize()
        {
            base.Initialize();
            Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
                
                if (Tasks != null)
                {
                    Tasks.Clear();
                    Tasks = null;
                }
            }

            base.Dispose(disposing);
        }

        private void Start()
        {
            if (worker == null)
            {
                StopRequested = false;
                worker = new Thread(ActionTask);
                worker.Start();
            }
        }

        private void Stop()
        {
            if (worker != null)
            {
                StopRequested = true;
                worker.Join(Constants.TaskActionInterval);
                worker = null;
            }
        }

        protected virtual void ActionTask()
        {
            while (!StopRequested)
            {
                if (Tasks.Count > 0)
                {
                    var task = (ScheduledTask)Tasks.Dequeue();
                    if (task.IsPastDue)
                    {
                        task.Action();
                    }
                    else
                    {
                        Tasks.Enqueue(task);
                    }
                }

                Thread.Sleep(Constants.TaskActionInterval);
            }
        }
    }
}
