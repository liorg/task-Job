using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kipodeal.Contract.TaskJob
{
    public interface ITaskJob
    {
        void Excute(ITaskItem taskitem);
        ITaskItem Notifiy();
    }

}
