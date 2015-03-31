
using Kipodeal.Contract.TaskJob;
using Kipodeal.Helper.Exstension.AsyncLock;
using Kipodeal.TaskJob;
using Lior.Job.Contract;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TasksJobs.TaskJob;


namespace Kipodeal.TaskJob
{
     public interface IExport {

        void InHere();
    }
    public class TasksManagerAsync2<THub> where THub : Hub
    {
        private static volatile bool IsStartService = false;
        [Import(AllowRecomposition = true, AllowDefault = true)]
        Lazy<IDal> a;

        [ImportMany(AllowRecomposition = true)]
        private IEnumerable<Lazy<ITaskJob, IJobMetadata>> AllPlugIns { get; set; }

        private readonly static Lazy<TasksManagerAsync2<THub>> _instance = new Lazy<TasksManagerAsync2<THub>>
            (() => new TasksManagerAsync2<THub>(GlobalHost.ConnectionManager.GetHubContext<THub>().Clients));

        private readonly ConcurrentDictionary<Guid, TaskItem> _tasks = new ConcurrentDictionary<Guid, TaskItem>();

        private static ConcurrentDictionary<Guid, string> SessionCallers = new ConcurrentDictionary<Guid, string>();

        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(10);// TimeSpan.FromMilliseconds(250);

        private readonly Random _updateOrNotRandom = new Random();

        private volatile bool _updatingTaskFlag = false;

        public TasksManagerAsync2(IHubConnectionContext<dynamic> client)
        {
            // TODO: Complete member initialization
            Clients = client;

            _tasks.Clear();
            var allTasks = new List<TaskItem>
            {
                new TaskItem{TaskId=Guid.Parse("{8F397C1B-5395-1111-1111-42BD4B4856AB}"),
                    TaskName="t1",CreateCount=0,UpdateCount=0,IsRunning=false,TaskJobType=TaskJobType.MemberEmailSender  }, 

                new TaskItem{TaskId=Guid.Parse("{8F397C1B-5395-2222-2222-42BD4B4856AB}"),
                    TaskName="t2",CreateCount=0,UpdateCount=0,IsRunning=false,TaskJobType=TaskJobType.WebsiteEmailSender
                }, new TaskItem{TaskId=Guid.Parse("{8F397C1B-5395-3333-3333-42BD4B4856AB}"),
                    TaskName="t3",CreateCount=0,UpdateCount=0,IsRunning=false,TaskJobType=TaskJobType.SupplierEmailSender
                },
            };
            allTasks.ForEach(taskItem => _tasks.TryAdd(taskItem.TaskId, taskItem));


            LoadPlugins();
        }
        CompositionContainer _container;
       
        public void LoadPlugins()
        {
            //Step 1:
            //Find the assembly (.dll) that has the stuff we need 
            //(i.e. [Export]ed stuff) and put it in our catalog
            //  DirectoryCatalog catalog = new DirectoryCatalog
            //(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin"));
            //  DirectoryCatalog catalog = new DirectoryCatalog(".");
            //
            var df = System.Configuration.ConfigurationManager.AppSettings["pluginPath"]; //System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
            var connStr = System.Configuration.ConfigurationManager.AppSettings["connStr"]; // System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

            //  var catalog = new AggregateCatalog();
            //catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            ////  catalog.Catalogs.Add(new DirectoryCatalog("."));
            //  catalog.Catalogs.Add(new DirectoryCatalog(df));
            // var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            //Step 2:
            //To do anything with the stuff in the catalog, 
            //we need to put into a container (Which has methods to do the magic stuff)

            // var di = new DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath("../../bin/"));
            var di = new DirectoryInfo(df);

            if (!di.Exists) throw new Exception("Folder not exists: " + di.FullName);

            //var dlls = di.GetFileSystemInfos("*.dll");
            AggregateCatalog catalog = new AggregateCatalog();
            //foreach (var fi in dlls)
            //{
            //    try
            //    {
            //        var ac = new AssemblyCatalog(Assembly.LoadFile(fi.FullName));
            //        var parts = ac.Parts.ToArray(); // throws ReflectionTypeLoadException 
            //        catalog.Catalogs.Add(ac);
            //    }
            //    catch (ReflectionTypeLoadException ex)
            //    {
            //        // Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            //    }
            //}

            // catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            var dcatalog = new DirectoryCatalog(di.FullName);
            catalog.Catalogs.Add(dcatalog);



            // _container = new CompositionContainer(catalog);
            _container = new CompositionContainer(dcatalog);

            _container.ComposeExportedValue("ConnStr", connStr);



            //   _batch = new CompositionBatch();

            // Store the settingsPart for later removal...


            //Step 3:
            //Now lets do the magic bit - Wiring everything up
            _container.ComposeParts(this);
            //_container.ExportsChanging += (s, e) =>
            //{// getting the changes gap
            //    dcatalog.Refresh();
            //    Debug.WriteLine("ss");
            //};
            _container.ExportsChanged += (s, e) =>
            {// getting the changes gap

                dcatalog.Refresh();
                Debug.WriteLine("ss");
            };


            //var cachePath = @"e:\ShadowDLL";

            //if (!Directory.Exists(cachePath))
            //{
            //    Directory.CreateDirectory(cachePath);
            //}
            //if (!Directory.Exists(di.FullName))
            //{
            //    Directory.CreateDirectory(di.FullName);
            //}
            //var setup = new AppDomainSetup
            //{
            //    CachePath = cachePath,
            //    ShadowCopyFiles = "true",
            //    ShadowCopyDirectories = di.FullName
            //};
           // AppDomain domain = AppDomain.CreateDomain("DynamicHost_AppDomain", AppDomain.CurrentDomain.Evidence, setup);
            
            //  IsStartService = true;
        }

