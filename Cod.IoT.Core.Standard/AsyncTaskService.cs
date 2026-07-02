using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cod.IoT
{
    internal class AsyncTaskService : TaskService, IAsyncTaskService
    {
        public void Schedule(Func<Task> task) => Tasks.Enqueue(new ScheduledAsyncTask(task));

        public void Schedule(Func<Task> task, DateTimeOffset due) => Tasks.Enqueue(new ScheduledAsyncTask(task, due));

        protected override async void ActionTask()
        {
            while (!StopRequested)
            {
                if (Tasks.Count > 0)
                {
                    var task = Tasks.Dequeue();
                    if (task is ScheduledAsyncTask asyncTask && asyncTask.IsPastDue)
                    {
                        await asyncTask.Task();
                        continue;
                    }
                    else if (task is ScheduledTask t && t.IsPastDue)
                    {
                        t.Action();
                        continue;
                    }

                    Tasks.Enqueue(task);
                }

                Thread.Sleep(Constants.TaskActionInterval);
            }
        }
    }
}
