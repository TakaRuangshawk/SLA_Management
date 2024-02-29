using Microsoft.AspNetCore.SignalR;
using MySqlX.XDevAPI;
using Serilog;
using SLA_Management.Commons.SignalR;
using SLA_Management.Data;
using SLA_Management.Models.COMLogModel;
using System.Data.SqlClient;

namespace SLA_Management.Commons
{
    public class ServiceRPT : ServiceFTP
    {
        public ConnectSQL_Server connectSQL_Server;
        public ConnectMySQL connectMySQL;
        public string pathUploadFileDirectory = "UploadFile";
        public IHubContext<RPTHub> jobRPTHub {  get; set; }
        private static Task? jobRPT { get; set; }
        private static string? jobRPT_NameTable { get; set; }

        public ServiceRPT(string IP, int port, string username, string password, string slaSQL, string reportMySQL , IHubContext<RPTHub> jobRPTHub) : base(IP, port, username, password)
        {
            connectSQL_Server = new ConnectSQL_Server(slaSQL);
            connectMySQL = new ConnectMySQL(reportMySQL);
            this.jobRPTHub = jobRPTHub;
            if (!Directory.Exists(pathUploadFileDirectory))
            {
                Directory.CreateDirectory(pathUploadFileDirectory);
            }

        }

        public static bool GetStatusJobRPT()
        {
            if (jobRPT == null || jobRPT.IsCompleted)
            {

                return true;
            }
            else
            {
                return false;
            }
        }
        public static string GetJobRPT_NameTable()
        {
            if (jobRPT_NameTable == null || jobRPT_NameTable == "")
            {

                return "NULL";
            }
            else
            {
                return jobRPT_NameTable;
            }
        }


        public void StartJob(string[] filesUpload,string tableName)
        {
            jobRPT = Task.Factory.StartNew(() => ReadFileToSlaDB(filesUpload, tableName), TaskCreationOptions.LongRunning);
        }





        public async Task ReadFileToSlaDB(string[] pathFileCSVs,string tableName)
        {
            await jobRPTHub.Clients.All.SendAsync("RPT_Job_Process", false, tableName);
            jobRPT_NameTable = tableName;
            DeleteTableAndCreateSlaRPT(tableName);
            Parallel.ForEach(pathFileCSVs,CSV =>{
                if (File.Exists(CSV))
                {
                    using (StreamReader file = new StreamReader(CSV))
                    {
                        int counter = 0;
                        string ln;

                        List<SlaRPTLog> itemFile = new List<SlaRPTLog>();
                        while ((ln = file.ReadLine()) != null)
                        {
                            var data = ln.Split(',');
                            try
                            {
                                itemFile.Add(new SlaRPTLog(data[0], data[1], data[2], data[3]));
                            }
                            catch (Exception ex)
                            {

                            }
                            counter++;
                            if (counter >= 1000)
                            {
                                SqlBulkCopyInsert(itemFile, tableName);
                                counter = 0;
                                itemFile.Clear();
                            }
                        }
                        if (itemFile.Count != 0)
                        {
                            SqlBulkCopyInsert(itemFile, tableName);
                            counter = 0;
                            itemFile.Clear();
                        }
                        file.Close();
                    }
                    File.Delete(CSV);
                }
            });
            await jobRPTHub.Clients.All.SendAsync("RPT_Job_Process", true, tableName);
            jobRPT_NameTable = "";
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
           
            Guid myuuid = Guid.NewGuid();
            string myuuidAsString = myuuid.ToString();
           

            string targetFileSave = Path.Combine(pathUploadFileDirectory, myuuidAsString + "_" + file.FileName);
            
            using (var stream = File.Create(targetFileSave))
            {
                await file.CopyToAsync(stream);
            }
            return targetFileSave;
        }

        private bool CheckTableSlaRPT(string tableName)
        {
            string checkTable = @"SELECT * FROM INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}'";
            SqlCommand com = new SqlCommand();
            com.CommandText = string.Format(checkTable, tableName);

            if (connectSQL_Server.GetDatatable(com).Rows.Count == 0)
            {
                return false;
                
            }
            else
            {
                return true;
            }
        }
        private void CreateTableSlaRPT(string tableName)
        {
            string newTable = @"CREATE TABLE {0}(
	[EMSID] [bigint] IDENTITY(1,1) NOT NULL,
	[TERM_ID] [nvarchar](32) NULL,
	[START_DATETIME] [datetime] NULL,
	[EVENT_NO] [nvarchar](32) NULL,
	[ID] [nvarchar](32) NULL,
	[MSG_CONTENT] [nvarchar](4000) NULL,
	[UPDATE_BY] [nvarchar](32) NULL,
	[REMARK] [nvarchar](4000) NULL,
PRIMARY KEY CLUSTERED 
(
	[EMSID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";
            connectSQL_Server.createTable(string.Format(newTable, tableName));
        }

        private void DeleteTableAndCreateSlaRPT(string tableName)
        {
            string deleteTable = @"
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}') AND type in (N'U'))
DROP TABLE {0}



SET ANSI_NULLS ON


SET QUOTED_IDENTIFIER ON


CREATE TABLE {0}(
	[EMSID] [bigint] IDENTITY(1,1) NOT NULL,
	[TERM_ID] [nvarchar](32) NULL,
	[START_DATETIME] [datetime] NULL,
	[EVENT_NO] [nvarchar](32) NULL,
	[ID] [nvarchar](32) NULL,
	[MSG_CONTENT] [nvarchar](4000) NULL,
	[UPDATE_BY] [nvarchar](32) NULL,
	[REMARK] [nvarchar](4000) NULL,
PRIMARY KEY CLUSTERED 
(
	[EMSID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]



";
            SqlCommand com = new SqlCommand();
            com.CommandText = string.Format(deleteTable, tableName);
            connectSQL_Server.deleteData(com);
        }


        private void SqlBulkCopyInsert(List<SlaRPTLog> slaEmsLog, string tableName)
        {
            try
            {
                if (connectSQL_Server.GetstateDB() == false)
                {
                    connectSQL_Server.OpenDB();
                }
                using (var copy = new SqlBulkCopy(connectSQL_Server.GetSqlConnection()))
                {
                    copy.DestinationTableName = tableName;
                    // Add mappings so that the column order doesn't matter
                    copy.ColumnMappings.Add(nameof(SlaRPTLog.EMSID), "EMSID");
                    copy.ColumnMappings.Add(nameof(SlaRPTLog.TERM_ID), "TERM_ID");
                    copy.ColumnMappings.Add(nameof(SlaRPTLog.START_DATETIME), "START_DATETIME");
                    copy.ColumnMappings.Add(nameof(SlaRPTLog.EVENT_NO), "EVENT_NO");
                    copy.ColumnMappings.Add(nameof(SlaRPTLog.ID), "ID");
                    copy.ColumnMappings.Add(nameof(SlaRPTLog.MSG_CONTENT), "MSG_CONTENT");
                    copy.ColumnMappings.Add(nameof(SlaRPTLog.UPDATE_BY), "UPDATE_BY");
                    copy.ColumnMappings.Add(nameof(SlaRPTLog.REMARK), "REMARK");
                    copy.WriteToServer(ConvertDataTableToModel.ToDataTable<SlaRPTLog>(slaEmsLog));
                    copy.Close();
                }
            }catch(Exception ex)
            {
                Log.Error("SqlBulkCopyInsert Error : "+ex.ToString());
            }
        }
    }
}
