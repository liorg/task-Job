using Lior.Job.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lior.Jobs.TaskDal
{
    [Export(typeof(IDal))]
    public class TaskRepository : IDal
    {
        public string ConnectionString;

        public TaskRepository()
        {
            //  This constructor would not be called. 
            //  Since we use a ImportingConstructor attribute
        }

        [ImportingConstructor]
        public TaskRepository([Import("ConnStr")]string connstr)
        {
            ConnectionString = connstr;
        }

        
        public string show()
        {
            return "Welcome" +ConnectionString;
        }
    }
}
