using Guardian.Menta.Interfaces.Jobs;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Security;
using System.Runtime.Serialization;

using System.Runtime;
using System.Security;

using System.ServiceModel;
using System.Data.SqlClient;

using Guardian.Menta.JobsPorvider.DataModel;
using Guardian.Menta.Common.DTO;
using Guardian.Menta.Common.DTO.JobModel;

namespace Guardian.Menta.JobsPorvider
{

    public class JobsProvider<T> : IJobProvider<T> where T : JobRecordBase
    {
        #region Field
        enum StatusRecord { Ready = 1, OnProgress = 2, Finish = 3, Failed = 4 }
        public const string JobProviderConnectionString = "JobProviderConnectionString";
        ICommandJob<T> commandJobHandler;
        Action<string> trace;
        IProccessCrmLogHandler crmLogHandler;
        string connectionString;
        RunningJob runningJob;
        List<ErrorMessage> errMangerList;
        static object lockObj = new object();
        

        #endregion

        #region Ctor

        //public JobsProvider(ICommandJob<T> commandJob, IProccessCrmLogHandler crmLog, Action<string> traceLog,
        //    IServiceManagement<IOrganizationService> serviceManagement, AuthenticationCredentials autoCredentials)
        //    : this(commandJob, crmLog, traceLog, null, serviceManagement, autoCredentials)
        //{
        //}

        //public JobsProvider(ICommandJob<T> commandJob, IProccessCrmLogHandler crmLog, Action<string> traceLog, string conn)
        //    : this(commandJob, crmLog, traceLog, conn)//, null, null)
        //{

        //}

        //public JobsProvider(ICommandJob<T> commandJob, IProccessCrmLogHandler crmLog, Action<string> traceLog, string conn,
        //    IServiceManagement<IOrganizationService> serviceManagement, AuthenticationCredentials autoCredentials)
        //{
        //    if (String.IsNullOrEmpty(conn))
        //        connectionString = System.Configuration.ConfigurationManager.AppSettings[JobProviderConnectionString];
        //    else
        //        connectionString = conn;
        //    if (mServiceManagement == null)
        //    {
        //      //  LoadRelatedObjectsCrmServiceProxy();
        //    }
        //    else
        //    {
        //        mServiceManagement = serviceManagement;
        //        mAutoCredentials = autoCredentials;
        //    }

        //    commandJobHandler = commandJob;
        //    trace = traceLog;
        //    crmLogHandler = crmLog;
        //    commandJobHandler.JobProvider = this;
        //    runningJob = new RunningJob();
        //}

        public JobsProvider(ICommandJob<T> commandJob, IProccessCrmLogHandler errLog, Action<string> traceLog)
            //: this(commandJob, errLog, traceLog, System.Configuration.ConfigurationManager.AppSettings[JobProviderConnectionString])
        {
        }

        #endregion

        #region Run
        public void Run()
        {
            try
            {
                crmLogHandler.Init();
                InsertToSql();
                MonitorRunningBegin();
                LoadMessageError();
                ExcuteRecords();
                commandJobHandler.PostExcute(trace);
                MonitorRunningEnd();
                crmLogHandler.Finish(runningJob);
            }
            catch (Exception e)
            {
                if (crmLogHandler.EnsureCanWriteError())
                    crmLogHandler.WriteError(e);

                trace(e.ToString());
            }

        }
        #endregion

        #region Connection

        //private void LoadRelatedObjectsCrmServiceProxy()
        //{
        //    ConfigurationCrmService config = null;
        //    if (mServiceManagement == null)
        //    {
        //        lock (lockObj)
        //        {
        //            if (mServiceManagement == null)
        //            {
        //                config = Guardian.SDKServiceProvider.CrmServices.GetConfig();
        //                var uri = config.OrganizationUri;
        //                mServiceManagement = ServiceConfigurationFactory.CreateManagement<IOrganizationService>(uri);

        //            }
        //        }
        //    }
        //    if (mAutoCredentials == null)
        //    {
        //        lock (lockObj)
        //        {
        //            if (mAutoCredentials == null)
        //            {
        //                mAutoCredentials = new AuthenticationCredentials();
        //                config = config ?? Guardian.SDKServiceProvider.CrmServices.GetConfig();
        //                mAutoCredentials.ClientCredentials = config.Credentials;
        //            }
        //        }
        //    }
        //}

        protected SqlConnection GetSqlConnection()
        {
            return new SqlConnection(connectionString);
        }
        #endregion

        #region Monitor

        void MonitorRunningBegin()
        {
            using (var connection = GetSqlConnection())
            {
                var command = new SqlCommand(@"dbo.insertRunningJob", connection);
                command.Parameters.AddWithValue("@jobid", runningJob.JobId);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();
                runningJob.ID = (Guid)command.ExecuteScalar();
            }
            runningJob.BeginRun = DateTime.Now;
        }

