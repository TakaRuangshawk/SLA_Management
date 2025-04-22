using K4os.Compression.LZ4.Internal;
using MySql.Data.MySqlClient;
using SLA_Management.Commons;
using SLA_Management.Models;
using SLA_Management.Models.LogMonitorModel;
using SLA_Management.Models.OperationModel;
using SLA_Management.Models.TermProbModel;
using System.Data;

namespace SLA_Management.Data.TermProb
{
    public class DBService_TermProb : DBService
    {

        #region Constructor
        public DBService_TermProb(IConfiguration myConfiguration) : base(myConfiguration)
        {
            ConnectString_MySQL = myConfiguration;

            FullNameConnection = ConnectString_MySQL.GetValue<string>("ConnectString_MySQL:FullNameConnection");

            _objDb = new ConnectMySQL(FullNameConnection);
        }

        public DBService_TermProb(IConfiguration myConfiguration, string fullNameConnection) : base(myConfiguration)
        {
            ConnectString_MySQL = myConfiguration;

            FullNameConnection = fullNameConnection;
            //ConnectString_MySQL.GetValue<string>("ConnectString_MySQL:FullNameConnection");

            _objDb = new ConnectMySQL(FullNameConnection);
        }

        // เพิ่มฟังก์ชัน GetAllCountLogMonitoringFail ที่รับ terminalID
        public  (int ejLogFalseCount, int eCatLogFalseCount, int comLogFalseCount, int imageLogFalseCount) GetAllCountLogMonitoringFail(string terminalID)
        {
            // สร้างการเชื่อมต่อกับฐานข้อมูล
            using (MySqlConnection conn = new MySqlConnection(FullNameConnection))
            {
                // สร้างคำสั่ง SQL ที่มีเงื่อนไข terminal_id
                string query = @"
                SELECT 
                    COUNT(CASE WHEN `ejLog` = 'false' THEN 1 END) AS `ejLog_false_count`,
                    COUNT(CASE WHEN `eCatLog` = 'false' THEN 1 END) AS `eCatLog_false_count`,
                    COUNT(CASE WHEN `comLog` = 'false' THEN 1 END) AS `comLog_false_count`,
                    COUNT(CASE WHEN `imageLog` = 'false' THEN 1 END) AS `imageLog_false_count`
                FROM `gsb_logview`.`filelog_monitoring_report`
                WHERE `date` BETWEEN DATE_SUB(CURDATE(), INTERVAL 1 YEAR) AND CURDATE()
                AND `terminal_id` = @terminalID;
            ";

                // เปิดการเชื่อมต่อกับฐานข้อมูล
                conn.Open();

                // สร้างคำสั่ง SQL
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    // กำหนดค่า @terminalID เป็นพารามิเตอร์ใน SQL query
                    cmd.Parameters.AddWithValue("@terminalID", terminalID);

                    // รันคำสั่งและดึงข้อมูลจากฐานข้อมูล
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // ดึงข้อมูลจากฐานข้อมูลและเก็บไว้ในตัวแปร
                            return (
                                reader.GetInt32("ejLog_false_count"),
                                reader.GetInt32("eCatLog_false_count"),
                                reader.GetInt32("comLog_false_count"),
                                reader.GetInt32("imageLog_false_count")
                            );
                        }
                        else
                        {
                            // ถ้าไม่มีข้อมูล
                            return (0, 0, 0, 0);
                        }
                    }
                }
            }
        }

        #endregion

        #region Public Functions     

        public class checkuserfeelview
        {
            public string check { get; set; }
        }
        public bool InsertDataToProbMaster(string probCode, string probName, string probType, string probTerm, string memo, string username, string displayflag)
        {
            bool result = false;

            string _sql = string.Empty;

            try
            {
                _sql = "INSERT INTO `gsb_logview`.`ejlog_problemmascode` (`probcode`,`probname`,`probtype`,`probterm`,`status`,`displayflag`,`memo`,`createdate`,`updatedate`,`updateby`)VALUE ( '" + probCode + "','" + probName + "','" + probType + "','" + probTerm + "','" + "1" + "','" + displayflag + "','" + memo + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + username + "') ;";


                result = _objDb.ExecuteQueryNoneParam(_sql);

                if (result == false)
                {
                    if (_objDb.ErrorMessDB != null)
                        ErrorMessage = _objDb.ErrorMessDB;
                }


                
            }
            catch (Exception ex)
            { throw ex; }
            return result;
        }

        public List<fileLog_monitoring_report> GetFileLogMonitoringReportsFail(string startDate, string endDate, string id)
        {
            List<fileLog_monitoring_report> reports = new List<fileLog_monitoring_report>();

            // SQL query to fetch data within the last 1 year and check for false values
            string query = @"
    SELECT 
        `id`, 
        `terminal_id`, 
        `date`, 
        `ejLog`, 
        `eCatLog`, 
        `comLog`, 
        `imageLog`, 
        `status`, 
        `fileLog_monitoring_cycle_id`
    FROM `gsb_logview`.`filelog_monitoring_report`
    WHERE `date` BETWEEN DATE_SUB(CURDATE(), INTERVAL 1 YEAR) AND CURDATE()
    AND (
        `ejLog` = 'false' OR
        `eCatLog` = 'false' OR
        `comLog` = 'false' OR
        `imageLog` = 'false'
    )
    ORDER BY `date` DESC;
    ";

            try
            {
                var data = _objDb.GetDatatableNotParam(query);

                if (data != null && data.Rows.Count > 0)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        fileLog_monitoring_report report = new fileLog_monitoring_report
                        {
                            id = Convert.ToInt32(row["id"]),
                            terminal_id = row["terminal_id"].ToString(),
                            date = Convert.ToDateTime(row["date"]),
                            ejLog = row["ejLog"].ToString(),
                            eCatLog = row["eCatLog"].ToString(),
                            comLog = row["comLog"].ToString(),
                            imageLog = row["imageLog"].ToString(),
                            status = row["status"].ToString(),
                            fileLog_monitoring_cycle_id = row["fileLog_monitoring_cycle_id"].ToString()
                        };

                        reports.Add(report);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return reports;
        }

        public List<fileLog_monitoring_report> GetFileLogMonitoringCycleIdsByStatusFalseAndCycleId(string fileLogMonitoringCycleId)
        {
            List<fileLog_monitoring_report> reports = new List<fileLog_monitoring_report>();

            string query = @"
        SELECT  terminal_id, date, ejLog, eCatLog, comLog, imageLog, status, fileLog_monitoring_cycle_id
        FROM gsb_logview.filelog_monitoring_report
        WHERE status = 'Not completed' AND fileLog_monitoring_cycle_id = @fileLog_monitoring_cycle_id;
    ";

            try
            {
                using (var connection = new MySqlConnection(FullNameConnection))
                {
                    connection.Open();

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@fileLog_monitoring_cycle_id", fileLogMonitoringCycleId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var report = new fileLog_monitoring_report
                                {                                  
                                    terminal_id = reader.IsDBNull(reader.GetOrdinal("terminal_id")) ? null : reader.GetString("terminal_id"),
                                    date = reader.IsDBNull(reader.GetOrdinal("date")) ? (DateTime?)null : reader.GetDateTime("date"),
                                    ejLog = reader.IsDBNull(reader.GetOrdinal("ejLog")) ? null : reader.GetString("ejLog"),
                                    eCatLog = reader.IsDBNull(reader.GetOrdinal("eCatLog")) ? null : reader.GetString("eCatLog"),
                                    comLog = reader.IsDBNull(reader.GetOrdinal("comLog")) ? null : reader.GetString("comLog"),
                                    imageLog = reader.IsDBNull(reader.GetOrdinal("imageLog")) ? null : reader.GetString("imageLog"),
                                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString("status"),
                                    fileLog_monitoring_cycle_id = reader.IsDBNull(reader.GetOrdinal("fileLog_monitoring_cycle_id")) ? null : reader.GetString("fileLog_monitoring_cycle_id")
                                };

                                reports.Add(report);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching fileLog_monitoring_report data where status is 'Not completed' and cycle_id matches", ex);
            }

            return reports;
        }



        public List<logmonitorjob> GetJobLogMonitor(string startDate, string endDate, string status)
        {
            List<logmonitorjob> logmonitorjobList = new List<logmonitorjob>();

            // เริ่มต้นสร้าง SQL query โดยไม่มีเงื่อนไข terminal
            string query = @"
        SELECT Job_ID, Job_Type, UploadDate, UploadBy, Status, CountFile, TerminalID ,CloseJobDate
        FROM gsb_logview.logmonitorjob
        WHERE UploadDate BETWEEN @startDate AND @endDate";

            // ถ้ามี terminal ให้เพิ่มเงื่อนไข WHERE TerminalID = @terminal
            if (!string.IsNullOrEmpty(status))
            {
                query += " AND Status = @status";
            }

            query += " ORDER BY UploadDate desc";

            try
            {
                using (var connection = new MySqlConnection(FullNameConnection))
                {
                    connection.Open();

                    // Create the command and add parameters
                    using (var command = new MySqlCommand(query, connection))
                    {
                        // กำหนดพารามิเตอร์สำหรับวันที่
                        command.Parameters.AddWithValue("@startDate", startDate);
                        command.Parameters.AddWithValue("@endDate", endDate);

                        // กำหนดพารามิเตอร์สำหรับ terminal ถ้ามีค่า
                        if (!string.IsNullOrEmpty(status))
                        {
                            command.Parameters.AddWithValue("@status", status);
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // สร้างวัตถุ logmonitorjob
                                logmonitorjob job = new logmonitorjob
                                {
                                    Job_ID = reader.IsDBNull(reader.GetOrdinal("Job_ID")) ? string.Empty : reader.GetString("Job_ID"),
                                    Job_Type = reader.IsDBNull(reader.GetOrdinal("Job_Type")) ? string.Empty : reader.GetString("Job_Type"),
                                    UploadDate = reader.IsDBNull(reader.GetOrdinal("UploadDate")) ? DateTime.MinValue : reader.GetDateTime("UploadDate"),
                                    UploadBy = reader.IsDBNull(reader.GetOrdinal("UploadBy")) ? string.Empty : reader.GetString("UploadBy"),
                                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? string.Empty : reader.GetString("Status"),
                                    CountFile = reader.IsDBNull(reader.GetOrdinal("CountFile")) ? 0 : reader.GetInt32("CountFile"),
                                    TerminalID = reader.IsDBNull(reader.GetOrdinal("TerminalID")) ? string.Empty : reader.GetString("TerminalID"),
                                    CloseJobDate = reader.IsDBNull(reader.GetOrdinal("CloseJobDate")) ? DateTime.MinValue : reader.GetDateTime("CloseJobDate"),

                                };

                                // เพิ่ม job ลงใน list
                                logmonitorjobList.Add(job);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching job log monitoring reports", ex);
            }

            return logmonitorjobList;
        }



        public List<fileLog_monitoring_report> GetFileLogMonitoringReports(string startDate, string endDate)
        {
            List<fileLog_monitoring_report> reports = new List<fileLog_monitoring_report>();

            // SQL query to fetch data from fileLog_monitoring_cycle first, and then link filelog_monitoring_report based on fileLog_monitoring_cycle_id
            string query = @"
SELECT 
    flmc.`Id` AS `fileLog_monitoring_cycle_id`, 
    flmc.`terminal_id`, -- terminal_id จาก fileLog_monitoring_cycle
    flmc.`date_data`, -- date_data จาก fileLog_monitoring_cycle
    flmc.`job_id`,
    COUNT(CASE WHEN flr.`ejLog` = 'true' AND flr.`date` BETWEEN DATE_SUB(flmc.`date_process`, INTERVAL 367 DAY) AND flmc.`date_process` THEN 1 END) AS ejLogTrueCount,
    COUNT(CASE WHEN flr.`eCatLog` = 'true' AND flr.`date` BETWEEN DATE_SUB(flmc.`date_process`, INTERVAL 367 DAY) AND flmc.`date_process` THEN 1 END) AS eCatLogTrueCount,
    COUNT(CASE WHEN flr.`comLog` = 'true' AND flr.`date` BETWEEN DATE_SUB(flmc.`date_process`, INTERVAL 367 DAY) AND flmc.`date_process` THEN 1 END) AS comLogTrueCount,
    COUNT(CASE WHEN flr.`imageLog` = 'true' AND flr.`date` BETWEEN DATE_SUB(flmc.`date_process`, INTERVAL 92 DAY) AND flmc.`date_process` THEN 1 END) AS imageLogTrueCount
FROM `gsb_logview`.`fileLog_monitoring_cycle` flmc
LEFT JOIN `gsb_logview`.`filelog_monitoring_report` flr
    ON flmc.`Id` = flr.`fileLog_monitoring_cycle_id` 
WHERE flmc.`date_data` BETWEEN @startDate AND @endDate
GROUP BY flmc.`Id`, flmc.`terminal_id`, flmc.`date_data` order by flmc.`date_data` desc";

            try
            {
                using (var connection = new MySqlConnection(FullNameConnection))
                {
                    connection.Open();

                    // Create the command and add parameters
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@startDate", startDate);
                        command.Parameters.AddWithValue("@endDate", endDate);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Get the count of true occurrences for each terminal_id and cycle_id
                                int ejLogTrueCount = reader.GetInt32("ejLogTrueCount");
                                int eCatLogTrueCount = reader.GetInt32("eCatLogTrueCount");
                                int comLogTrueCount = reader.GetInt32("comLogTrueCount");
                                int imageLogTrueCount = reader.GetInt32("imageLogTrueCount");

                                // Retrieve the date_data from fileLog_monitoring_cycle
                                DateTime dateData = reader.GetDateTime("date_data");

                                // Check if the counts are as expected
                                string status = "Success"; // Default status
                                if (ejLogTrueCount != 365 || eCatLogTrueCount != 365 || comLogTrueCount != 365 || imageLogTrueCount != 90)
                                {
                                    status = "Warning"; // If any count is not as expected, set status to "Warning"
                                }

                                // Display the count as "true_count / total_count"
                                string ejLogDisplay = $"{ejLogTrueCount}/365";
                                string eCatLogDisplay = $"{eCatLogTrueCount}/365";
                                string comLogDisplay = $"{comLogTrueCount}/365";
                                string imageLogDisplay = $"{imageLogTrueCount}/90";

                                // Create a report object with the count display for each terminal_id and cycle_id
                                fileLog_monitoring_report report = new fileLog_monitoring_report
                                {
                                    terminal_id = reader.GetString("terminal_id"),
                                    fileLog_monitoring_cycle_id = reader.GetString("fileLog_monitoring_cycle_id"), // Make sure to convert to string if needed
                                    ejLog = ejLogDisplay,
                                    eCatLog = eCatLogDisplay,
                                    comLog = comLogDisplay,
                                    imageLog = imageLogDisplay,
                                    date = dateData, // Use the date from fileLog_monitoring_report as date_data
                                    status = status, // Add the status to the report
                                    job_id = reader.IsDBNull("job_id") ? string.Empty : reader.GetString("job_id")
                                };

                                reports.Add(report);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching file log monitoring reports", ex);
            }

            return reports;
        }







        public bool InsertDataToJobEJ(string jobId ,string uploadBy,int countFile)
        {
            bool result = false;

          
            string _sqlInsert = string.Empty;

            try
            {
                // ใช้คำสั่ง SQL ใหม่ที่คุณให้มา
                _sqlInsert = "INSERT INTO `gsb_logview`.`ejjob` (`Job_ID`, `UploadDate`, `UploadBy`, `Status`,`CountFile`) " +
                             "VALUES ('" + jobId + "', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + uploadBy + "', 'open'," + countFile + ");";

                // ใช้ ExecuteQueryNoneParam สำหรับการเพิ่มข้อมูล (ไม่มีพารามิเตอร์ที่ต้องใช้)
                result = _objDb.ExecuteQueryNoneParam(_sqlInsert);

                if (!result)
                {
                    if (_objDb.ErrorMessDB != null)
                    {
                        ErrorMessage = _objDb.ErrorMessDB;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        public bool InsertDataToJobLogMonitor(string jobId, string uploadBy, int countFile)
        {
            bool result = false;


            string _sqlInsert = string.Empty;

            try
            {
                // ใช้คำสั่ง SQL ใหม่ที่คุณให้มา
                _sqlInsert = "INSERT INTO `gsb_logview`.`logmonitorjob` (`Job_ID`, `UploadDate`, `UploadBy`, `Status`,`CountFile`,`TerminalID`) " +
                             "VALUES ('" + jobId + "', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + uploadBy + "', 'open'," + countFile + ",' ');";

                // ใช้ ExecuteQueryNoneParam สำหรับการเพิ่มข้อมูล (ไม่มีพารามิเตอร์ที่ต้องใช้)
                result = _objDb.ExecuteQueryNoneParam(_sqlInsert);

                if (!result)
                {
                    if (_objDb.ErrorMessDB != null)
                    {
                        ErrorMessage = _objDb.ErrorMessDB;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }





        public List<EJ_Job> SelectDataFromJobEJ(string startDate, string endDate)
        {
            List<EJ_Job> jobList = new List<EJ_Job>();

            
            string _sqlSelect = "SELECT `Job_ID`, `UploadDate`, `UploadBy`, `Status` ,`CountFile`,`CloseJobDate` FROM `gsb_logview`.`ejjob` " +
                                "WHERE `UploadDate` BETWEEN '" + startDate + " 00:00:00' AND '" + endDate + " 23:59:59' ORDER BY `UploadDate` DESC";

            try
            {

                var data = _objDb.GetDatatableNotParam(_sqlSelect);

                if (data != null && data.Rows.Count > 0)
                {
                   
                    foreach (DataRow row in data.Rows)
                    {
                        EJ_Job job = new EJ_Job
                        {
                            Job_ID = row["Job_ID"].ToString(),
                            CountFile = (int)row["CountFile"],
                            UploadDate = Convert.ToDateTime(row["UploadDate"]),
                            UploadBy = row["UploadBy"].ToString(),
                            Status = row["Status"].ToString(),
                            CloseJobDate = row["CloseJobDate"] != DBNull.Value
               ? Convert.ToDateTime(row["CloseJobDate"])
               : (DateTime?)null
                        };

                        jobList.Add(job);
                    }
                }
            }
            catch (Exception ex)
            {
               
                throw ex;
            }

            return jobList;
        }




        public bool AddJobTaskDeviceTermProb(string startDate,string probCode,string atmType)
        {
            bool result = false;

            string _sql = string.Empty;

            try
            {
                _sql = "INSERT INTO `gsb_logview`.`taskjob_devicetermprob` (`startdate`,`status`,`insert_data_status`,`remark`,`updatedate`,`createdate`,`atmtype`)VALUE ( '" + startDate + "',0,'Queuing','" + probCode + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + atmType + "');";


                result = _objDb.ExecuteQueryNoneParam(_sql);

                if (result == false)
                {
                    if (_objDb.ErrorMessDB != null)
                        ErrorMessage = _objDb.ErrorMessDB;
                }
            }
            catch (Exception ex)
            { throw ex; }
            return result;
        }

        public List<Device_info_record> GetDeviceInfoFeelview()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM fv_device_info order by TERM_SEQ;";
            DataTable tableTemp = _objDb.GetDatatable(com);

            List<Device_info_record> deviceInfoRecordsList = ConvertDataTableToModel.ConvertDataTable<Device_info_record>(tableTemp);

            return deviceInfoRecordsList;
        }

        public List<ejlog_compare> GetAllEjLogCompare()
        {
            List<ejlog_compare> result = new List<ejlog_compare>();
            DataTable dt = new DataTable();

            try
            {
                string sql = "SELECT * FROM ejlog_compare ORDER BY terminalid, ejlog_datetime;";
                dt = _objDb.GetDatatableNotParam(sql);

                foreach (DataRow dr in dt.Rows)
                {
                    ejlog_compare item = new ejlog_compare();

                    item.idejlog_compare = dr["idejlog_compare"].ToString();
                    item.updatedate = Convert.ToDateTime(dr["updatedate"]).ToString();
                    item.ejlog_name = dr["ejlog_name"].ToString();
                    item.ejlog_datetime = Convert.ToDateTime(dr["ejlog_datetime"]).ToString();
                    item.status = dr["status"].ToString();
                    item.terminalid = dr["terminalid"].ToString();
                    item.remark = dr.Table.Columns.Contains("remark") && dr["remark"] != DBNull.Value ? dr["remark"].ToString() : "";

                    result.Add(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in GetAllEjLogCompare(): " + ex.Message);
            }

            return result;
        }


        public DataTable GetAllMasterProblem()
        {
            DataTable _dt = new DataTable();
            string _sql = string.Empty;

            try
            {
                _sql = "Select * From ejlog_problemmascode where status = '1' order by CASE WHEN memo IS NULL or memo = '' THEN 1 END, LENGTH(memo),probcode asc;";
                _dt = _objDb.GetDatatableNotParam(_sql);
                return _dt;
            }
            catch (Exception ex)
            { throw ex; }
        }

        public DataTable GetAllMasterProblemByDisplayFlagIs1()
        {
            DataTable _dt = new DataTable();
            string _sql = string.Empty;

            try
            {
                _sql = "Select * From ejlog_problemmascode where status = '1' and displayflag = '1' order by CASE WHEN memo IS NULL or memo = '' THEN 1 END, LENGTH(memo),probcode asc;";
                _dt = _objDb.GetDatatableNotParam(_sql);
                return _dt;
            }
            catch (Exception ex)
            { throw ex; }
        }

      

        public List<ProblemMaster> GetAllMasterSysErrorWord()
        {
            List<ProblemMaster> _result = new List<ProblemMaster>();
            DataTable _dt = new DataTable();

            try
            {

                _dt = GetAllMasterProblem();
                foreach (DataRow _dr in _dt.Rows)
                {
                    ProblemMaster obj = new ProblemMaster();
                    obj.ProblemCode = _dr["probcode"].ToString();
                    obj.ProblemName = _dr["probname"].ToString();
                    obj.Memo = _dr["memo"].ToString();
                    obj.ProbType = _dr["probtype"].ToString();
                    _result.Add(obj);
                }
            }
            catch (Exception ex)
            {

            }
            return _result;
        }

        public List<ProblemMaster> GetMasterSysErrorWord()
        {
            List<ProblemMaster> _result = new List<ProblemMaster>();
            DataTable _dt = new DataTable();

            try
            {

                _dt = GetAllMasterProblemByDisplayFlagIs1();
                foreach (DataRow _dr in _dt.Rows)
                {
                    ProblemMaster obj = new ProblemMaster();
                    obj.ProblemCode = _dr["probcode"].ToString();
                    obj.ProblemName = _dr["probname"].ToString();
                    obj.Memo = _dr["memo"].ToString();
                    obj.ProbType = _dr["probtype"].ToString();
                    _result.Add(obj);
                }
            }
            catch (Exception ex)
            {

            }
            return _result;
        }

        public DataTable GetClientFromDB()
        {

            DataTable _result = null;
            try
            {
                _result = GetClientData();
            }
            catch (Exception ex)
            { }
            return _result;
        }

        public List<ej_trandeviceprob> GetErrorTermDeviceEJLogCollectionFromReader(IDataReader reader)
        {
            int _seqNo = 1;
            List<ej_trandeviceprob> recordlst = new List<ej_trandeviceprob>();
            try
            {
                while (reader.Read())
                {
                    recordlst.Add(GetErrorTermDeviceEJLogFromReader(reader, _seqNo));
                    _seqNo++;
                }
            }
            catch (Exception ex)
            {

            }
            

            return recordlst;
        }



        public List<ej_trandeviceprob> GetErrorTermDeviceEJLog_Database(ej_trandada_seek model)
        {
            try
            {
                using (MySqlConnection cn = new MySqlConnection(FullNameConnection))
                {
                    MySqlCommand cmd = new MySqlCommand("GenDeviceProblemError", cn);

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("?pStratDate", model.FRDATE));
                    cmd.Parameters.Add(new MySqlParameter("?pEndDate", model.TODATE));
                    cmd.Parameters.Add(new MySqlParameter("?pTerminalID", model.TERMID));
                    cmd.Parameters.Add(new MySqlParameter("?pProbMaster", model.PROBNAME));

                    cn.Open();
                    return GetErrorTermDeviceEJLogCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                string err = ex.Message;
                return null;
            }
        }


        public List<ej_trandeviceprob> GetErrorTermDeviceEJLog_Database_sla(ej_trandada_seek model)
        {
            try
            {
                using (MySqlConnection cn = new MySqlConnection(FullNameConnection))
                {
                    MySqlCommand cmd = new MySqlCommand("GenDeviceProblemError_sla", cn);

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("?pStratDate", model.FRDATE));
                    cmd.Parameters.Add(new MySqlParameter("?pEndDate", model.TODATE));
                    cmd.Parameters.Add(new MySqlParameter("?pTerminalID", model.TERMID));
                    cmd.Parameters.Add(new MySqlParameter("?pProbMaster", model.PROBNAME));

                    cn.Open();
                    return GetErrorTermDeviceEJLogCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                string err = ex.Message;
                return null;
            }
        }

        public List<ej_trandeviceprob> GetErrorTermDeviceEJLog_Database_ejreport(ej_trandada_seek model)
        {
            try
            {
                using (MySqlConnection cn = new MySqlConnection(FullNameConnection))
                {
                    MySqlCommand cmd = new MySqlCommand("GenDeviceProblemError_ejreport", cn);

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("?pStratDate", model.FRDATE));
                    cmd.Parameters.Add(new MySqlParameter("?pEndDate", model.TODATE));
                    cmd.Parameters.Add(new MySqlParameter("?pTerminalID", model.TERMID));
                    cmd.Parameters.Add(new MySqlParameter("?pProbMaster", model.PROBNAME));

                    cn.Open();
                    return GetErrorTermDeviceEJLogCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                string err = ex.Message;
                return null;
            }
        }

        public ej_trandeviceprob GetErrorTermDeviceEJLogFromReader(IDataReader reader, int pSeqNo)
        {
            ej_trandeviceprob record = new ej_trandeviceprob();

            record.Seqno = reader["serialnumber"].ToString();
            record.TerminalID = reader["terminalid"].ToString();
            record.BranchName = reader["branchname"].ToString();
            record.Location = reader["locationbranch"].ToString();
            record.ProbName = reader["probname"].ToString();
            record.Remark = reader["remark"].ToString();
            record.Memo = reader["probMemo"].ToString();
            record.TransactionDate = Convert.ToDateTime(reader["trxdatetime"]);

            return record;
        }
        public List<ej_trandeviceprob> GetErrorTermDeviceKWEJLog_Database(ej_trandada_seek model)
        {
            try
            {
                using (MySqlConnection cn = new MySqlConnection(FullNameConnection))
                {
                    MySqlCommand cmd = new MySqlCommand("GenDeviceProblemErrorKW", cn);

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("?pStratDate", model.FRDATE));
                    cmd.Parameters.Add(new MySqlParameter("?pEndDate", model.TODATE));
                    cmd.Parameters.Add(new MySqlParameter("?pTerminalID", model.TERMID));
                    cmd.Parameters.Add(new MySqlParameter("?pProbKeyWord", model.PROBKEYWORD));

                    cn.Open();
                    return GetErrorTermDeviceEJLogCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                string err = ex.Message;
                return null;
            }
        }

        public List<checkuserfeelview> GetCheckUserFeelview(string user, string email)
        {
            string _sql = string.Empty;

            try
            {
                using (MySqlConnection cn = new MySqlConnection(FullNameConnection))
                {

                    _sql = "SELECT CASE WHEN COUNT(*) > 0 THEN 'yes' ELSE 'no' END AS _check FROM fv_system_users WHERE   ";
                    _sql += " ACCOUNT = '" + user + "' AND EMAIL = '" + email + "'; ";
                    cn.Open();

                    MySqlCommand cmd = new MySqlCommand(_sql, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                    return GetCheckUserFeelviewCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                return null;
            }
        }

        #endregion

        #region Protected/Private function
        protected virtual List<checkuserfeelview> GetCheckUserFeelviewCollectionFromReader(IDataReader reader)
        {
            List<checkuserfeelview> recordlst = new List<checkuserfeelview>();
            while (reader.Read())
            {
                recordlst.Add(GetCheckUserFeelviewFromReader(reader));
            }
            return recordlst;
        }
        protected virtual checkuserfeelview GetCheckUserFeelviewFromReader(IDataReader reader)
        {
            checkuserfeelview record = new checkuserfeelview();

            record.check = reader["_check"].ToString();
            return record;
        }

        #endregion

    }
}
