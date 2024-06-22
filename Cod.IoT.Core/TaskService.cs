using System;
using System.Collections;
using System.Threading;

namespace Cod.IoT
{
    public class TaskService : GenericService, ITaskService
    {
        private bool stop;
        private Thread worker;
        
        protected Queue Tasks { get; private set; }

        public override int ID => Constants.TaskServiceID;

        public TaskService()
        {
            Tasks = new Queue();
        }

        public virtual void Schedule(Action task)
        {
            Tasks.Enqueue(task);
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
                stop = false;
                worker = new Thread(ActionTask);
                worker.Start();
            }
        }

        private void Stop()
        {
            if (worker != null)
            {
                stop = true;
                worker.Join(Constants.TaskActionInterval);
                worker = null;
            }
        }

        protected virtual void ActionTask()
        {
            while (!stop)
            {
                if (Tasks.Count > 0)
                {
                    var action = (Action)Tasks.Dequeue();
                    action();
                }

                Thread.Sleep(Constants.TaskActionInterval);
            }
        }
    }
}
