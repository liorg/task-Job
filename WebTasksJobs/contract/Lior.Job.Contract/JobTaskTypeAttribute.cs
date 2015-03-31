using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kipodeal.Contract.TaskJob
{

    [MetadataAttribute]
    //[AttributeUsage(AttributeTargets.Class)]
    [AttributeUsageAttribute(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class JobTaskTypeAttribute : Attribute
    {
        public JobTaskTypeAttribute(TaskJobType taskJobType)
        {
            TaskJobType = taskJobType;
        }

        public TaskJobType TaskJobType { get; set; }
    }
}
