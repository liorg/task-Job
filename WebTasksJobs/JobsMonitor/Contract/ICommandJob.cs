using Guardian.Menta.Common.DTO.JobModel;
//using Microsoft.Xrm.Sdk;
//using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;

namespace Guardian.Menta.Interfaces.Jobs
{
    public interface ICommandJob<T> where T : JobRecordBase
    {
        IJobProvider<T> JobProvider { get; set; }
        CofigurationJob CofigurationJob { get; }
        IEnumerable<T> Get(Action<string> log);
        ResultJob Excute(T job, Action<string> log);
        void PostExcute(Action<string> log);
        void Received(IEnumerable<T> jobs, Action<string> trace);
    }
}
