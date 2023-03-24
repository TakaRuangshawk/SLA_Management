using Renci.SshNet;
using SLA_Management.Models;
using SLAManagement.Data;
using System.Data;
using System.Data.SqlClient;


namespace SLA_Management.Commons
{
    public class CheckFileInFileServer
    {
        private static ConnectSQL_Server db;
        public LinkedList<string> termIds = GetTermId();


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


        public CheckFileInFileServer()
        {

        }

        public CheckFileInFileServer(string ip, int port, string username, string password, string partLinuxUploadFile, string sqlConnection)
        {
            Ip = ip;
            Port = port;
            Username = username;
            Password = password;
            PartLinuxUploadFile = partLinuxUploadFile;
            SqlConnection = sqlConnection;
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

            LinkedList<ListDataComlogError> lostFiles = GetComlogFileServerLost(TERM_ID, start, DateStartBetweenDateEnd);
            LinkedList<ListDataComlogError> dataZeroRow = GetComLogRecodeZero(TERM_ID, start, DateStartBetweenDateEnd);
            LinkedList<ListDataComlogError> dataMiss = GetComLogFVMissing(start, DateStartBetweenDateEnd);

            foreach (string termId in termIds)
            {
                IList<ListDataComlogError> datas = dataZeroRow.Where(e => e.Trem_id == termId).ToList();

                IList<ListDataComlogError> datass = lostFiles.Where(e => e.Trem_id == termId).ToList();

                IList<ListDataComlogError> datasss = dataMiss.Where(e => e.Trem_id == termId).ToList();
                if (datasss.Count != 0 || datass.Count != 0 || datas.Count != 0)
                {
                    foreach (ListDataComlogError data in datass)
                    {
                        InsertListFileComLog dataListFileError = new InsertListFileComLog();
                        dataListFileError.TERM_ID = data.Trem_id;
                        dataListFileError.ComLog = data.ComLogDate;



                        if (datasss.FirstOrDefault(e => e.Trem_id == data.Trem_id && e.ComLogDate == data.ComLogDate) == null)
                        {

                            dataListFileError.FileServer = "Not Exist";
                            dataListFileError.TOTAL_RECORD = "-";

                        }
                        else
                        {
                            dataListFileError.FileServer = "FV log";
                            foreach (ListDataComlogError fv in datasss)
                            {
                                if (fv.Trem_id == data.Trem_id && fv.ComLogDate == data.ComLogDate)
                                {
                                    dataListFileError.TOTAL_RECORD = fv.Total_record.ToString();
                                    break;
                                }
                            }

                        }

                        listLogError.Add(dataListFileError);
                    }

                    foreach (ListDataComlogError data in datas)
                    {
                        if (datass.FirstOrDefault(e => e.ComLogDate == data.ComLogDate) == null)
                        {
                            InsertListFileComLog dataListFileError = new InsertListFileComLog();
                            dataListFileError.TERM_ID = data.Trem_id;
                            dataListFileError.ComLog = data.ComLogDate;
                            dataListFileError.FileServer = "ComLog have on File Server";
                            dataListFileError.TOTAL_RECORD = "0";
                            listLogError.Add(dataListFileError);
                        }
                        else
                        {
                            for (int i = 0; i < listLogError.Count; i++)
                            {
                                if (listLogError[i].TERM_ID == data.Trem_id && listLogError[i].ComLog == data.ComLogDate)
                                {
                                    listLogError[i].TOTAL_RECORD = "0";
                                    break;
                                }

                            }

                        }

                    }
                    foreach (ListDataComlogError data in datasss)
                    {
                        int check = 0;
                        for (int i = 0; i < listLogError.Count; i++)
                        {
                            if (listLogError[i].TERM_ID == data.Trem_id && listLogError[i].ComLog == data.ComLogDate)
                            {
                                check++;
                                break;
                            }

                        }
                        if (check == 0)
                        {
                            InsertListFileComLog dataListFileError = new InsertListFileComLog();

                            dataListFileError.TERM_ID = data.Trem_id;
                            dataListFileError.ComLog = data.ComLogDate;

                            dataListFileError.FileServer = "FV log";
                            dataListFileError.TOTAL_RECORD = data.Total_record.ToString();


                            listLogError.Add(dataListFileError);
                        }
                    }

                }

            }

            return listLogError;
        }


        private LinkedList<ListDataComlogError> GetComlogFileServerLost(string termID, DateTime start, int DateStartBetweenDateEnd)
        {
            LinkedList<ListDataComlogError> listData = new LinkedList<ListDataComlogError>();
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
                            foreach (string FV_TERM_ID in termIds)
                            {
                                if (!sftp.Exists(configPath + FV_TERM_ID + "/" + comLogName))
                                {
                                    listData.AddFirst(new ListDataComlogError(FV_TERM_ID, comLogName, 0));
                                }

                            }

                        }
                        else
                        {
                            if (!sftp.Exists(configPath + termID + "/" + comLogName))
                            {
                                listData.AddFirst(new ListDataComlogError(termID, comLogName, 0));
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



        private static LinkedList<string> GetTermId()
        {
            db = new ConnectSQL_Server(SqlConnection);
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


        private static LinkedList<ListDataComlogError> GetComLogFVMissing(DateTime start, int DateStartBetweenDateEnd)
        {
            //LinkedList<string> listDateString = checkYearAndMount(start, DateStartBetweenDateEnd);
            db = new ConnectSQL_Server(SqlConnection);
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
            //LinkedList<string> listDateString = checkYearAndMount(start, DateStartBetweenDateEnd);
            db = new ConnectSQL_Server(SqlConnection);
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
        }

    }
}