        public void StopPlugins()
        {
            IsStartService = false;
            // _container.ReleaseExports<IEnumerable<Lazy<ITaskJob, IJobMetadata>>>(AllPlugIns);

            //var export = _container.GetExports<ITaskJob, IJobMetadata>();
            //_container.ReleaseExports(export);
            //_container.ComposeParts(this);
         
        }
        public async Task InitTask(Guid taskid, string callerId)
        {
            var d = a.Value.show();
            // var df = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
            LoggerManager log = new LoggerManager();
            log.Trace("TasksManagerAsync", "InitTask", "begin: taskid" + taskid.ToString() + ",callerid" + callerId);
            IsStartService = true;
            if (_tasks.ContainsKey(taskid))
            {
                var callerid = SessionCallers.GetOrAdd(taskid, callerId);

                _tasks[taskid].IsRunning = true;

                var plugin = from lazyPlugin in AllPlugIns
                             let metadata = lazyPlugin.Metadata
                             where metadata.TaskJobType == _tasks[taskid].TaskJobType
                             select lazyPlugin.Value;
                if (plugin != null && plugin.Any())
                    plugin.First().Excute(_tasks[taskid]);

            }
            await PollNotifayAll();

        }

        public async Task PollNotifayAll()
        {
            while (IsStartService)
            {
                await Task.Delay(_updateInterval);
                try
                {
                    await NotifayAll(null);

                }
                catch { /* ignore errors */ }
            }
        }

        public async Task StopTask(Guid taskid, string callerId)
        {
            LoggerManager log = new LoggerManager();
            log.Trace("TasksManagerAsync", "StopTask", "begin: taskid=" + taskid + ",callerId=" + callerId);
            //  string callerId = "";
            if (_tasks.ContainsKey(taskid))
            {
                log.Trace("TasksManagerAsync", "StopTask", "is stopped:" + taskid);

                _tasks[taskid].IsRunning = false;
                SessionCallers.TryRemove(taskid, out callerId);

            }
            if (SessionCallers.Count == 0)
            {
                log.Trace("TasksManagerAsync", "StopTask", "stop all services:" + callerId);

                IsStartService = false;
                await NotifayAll(null);
            }
            else
            {
                log.Trace("TasksManagerAsync", "StopTask", "continue poliing:" + callerId);

                await PollNotifayAll();
            }
            //  await PollNotifayAll();
        }

        public async Task StopTaskByCallId(string callerId)
        {
            if (String.IsNullOrEmpty(callerId))
                return;
            LoggerManager log = new LoggerManager();
            log.Trace("TasksManagerAsync", "StopTaskByCallId", "begin:" + callerId);
            var tasksCaller = SessionCallers.Where(k => k.Value == callerId).Select(kk => kk.Key).AsEnumerable();
            //var tasksCaller= SessionCallers.Values.Where(c => c == callerId).AsEnumerable();
            if (tasksCaller.Any())
            {
                foreach (var taskid in tasksCaller)
                {
                    //        task 
                    if (_tasks.ContainsKey(taskid))
                    {
                        log.Trace("TasksManagerAsync", "StopTaskByCallId", "task id is stopped" + taskid);
                        _tasks[taskid].IsRunning = false;

                        SessionCallers.TryRemove(taskid, out callerId);

                    }
                }
            }
            if (SessionCallers.Count == 0)
            {
                log.Trace("TasksManagerAsync", "StopTaskByCallId", "stop all services:" + callerId);

                IsStartService = false;
                await NotifayAll(null);
            }
            else
            {
                log.Trace("TasksManagerAsync", "StopTaskByCallId", "continue poliing:" + callerId);

                await PollNotifayAll();
            }
        }

        public async Task<IEnumerable<TaskItem>> GetAllTasks()
        {
            LoggerManager log = new LoggerManager();
            log.Trace("TasksManagerAsync", "GetAllTasks", "begin:");

            TaskFactory tf = new TaskFactory();
            var result = await tf.StartNew<IEnumerable<TaskItem>>(new Func<IEnumerable<TaskItem>>(() => _tasks.Values));
            return result;
        }

        private async Task NotifayAll(object state)
        {

            AsyncLock myLock = new AsyncLock();
            using (var releaser = await myLock.LockAsync())
            {
                // do synchronized stuff here

                //  lock (_updateStockPricesLock)
                //  {
                if (!_updatingTaskFlag)
                {
                    _updatingTaskFlag = true;
                    var runOnly = _tasks.Values.Where(d => d.IsRunning == true);
                    foreach (var task in runOnly)
                    {
                        var plugin = (from lazyPlugin in AllPlugIns
                                      let metadata = lazyPlugin.Metadata
                                      where metadata.TaskJobType == task.TaskJobType
                                      select lazyPlugin.Value).FirstOrDefault();
                        if (plugin != null)
                        {
                            var result = plugin.Notifiy();

                            await BroadcastClients(result);
                        }
                    }

                    _updatingTaskFlag = false;
                }
                // }
            }
        }

        private async Task BroadcastClients(ITaskItem stock)
        {
            // LoggerManager log = new LoggerManager();
            //  log.Trace("TasksManagerAsync", "BroadcastClients", "begin:");
            await Clients.All.updateJobCount(stock);
        }

        private IHubConnectionContext<dynamic> Clients
        {
            get;
            set;
        }

        public static TasksManagerAsync2<THub> Instance
        {
            get
            {
                return _instance.Value;
            }


        }

    }
}
