﻿using MySql.Data.MySqlClient;
using Renci.SshNet;
using Serilog;
using SLA_Management.Data;
using SLA_Management.Models.COMLogModel;
using SLA_Management.Models.OperationModel;
using SLA_Management.Data;
using System.Data;
using System.Data.SqlClient;

namespace SLA_Management.Commons
{
    public class CheckFileInFileServerNew
    {
        private  ConnectSQL_Server slaDB;

        private  ConnectMySQL reportDB;


        public List<Device_info_record> termIds;


        private string ip;
        private int port;
        private string username;
        private string password;

        private string partLinuxUploadFile;


        private string sqlConnectionSlaDB;
        private string mySqlConnectionReportDB;


        public string Ip { get => ip; set => ip = value; }
        public int Port { get => port; set => port = value; }
        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        public string PartLinuxUploadFile { get => partLinuxUploadFile; set => partLinuxUploadFile = value; }
        public string SqlConnectionSlaDB { get => sqlConnectionSlaDB; set => sqlConnectionSlaDB = value; }
        public string MySqlConnectionReportDB { get => mySqlConnectionReportDB; set => mySqlConnectionReportDB = value; }

        public CheckFileInFileServerNew(string ip, int port, string username, string password, string partLinuxUploadFile, string sqlConnectionSlaDB , string mySqlConnectionReportDB)
        {
            Ip = ip;
            Port = port;
            Username = username;
            Password = password;
            PartLinuxUploadFile = partLinuxUploadFile;
            SqlConnectionSlaDB = sqlConnectionSlaDB;
            MySqlConnectionReportDB = mySqlConnectionReportDB;

            slaDB = new ConnectSQL_Server(sqlConnectionSlaDB);
            reportDB = new ConnectMySQL(mySqlConnectionReportDB);
            //termIds = GetDeviceInfoRecord();
            termIds = GetFVDeviceInfo();
        }



        private class ListDataComlogError
        {
            private string trem_id;
            private string comLogDate;
            private int total_record;
            private string serialNo;
            private string terminalName;

            public ListDataComlogError(string trem_id, string comLogDate, int total_record, string serialNo, string terminalName)
            {
                this.Trem_id = trem_id;
                this.ComLogDate = comLogDate;
                this.Total_record = total_record;
                this.serialNo = serialNo;
                this.terminalName = terminalName;

            }

            public string Trem_id { get => trem_id; set => trem_id = value; }
            public string ComLogDate { get => comLogDate; set => comLogDate = value; }
            public int Total_record { get => total_record; set => total_record = value; }
            public string SerialNo { get => serialNo; set => serialNo = value; }
            public string TerminalName { get => terminalName; set => terminalName = value; }
        }

        public List<InsertListFileComLog> GetListFileComLog(string TERM_ID, DateTime start, DateTime end)
        {
            List<InsertListFileComLog> listLogError = new List<InsertListFileComLog>();

            int DateStartBetweenDateEnd = ((end - start).Days) + 1;
            bool getLostFilesFun = GetStatusFileServer();
            
            List <ListDataComlogError> lostFiles = new List<ListDataComlogError>();
            List<string> termidList = termIds.Select(q => q.TERM_ID).ToList();
            if (getLostFilesFun)
            {
                lostFiles = GetComlogFileServerLost(TERM_ID, start, DateStartBetweenDateEnd).Where(e => termidList.Contains(e.Trem_id)).ToList();
            }

            //var test = termIds.Select(q => q.TERM_ID);

            var lostFilesDB = GetComlogError(TERM_ID, start, end).Where(e => termidList.Contains(e.TERM_ID)).ToList();




            if (lostFiles.Count != 0 || lostFilesDB.Count() != 0)
            {
                if (lostFiles.Count != 0)
                {
                    listLogError.AddRange(lostFiles.Select(q => new InsertListFileComLog() { Id = 0, Term_ID = q.Trem_id, SerialNo = q.SerialNo,TerminalName = q.TerminalName,ComLog = q.ComLogDate, FileServer= "FileServer not Exist", StatusFile = false, TOTAL_RECORD = "-" }));
                }
                if (lostFilesDB.Count() != 0)
                {
                    foreach (var item in lostFilesDB)
                    {
                        //listLogError.Where(itemError => itemError.Term_ID == item.TERM_ID && itemError.ComLog == item.MSG_SOURCE.Split('_')[1]).ToList().ForEach(newError => newError.FileServer += test(item));
                        //var aa = listLogError.Where(itemError => itemError.Term_ID == item.TERM_ID && itemError.ComLog == item.MSG_SOURCE.Split('_')[1]).ToList().ForEach(newError => newError.FileServer += test(item));
                        //listLogError.Where(itemError => itemError.Term_ID == item.TERM_ID && itemError.ComLog == item.MSG_SOURCE.Split('_')[1]).ToList()?.ForEach(newError => newError.FileServer += Investigate(item));
                        var listLogErrorContean = listLogError.Where(itemError => itemError.Term_ID == item.TERM_ID && itemError.ComLog == item.MSG_SOURCE.Split('_')[1]).ToList();
                        var investigate = Investigate(item, lostFiles);
                        if (listLogErrorContean.Count() !=0)
                        {

                            //listLogErrorContean?.ForEach(newError => newError = investigate);
                            if (investigate != null)
                            {
                                listLogError.Remove(listLogErrorContean.First());
                                listLogError.Add(investigate);
                            }
                           

                        }
                        else
                        {
                            listLogError.Add(investigate);
                        }
                    }
                    
                }
            }



            return listLogError;
        }

