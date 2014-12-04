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
    public class TasksLongRunningAscyn : Hub
    {
        private readonly TasksManagerAsync<TasksLongRunningAscyn> _tasks;

        public TasksLongRunningAscyn(TasksManagerAsync<TasksLongRunningAscyn> tasks)
        {
            _tasks = tasks;
        }

        public TasksLongRunningAscyn() : this(TasksManagerAsync<TasksLongRunningAscyn>.Instance) { }

        public async Task<IEnumerable<TaskItem>> GetAllTasks()
        {
            return await _tasks.GetAllTasks();
        }

        public async Task StopTask(Guid taskid)
        {
            string callerid = this.Context != null ? this.Context.ConnectionId : "";
            await _tasks.StopTask(taskid, callerid);
        }

        public async Task LoadTask(Guid taskid)
        {
            string callerid = this.Context != null ? this.Context.ConnectionId : "";
            await _tasks.InitTask(taskid, callerid);
        }

        public override async System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            string callerid = this.Context != null ? this.Context.ConnectionId : "";
            await _tasks.StopTaskByCallId(callerid);
            await base.OnDisconnected(stopCalled);
        }
        public override async Task OnReconnected()
        {
            string callerid = this.Context != null ? this.Context.ConnectionId : "";
            await _tasks.StopTaskByCallId(callerid);
            await base.OnReconnected();
        }

    }

}