        void MonitorRunningEnd()
        {
            using (var connection = GetSqlConnection())
            {
                var command = new SqlCommand(@"dbo.updateRunningJob", connection);
                command.Parameters.AddWithValue("@id", runningJob.ID);
                command.Parameters.AddWithValue("@total", runningJob.Total);
                command.Parameters.AddWithValue("@noupdate", runningJob.NoUpdate);
                command.Parameters.AddWithValue("@update", runningJob.Update);
                command.Parameters.AddWithValue("@insert", runningJob.Insert);
                command.Parameters.AddWithValue("@success", runningJob.Success);
                command.Parameters.AddWithValue("@failed", runningJob.Failed);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();
                command.ExecuteNonQuery();
            }
            runningJob.EndRun = DateTime.Now;
        }
        #endregion

        #region Set And Get Bussiness Rows

        void InsertToSql()
        {
            var jobs = commandJobHandler.Get(trace);
            if (jobs == null || !jobs.Any())
            {
                runningJob.JobId = GetJobIdByJobName();
                if (runningJob.JobId == null)
                    throw new ArgumentNullException("there is no any jobid for " + commandJobHandler.CofigurationJob.FullName);
                return;
            }

            foreach (var job in jobs)
            {
                var xmlObject = SerializeToXml(job);
                using (var connection = GetSqlConnection())
                {
                    var command = new SqlCommand(@"dbo.GS_InsertRecordJob", connection);
                    command.Parameters.AddWithValue("@jobName", commandJobHandler.CofigurationJob.FullName);
                    command.Parameters.AddWithValue("@ModelXml", xmlObject);
                    command.Parameters.AddWithValue("@ModelTypeXml", typeof(T).FullName);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    connection.Open();

                    if (runningJob.JobId == null)
                        runningJob.JobId = (Guid)command.ExecuteScalar();
                    else
                        command.ExecuteNonQuery();
                }
            }
            commandJobHandler.Received(jobs, trace);
        }

        private Guid? GetJobIdByJobName()
        {
            Guid? jobid = null;
            CofigurationJob cofigurationJob = commandJobHandler.CofigurationJob;
            using (var connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(@"dbo.GS_GetJobIdByName", connection);
                command.Parameters.AddWithValue("@jobName", commandJobHandler.CofigurationJob.FullName);

                command.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();
                var returnJobid = command.ExecuteScalar();
                if (returnJobid is Guid)
                {
                    jobid = (Guid)returnJobid;
                }
            }
            return jobid;
        }

        public IEnumerable<RecordJob<T>> GetRecordsById()
        {
            var list = new System.Collections.Generic.List<RecordJob<T>>();

            using (var connection = GetSqlConnection())
            {
                var command = new SqlCommand(@"dbo.getRecordsByJobId", connection);
                command.Parameters.AddWithValue("@JobId", runningJob.JobId);

                command.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();
                var drOutput = command.ExecuteReader();

                while (drOutput.Read())
                {
                    var recordJob = new RecordJob<T>();
                    var xml = (drOutput["ModelXml"] != Convert.DBNull) ? drOutput["ModelXml"].ToString() : null;
                    recordJob.JobId = (drOutput["JobId"] != Convert.DBNull) ? Guid.Parse(drOutput["JobId"].ToString()) : Guid.Empty;
                    recordJob.RecordId = (drOutput["ID"] != Convert.DBNull) ? Guid.Parse(drOutput["ID"].ToString()) : Guid.Empty;
                    recordJob.Retry = (drOutput["Retry"] != Convert.DBNull) ? int.Parse(drOutput["Retry"].ToString()) : 0;
                    recordJob.JobRecord = DeserializeFromXml(xml);
                    list.Add(recordJob);
                }
            }
            return list;
        }

        #endregion

        #region Excute Bussiness Row

        public void ExcuteRecords()
        {
            var getRecordsById = GetRecordsById();
            //1	מוכן לטעינה
            //2	בתהליך
            //3	הסתיים
            //4	נכשל
            if (getRecordsById.Any())
                runningJob.Total = getRecordsById.Count();

            foreach (var record in getRecordsById)
            {
                var recordModel = record != null ? record.JobRecord : default(T);
                int status = 0;
                string action = "";
                try
                {
                    UpdateOnProgressRow(record);
                    var returnValues = commandJobHandler.Excute(recordModel, trace);
                    action = returnValues.ToString();

                    if (returnValues.HasFlag(ResultJob.Insert))
                        runningJob.Insert++;
                    if (returnValues.HasFlag(ResultJob.Update))
                        runningJob.Update++;
                    if (returnValues.HasFlag(ResultJob.NoUpdate))
                        runningJob.NoUpdate++;
                    if (returnValues.HasFlag(ResultJob.Failed) || returnValues.HasFlag(ResultJob.FailedRetry))
                        runningJob.Failed++;
                    else
                        runningJob.Success++;

                    if (returnValues.HasFlag(ResultJob.FailedRetry))
                        status = (int)StatusRecord.Failed;
                    else
                        status = (int)StatusRecord.Finish;
                }
                //catch (FaultException<OrganizationServiceFault> fex)
                //{
                //    runningJob.Failed++;
                //    HandleFualtException(recordModel, fex, ref status);
                //}
                catch (Exception e)
                {
                    runningJob.Failed++;
                    HandleUnHandeledExc(recordModel, e, ref status);
                }
                finally
                {
                    UpdateRow(record, status, action);
                }
            }
        }

