using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guardian.Menta.Common.DTO.JobModel
{
    public class JobRecordBase
    {
        public virtual int CurrentStep { get; set; }
        public virtual int MaxStep { get; set; }
    }

    public enum StatusJob { Failed=1,Success=2,Pending=3  }

    public class Job
    {
        public Guid JobId { get; set; }
        public string JobName { get; set; }
    }

    public class JobRecord
    {
        public Guid ID { get; set; }

        public string ModelXml { get; set; }

        public StatusJob StatusId { get; set; }

        public DateTime? CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public int Retry { get; set; }

        public Guid JobId { get; set; }

        public string ModelTypeXml { get; set; }
    }


    public class RunningJob
    {
        public Guid ID { get; set; }

        public DateTime? BeginRun { get; set; }
        public DateTime? EndRun { get; set; }
        public Guid? JobId { get; set; }

        public int Total { get; set; }
        public int NoUpdate { get; set; }
        public int Update { get; set; }
        public int Insert { get; set; }
        public int Success { get; set; }
        public int Failed { get; set; }

        public int Status { get; set; }
    }
}
