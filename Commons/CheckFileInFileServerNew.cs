using Renci.SshNet;
using SLA_Management.Models;
using SLAManagement.Data;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SLA_Management.Commons
{
    public class CheckFileInFileServerNew
    {
        private static ConnectSQL_Server db;
        public LinkedList<string> termIds;


        private static string ip;
        private static int port;
        private static string username;
        private static string password;

        private static string partLinuxUploadFile;
        private static string sqlConnection;


        public static string Ip { get => ip; set => ip = value; }
        public static int Port { get => port; set => port = value; }
        public static string Username { get => username; set => username = value; }
        public static string Password { get => password; set => password = value; }
        public static string PartLinuxUploadFile { get => partLinuxUploadFile; set => partLinuxUploadFile = value; }
        public static string SqlConnection { get => sqlConnection; set => sqlConnection = value; }




        public CheckFileInFileServerNew(string ip, int port, string username, string password, string partLinuxUploadFile, string sqlConnection)
        {
            Ip = ip;
            Port = port;
            Username = username;
            Password = password;
            PartLinuxUploadFile = partLinuxUploadFile;
            SqlConnection = sqlConnection;

            db = new ConnectSQL_Server(SqlConnection);
            termIds = GetTermId();
        }



        private class ListDataComlogError
        {
            private string trem_id;
            private string comLogDate;
            private int total_record;

            public ListDataComlogError(string trem_id, string comLogDate, int total_record)
            {
                this.Trem_id = trem_id;
                this.ComLogDate = comLogDate;
                this.Total_record = total_record;

            }

            public string Trem_id { get => trem_id; set => trem_id = value; }
            public string ComLogDate { get => comLogDate; set => comLogDate = value; }
            public int Total_record { get => total_record; set => total_record = value; }
        }

        public List<InsertListFileComLog> GetListFileComLog(string TERM_ID, DateTime start, DateTime end)
        {
            List<InsertListFileComLog> listLogError = new List<InsertListFileComLog>();

            int DateStartBetweenDateEnd = ((end - start).Days) + 1;
            bool getLostFilesFun = GetStatusFileServer();
            
            List <ListDataComlogError> lostFiles = new List<ListDataComlogError>();
            if (getLostFilesFun)
            {
                lostFiles = GetComlogFileServerLost(TERM_ID, start, DateStartBetweenDateEnd).Where(e => termIds.Contains(e.Trem_id)).ToList();
            }
           

            var lostFilesDB = GetComlogError(TERM_ID, start, end).Where(e => termIds.Contains(e.TERM_ID)).ToList();




            if (lostFiles.Count != 0 || lostFilesDB.Count != 0)
            {
                if (lostFiles.Count != 0)
                {
                    listLogError.AddRange(lostFiles.Select(q => new InsertListFileComLog() { Id = 0, Term_ID = q.Trem_id, ComLog = q.ComLogDate, FileServer= "FileServer not Exist", TOTAL_RECORD = "-" }));
                }
                if (lostFilesDB.Count != 0)
                {
                    foreach (var item in lostFilesDB)
                    {
                        //listLogError.Where(itemError => itemError.Term_ID == item.TERM_ID && itemError.ComLog == item.MSG_SOURCE.Split('_')[1]).ToList().ForEach(newError => newError.FileServer += test(item));
                        //var aa = listLogError.Where(itemError => itemError.Term_ID == item.TERM_ID && itemError.ComLog == item.MSG_SOURCE.Split('_')[1]).ToList().ForEach(newError => newError.FileServer += test(item));
                        //listLogError.Where(itemError => itemError.Term_ID == item.TERM_ID && itemError.ComLog == item.MSG_SOURCE.Split('_')[1]).ToList()?.ForEach(newError => newError.FileServer += Investigate(item));
                        if (listLogError.Where(itemError => itemError.Term_ID == item.TERM_ID && itemError.ComLog == item.MSG_SOURCE.Split('_')[1]).Count() !=0)
                        {
                            listLogError.Where(itemError => itemError.Term_ID == item.TERM_ID && itemError.ComLog == item.MSG_SOURCE.Split('_')[1]).ToList()?.ForEach(newError => newError.FileServer += Investigate(item, lostFiles, getLostFilesFun));

                        }
                        else
                        {
                            listLogError.Add(new InsertListFileComLog() { Term_ID = item.TERM_ID, ComLog = item.MSG_SOURCE.Split('_')[1], FileServer = Investigate(item, lostFiles, getLostFilesFun),TOTAL_RECORD = item.TOTAL_RECORD+"" });
                        }
                    }
                    
                }
            }



            return listLogError;
        }

        private string Investigate(Comlog_recordNew data,List<ListDataComlogError> lostFiles ,bool getLostFilesFun)
        {
            string investigate = "";
            if (data.ERROR == "" || data.ERROR == null)
            {
                if (data.TOTAL_RECORD == 0)
                {
                    investigate += "\nInsert data 0 line";
                }
                else if(data.FLAG == 0)
                {
                   
                    investigate += "\nWaiting to translate";
                }
            }
            else 
            {
                if (getLostFilesFun == true)
                {
                    if (lostFiles.Where(i => i.Trem_id == data.TERM_ID && i.ComLogDate == data.MSG_SOURCE.Split('_')[1]).Count() == 0)
                    {
                        investigate += "\nWaiting to insert in database";
                    }
                }
                else
                {
                    if ("File Not Found".Equals(data.ERROR))
                    {
                        investigate += "\nFileServer not Exist";
                    }
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
                        if (termID == "")
                        {
                            foreach (string termId in termIds)
                            {
                                if (!sftp.Exists(configPath + termId + "/" + comLogName))
                                {
                                    listData.Add(new ListDataComlogError(termId, comLogName.Replace(".txt", ""), 0));
                                }

                            }

                        }
                        else
                        {
                            if (!sftp.Exists(configPath + termID + "/" + comLogName))
                            {
                                listData.Add(new ListDataComlogError(termID, comLogName.Replace(".txt", ""), 0));
                            }
                        }
                    }
                    sftp.Dispose();
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
            var data = ConvertDataTableToModel.ConvertDataTable<Comlog_recordNew>(db.GetDatatable(com));
            //var dataConvetModel = from item in data select new ListDataComlogError(item.TERM_ID, item.MSG_SOURCE,item.TOTAL_RECORD);
            return data;
        }



        private static LinkedList<string> GetTermId()
        {

            SqlCommand com = new SqlCommand();
            com.CommandText = "SELECT TERM_ID FROM [SLADB].[dbo].[device_info_record] GROUP BY TERM_ID;";
            DataTable testss = db.GetDatatable(com);
            LinkedList<string> test = new LinkedList<string>();
            foreach (DataRow item in testss.Rows)
            {
                test.AddFirst((string)item["TERM_ID"]);
            }
            return test;
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
