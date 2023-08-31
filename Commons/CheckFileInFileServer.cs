using Renci.SshNet;
using SLA_Management.Models.COMLogModel;
using SLA_Management.Models.OperationModel;
using SLAManagement.Data;
using System.Data;
using System.Data.SqlClient;


namespace SLA_Management.Commons
{
    public class CheckFileInFileServer
    {
        private static ConnectSQL_Server db;
        public List<Device_info_record> termIds;


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


       

        public CheckFileInFileServer(string ip, int port, string username, string password, string partLinuxUploadFile, string sqlConnection)
        {
            Ip = ip;
            Port = port;
            Username = username;
            Password = password;
            PartLinuxUploadFile = partLinuxUploadFile;
            SqlConnection = sqlConnection;
            
            db = new ConnectSQL_Server(SqlConnection);
            termIds = GetDeviceInfoRecord();
        }



        private class ListDataComlogError
        {
            private string trem_id;
            private string comLogDate;
            private int total_record;
            private string serialNo;
            private string terminalName;

            public ListDataComlogError(string trem_id, string comLogDate, int total_record,string serialNo,string terminalName)
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

            List<ListDataComlogError> lostFiles = GetComlogFileServerLost(TERM_ID, start, DateStartBetweenDateEnd);
            List<ListDataComlogError> dataZeroRow = GetComLogRecodeZero(TERM_ID, start, DateStartBetweenDateEnd);
            List<ListDataComlogError> dataMiss = GetComLogFVMissing(start, DateStartBetweenDateEnd);

            foreach (Device_info_record termId in termIds)
            {
                IList<ListDataComlogError> zeroRow = dataZeroRow.Where(e => e.Trem_id == termId.TERM_ID).ToList();

                IList<ListDataComlogError> lost = lostFiles.Where(e => e.Trem_id == termId.TERM_ID).ToList();

                IList<ListDataComlogError> miss = dataMiss.Where(e => e.Trem_id == termId.TERM_ID).ToList();
                if (miss.Count != 0 || lost.Count != 0 || zeroRow.Count != 0)
                {
                    foreach (ListDataComlogError data in lost)
                    {
                        //InsertListFileComLog dataListFileError = new InsertListFileComLog();
                        string Term_ID = data.Trem_id;
                        string SerialNo = data.SerialNo;
                        string TerminalName = data.TerminalName;
                        string ComLog = data.ComLogDate;
                        string FileServer = "";
                        bool StatusFile = false; 
                        string TOTAL_RECORD = "-";



                        if (miss.FirstOrDefault(e => e.Trem_id == data.Trem_id && e.ComLogDate == data.ComLogDate) == null)
                        {

                            FileServer = "Not Exist";
                            

                        }
                       
                        InsertListFileComLog dataListFileError = new InsertListFileComLog(Term_ID, SerialNo, TerminalName, ComLog,FileServer, StatusFile, TOTAL_RECORD);
                        listLogError.Add(dataListFileError);
                    }

                    foreach (ListDataComlogError data in zeroRow)
                    {
                        if (listLogError.FirstOrDefault(e => e.ComLog == data.ComLogDate) == null)
                        {
                            //InsertListFileComLog dataListFileError = new InsertListFileComLog();
                            string Term_ID = data.Trem_id;
                            string SerialNo = data.SerialNo;
                            string TerminalName = data.TerminalName;
                            string ComLog = data.ComLogDate;
                            string FileServer = "ComLog have on File Server";
                            bool StatusFile = true;
                            string TOTAL_RECORD = "0";
                            InsertListFileComLog dataListFileError = new InsertListFileComLog(Term_ID, SerialNo, TerminalName, ComLog, FileServer, StatusFile, TOTAL_RECORD);
                            listLogError.Add(dataListFileError);
                        }
                       /* else
                        {
                            for (int i = 0; i < listLogError.Count; i++)
                            {
                                if (listLogError[i].Term_ID == data.Trem_id && listLogError[i].ComLog == data.ComLogDate)
                                {
                                    listLogError[i].
                                    listLogError[i].TOTAL_RECORD = "0";
                                    break;
                                }

                            }

                        }*/

                    }
                    foreach (ListDataComlogError data in miss)
                    {


                        /*int check = 0;
                        for (int i = 0; i < listLogError.Count; i++)
                        {
                            if (listLogError[i].Term_ID == data.Trem_id && listLogError[i].ComLog == data.ComLogDate)
                            {
                                check++;
                                break;
                            }

                        }*/
                       


                        var datatest  = listLogError.Where(q => q.ComLog == data.ComLogDate && q.Term_ID == data.Trem_id).FirstOrDefault();
                        
                        var fvInsert = miss.FirstOrDefault(e => e.Trem_id == data.Trem_id && e.ComLogDate == data.ComLogDate);



                        string Term_ID = data.Trem_id;
                        string SerialNo = data.SerialNo;
                        string TerminalName = data.TerminalName;
                        string ComLog = data.ComLogDate;
                        string FileServer = "";
                        bool StatusFile = false;
                        string TOTAL_RECORD = "-";
                       


                        if (datatest == null)
                        {
                            
                            InsertListFileComLog dataListFileError = new InsertListFileComLog(Term_ID, SerialNo, TerminalName, ComLog, FileServer, StatusFile, TOTAL_RECORD);

                            listLogError.Add(dataListFileError);
                        }
                        else
                        {
                            if (fvInsert != null)
                            {
                                listLogError.Remove(datatest);
                                FileServer = "Insert From FV Log";
                                TOTAL_RECORD = fvInsert.Total_record.ToString();
                                datatest.TOTAL_RECORD = TOTAL_RECORD;
                                datatest.FileServer = FileServer;
                                listLogError.Add(datatest);





                            }
                            
                           
                        }

                       


                       /* if (check == 0)
                        {
                            InsertListFileComLog dataListFileError = new InsertListFileComLog();

                            dataListFileError.Term_ID = data.Trem_id;
                            dataListFileError.ComLog = data.ComLogDate;

                            dataListFileError.FileServer = "FV log";
                            dataListFileError.TOTAL_RECORD = data.Total_record.ToString();


                            listLogError.Add(dataListFileError);
                        }*/
                    }

                }

            }

