using Guardian.Menta.Common.DTO.JobModel;
//using Microsoft.Xrm.Sdk;
//using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;

namespace Guardian.Menta.Interfaces.Jobs
{
    public interface IJobProvider<T> where T : JobRecordBase
    {
        //IServiceManagement<IOrganizationService> ServiceManagement { get; }
        //AuthenticationCredentials AutoCredentials { get; }
        RunningJob RunningJob { get; }
        bool IsJobRunToday { get; }
        DateTime? GetLastJobDate();
    }
}
