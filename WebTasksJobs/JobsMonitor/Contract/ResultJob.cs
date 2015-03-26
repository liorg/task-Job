
//using Microsoft.Xrm.Sdk;
//using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;

namespace Guardian.Menta.Interfaces.Jobs
{
    [Flags]
    public enum ResultJob
    {

        Success = 0, Insert = 1, Update = 2, NoUpdate = 4, Failed = 8, FailedRetry = 16
    }
}