            return listLogError;
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
                        if (termID == "")
                        {
                            foreach (Device_info_record data in termIds)
                            {
                                if (!sftp.Exists(configPath + data + "/" + comLogName))
                                {
                                    listData.Add(new ListDataComlogError(data.TERM_ID, comLogName, 0, data.TERM_SEQ,data.TERM_NAME));
                                }
                            }
                        }
                        else
                        {
                            if (!sftp.Exists(configPath + termID + "/" + comLogName))
                            {
                                
                                if (device_info_record == null)
                                {
                                    listData.Add(new ListDataComlogError(termID, comLogName, 0, device_info_record.TERM_SEQ, device_info_record.TERM_NAME));

                                }
                                else
                                {
                                    listData.Add(new ListDataComlogError(termID, comLogName, 0, "-", "-"));
                                }
                                
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



        private static List<Device_info_record> GetDeviceInfoRecord()
        {
          
            SqlCommand com = new SqlCommand();
            com.CommandText = "SELECT * FROM [device_info_record] GROUP BY TERM_ID;";
            DataTable testss = db.GetDatatable(com);

            List<Device_info_record> test = ConvertDataTableToModel.ConvertDataTable<Device_info_record>(testss);
            
            /*foreach (DataRow item in testss.Rows)
            {
                test.AddFirst((string)item["TERM_ID"]);
            }*/
            return test;
        }


        private static List<ListDataComlogError> GetComLogFVMissing(DateTime start, int DateStartBetweenDateEnd)
        {

            List<ListDataComlogError> data = new List<ListDataComlogError>();
            //foreach (string YYYYMM in listDateString)
            for (int itemDate = 0; itemDate < DateStartBetweenDateEnd; itemDate++)
            {
                DateTime date = start.AddDays(itemDate);
                string YYYYMMDD = date.Year + date.Month.ToString("00") + date.Day.ToString("00");
                SqlCommand com = new SqlCommand();
                com.CommandText = "SELECT a.TERM_ID,a.MSG_SOURCE,a.TOTAL_RECORD ,b.TERM_NAME,b.TERM_SEQ FROM [dbo].[comlog_record] a\r\nLEFT JOIN [dbo].[device_info_record] b\r\nON a.TERM_ID = b.TERM_ID where REMARK = 'FVMISSING' and MSG_SOURCE like '%COM" + YYYYMMDD + "%';";
                DataTable testss = db.GetDatatable(com);

                foreach (DataRow item in testss.Rows)
                {
                    string TERM_ID = (string)item["TERM_ID"];
                    string MSG_SOURCE = ((string)item["MSG_SOURCE"]).Split('_')[1];
                    int TOTAL_RECORD = (int)item["TOTAL_RECORD"];
                    string serialNo = (string)item["TERM_SEQ"];
                    string terminalName = (string)item["TERM_NAME"]; 

                    data.Add(new ListDataComlogError(TERM_ID, MSG_SOURCE, TOTAL_RECORD, serialNo, terminalName));
                }
            }

            return data;
        }
        private List<ListDataComlogError> GetComLogRecodeZero(string termId, DateTime start, int DateStartBetweenDateEnd)
        {

            List<ListDataComlogError> data = new List<ListDataComlogError>();
            
            //foreach (string YYYYMM in listDateString)
            for (int itemDate = 0; itemDate < DateStartBetweenDateEnd; itemDate++)
            {
                DateTime date = start.AddDays(itemDate);
                string YYYYMMDD = date.Year + date.Month.ToString("00") + date.Day.ToString("00");
                SqlCommand com = new SqlCommand();
                if (termId == "")
                {
                    com.CommandText = "SELECT a.TERM_ID,a.MSG_SOURCE,a.TOTAL_RECORD ,b.TERM_NAME,b.TERM_SEQ FROM [dbo].[comlog_record] a\r\nLEFT JOIN [dbo].[device_info_record] b\r\nON a.TERM_ID = b.TERM_ID where a.TOTAL_RECORD = 0 and a.MSG_SOURCE like '%COM202301%'and a.ERROR is null;";

                }
                else
                {
                    com.CommandText = "SELECT a.TERM_ID,a.MSG_SOURCE,a.TOTAL_RECORD ,b.TERM_NAME,b.TERM_SEQ FROM [dbo].[comlog_record] a\r\nLEFT JOIN [dbo].[device_info_record] b\r\nON a.TERM_ID = b.TERM_ID where a.[TERM_ID] = @TERM_ID and a.TOTAL_RECORD = 0 and a.MSG_SOURCE like '%COM202301%'and a.ERROR is null";
                    com.Parameters.AddWithValue("@TERM_ID", termId);
                }

                List<ListDataComlogError> listDataComlogError = ConvertDataTableToModel.ConvertDataTable<ListDataComlogError>(db.GetDatatable(com));
                //IList<Comlog_record> testss = Comlog_record.mapToList(db.GetDatatable(com));
                data.AddRange(listDataComlogError);



                /*foreach (ListDataComlogError item in listDataComlogError)
                {
                    string TERM_ID = item.Term_id;
                    string MSG_SOURCE = item.Msg_source.Split('_')[1];
                    int TOTAL_RECORD = item.Total_record;
                    //Device_info_record device_info_record = termIds.Where(q => q.TERM_ID == item.Term_id).FirstOrDefault();
                    if (device_info_record != null)
                    {
                        data.AddFirst(new ListDataComlogError(TERM_ID, MSG_SOURCE, TOTAL_RECORD, device_info_record.TERM_SEQ, device_info_record.TERM_NAME));
                    }
                    else
                    {
                        data.AddFirst(new ListDataComlogError(TERM_ID, MSG_SOURCE, TOTAL_RECORD,"-", "-"));
                    }
                    
                }*/
            }

            return data;
        }

    }
}
