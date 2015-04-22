
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lior.Job.Sdk.Contract
{
    public interface IJobTask
    {
         Guid TaskId { get; set; }
    }
}
