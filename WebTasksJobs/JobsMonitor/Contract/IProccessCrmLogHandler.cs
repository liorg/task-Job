using Guardian.Menta.Common.DTO.JobModel;
using Guardian.Menta.Interfaces.Jobs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Guardian.Menta.Interfaces.Jobs
{
    public interface IProccessCrmLogHandler
    {
        void Init();
        void WriteStatistics(RunningJob jobstatus);
        void WriteValidationBussinessLogic(string log,string stackOverflow,string xmlObject);
        void WriteErrorRecord(string xmlObject, Exception e);
        void WriteError(Exception e);
        void Finish(RunningJob jobDetatil);
        bool EnsureCanWriteError();
    }
}