        private InsertListFileComLog Investigate(Comlog_recordNew data,List<ListDataComlogError> lostFiles)
        {
            InsertListFileComLog investigate = new InsertListFileComLog();
            var device_info_record = termIds.Where(q =>q.TERM_ID == data.TERM_ID).FirstOrDefault();
            investigate.Term_ID = data.TERM_ID;
            investigate.SerialNo = device_info_record.TERM_SEQ;
            investigate.TerminalName = device_info_record.TERM_NAME;
            investigate.ComLog = data.MSG_SOURCE.Split('_')[1];
            investigate.StatusFile = false;
            investigate.TOTAL_RECORD = "-";
            if (data.ERROR == "" || data.ERROR == null)
            {
                investigate.StatusFile = true;
                if (data.TOTAL_RECORD == 0)
                {
                    investigate.FileServer = "Insert data 0 line";
                    investigate.TOTAL_RECORD = "0";
                }
                else if(data.FLAG == 0)
                {

                    investigate.FileServer = "Waiting to translate";
                    investigate.TOTAL_RECORD = data.TOTAL_RECORD+"";
                }
               
            }
            else 
            {
                if (lostFiles.Where(i => i.Trem_id == data.TERM_ID && i.ComLogDate == data.MSG_SOURCE.Split('_')[1]).Count() == 0 && data.TOTAL_RECORD == 0)
                {
                    investigate.FileServer = "Waiting to insert in database";
                    investigate.StatusFile = true;
                }
                else
                {
                    return null;
                }


                
            }
            return investigate;

        }


