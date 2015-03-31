using Kipodeal.Contract.TaskJob;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kipodeal.TaskJob
{
 // [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export(typeof(ITaskJob))]
    [JobTaskType(TaskJobType.MemberEmailSender)]
    public class TaskJobMemberEmail : ITaskJob
    {

        ITaskItem _taskitem;

        public void Excute(ITaskItem taskitem)
        {

            _taskitem = taskitem;
            TaskFactory tf = new TaskFactory();
            var result = tf.StartNew(() =>
            {
                _taskitem.UpdateCount = 1990;
                _taskitem.TaskName = "start MEMEBER11!!!";

            });

        }

        public ITaskItem Notifiy()
        {
            var random = new Random(9);
            _taskitem.CountAll += random.Next();
            return _taskitem;
        }


    }
}
