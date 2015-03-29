//using Kipodeal.Contract.TaskJob;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.Composition;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Kipodeal.TaskJob
//{
//    [Export(typeof(ITaskJob))]
//    [JobTaskType(TaskJobType.SupplierEmailSender)]
//    public class TaskJobSupplierEmail : ITaskJob
//    {

//        ITaskItem _taskitem;

//        public void Excute(ITaskItem taskitem)
//        {

//            _taskitem = taskitem;
//            TaskFactory tf = new TaskFactory();
//            var result = tf.StartNew(() =>
//            {
//                _taskitem.UpdateCount = 1922;
//                _taskitem.TaskName = "startSupplier";

//            });

//        }

//        public ITaskItem Notifiy()
//        {
//            var random = new Random(99);
//            _taskitem.CountAll += random.Next();
//            return _taskitem;
//        }
//    }
//}
