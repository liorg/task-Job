//using Kipodeal.Contract.TaskJob;
//using Kipodeal.TaskJob;
//using Microsoft.AspNet.SignalR;
//using Microsoft.AspNet.SignalR.Hubs;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.ComponentModel.Composition;
//using System.ComponentModel.Composition.Hosting;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Kipodeal.TaskJob
//{

//    public class TasksManager<THub> where THub:Hub
//    {
//        [ImportMany]
//        private IEnumerable<Lazy<ITaskJob, IJobMetadata>> AllPlugIns { get; set; }

//        private readonly static Lazy<TasksManager<THub>> _instance = new Lazy<TasksManager<THub>>(() =>
//            new TasksManager<THub>(GlobalHost.ConnectionManager.GetHubContext<THub>().Clients));

//        private readonly ConcurrentDictionary<Guid, TaskItem> _tasks = new ConcurrentDictionary<Guid, TaskItem>();

//        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(250);
//        private readonly Random _updateOrNotRandom = new Random();
//        private readonly object _updateStockPricesLock = new object();
//        private Timer _timer;
//        private volatile bool _updatingTaskFlag = false;

//        public TasksManager(IHubConnectionContext<dynamic> client)
//        {
//            // TODO: Complete member initialization
//            Clients = client;

//            _tasks.Clear();

//            // demo data
//            var allTasks = new List<TaskItem>
//            {
//                new TaskItem{TaskId=Guid.Parse("{8F397C1B-5395-1111-1111-42BD4B4856AB}"),
//                    TaskName="t1",CreateCount=0,UpdateCount=0,IsRunning=false,TaskJobType=TaskJobType.MemberEmailSender  }, 

//                new TaskItem{TaskId=Guid.Parse("{8F397C1B-5395-2222-2222-42BD4B4856AB}"),
//                    TaskName="t2",CreateCount=0,UpdateCount=0,IsRunning=false,TaskJobType=TaskJobType.WebsiteEmailSender
//                }, new TaskItem{TaskId=Guid.Parse("{8F397C1B-5395-3333-3333-42BD4B4856AB}"),
//                    TaskName="t3",CreateCount=0,UpdateCount=0,IsRunning=false,TaskJobType=TaskJobType.SupplierEmailSender
//                },
//            };
//            allTasks.ForEach(taskItem => _tasks.TryAdd(taskItem.TaskId, taskItem));

//            //Step 1:
//            //Find the assembly (.dll) that has the stuff we need 
//            //(i.e. [Export]ed stuff) and put it in our catalog
//            DirectoryCatalog catalog = new DirectoryCatalog
//          (System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin"));

//            //Step 2:
//            //To do anything with the stuff in the catalog, 
//            //we need to put into a container (Which has methods to do the magic stuff)
//            CompositionContainer container = new CompositionContainer(catalog);

//            //Step 3:
//            //Now lets do the magic bit - Wiring everything up
//            container.ComposeParts(this);


//        }

//        public void InitTask(Guid taskid)
//        {
//            if (_timer != null)
//                _timer.Dispose();
//            if (_tasks.ContainsKey(taskid))
//            {
//                _tasks[taskid].IsRunning = true;

//                var plugin = from lazyPlugin in AllPlugIns
//                             let metadata = lazyPlugin.Metadata
//                             where metadata.TaskJobType == _tasks[taskid].TaskJobType
//                             select lazyPlugin.Value;
//                if (plugin != null && plugin.Any())
//                    plugin.First().Excute(_tasks[taskid]);

//            }
//            _timer = new Timer(NotifayAll, null, _updateInterval, _updateInterval);

//        }

//        public void StopTask(Guid taskid)
//        {
//            if (_timer != null)
//                _timer.Dispose();

//            if (_tasks.ContainsKey(taskid))
//                _tasks[taskid].IsRunning = false;

//            _timer = new Timer(NotifayAll, null, _updateInterval, _updateInterval);

//        }

//        public async Task<IEnumerable<TaskItem>> GetAllTasks()
//        {
//            TaskFactory tf = new TaskFactory();
//            var result = await tf.StartNew<IEnumerable<TaskItem>>(new Func<IEnumerable<TaskItem>>(() => _tasks.Values));
//            return result;
//            //  return _tasks.Values;
//        }

//        private void NotifayAll(object state)
//        {
//            lock (_updateStockPricesLock)
//            {
//                if (!_updatingTaskFlag)
//                {
//                    _updatingTaskFlag = true;
//                    var runOnly = _tasks.Values.Where(d => d.IsRunning == true);
//                    foreach (var task in runOnly)
//                    {
//                        var plugin = (from lazyPlugin in AllPlugIns
//                                      let metadata = lazyPlugin.Metadata
//                                      where metadata.TaskJobType == task.TaskJobType
//                                      select lazyPlugin.Value).FirstOrDefault();
//                        if (plugin != null)
//                        {
//                            var result = plugin.Notifiy();
//                            BroadcastClients(result);
//                        }
//                    }

//                    _updatingTaskFlag = false;
//                }
//            }
//        }
   
//        private async Task BroadcastClients(ITaskItem stock)
//        {
//            await Clients.All.updateJobCount(stock);
//        }

//        private IHubConnectionContext<dynamic> Clients
//        {
//            get;
//            set;
//        }

//        public static TasksManager<THub> Instance
//        {
//            get
//            {
//                return _instance.Value;
//            }


//        }
//    }
//}