        private bool GetStatusFileServer()
        {
           
            try
            {
                using (SftpClient sftp = new SftpClient(Ip, Port, Username, Password))
                {
                    sftp.Connect();

                   
                    sftp.Dispose();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
           
        }

        private List<ListDataComlogError> GetComlogFileServerLost(string termID, DateTime start, int DateStartBetweenDateEnd)
        {
            List<ListDataComlogError> listData = new List<ListDataComlogError>();
            Device_info_record device_info_record = termIds.Where(q => q.TERM_ID == termID).FirstOrDefault();
            try
            {
                using (SftpClient sftp = new SftpClient(Ip, Port, Username, Password))
                {
                    sftp.Connect();

                    string configPath = partLinuxUploadFile;
                    /*string configPath = Path.GetFullPath(".\\UploadFolder");

                   if (!Directory.Exists(configPath))
                   {
                       Directory.CreateDirectory(configPath);
                   }*/

                    for (int i = 0; i < DateStartBetweenDateEnd; i++)
                    {
                        DateTime getDate = start.AddDays(i);
                        //Console.WriteLine(getDate.ToString());
                        string comLogName = "COM" + getDate.Year + "" + getDate.Month.ToString("00") + "" + getDate.Day.ToString("00") + ".txt";
                        if (termID == "" || termID==null)
                        {
                            foreach (Device_info_record term in termIds)
                            {
                                if (!sftp.Exists(configPath + term.TERM_ID + "/" + comLogName))
                                {
                                    listData.Add(new ListDataComlogError(term.TERM_ID, comLogName.Replace(".txt", ""), 0, term.TERM_SEQ, term.TERM_NAME));
                                }

                            }

                        }
                        else
                        {
                            if (!sftp.Exists(configPath + termID + "/" + comLogName))
                            {
                                listData.Add(new ListDataComlogError(device_info_record.TERM_ID, comLogName.Replace(".txt", ""), 0, device_info_record.TERM_SEQ, device_info_record.TERM_NAME));
                            }
                        }
                    }
                    sftp.Disconnect();
                }
            }
            catch (Exception ex)
            {

            }
            return listData;
        }
        private List<Comlog_recordNew> GetComlogError(string termID, DateTime start, DateTime end)
        {
            
            SqlCommand com = new SqlCommand();
            if (termID =="")
            {
                com.CommandText = "SELECT [ID],[TERM_ID],[MSG_SOURCE],[UPDATE_DATE],[UPDATE_BY],[REMARK],[TOTAL_RECORD],[ERROR],[COMLOGDATE],[FLAG] FROM  [dbo].[comlog_record]where ([COMLOGDATE] between @start and @end ) and (([TOTAL_RECORD] = 0 )or ([ERROR] is not null) or ([FLAG] is null));";
                com.Parameters.AddWithValue("@start", start);
                com.Parameters.AddWithValue("@end", end);
            }
            else
            {
                com.CommandText = "SELECT [ID],[TERM_ID],[MSG_SOURCE],[UPDATE_DATE],[UPDATE_BY],[REMARK],[TOTAL_RECORD],[ERROR],[COMLOGDATE],[FLAG] FROM  [dbo].[comlog_record] where ([TERM_ID] = @TERM_ID )  and ([COMLOGDATE] between @start and @end )  and (([TOTAL_RECORD] = 0 )or ([ERROR] is not null) or ([FLAG] is null));";
                com.Parameters.AddWithValue("@TERM_ID", termID);
                com.Parameters.AddWithValue("@start", start);
                com.Parameters.AddWithValue("@end", end);
            }
            var data = ConvertDataTableToModel.ConvertDataTable<Comlog_recordNew>(slaDB.GetDatatable(com));
            //var dataConvetModel = from item in data select new ListDataComlogError(item.TERM_ID, item.MSG_SOURCE,item.TOTAL_RECORD);
            return data;
        }



        private List<Device_info_record> GetDeviceInfoRecord()
        {

            SqlCommand com = new SqlCommand();
            com.CommandText = "SELECT * FROM [device_info_record] where STATUS = 1  order by TERM_SEQ;";
            DataTable testss = slaDB.GetDatatable(com);

            List<Device_info_record> test = ConvertDataTableToModel.ConvertDataTable<Device_info_record>(testss);
            return test;
        }
        private List<Device_info_record> GetFVDeviceInfo()
        {

            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand();
                mySqlCommand.CommandText = "SELECT TERM_ID,TERM_SEQ,TERM_NAME,TYPE_ID FROM fv_device_info  where STATUS = 'use' or STATUS = 'roustop';";
                return ConvertDataTableToModel.ConvertDataTable<Device_info_record>(reportDB.GetDatatable(mySqlCommand));
            }
            catch (Exception ex)
            {
                Log.Error($"GetFVDeviceInfo Error : {ex}");
                return new List<Device_info_record>();
            }
        }


        /*private static LinkedList<ListDataComlogError> GetComLogFVMissing(DateTime start, int DateStartBetweenDateEnd)
        {

            LinkedList<ListDataComlogError> data = new LinkedList<ListDataComlogError>();
            //foreach (string YYYYMM in listDateString)
            for (int itemDate = 0; itemDate < DateStartBetweenDateEnd; itemDate++)
            {
                DateTime date = start.AddDays(itemDate);
                string YYYYMMDD = date.Year + date.Month.ToString("00") + date.Day.ToString("00");
                SqlCommand com = new SqlCommand();
                com.CommandText = "SELECT TERM_ID,MSG_SOURCE,TOTAL_RECORD FROM [dbo].[comlog_record] where REMARK = 'FVMISSING' and MSG_SOURCE like '%COM" + YYYYMMDD + "%';";
                DataTable testss = db.GetDatatable(com);

                foreach (DataRow item in testss.Rows)
                {
                    string TERM_ID = (string)item["TERM_ID"];
                    string MSG_SOURCE = ((string)item["MSG_SOURCE"]).Split('_')[1];
                    int TOTAL_RECORD = (int)item["TOTAL_RECORD"];

                    data.AddFirst(new ListDataComlogError(TERM_ID, MSG_SOURCE, TOTAL_RECORD));
                }
            }

            return data;
        }
        private LinkedList<ListDataComlogError> GetComLogRecodeZero(string termId, DateTime start, int DateStartBetweenDateEnd)
        {

            LinkedList<ListDataComlogError> data = new LinkedList<ListDataComlogError>();
            //foreach (string YYYYMM in listDateString)
            for (int itemDate = 0; itemDate < DateStartBetweenDateEnd; itemDate++)
            {
                DateTime date = start.AddDays(itemDate);
                string YYYYMMDD = date.Year + date.Month.ToString("00") + date.Day.ToString("00");
                SqlCommand com = new SqlCommand();
                if (termId == "")
                {
                    com.CommandText = "SELECT * FROM [dbo].[comlog_record] where TOTAL_RECORD = 0 and MSG_SOURCE like '%COM" + YYYYMMDD + "%';";

                }
                else
                {
                    com.CommandText = "SELECT * FROM [dbo].[comlog_record] where [TERM_ID] = @TERM_ID and TOTAL_RECORD = 0 and MSG_SOURCE like '%COM" + YYYYMMDD + "%';";
                    com.Parameters.AddWithValue("@TERM_ID", termId);
                }

                IList<Comlog_record> testss = Comlog_record.mapToList(db.GetDatatable(com));

                foreach (Comlog_record item in testss)
                {
                    string TERM_ID = item.Term_id;
                    string MSG_SOURCE = item.Msg_source.Split('_')[1];
                    int TOTAL_RECORD = item.Total_record;

                    data.AddFirst(new ListDataComlogError(TERM_ID, MSG_SOURCE, TOTAL_RECORD));
                }
            }

            return data;
        }*/
    }
}
