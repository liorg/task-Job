using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guardian.Menta.Interfaces.Jobs
{

    public class CofigurationJob
    {
        public string JobName { get; set; }
        public string Version { get; set; }
        public int? MaxRetries { get; set; }
        public string FullName
        {
            get
            {
                return this.JobName + this.Version;
            }
        }
    }
}
