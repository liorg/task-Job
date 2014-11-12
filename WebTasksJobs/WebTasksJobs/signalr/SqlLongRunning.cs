using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
namespace SignalRChat
{
    public class TaskItem
    {
        public Guid TaskId { get; set; }
        public string TaskName { get; set; }
        public int SenderCount { get; set; }
        public int TaskCompleteCount { get; set; }
        public int CountAll { get; set; }
    }

    public class TasksFactory
    {
        private readonly static Lazy<TasksFactory> _instance = new Lazy<TasksFactory>(() => new TasksFactory(GlobalHost.ConnectionManager.GetHubContext<TaskLongRunningHub>().Clients));

        private readonly ConcurrentDictionary<Guid, TaskItem> _tasks = new ConcurrentDictionary<Guid, TaskItem>();

        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(250);
        private readonly Random _updateOrNotRandom = new Random();
        private readonly object _updateStockPricesLock = new object();
        private readonly Timer _timer;
        private volatile bool _updatingTaskFlag = false;

        public TasksFactory(IHubConnectionContext<dynamic> client)
        {
            // TODO: Complete member initialization
            Clients = client;

            _tasks.Clear();
            var allTasks = new List<TaskItem>
            {
                new TaskItem{TaskId=Guid.NewGuid(),
                    TaskName="t1",TaskCompleteCount=0,SenderCount=0
                }, 
                new TaskItem{TaskId=Guid.NewGuid(),
                    TaskName="t2",TaskCompleteCount=0,SenderCount=0
                },
            };
            allTasks.ForEach(taskItem => _tasks.TryAdd(taskItem.TaskId, taskItem));

            _timer = new Timer(UpdateStockPrices, null, _updateInterval, _updateInterval);
          
        }

        public IEnumerable<TaskItem> GetAllTasks()
        {
            return _tasks.Values;
        }

        private void UpdateStockPrices(object state)
        {
            lock (_updateStockPricesLock)
            {
                if (!_updatingTaskFlag)
                {
                    _updatingTaskFlag = true;

                    foreach (var stock in _tasks.Values)
                    {
                        if (TryUpdateCountDemo(stock))
                        {
                            BroadcastCount(stock);
                        }
                    }

                    _updatingTaskFlag = false;
                }
            }
        }
  
        private bool TryUpdateCountDemo(TaskItem stock)
        {
            // Randomly choose whether to update this stock or not
            var r = _updateOrNotRandom.NextDouble();
            if (r > .1)
            {
                return false;
            }

            // Update the stock price by a random factor of the range percent
            var random = new Random(100);
           
            stock.CountAll += random.Next();
            return true;
        }

        private void BroadcastCount(TaskItem stock)
        {
            Clients.All.updateJobCount(stock);
        }
     
        private IHubConnectionContext<dynamic> Clients
        {
            get;
            set;
        }

        public static TasksFactory Instance
        {
            get
            {
                return _instance.Value;
            }


        }
    }

   // [HubName("tasksJob")]
    public class TaskLongRunningHub : Hub
    {
        private readonly TasksFactory _tasks;

        public TaskLongRunningHub(TasksFactory tasks)
        {
            _tasks = tasks;
        }
        public TaskLongRunningHub()
            : this(TasksFactory.Instance)
        { }

        public IEnumerable<TaskItem> GetAllTasks()
        {
            return _tasks.GetAllTasks();
        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }
    }

}