        void UpdateOnProgressRow(RecordJob<T> record, string action = "")
        {
            int status = (int)StatusRecord.OnProgress;
            UpdateStatus(record, status);
        }

        void UpdateStatus(RecordJob<T> record, int status)
        {
            using (var connection = GetSqlConnection())
            {
                var command = new SqlCommand(@"dbo.UpdateRecord", connection);
                command.Parameters.AddWithValue("@recordid", record.RecordId);
                command.Parameters.AddWithValue("@status", status);
                // command.Parameters.AddWithValue("@retry", record.Retry);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
      
        void UpdateRow(RecordJob<T> record, int status, string action = "")
        {
            record.Retry += 1; 
            var xmlObject = SerializeToXml(record.JobRecord);
            using (var connection = GetSqlConnection())
            {
                var command = new SqlCommand(@"dbo.UpdateRecord", connection);
                command.Parameters.AddWithValue("@recordid", record.RecordId);
                command.Parameters.AddWithValue("@status", status);
                command.Parameters.AddWithValue("@retry", record.Retry);
                command.Parameters.AddWithValue("@ModelXml", xmlObject);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();
                command.ExecuteNonQuery();
            }
            crmLogHandler.WriteStatistics(runningJob);
        }
        #endregion

        #region Handler Errors

        private void LoadMessageError()
        {
            ErrorManager errManger = new ErrorManager();
              errMangerList = errManger.ErrMsgList;
            //using (var service = new OrganizationServiceProxy(ServiceManagement, AutoCredentials.ClientCredentials))
            //{
            //    ErrorManager errManger = new ErrorManager(service);
            //    errMangerList = errManger.ErrMsgList;
            //}
        }

        private void HandleFualtException(T model,Exception fex, ref int status)
        {
            ErrorMessage msg = errMangerList.Where(n => n.Value == fex.Message).FirstOrDefault();
            if (msg != null)
                HandleLogicExc(model, fex, ref status, msg.Value);
            else
                HandleUnHandeledExc(model, fex, ref status);
        }

        private void HandleLogicExc(T model, Exception e, ref int status, string messageLog)
        {
            var xmlObject = SerializeToXml(model);
            crmLogHandler.WriteValidationBussinessLogic(messageLog, e.StackTrace, xmlObject);
            status = (int)StatusRecord.Finish;
        }

        private void HandleUnHandeledExc(T model, Exception e, ref int status)
        {
            var xmlObject = SerializeToXml(model);
            crmLogHandler.WriteErrorRecord(xmlObject, e);
            status = (int)StatusRecord.Failed;
        }

        #endregion

        #region Helper

        public static string SerializeToXml(T obj)
        {
            using (StringWriter textWriter = new StringWriter())
            {
                var ser = new XmlSerializer(typeof(T));
                ser.Serialize(textWriter, obj);
                return textWriter.ToString();
            }
        }

        public static T DeserializeFromXml(string xml)
        {
            T result;
            var ser = new XmlSerializer(typeof(T));
            using (var tr = new StringReader(xml))
            {
                result = (T)ser.Deserialize(tr);
            }
            return result;
        }

        #endregion

        #region Get Job Details

        /// <summary>
        /// prevent getting some data again
        /// </summary>
        /// 
        public bool IsJobRunToday
        {
            get
            {
                DateTime? lastJobDate = GetLastJobDate();
                if (lastJobDate == null)
                    return false;
                return lastJobDate.Value.Date == DateTime.Today;
            }
        }

        public DateTime? GetLastJobDate()
        {
            DateTime lastDate = DateTime.Now;
            bool succeeded = false; var configJob = commandJobHandler.CofigurationJob;
            using (var connection = GetSqlConnection())
            {
                SqlCommand command = new SqlCommand(@"dbo.GS_GetLastJobDate", connection);
                command.Parameters.AddWithValue("@JobName", commandJobHandler.CofigurationJob.FullName);

                command.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();
                var drOutput = command.ExecuteReader();

                while (drOutput.Read())
                {
                    if (DateTime.TryParse(drOutput["StartedAt"].ToString(), out lastDate))
                        succeeded = true;
                }
            }
            if (succeeded)
                return (DateTime?)lastDate;
            else
                return null;
        }

        #endregion

        #region Implementation IJobProvider

        //public IServiceManagement<IOrganizationService> ServiceManagement
        //{
        //    get
        //    {
        //       // if (mServiceManagement == null)
        //            //LoadRelatedObjectsCrmServiceProxy();

        //        return mServiceManagement;
        //    }

        //}

        //public AuthenticationCredentials AutoCredentials
        //{
        //    get
        //    {
        //        //if (mAutoCredentials == null)
        //          //  LoadRelatedObjectsCrmServiceProxy();

        //        return mAutoCredentials;
        //    }

        //}

        public RunningJob RunningJob
        {
            get { return runningJob; }
        }
        #endregion
    }
}
