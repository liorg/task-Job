using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Kipodeal.Contract.TaskJob;
using Kipodeal.TaskJob;
namespace Kipodeal.RT
{
    public class TasksEmailLongRunning : Hub
    {
        private readonly TasksManager _tasks;

        public TasksEmailLongRunning(TasksManager tasks)
        {
            _tasks = tasks;
        }
        public TasksEmailLongRunning()
            : this(TasksManager.Instance)
        { }

        public async Task<IEnumerable<TaskItem>> GetAllTasks()
        {
            return await _tasks.GetAllTasks();
        }

        public void LoadTask(Guid taskid)
        {
            _tasks.InitTask(taskid);
        }

        public override async System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            await base.OnDisconnected(stopCalled);
        }
    }

}