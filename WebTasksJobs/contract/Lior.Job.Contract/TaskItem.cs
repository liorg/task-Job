using System;
using System.Web;

using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using Kipodeal.Contract.TaskJob;
namespace Kipodeal.TaskJob
{
    public class TaskItem : ITaskItem
    {
        public Guid TaskId { get; set; }
        public string TaskName { get; set; }
        public int UpdateCount { get; set; }
        public int CreateCount { get; set; }
        public int CountAll { get; set; }
        public bool IsRunning { get; set; }
        public TaskJobType TaskJobType { get; set; }
        public int FailedCount { get; set; }

        public string IsRunningS
        {
            get
            {
                return IsRunning.ToString();
            }
        }
    }

}