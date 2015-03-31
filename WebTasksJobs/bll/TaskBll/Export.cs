using Kipodeal.TaskJob;
using Lior.Job.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskBll
{
    public class Export:IExport
    {
        public void InHere()
        {
            Console.WriteLine("test1333337");
            Console.WriteLine("In MEF Library1: AppDomain: {0}", AppDomain.CurrentDomain.FriendlyName);
        }
    }
}
