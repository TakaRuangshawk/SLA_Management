﻿using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using SLA_Management.Commons;
using SLA_Management.Data;
using SLA_Management.Data.ExcelUtilitie;
using SLA_Management.Models.LogMonitorModel;
using SLA_Management.Models.OperationModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using static SLA_Management.Controllers.ReportController;

namespace SLA_Management.Controllers
{
    public class MonitorController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");
        private IConfiguration _myConfiguration;
        private static ConnectMySQL db_fv;
        private static ConnectMySQL db_all;
        private static ConnectSQL_Server db_sql;
        private static List<LastTransactionModel> lasttransaction_dataList = new List<LastTransactionModel>();
        private static List<CardRetainModel> cardretain_dataList = new List<CardRetainModel>();
        private static List<TransactionModel> transaction_dataList = new List<TransactionModel>();
        private static List<EJReportMonitorModel> ejmonitor_dataList = new List<EJReportMonitorModel>();
        private static List<file_upload_historyModel> file_upload_history_dataList = new List<file_upload_historyModel>();
        #region export transaction parameter
        public static string bankname_ej;
        public static string fromtodate_ej;
        public static string sortby_ej;
        public static string orderby_ej;
        public static string term_ej;
        public static string branchname_ej;
        public static string status_ej;
        public static string totaltransaction_ej;
        public static string trxtype_ej;
        public static string rc_ej;
        #endregion
        public MonitorController(IConfiguration myConfiguration, IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _myConfiguration = myConfiguration;
            db_fv = new ConnectMySQL(myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection"));
            db_all = new ConnectMySQL(myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection"));
            db_sql = new ConnectSQL_Server(myConfiguration.GetValue<string>("ConnectionStrings:DefaultConnection"));

        }

        private static List<Device_info_record> GetDeviceInfoFeelview()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM device_info order by TERM_SEQ;";
            DataTable testss = db_fv.GetDatatable(com);

            List<Device_info_record> test = ConvertDataTableToModel.ConvertDataTable<Device_info_record>(testss);

            return test;
        }
        public IActionResult TransactionSummary()
        {
            ViewBag.CurrentTID = GetDeviceInfoFeelview();
            return View();
        }
        // This action method handles the AJAX request and returns JSON response
        [HttpPost]
        public IActionResult GetTerminalData([FromBody] TerminalTransactionModel model)
        {
            List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();
            string finalQuery = string.Empty;
            string fromDateStr = model.fromDate.ToString("yyyy-MM-dd HH:mm:dd") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:dd");
            string toDateStr = model.toDate.ToString("yyyy-MM-dd HH:mm:dd") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:dd");
            string terminalStr = model.terminalId ?? "";
            string terminaltypeStr = model.terminalType ?? "";
            string trxtpyeStr = model.transactionType ?? "";
            string terminalQuery = string.Empty;
            string terminalFinalQuery = string.Empty;
            DateTime fromDate = DateTime.Parse(fromDateStr);
            DateTime toDate = DateTime.Parse(toDateStr);
            string trxstatusStr = string.Empty;
            string tablequery = string.Empty;
            var allowedValues = _myConfiguration.GetSection("Receipt").Get<string[]>();
            string probcodes = string.Join("','", allowedValues);
            if (terminalStr != "")
            {
                terminalQuery += " and terminalid = '" + terminalStr + "' ";
                terminalFinalQuery += " and fdi.TERM_ID = '" + terminalStr + "' ";

            }
            if (terminaltypeStr != "")
            {
                terminalQuery += " and terminalid like '%" + terminaltypeStr + "' ";
                terminalFinalQuery += " and fdi.TERM_ID like '%" + terminaltypeStr + "' ";
            }
            switch (trxtpyeStr)
            {
                case "Deposit":
                    terminalQuery += " AND trx_type IN ('DEP_DCA' , 'DEP_DCC' , 'DEP_P00', 'DEP_P01','RFT_DCA') ";
                    tablequery = "ejhistory";
                    trxstatusStr = " AND trx_status = 'OK' ";
                    break;
                case "Withdraw":
                    terminalQuery += " AND trx_type IN ('FAS' , 'MCASH' , 'WDL','CL_WDL') ";
                    tablequery = "ejhistory";
                    trxstatusStr = " AND trx_status = 'OK' ";
                    break;
                case "":
                    terminalQuery += " AND trx_type IN ('DEP_DCA' , 'DEP_DCC' , 'DEP_P00', 'DEP_P01','RFT_DCA','FAS' , 'MCASH' , 'WDL','CL_WDL') ";
                    tablequery = "ejhistory";
                    break;
                case "Receipt":
                    terminalQuery += $" AND a.probcode IN ('{probcodes}') ";
                    tablequery = "termprobsla";
                    trxstatusStr = "";
                    break;
                case "Barcode":
                    terminalQuery += " AND trx_type IN ('BAR_P00','BAR_PCB') ";
                    tablequery = "ejhistory";
                    trxstatusStr = "";
                    break;
                default:
                    terminalQuery += " AND trx_type IN ('DEP_DCA' , 'DEP_DCC' , 'DEP_P00', 'DEP_P01','RFT_DCA','FAS' , 'MCASH' , 'WDL','CL_WDL') ";
                    tablequery = "ejhistory";
                    trxstatusStr = " AND trx_status = 'OK' ";
                    break;
            }
            StringBuilder queryBuilder = new StringBuilder();
            switch (tablequery)
            {
                case "ejhistory":
                    if (fromDate.ToString("yyyy-MM-dd") == toDate.ToString("yyyy-MM-dd"))
                    {
                        queryBuilder.AppendLine(@"  SELECT  (@row_number:=@row_number + 1) AS No,fdi.TERM_ID as TerminalNo,fdi.TERM_NAME as TerminalName,fdi.TYPE_ID as TerminalType,fdi.TERM_SEQ as DeviceSerialNo, COALESCE(_" + fromDate.ToString("yyyyMMdd") + "._" + fromDate.ToString("yyyyMMdd") + ",0) as _" + fromDate.ToString("yyyyMMdd") + "");
                    }
                    else
                    {
                        queryBuilder.AppendLine(@"  SELECT  (@row_number:=@row_number + 1) AS No,fdi.TERM_ID as TerminalNo,fdi.TERM_NAME as TerminalName,fdi.TYPE_ID as TerminalType,fdi.TERM_SEQ as DeviceSerialNo, COALESCE(_" + fromDate.ToString("yyyyMMdd") + "._" + fromDate.ToString("yyyyMMdd") + ",0) as _" + fromDate.ToString("yyyyMMdd") + ",");
                    }

                    for (DateTime date = fromDate.AddDays(1); date.Date <= toDate.Date; date = date.AddDays(1))
                    {
                        string dateStr = date.ToString("yyyyMMdd");
                        if (date.ToString("yyyy-MM-dd") != toDate.ToString("yyyy-MM-dd"))
                        {
                            queryBuilder.AppendLine("\tCOALESCE(_" + dateStr + "._" + dateStr + ", 0) AS _" + dateStr + ",");
                        }
                        else
                        {
                            queryBuilder.AppendLine("\tCOALESCE(_" + dateStr + "._" + dateStr + ", 0) AS _" + dateStr);
                        }
                    }

                    queryBuilder.AppendLine(@"FROM fv_device_info fdi
                    LEFT JOIN
                    (SELECT 
                        terminalid,
                        SUM(CASE WHEN DATE(trx_datetime) = '" + fromDate.ToString("yyyy-MM-dd") + "' THEN 1 ELSE 0 END) AS _" + fromDate.ToString("yyyyMMdd") + @"
                        FROM 
                            ejlog_history as a 
                        WHERE 
                            trxid IS NOT NULL " + terminalQuery + @"
                            AND trx_datetime BETWEEN '" + fromDate.ToString("yyyy-MM-dd") + @" 00:00:00' AND '" + fromDate.ToString("yyyy-MM-dd") + @" 23:59:59'
                            " + trxstatusStr + @"
                        GROUP BY 
                            terminalid) AS _" + fromDate.ToString("yyyyMMdd") + " ON fdi.TERM_ID = _" + fromDate.ToString("yyyyMMdd") + ".terminalid");

                    for (DateTime date = fromDate.AddDays(1); date.Date <= toDate.Date; date = date.AddDays(1))
                    {
                        string dateStr = date.ToString("yyyyMMdd");
                        queryBuilder.AppendLine(@"    LEFT JOIN
                                (SELECT 
                                    terminalid,
                                    SUM(CASE WHEN DATE(trx_datetime) = '" + date.ToString("yyyy-MM-dd") + "' THEN 1 ELSE 0 END) AS _" + dateStr + @"
                                FROM 
                                    ejlog_history as a 
                                WHERE 
                                    trxid IS NOT NULL " + terminalQuery + @"
                                    AND trx_datetime BETWEEN '" + date.ToString("yyyy-MM-dd") + @" 00:00:00' AND '" + date.ToString("yyyy-MM-dd") + @" 23:59:59'
                                    " + trxstatusStr + @"
                                GROUP BY 
                                    terminalid) AS _" + dateStr + @"
                                ON 
                                    fdi.TERM_ID = _" + dateStr + @".terminalid");
                    }

                    finalQuery = queryBuilder.ToString();
                    finalQuery += @" join (SELECT @row_number:=0) AS t   where fdi.TERM_ID is not null " + terminalFinalQuery + " order by fdi.TERM_SEQ asc ";
                    break;

                case "termprobsla":
                    if (fromDate.ToString("yyyy-MM-dd") == toDate.ToString("yyyy-MM-dd"))
                    {
                        queryBuilder.AppendLine(@"  SELECT  (@row_number:=@row_number + 1) AS No,fdi.TERM_ID as TerminalNo,fdi.TERM_NAME as TerminalName,fdi.TYPE_ID as TerminalType,fdi.TERM_SEQ as DeviceSerialNo, COALESCE(_" + fromDate.ToString("yyyyMMdd") + "._" + fromDate.ToString("yyyyMMdd") + ",0) as _" + fromDate.ToString("yyyyMMdd") + "");
                    }
                    else
                    {
                        queryBuilder.AppendLine(@"  SELECT  (@row_number:=@row_number + 1) AS No,fdi.TERM_ID as TerminalNo,fdi.TERM_NAME as TerminalName,fdi.TYPE_ID as TerminalType,fdi.TERM_SEQ as DeviceSerialNo, COALESCE(_" + fromDate.ToString("yyyyMMdd") + "._" + fromDate.ToString("yyyyMMdd") + ",0) as _" + fromDate.ToString("yyyyMMdd") + ",");
                    }

                    for (DateTime date = fromDate.AddDays(1); date.Date <= toDate.Date; date = date.AddDays(1))
                    {
                        string dateStr = date.ToString("yyyyMMdd");
                        if (date.ToString("yyyy-MM-dd") != toDate.ToString("yyyy-MM-dd"))
                        {
                            queryBuilder.AppendLine("\tCOALESCE(_" + dateStr + "._" + dateStr + ", 0) AS _" + dateStr + ",");
                        }
                        else
                        {
                            queryBuilder.AppendLine("\tCOALESCE(_" + dateStr + "._" + dateStr + ", 0) AS _" + dateStr);
                        }
                    }

                    queryBuilder.AppendLine(@" FROM fv_device_info fdi
                    LEFT JOIN
                    (SELECT 
                        terminalid,
                        SUM(CASE WHEN DATE(trxdatetime) = '" + fromDate.ToString("yyyy-MM-dd") + "' THEN 1 ELSE 0 END) AS _" + fromDate.ToString("yyyyMMdd") + @"
                        FROM 
                            ejlog_devicetermprob_sla as a 
                        WHERE 
                            a.seqno IS NOT NULL " + terminalQuery + @"
                            AND trxdatetime BETWEEN '" + fromDate.ToString("yyyy-MM-dd") + @" 00:00:00' AND '" + fromDate.ToString("yyyy-MM-dd") + @" 23:59:59'
                        GROUP BY 
                            terminalid) AS _" + fromDate.ToString("yyyyMMdd") + " ON fdi.TERM_ID = _" + fromDate.ToString("yyyyMMdd") + ".terminalid");

                    for (DateTime date = fromDate.AddDays(1); date.Date <= toDate.Date; date = date.AddDays(1))
                    {
                        string dateStr = date.ToString("yyyyMMdd");
                        queryBuilder.AppendLine(@"    LEFT JOIN
                                (SELECT 
                                    terminalid,
                                    SUM(CASE WHEN DATE(trxdatetime) = '" + date.ToString("yyyy-MM-dd") + "' THEN 1 ELSE 0 END) AS _" + dateStr + @"
                                FROM 
                                    ejlog_devicetermprob_sla as a  
                                WHERE 
                                    a.seqno IS NOT NULL " + terminalQuery + @"
                                    AND trxdatetime BETWEEN '" + date.ToString("yyyy-MM-dd") + @" 00:00:00' AND '" + date.ToString("yyyy-MM-dd") + @" 23:59:59'

                                GROUP BY 
                                    terminalid) AS _" + dateStr + @"
                                ON 
                                    fdi.TERM_ID = _" + dateStr + @".terminalid");
                    }

                    finalQuery = queryBuilder.ToString();
                    finalQuery += @"  join (SELECT @row_number:=0) AS t 
                                       where fdi.TERM_ID is not null " + terminalFinalQuery + " order by fdi.TERM_SEQ asc ";
                    break;
                default:

                    break;
            }



            try
            {
                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(finalQuery, connection))
                    {
                        command.CommandText = finalQuery;
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 120;
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> row = new Dictionary<string, object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.GetValue(i);
                                }

                                resultList.Add(row);
                            }
                        }
                    }



                }
                return Json(resultList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString);
                return Json(resultList);
            }

        }
        public class TerminalTransactionModel
        {
            public string terminalId { get; set; }
            public DateTime fromDate { get; set; }
            public DateTime toDate { get; set; }
            public string terminalType { get; set; }
            public string transactionType { get; set; }
        }

        #region SLALogMonitor



        public static List<file_upload_history> GetFileUploadHistory()
        {


            SqlCommand command = new SqlCommand
            {
                CommandText = @"
            SELECT [ID], [TERM_ID], [UPLOAD_DATE], [UPDATE_BY], 
                   [REMARK], [FILE_NAME], [FILE_DATETIME], [STATUS],[STEP]
            FROM [SLADB].[dbo].[file_upload_history]
            ORDER BY [UPLOAD_DATE] DESC;"
            };

            DataTable dt = db_sql.GetDatatable(command);

            if (dt == null || dt.Rows.Count == 0)
                return new List<file_upload_history>();

            return db_sql.ConvertDataTable<file_upload_history>(dt);
        }

        public static LatestStatusDto GetLatestFileUploadSummary()
        {
            // Get COUNT_ALL_TERMINAL จาก Device_info ทั้งหมด
            List<Device_info> deviceInfoList = GetDeviceInfoALL();
            int totalDevices = deviceInfoList?.Count ?? 0;

            SqlCommand command = new SqlCommand
            {
                CommandText = @"
        SELECT TOP 1
            COUNT_TERMINAL,
            COUNT_TASK_TERMINAL,
            COUNT_TASK_UPLOAD_SUCCESSFUL,
            COUNT_UPLOAD_COMLOG_SUCCESSFUL,
            COUNT_INSERT_COMLOG_SUCCESSFUL,
            COUNT_TASK_UPLOAD_UNSUCCESSFUL,
            COUNT_UPLOAD_COMLOG_UNSUCCESSFUL,
            COUNT_INSERT_COMLOG_UNSUCCESSFUL
        FROM [SLADB].[dbo].[file_upload_report]
        ORDER BY UPDATE_DATE DESC;"
            };

            DataTable dt = db_sql.GetDatatable(command);

            if (dt == null || dt.Rows.Count == 0)
            {
                return new LatestStatusDto
                {
                    COUNT_ALL_TERMINAL = totalDevices
                };
            }

            var row = dt.Rows[0];

            return new LatestStatusDto
            {
                COUNT_ALL_TERMINAL = totalDevices,
                COUNT_TERMINAL = Convert.ToInt32(row["COUNT_TERMINAL"]),
                COUNT_TASK_TERMINAL = Convert.ToInt32(row["COUNT_TASK_TERMINAL"]),
                COUNT_TASK_UPLOAD_SUCCESSFUL = Convert.ToInt32(row["COUNT_TASK_UPLOAD_SUCCESSFUL"]),
                COUNT_UPLOAD_COMLOG_SUCCESSFUL = Convert.ToInt32(row["COUNT_UPLOAD_COMLOG_SUCCESSFUL"]),
                COUNT_INSERT_COMLOG_SUCCESSFUL = Convert.ToInt32(row["COUNT_INSERT_COMLOG_SUCCESSFUL"]),
                COUNT_TASK_UPLOAD_UNSUCCESSFUL = Convert.ToInt32(row["COUNT_TASK_UPLOAD_UNSUCCESSFUL"]),
                COUNT_UPLOAD_COMLOG_UNSUCCESSFUL = Convert.ToInt32(row["COUNT_UPLOAD_COMLOG_UNSUCCESSFUL"]),
                COUNT_INSERT_COMLOG_UNSUCCESSFUL = Convert.ToInt32(row["COUNT_INSERT_COMLOG_UNSUCCESSFUL"])
            };
        }




        [HttpGet]
        public IActionResult SLALogMonitor()
        {

            int pageSize = 20;
            int? maxRows = 20;
            pageSize = maxRows.HasValue ? maxRows.Value : 100;
            ViewBag.maxRows = pageSize;
            ViewBag.CurrentTID = GetDeviceInfoALL();
            ViewBag.pageSize = pageSize;

            ViewBag.StatusAll = "";
            ViewBag.StepAll = "";


            var fileUploadHistory = GetFileUploadHistory();

            ViewBag.StatusAll = fileUploadHistory
                     .Select(x => x.STATUS)
                     .Where(x => !string.IsNullOrEmpty(x))
                     .Distinct()
                     .ToList();

            ViewBag.StepAll = fileUploadHistory
                           .Select(x => x.STEP)
                           .Where(x => !string.IsNullOrEmpty(x))
                           .Distinct()
                           .ToList();



            return View();
        }

        public IActionResult SLALogMonitorFetchData(string terminalno, string row, string page, string search, string status, DateTime? date, string step)
        {

            if (!date.HasValue)
            {
                date = DateTime.Now.AddDays(-1);
            }

            int _page = page == null || search == "search" ? 1 : int.Parse(page);
            int _row = row == null ? 20 : int.Parse(row);

            terminalno = terminalno ?? "";
            status = status ?? "";
            step = step ?? "";

            if (!string.IsNullOrEmpty(status)) ViewBag.Status = status;
            if (!string.IsNullOrEmpty(step)) ViewBag.Step = step;

            var parameters = new DynamicParameters();
            var sql = @"
        SELECT ID, TERM_ID, UPLOAD_DATE, UPDATE_BY, REMARK, FILE_NAME, FILE_DATETIME, STATUS, STEP
        FROM [SLADB].[dbo].[file_upload_history]
        WHERE 1=1";

            if (!string.IsNullOrEmpty(terminalno))
            {
                sql += " AND TERM_ID = @terminalno";
                parameters.Add("terminalno", terminalno);
            }
            if (!string.IsNullOrEmpty(status))
            {
                sql += " AND STATUS = @status";
                parameters.Add("status", status);
            }
            if (!string.IsNullOrEmpty(step))
            {
                sql += " AND STEP = @step";
                parameters.Add("step", step);
            }
            if (date.HasValue)
            {
                string fileDateStr = "COM" + date.Value.ToString("yyyyMMdd");
                sql += " AND FILE_NAME LIKE @fileDateStr";
                parameters.Add("fileDateStr", fileDateStr + "%");
            }

            sql += " ORDER BY UPLOAD_DATE DESC";

            List<Device_info> deviceInfo = GetDeviceInfoALL();
            int countAllTerminal = deviceInfo.Count;
            List<file_upload_historyModel> result;

            using (var connection = new SqlConnection(_myConfiguration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                var historyList = connection.Query<file_upload_history>(sql, parameters).ToList();
                result = new List<file_upload_historyModel>();
                int count = 1;

                foreach (var history in historyList)
                {
                    var device = deviceInfo.FirstOrDefault(d => d.TERM_ID == history.TERM_ID);

                    var model = new file_upload_historyModel
                    {
                        no = count.ToString(),
                        term_seq = device?.TERM_SEQ ?? "",
                        term_id = history.TERM_ID,
                        term_name = device?.TERM_NAME ?? "",
                        file_name = history.FILE_NAME,
                        status = history.STATUS,
                        step = history.STEP,
                        upload_date = history.UPLOAD_DATE,
                        update_by = history.UPDATE_BY,
                        remark = history.REMARK
                    };
                    result.Add(model);
                    count++;
                }

                // ---------------------
                // ดึง summary report แยกต่างหาก
                // ใช้ LOG_DATE จาก FILE_NAME ตัวแรกใน result (ถ้ามี)
                // ---------------------
                file_upload_report latestReport = null;

                var firstFileName = result.FirstOrDefault()?.file_name;
                if (!string.IsNullOrEmpty(firstFileName) && firstFileName.Length >= 11)
                {
                    var logDate = firstFileName.Substring(3, 8); // COM20250527 -> 20250527

                    var reportSql = @"
        SELECT TOP 1
            COUNT_TERMINAL,
            COUNT_TASK_TERMINAL,
            COUNT_TASK_UPLOAD_SUCCESSFUL,
            COUNT_UPLOAD_COMLOG_SUCCESSFUL,
            COUNT_INSERT_COMLOG_SUCCESSFUL,
            COUNT_TASK_UPLOAD_UNSUCCESSFUL,
            COUNT_UPLOAD_COMLOG_UNSUCCESSFUL,
            COUNT_INSERT_COMLOG_UNSUCCESSFUL
        FROM [SLADB].[dbo].[file_upload_report]
        WHERE CONVERT(VARCHAR(8), LOG_DATE, 112) = @logDate
        ORDER BY UPDATE_DATE DESC";

                    var reportData = connection.QueryFirstOrDefault<file_upload_report>(reportSql, new { logDate });

                    if (reportData != null)
                    {
                        reportData.COUNT_ALL_TERMINAL = countAllTerminal; // เพิ่มค่า
                        latestReport = reportData;
                    }
                    else
                    {
                        latestReport = new file_upload_report
                        {
                            COUNT_ALL_TERMINAL = countAllTerminal // อย่างน้อยให้มีค่าทั้งหมด
                        };
                    }
                }
                else
                {
                    // fallback: ไม่มี file เลย ก็ใส่ countAllTerminal อย่างเดียว
                    latestReport = new file_upload_report
                    {
                        COUNT_ALL_TERMINAL = countAllTerminal
                    };
                }


                int totalRecords = result.Count;
                int totalPages = (int)Math.Ceiling((double)totalRecords / _row);
                var pagedResult = result.Skip((_page - 1) * _row).Take(_row).ToList();

                var response = new DataResponse_file_upload_history
                {
                    JsonData = pagedResult,
                    Page = totalPages,
                    currentPage = _page,
                    TotalTerminal = totalRecords,
                    latestReport = latestReport
                };
       

                return Json(response);
            }
        }

        public class file_upload_historyModel
        {
            public string no { get; set; }
            public string term_seq { get; set; }
            public string term_id { get; set; }
            public string term_name { get; set; }
            public string file_name { get; set; }
            public string remark { get; set; }
            public string status { get; set; }
            public string step { get; set; }

            public DateTime? upload_date { get; set; }
            public string update_by { get; set; }

            // สำหรับเก็บข้อมูล summary จาก file_upload_report
            public LatestStatusDto report { get; set; }
        }


        public class DataResponse_file_upload_history
        {
            public List<file_upload_historyModel> JsonData { get; set; }
            public int Page { get; set; }
            public int currentPage { get; set; }
            public int TotalTerminal { get; set; }

            public file_upload_report latestReport { get; set; }
        }

        static List<file_upload_historyModel> RangeFilter_file_upload_history<file_upload_historyModel>(List<file_upload_historyModel> inputList, int page, int row)
        {
            int start_row;
            int end_row;
            if (page == 1)
            {
                start_row = 0;
            }
            else
            {
                start_row = (page - 1) * row;
            }
            end_row = start_row + row - 1;
            if (inputList.Count < end_row)
            {
                end_row = inputList.Count - 1;
            }
            return inputList.Skip(start_row).Take(row).ToList();
        }


        #endregion

        #region Excel SLALogMonitor


        [HttpPost]
        public IActionResult SLALogMonitor_ExportExc([FromBody] ExportParamModel data)
        {
            try
            {
                var splitParams = data.exparams.Split('|');
                string terminalId = splitParams.Length > 1 ? splitParams[1] : null;
                string dateStr = splitParams.Length > 2 ? splitParams[2] : null;
                string status = splitParams.Length > 3 ? splitParams[3] : null;
                string step = splitParams.Length > 4 ? splitParams[4] : null;

                DateTime filterDate;
                bool hasDate = DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out filterDate);

                if (!hasDate)
                {
                    filterDate = DateTime.Now.AddDays(-1).Date;
                    hasDate = true;
                }

                var query = @"
SELECT ID, TERM_ID, UPLOAD_DATE, UPDATE_BY, REMARK, FILE_NAME, FILE_DATETIME, STEP, STATUS
FROM [SLADB].[dbo].[file_upload_history]
WHERE 1=1";

                if (hasDate)
                {
                    // กรองโดยแปลง substring จาก FILE_NAME ที่เริ่มด้วย 'COM' ตามด้วยวันที่ YYYYMMDD
                    query += @"
    AND TRY_CAST(SUBSTRING(FILE_NAME, 4, 8) AS DATE) = @date";
                }
                if (!string.IsNullOrEmpty(terminalId))
                    query += " AND TERM_ID = @terminalId";
                if (!string.IsNullOrEmpty(status))
                    query += " AND STATUS = @status";
                if (!string.IsNullOrEmpty(step))
                    query += " AND STEP = @step";

                var results = new List<file_upload_history>();
                string connStr = _myConfiguration.GetValue<string>("ConnectionStrings:DefaultConnection");

                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand(query, conn))
                {
                    if (hasDate) cmd.Parameters.AddWithValue("@date", filterDate);
                    if (!string.IsNullOrEmpty(terminalId)) cmd.Parameters.AddWithValue("@terminalId", terminalId);
                    if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@status", status);
                    if (!string.IsNullOrEmpty(step)) cmd.Parameters.AddWithValue("@step", step);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new file_upload_history
                            {
                                ID = Convert.ToInt64(reader["ID"]),
                                TERM_ID = reader["TERM_ID"].ToString(),
                                UPLOAD_DATE = reader["UPLOAD_DATE"] as DateTime?,
                                UPDATE_BY = reader["UPDATE_BY"].ToString(),
                                REMARK = reader["REMARK"].ToString(),
                                FILE_NAME = reader["FILE_NAME"].ToString(),
                                FILE_DATETIME = reader["FILE_DATETIME"] as DateTime?,
                                STEP = reader["STEP"].ToString(),
                                STATUS = reader["STATUS"].ToString()
                            });
                        }
                    }
                }

                if (results.Count == 0)
                    return Json(new { success = "F", filename = "", errstr = "ไม่พบข้อมูล!" });

                string folderPath = Path.Combine(Environment.CurrentDirectory, "wwwroot", "SLALogExcel", "excelfiles");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string templateFolder = Path.Combine(Environment.CurrentDirectory, "wwwroot", "SLALogExcel", "InputTemplate");

                string filename = "SLALogMonitor_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                string outputPath = Path.Combine(folderPath, filename);

                var exportUtil = new ExcelUtilities_SLALogMonitor();
                exportUtil.PathDefaultTemplate = templateFolder;
                var latestSummary = GetLatestFileUploadSummary();

                exportUtil.ExportToExcel(results.Select(r => new Dictionary<string, object>
                {
                    ["Terminal ID"] = r.TERM_ID,
                    ["File Name"] = r.FILE_NAME,
                    ["Status"] = r.STATUS,
                    ["Step"] = r.STEP,
                    ["Upload Date"] = r.UPLOAD_DATE?.ToString("yyyy-MM-dd HH:mm"),
                    ["Updated By"] = r.UPDATE_BY,
                    ["Remark"] = r.REMARK
                }).ToList(), latestSummary, outputPath);

                return Json(new { success = "S", filename = filename, errstr = "" });
            }
            catch (Exception ex)
            {
                return Json(new { success = "F", filename = "", errstr = ex.Message });
            }
        }






        [HttpGet]
        public IActionResult SLALogMonitor_DownloadExportFile(string rpttype, string filename)
        {
            try
            {
                if (string.IsNullOrEmpty(filename))
                    return BadRequest("Invalid file name.");

                // แก้ path ให้ตรงกับที่เก็บไฟล์จริง
                string folderPath = Path.Combine(Environment.CurrentDirectory, "wwwroot", "SLALogExcel", "excelfiles");
                string fullPath = Path.Combine(folderPath, filename);

                if (!System.IO.File.Exists(fullPath))
                    return NotFound("File not found.");

                string contentType = "application/octet-stream";
                switch (rpttype?.ToLower())
                {
                    case "xlsx":
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;
                    case "csv":
                        contentType = "text/csv";
                        break;
                    case "pdf":
                        contentType = "application/pdf";
                        break;
                }

                return PhysicalFile(fullPath, contentType, filename);
            }
            catch (Exception ex)
            {
                return BadRequest("Download error: " + ex.Message);
            }
        }




        #endregion



        #region Excel TransactionSummary

        [HttpGet]
        public ActionResult TransactionSummary_ExportExc(string terminalId, DateTime fromDate, DateTime toDate, string terminalType, string transactionType)
        {
            string fname = "";
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;
            string fromDateStr = fromDate.ToString("yyyy-MM-dd HH:mm:dd") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:dd");
            string toDateStr = toDate.ToString("yyyy-MM-dd HH:mm:dd") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:dd");
            string terminalStr = terminalId ?? "";
            string terminaltypeStr = terminalType ?? "";
            string trxtpyeStr = transactionType ?? "";
            string terminalQuery = "";
            string terminalFinalQuery = "";
            string tablequery = "";
            string finalQuery = string.Empty;
            List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();
            fromDate = DateTime.Parse(fromDateStr);
            toDate = DateTime.Parse(toDateStr);
            var allowedValues = _myConfiguration.GetSection("Receipt").Get<string[]>();
            string probcodes = string.Join("','", allowedValues);
            if (terminalStr != "")
            {
                terminalQuery += " and terminalid = '" + terminalStr + "' ";
                terminalFinalQuery += " and fdi.TERM_ID = '" + terminalStr + "' ";

            }
            if (terminaltypeStr != "")
            {
                terminalQuery += " and terminalid like '%" + terminaltypeStr + "' ";
                terminalFinalQuery += " and fdi.TERM_ID like '%" + terminaltypeStr + "' ";
            }
            switch (trxtpyeStr)
            {
                case "Deposit":
                    terminalQuery += " AND trx_type IN ('DEP_DCA' , 'DEP_DCC' , 'DEP_P00', 'DEP_P01','RFT_DCA') ";
                    tablequery = "ejhistory";
                    break;
                case "Withdraw":
                    terminalQuery += " AND trx_type IN ('FAS' , 'MCASH' , 'WDL','CL_WDL') ";
                    tablequery = "ejhistory";
                    break;
                case "":
                    terminalQuery += " AND trx_type IN ('DEP_DCA' , 'DEP_DCC' , 'DEP_P00', 'DEP_P01','RFT_DCA','FAS' , 'MCASH' , 'WDL','CL_WDL') ";
                    tablequery = "ejhistory";
                    break;
                case "Receipt":
                    terminalQuery += $"AND a.probcode IN ('{probcodes}') ";
                    tablequery = "termprobsla";
                    break;
                default:
                    terminalQuery += " AND trx_type IN ('DEP_DCA' , 'DEP_DCC' , 'DEP_P00', 'DEP_P01','RFT_DCA','FAS' , 'MCASH' , 'WDL','CL_WDL') ";
                    tablequery = "ejhistory";
                    break;
            }
            StringBuilder queryBuilder = new StringBuilder();
            switch (tablequery)
            {
                case "ejhistory":
                    if (fromDate.ToString("yyyy-MM-dd") == toDate.ToString("yyyy-MM-dd"))
                    {
                        queryBuilder.AppendLine(@"  SELECT  (@row_number:=@row_number + 1) AS No,fdi.TERM_ID as TerminalNo,fdi.TERM_NAME as TerminalName,fdi.TYPE_ID as TerminalType,fdi.TERM_SEQ as DeviceSerialNo, COALESCE(_" + fromDate.ToString("yyyyMMdd") + "._" + fromDate.ToString("yyyyMMdd") + ",0) as _" + fromDate.ToString("yyyyMMdd") + "");
                    }
                    else
                    {
                        queryBuilder.AppendLine(@"  SELECT  (@row_number:=@row_number + 1) AS No,fdi.TERM_ID as TerminalNo,fdi.TERM_NAME as TerminalName,fdi.TYPE_ID as TerminalType,fdi.TERM_SEQ as DeviceSerialNo, COALESCE(_" + fromDate.ToString("yyyyMMdd") + "._" + fromDate.ToString("yyyyMMdd") + ",0) as _" + fromDate.ToString("yyyyMMdd") + ",");
                    }

                    for (DateTime date = fromDate.AddDays(1); date.Date <= toDate.Date; date = date.AddDays(1))
                    {
                        string dateStr = date.ToString("yyyyMMdd");
                        if (date.ToString("yyyy-MM-dd") != toDate.ToString("yyyy-MM-dd"))
                        {
                            queryBuilder.AppendLine("\tCOALESCE(_" + dateStr + "._" + dateStr + ", 0) AS _" + dateStr + ",");
                        }
                        else
                        {
                            queryBuilder.AppendLine("\tCOALESCE(_" + dateStr + "._" + dateStr + ", 0) AS _" + dateStr);
                        }
                    }

                    queryBuilder.AppendLine(@"FROM fv_device_info fdi
                    LEFT JOIN
                    (SELECT 
                        terminalid,
                        SUM(CASE WHEN DATE(trx_datetime) = '" + fromDate.ToString("yyyy-MM-dd") + "' THEN 1 ELSE 0 END) AS _" + fromDate.ToString("yyyyMMdd") + @"
                        FROM 
                            ejlog_history as a 
                        WHERE 
                            trxid IS NOT NULL " + terminalQuery + @"
                            AND trx_datetime BETWEEN '" + fromDate.ToString("yyyy-MM-dd") + @" 00:00:00' AND '" + fromDate.ToString("yyyy-MM-dd") + @" 23:59:59'
                            AND trx_status = 'OK'
                        GROUP BY 
                            terminalid) AS _" + fromDate.ToString("yyyyMMdd") + " ON fdi.TERM_ID = _" + fromDate.ToString("yyyyMMdd") + ".terminalid");

                    for (DateTime date = fromDate.AddDays(1); date.Date <= toDate.Date; date = date.AddDays(1))
                    {
                        string dateStr = date.ToString("yyyyMMdd");
                        queryBuilder.AppendLine(@"    LEFT JOIN
                                (SELECT 
                                    terminalid,
                                    SUM(CASE WHEN DATE(trx_datetime) = '" + date.ToString("yyyy-MM-dd") + "' THEN 1 ELSE 0 END) AS _" + dateStr + @"
                                FROM 
                                    ejlog_history as a 
                                WHERE 
                                    trxid IS NOT NULL " + terminalQuery + @"
                                    AND trx_datetime BETWEEN '" + date.ToString("yyyy-MM-dd") + @" 00:00:00' AND '" + date.ToString("yyyy-MM-dd") + @" 23:59:59'
                                    AND trx_status = 'OK'
                                GROUP BY 
                                    terminalid) AS _" + dateStr + @"
                                ON 
                                    fdi.TERM_ID = _" + dateStr + @".terminalid");
                    }

                    finalQuery = queryBuilder.ToString();
                    finalQuery += @" join (SELECT @row_number:=0) AS t   where fdi.TERM_ID is not null " + terminalFinalQuery + " order by fdi.TERM_SEQ asc ";
                    break;

                case "termprobsla":
                    if (fromDate.ToString("yyyy-MM-dd") == toDate.ToString("yyyy-MM-dd"))
                    {
                        queryBuilder.AppendLine(@"  SELECT  (@row_number:=@row_number + 1) AS No,fdi.TERM_ID as TerminalNo,fdi.TERM_NAME as TerminalName,fdi.TYPE_ID as TerminalType,fdi.TERM_SEQ as DeviceSerialNo, COALESCE(_" + fromDate.ToString("yyyyMMdd") + "._" + fromDate.ToString("yyyyMMdd") + ",0) as _" + fromDate.ToString("yyyyMMdd") + "");
                    }
                    else
                    {
                        queryBuilder.AppendLine(@"  SELECT  (@row_number:=@row_number + 1) AS No,fdi.TERM_ID as TerminalNo,fdi.TERM_NAME as TerminalName,fdi.TYPE_ID as TerminalType,fdi.TERM_SEQ as DeviceSerialNo, COALESCE(_" + fromDate.ToString("yyyyMMdd") + "._" + fromDate.ToString("yyyyMMdd") + ",0) as _" + fromDate.ToString("yyyyMMdd") + ",");
                    }

                    for (DateTime date = fromDate.AddDays(1); date.Date <= toDate.Date; date = date.AddDays(1))
                    {
                        string dateStr = date.ToString("yyyyMMdd");
                        if (date.ToString("yyyy-MM-dd") != toDate.ToString("yyyy-MM-dd"))
                        {
                            queryBuilder.AppendLine("\tCOALESCE(_" + dateStr + "._" + dateStr + ", 0) AS _" + dateStr + ",");
                        }
                        else
                        {
                            queryBuilder.AppendLine("\tCOALESCE(_" + dateStr + "._" + dateStr + ", 0) AS _" + dateStr);
                        }
                    }

                    queryBuilder.AppendLine(@" FROM fv_device_info fdi
                    LEFT JOIN
                    (SELECT 
                        terminalid,
                        SUM(CASE WHEN DATE(trxdatetime) = '" + fromDate.ToString("yyyy-MM-dd") + "' THEN 1 ELSE 0 END) AS _" + fromDate.ToString("yyyyMMdd") + @"
                        FROM 
                            ejlog_devicetermprob_sla as a 
                        WHERE 
                            a.seqno IS NOT NULL " + terminalQuery + @"
                            AND trxdatetime BETWEEN '" + fromDate.ToString("yyyy-MM-dd") + @" 00:00:00' AND '" + fromDate.ToString("yyyy-MM-dd") + @" 23:59:59'
                        GROUP BY 
                            terminalid) AS _" + fromDate.ToString("yyyyMMdd") + " ON fdi.TERM_ID = _" + fromDate.ToString("yyyyMMdd") + ".terminalid");

                    for (DateTime date = fromDate.AddDays(1); date.Date <= toDate.Date; date = date.AddDays(1))
                    {
                        string dateStr = date.ToString("yyyyMMdd");
                        queryBuilder.AppendLine(@"    LEFT JOIN
                                (SELECT 
                                    terminalid,
                                    SUM(CASE WHEN DATE(trxdatetime) = '" + date.ToString("yyyy-MM-dd") + "' THEN 1 ELSE 0 END) AS _" + dateStr + @"
                                FROM 
                                    ejlog_devicetermprob_sla as a  
                                WHERE 
                                    a.seqno IS NOT NULL " + terminalQuery + @"
                                    AND trxdatetime BETWEEN '" + date.ToString("yyyy-MM-dd") + @" 00:00:00' AND '" + date.ToString("yyyy-MM-dd") + @" 23:59:59'

                                GROUP BY 
                                    terminalid) AS _" + dateStr + @"
                                ON 
                                    fdi.TERM_ID = _" + dateStr + @".terminalid");
                    }

                    finalQuery = queryBuilder.ToString();
                    finalQuery += @"  join (SELECT @row_number:=0) AS t 
                                       where fdi.TERM_ID is not null " + terminalFinalQuery + " order by fdi.TERM_SEQ asc ";
                    break;
                default:

                    break;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(finalQuery, connection))
                    {
                        command.CommandText = finalQuery;
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 120;
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> row = new Dictionary<string, object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.GetValue(i);
                                }

                                resultList.Add(row);
                            }
                        }
                    }



                }





                if (resultList == null || resultList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;
                ExcelUtilities_TransactionSummary obj = new ExcelUtilities_TransactionSummary();


                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(resultList);



                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;

                if (trxtpyeStr != "")
                {
                    trxtpyeStr = "_" + trxtpyeStr;
                }

                fname = "TransactionSummary_" + DateTime.Now.ToString("yyyyMMdd") + trxtpyeStr;

                strPathDesc = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname + ".xlsx";


                if (obj.FileSaveAsXlsxFormat != null)
                {

                    if (System.IO.File.Exists(strPathDesc))
                        System.IO.File.Delete(strPathDesc);

                    if (!System.IO.File.Exists(strPathDesc))
                    {
                        System.IO.File.Copy(strPathSource, strPathDesc);
                        System.IO.File.Delete(strPathSource);
                    }
                    strSuccess = "S";
                    strErr = "";
                }
                else
                {
                    fname = "";
                    strSuccess = "F";
                    strErr = "Data Not Found";
                }

                ViewBag.ErrorMsg = "Error";
                return Json(new { success = strSuccess, filename = fname, errstr = strErr });
            }
            catch (Exception ex)
            {
                ViewBag.resultListag.ErrorMsg = ex.Message;
                return Json(new { success = "F", filename = "", errstr = ex.Message.ToString() });
            }
        }



        [HttpGet]
        public ActionResult DownloadExportFile_TransactionSummary(string rpttype, string transactionType)
        {
            string fname = "";
            string tempPath = "";
            string tsDate = "";
            string teDate = "";
            string trxtpyeStr = transactionType ?? "";
            try
            {
                if (trxtpyeStr != "")
                {
                    trxtpyeStr = "_" + trxtpyeStr;
                }
                fname = "TransactionSummary_" + DateTime.Now.ToString("yyyyMMdd") + trxtpyeStr;

                switch (rpttype.ToLower())
                {
                    case "csv":
                        fname = fname + ".csv";
                        break;
                    case "pdf":
                        fname = fname + ".pdf";
                        break;
                    case "xlsx":
                        fname = fname + ".xlsx";
                        break;
                }

                tempPath = Path.GetFullPath(Environment.CurrentDirectory + _myConfiguration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname);




                if (rpttype.ToLower().EndsWith("s") == true)
                    return File(tempPath + "xml", "application/vnd.openxmlformats-officedocument.spreadsheetml", fname);
                else if (rpttype.ToLower().EndsWith("f") == true)
                    return File(tempPath + "xml", "application/pdf", fname);
                else  //(rpttype.ToLower().EndsWith("v") == true)
                    return PhysicalFile(tempPath, "application/vnd.ms-excel", fname);



            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = "Download Method : " + ex.Message;
                return Json(new
                {
                    success = false,
                    fname
                });
            }
        }


        #endregion

        #region LastTransaction
        private static List<Device_info> GetDeviceInfoALL()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM fv_device_info order by TERM_SEQ;";
            DataTable testss = db_all.GetDatatable(com);

            List<Device_info> test = ConvertDataTableToModel.ConvertDataTable<Device_info>(testss);

            return test;
        }
        [HttpGet]
        public IActionResult LastTransaction()
        {

            int pageSize = 20;
            int? maxRows = 20;
            pageSize = maxRows.HasValue ? maxRows.Value : 100;
            ViewBag.maxRows = pageSize;
            ViewBag.CurrentTID = GetDeviceInfoALL();
            ViewBag.pageSize = pageSize;
            return View();
        }




        [HttpGet]
        public IActionResult LastTransactionFetchData(string terminalno, string row, string page, string search, string sort, string terminaltype)
        {
            int _page;

            if (page == null || search == "search")
            {
                _page = 1;
            }
            else
            {
                _page = int.Parse(page);
            }
            if (search == "next")
            {
                _page++;
            }
            else if (search == "prev")
            {
                _page--;
            }
            int _row;
            if (row == null)
            {
                _row = 20;
            }
            else
            {
                _row = int.Parse(row);
            }
            terminalno = terminalno ?? "";
            terminaltype = terminaltype ?? "";
            sort = sort ?? "term_id";
            List<LastTransactionModel> jsonData = new List<LastTransactionModel>();
            if (search == "search")
            {


                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    connection.Open();

                    // Modify the SQL query to use the 'input' parameter for filtering
                    string query = " SELECT adi.term_seq,adi.term_id,adi.term_name,MAX(ede.trxdatetime) AS lastest_trxdatetime,MAX(CASE WHEN ede.remark = ejm.probname AND ejm.status = 'success' THEN ede.trxdatetime else '' END) AS lastest_trxdatetime_success FROM fv_device_info adi JOIN ejlog_devicetermprob_ejreport ede ON adi.term_id = ede.terminalid LEFT JOIN ejlog_mastertransaction ejm ON ejm.probterm = SUBSTRING(adi.term_id, 1, 1) where ede.trxdatetime between '" + DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") + "' and ede.probcode in (select probcode from ejlog_problemmascode where probtype = 'EJLastTrans' ) ";


                    if (terminalno != "")
                    {
                        query += " and adi.TERM_ID like '%" + terminalno + "%' ";
                    }
                    if (terminaltype != "")
                    {
                        query += " and adi.TYPE_ID = '" + terminaltype + "' ";
                    }
                    query += " GROUP BY adi.term_seq, adi.term_id, adi.term_name ";
                    switch (sort)
                    {
                        case "term_id":
                            query += " ORDER BY adi.term_id asc;";
                            break;
                        case "branch_id":
                            query += " ORDER BY SUBSTRING_INDEX(adi.term_id, 'B', -1) asc;";
                            break;
                        case "term_seq":
                            query += " ORDER BY adi.term_seq asc;";
                            break;
                        case "last_transaction":
                            //query += " ORDER BY MAX(CASE WHEN ede.remark = ejm.probname AND ejm.status = 'success' THEN ede.trxdatetime else '' END) asc;";
                            query += " ORDER BY lastest_trxdatetime_success desc;";
                            break;
                        default:
                            query += " ORDER BY adi.term_id asc;";
                            break;
                    }
                    MySqlCommand command = new MySqlCommand(query, connection);

                    int id_row = 0;
                    string lastest_trxdatetime_row = "";
                    string lastest_trxdatetime_success_row = "";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            id_row += 1;
                            if (reader["lastest_trxdatetime"] != DBNull.Value && DateTime.TryParse(reader["lastest_trxdatetime"].ToString(), out DateTime trxDateTime))
                            {
                                lastest_trxdatetime_row = trxDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                lastest_trxdatetime_row = "-";
                            }
                            if (reader["lastest_trxdatetime_success"] != DBNull.Value && DateTime.TryParse(reader["lastest_trxdatetime_success"].ToString(), out DateTime trxDateTime_success))
                            {
                                lastest_trxdatetime_success_row = trxDateTime_success.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                lastest_trxdatetime_success_row = "-";
                            }
                            jsonData.Add(new LastTransactionModel
                            {

                                no = (id_row).ToString(),
                                term_seq = reader["term_seq"].ToString(),
                                term_id = reader["term_id"].ToString(),
                                term_name = reader["term_name"].ToString(),
                                last_transaction = lastest_trxdatetime_row,
                                last_transaction_success = lastest_trxdatetime_success_row,
                            });
                        }
                    }
                }
            }
            else
            {
                jsonData = lasttransaction_dataList;
            }
            lasttransaction_dataList = jsonData;
            int pages = (int)Math.Ceiling((double)jsonData.Count() / _row);
            List<LastTransactionModel> filteredData = RangeFilter_lt(jsonData, _page, _row);
            var response = new DataResponse_LastTransaction
            {
                JsonData = filteredData,
                Page = pages,
                currentPage = _page,
                TotalTerminal = jsonData.Count(),
            };
            return Json(response);
        }

        static List<LastTransactionModel> RangeFilter_lt<LastTransactionModel>(List<LastTransactionModel> inputList, int page, int row)
        {
            int start_row;
            int end_row;
            if (page == 1)
            {
                start_row = 0;
            }
            else
            {
                start_row = (page - 1) * row;
            }
            end_row = start_row + row - 1;
            if (inputList.Count < end_row)
            {
                end_row = inputList.Count - 1;
            }
            return inputList.Skip(start_row).Take(row).ToList();
        }

        public class LastTransactionModel
        {
            public string no { get; set; }
            public string term_id { get; set; }
            public string term_seq { get; set; }
            public string term_name { get; set; }
            public string last_transaction { get; set; }
            public string last_transaction_success { get; set; }

        }
        public class DataResponse_LastTransaction
        {
            public List<LastTransactionModel> JsonData { get; set; }
            public int Page { get; set; }
            public int currentPage { get; set; }
            public int TotalTerminal { get; set; }
        }


        #endregion

        #region CardRetain


        [HttpGet]
        public IActionResult CardRetain()
        {

            int pageSize = 20;
            int? maxRows = 20;
            pageSize = maxRows.HasValue ? maxRows.Value : 100;
            ViewBag.maxRows = pageSize;
            ViewBag.CurrentTID = GetDeviceInfoALL();
            ViewBag.pageSize = pageSize;
            return View();
        }
        [HttpGet]
        public IActionResult CardRetainFetchData(string terminalno, string row, string page, string search, string sort, string terminaltype)
        {
            int _page;

            if (page == null || search == "search")
            {
                _page = 1;
            }
            else
            {
                _page = int.Parse(page);
            }
            if (search == "next")
            {
                _page++;
            }
            else if (search == "prev")
            {
                _page--;
            }
            int _row;
            if (row == null)
            {
                _row = 20;
            }
            else
            {
                _row = int.Parse(row);
            }
            terminalno = terminalno ?? "";
            terminaltype = terminaltype ?? "";
            sort = sort ?? "trxdatetime";
            List<CardRetainModel> jsonData = new List<CardRetainModel>();
            if (search == "search")
            {
                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    connection.Open();
                    // Modify the SQL query to use the 'input' parameter for filtering
                    string query = " SELECT adi.term_seq as term_seq, t1.terminalid AS term_id, adi.term_name as term_name, SUBSTRING_INDEX(SUBSTRING_INDEX(UPPER(t1.remark), 'CARD NUMBER : ', -1), ' ', 4) AS card_number, t2.trxdatetime AS trxdatetime_ejreport FROM ejlog_devicetermprob_ejreport t1 LEFT JOIN ejlog_devicetermprob t2 ON t1.terminalid = t2.terminalid AND t1.trxdatetime BETWEEN DATE_SUB(t2.trxdatetime, INTERVAL 3 minute) AND t2.trxdatetime join fv_device_info adi on t1.terminalid = adi.term_id WHERE t1.probcode = 'EJREP_07' AND (t2.probcode = 'DEVICE05' OR t2.probcode = 'DEVICE14') AND (t2.remark LIKE '%RETAIN CARD%' or t2.remark like '%CARD RETAINED FAILED%') and t1.trxdatetime between '" + DateTime.Now.AddDays(-120).ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                    if (terminalno != "")
                    {
                        query += " and adi.TERM_ID like '%" + terminalno + "%' ";
                    }
                    if (terminaltype != "")
                    {
                        query += " and adi.TYPE_ID = '" + terminaltype + "' ";
                    }
                    query += " GROUP BY adi.term_seq, adi.term_id, adi.term_name ";
                    switch (sort)
                    {
                        case "trxdatetime":
                            query += " ORDER BY t2.trxdatetime asc; ";
                            break;
                        case "term_id":
                            query += " ORDER BY adi.term_id asc,t2.trxdatetime asc;";
                            break;
                        case "branch_id":
                            query += " ORDER BY SUBSTRING_INDEX(adi.term_id, 'B', -1) asc,t2.trxdatetime asc;";
                            break;
                        case "term_seq":
                            query += " ORDER BY adi.term_seq asc,t2.trxdatetime asc;";
                            break;
                        default:
                            query += " ORDER BY t2.trxdatetime asc;";
                            break;
                    }
                    MySqlCommand command = new MySqlCommand(query, connection);
                    int id_row = 0;
                    string _trxdatetime = "";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            id_row += 1;
                            if (reader["trxdatetime_ejreport"] != DBNull.Value && DateTime.TryParse(reader["trxdatetime_ejreport"].ToString(), out DateTime trxDateTime))
                            {
                                _trxdatetime = trxDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                _trxdatetime = "-";
                            }
                            jsonData.Add(new CardRetainModel
                            {

                                no = (id_row).ToString(),
                                term_seq = reader["term_seq"].ToString(),
                                term_id = reader["term_id"].ToString(),
                                term_name = reader["term_name"].ToString(),
                                card_number = reader["card_number"].ToString(),
                                trxdatetime = _trxdatetime,

                            });
                        }
                    }
                }
            }
            else
            {
                jsonData = cardretain_dataList;
            }
            cardretain_dataList = jsonData;
            int pages = (int)Math.Ceiling((double)jsonData.Count() / _row);
            List<CardRetainModel> filteredData = RangeFilter_cr(jsonData, _page, _row);
            var response = new DataResponse_CardRetain
            {
                JsonData = filteredData,
                Page = pages,
                currentPage = _page,
                TotalTerminal = jsonData.Count(),
            };
            return Json(response);
        }

        static List<CardRetainModel> RangeFilter_cr<CardRetainModel>(List<CardRetainModel> inputList, int page, int row)
        {
            int start_row;
            int end_row;
            if (page == 1)
            {
                start_row = 0;
            }
            else
            {
                start_row = (page - 1) * row;
            }
            end_row = start_row + row - 1;
            if (inputList.Count < end_row)
            {
                end_row = inputList.Count - 1;
            }
            return inputList.Skip(start_row).Take(row).ToList();
        }

        public class CardRetainModel
        {
            public string no { get; set; }
            public string term_id { get; set; }
            public string term_seq { get; set; }
            public string term_name { get; set; }
            public string card_number { get; set; }
            public string trxdatetime { get; set; }

        }
        public class DataResponse_CardRetain
        {
            public List<CardRetainModel> JsonData { get; set; }
            public int Page { get; set; }
            public int currentPage { get; set; }
            public int TotalTerminal { get; set; }
        }
        public IActionResult CardRetain2()
        {

            int pageSize = 20;
            int? maxRows = 20;
            pageSize = maxRows.HasValue ? maxRows.Value : 100;
            ViewBag.maxRows = pageSize;
            ViewBag.CurrentTID = GetDeviceInfoALL();
            ViewBag.pageSize = pageSize;
            return View();
        }
        [HttpPost]
        public IActionResult GetCardRetainData([FromBody] CardRetainQueryModel model)
        {
            List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();
            string finalQuery = string.Empty;
            string fromDateStr = model.fromDate.ToString("yyyy-MM-dd HH:mm:dd") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:dd");
            string toDateStr = model.toDate.ToString("yyyy-MM-dd HH:mm:dd") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:dd");
            string terminalStr = model.terminalId ?? "";
            string terminaltypeStr = model.terminalType ?? "";
            string terminalQuery = string.Empty;
            string sortStr = model.sort ?? "";
            string modeQuery = string.Empty;
            string terminalFinalQuery = string.Empty;
            DateTime fromDate = DateTime.Parse(fromDateStr);
            DateTime toDate = DateTime.Parse(toDateStr);
            string tablequery = string.Empty;
            string sortQuery = string.Empty;
            string groupCheck = string.Empty;
            var allowedValues = _myConfiguration.GetSection("Receipt").Get<string[]>();
            string probcodes = string.Join("','", allowedValues);
            if (terminalStr != "")
            {
                terminalQuery += " and t1.terminalid = '" + terminalStr + "' ";
                modeQuery = "term";
            }
            else
            {
                modeQuery = "sum";
            }
            if (terminaltypeStr != "")
            {
                terminalQuery += " and t1.terminalid like '%" + terminaltypeStr + "' ";

            }
            switch (sortStr)
            {
                case "term_id":
                    sortQuery += " ORDER BY adi.term_id asc,t1.trxdatetime desc ";
                    break;
                case "term_seq":
                    sortQuery += " ORDER BY adi.term_seq asc,t1.trxdatetime desc ";
                    break;
                case "branch":
                    sortQuery += " ORDER BY SUBSTRING_INDEX(adi.term_id, 'B', -1) asc,t1.terminalid asc,t1.trxdatetime desc ";
                    break;
                case "total":
                    sortQuery += " ORDER BY count(t1.remark) desc ";
                    break;

                default:
                    sortQuery += " ORDER BY terminalid asc, t1.trxdatetime desc";
                    break;
            }
            StringBuilder queryBuilder = new StringBuilder();
            switch (modeQuery)
            {
                case "sum":
                    queryBuilder.AppendLine(@"  SELECT adi.term_seq as SerialNo, t1.terminalid AS TerminalID, adi.term_name as TerminalName, count(t1.remark) as Total FROM ejlog_devicetermprob t1 join fv_device_info adi on t1.terminalid = adi.term_id WHERE adi.term_seq is not null and   remark not like '%[NOCARDTXNRETAINCARD]%' and (t1.probcode = 'DEVICE29') ");
                    groupCheck = " group by t1.terminalid ";
                    break;
                case "term":
                    queryBuilder.AppendLine(@" SELECT DISTINCT adi.term_seq as SerialNo, t1.terminalid AS TerminalID, adi.term_name as TerminalName,SUBSTRING_INDEX(SUBSTRING_INDEX(UPPER(t1.remark), 'CARD NUMBER : ', -1), ' ', 4) AS CardNumber,DATE_FORMAT(t2.trxdatetime, '%Y-%m-%d %H:%i:%s') AS TransactionDatetime FROM ejlog_devicetermprob_ejreport t1 LEFT JOIN ejlog_devicetermprob t2 ON t1.terminalid = t2.terminalid AND t1.trxdatetime BETWEEN DATE_SUB(t2.trxdatetime, INTERVAL 3 minute) AND t2.trxdatetime join fv_device_info adi on t1.terminalid = adi.term_id WHERE t1.probcode = 'EJREP_07' AND (t2.probcode = 'DEVICE29')");
                    sortQuery = " ORDER BY t1.terminalid asc, t1.trxdatetime desc";
                    break;
                default:
                    break;
            }
            queryBuilder.AppendLine(terminalQuery);
            queryBuilder.AppendLine(@"  and t1.trxdatetime between '" + fromDate.ToString("yyyy-MM-dd") + " 00:00:00' and '" + toDate.ToString("yyyy-MM-dd") + " 23:59:59' " + groupCheck + sortQuery);



            finalQuery = queryBuilder.ToString();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(finalQuery, connection))
                    {
                        command.CommandText = finalQuery;
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 120;
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> row = new Dictionary<string, object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.GetValue(i);
                                }

                                resultList.Add(row);
                            }
                        }
                    }



                }
                return Json(resultList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString);
                return Json(resultList);
            }

        }
        public class CardRetainQueryModel
        {
            public string terminalId { get; set; }
            public DateTime fromDate { get; set; }
            public DateTime toDate { get; set; }
            public string terminalType { get; set; }
            public string sort { get; set; }
        }
        public class CardRetainExportExcData
        {
            public string TerminalId { get; set; }
            public string FromDateStr { get; set; }
            public string ToDateStr { get; set; }
            public string TerminalType { get; set; }
            public string Sort { get; set; }
        }
        [HttpPost]
        public ActionResult CardRetain2_ExportExc([FromBody] CardRetainExportExcData data)
        {
            string fname = "";
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;
            List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();
            string finalQuery = string.Empty;
            data.FromDateStr = data.FromDateStr + " 00:00:00";
            data.ToDateStr = data.ToDateStr + " 23:59:59";
            DateTime fromDate = DateTime.Parse(data.FromDateStr);
            DateTime toDate = DateTime.Parse(data.ToDateStr);
            string terminalStr = data.TerminalId ?? "";
            string terminaltypeStr = data.TerminalType ?? "";
            string terminalQuery = string.Empty;
            string sortStr = data.Sort ?? "";
            string modeQuery = string.Empty;
            string terminalFinalQuery = string.Empty;
            string tablequery = string.Empty;
            string sortQuery = string.Empty;
            string groupCheck = string.Empty;
            var allowedValues = _myConfiguration.GetSection("Receipt").Get<string[]>();
            string probcodes = string.Join("','", allowedValues);
            if (terminalStr != "")
            {
                terminalQuery += " and t1.terminalid = '" + terminalStr + "' ";
                modeQuery = "term";
            }
            else
            {
                modeQuery = "sum";
            }
            if (terminaltypeStr != "")
            {
                terminalQuery += " and t1.terminalid like '%" + terminaltypeStr + "' ";

            }
            switch (sortStr)
            {
                case "term_id":
                    sortQuery += " ORDER BY adi.term_id asc,t1.trxdatetime desc ";
                    break;
                case "term_seq":
                    sortQuery += " ORDER BY adi.term_seq asc,t1.trxdatetime desc ";
                    break;
                case "branch":
                    sortQuery += " ORDER BY SUBSTRING_INDEX(adi.term_id, 'B', -1) asc,t1.terminalid asc,t1.trxdatetime desc ";
                    break;
                case "total":
                    sortQuery += " ORDER BY count(t1.remark) desc ";
                    break;

                default:
                    sortQuery += " ORDER BY terminalid asc, t1.trxdatetime desc";
                    break;
            }
            StringBuilder queryBuilder = new StringBuilder();
            switch (modeQuery)
            {
                case "sum":
                    queryBuilder.AppendLine(@"  SELECT adi.term_seq as SerialNo, t1.terminalid AS TerminalID, adi.term_name as TerminalName, count(t1.remark) as Total FROM ejlog_devicetermprob t1 join fv_device_info adi on t1.terminalid = adi.term_id WHERE adi.term_seq is not null and   remark not like '%[NOCARDTXNRETAINCARD]%' and (t1.probcode = 'DEVICE29') ");
                    groupCheck = " group by t1.terminalid ";
                    break;
                case "term":
                    queryBuilder.AppendLine(@" SELECT DISTINCT adi.term_seq as SerialNo, t1.terminalid AS TerminalID, adi.term_name as TerminalName,SUBSTRING_INDEX(SUBSTRING_INDEX(UPPER(t1.remark), 'CARD NUMBER : ', -1), ' ', 4) AS CardNumber,DATE_FORMAT(t2.trxdatetime, '%Y-%m-%d %H:%i:%s') AS TransactionDatetime FROM ejlog_devicetermprob_ejreport t1 LEFT JOIN ejlog_devicetermprob t2 ON t1.terminalid = t2.terminalid AND t1.trxdatetime BETWEEN DATE_SUB(t2.trxdatetime, INTERVAL 3 minute) AND t2.trxdatetime join fv_device_info adi on t1.terminalid = adi.term_id WHERE t1.probcode = 'EJREP_07' AND (t2.probcode = 'DEVICE29')");
                    sortQuery = " ORDER BY t1.terminalid asc, t1.trxdatetime desc";
                    break;
                default:
                    break;
            }
            queryBuilder.AppendLine(terminalQuery);
            queryBuilder.AppendLine(@"  and t1.trxdatetime between '" + fromDate.ToString("yyyy-MM-dd") + " 00:00:00' and '" + toDate.ToString("yyyy-MM-dd") + " 23:59:59' " + groupCheck + sortQuery);



            finalQuery = queryBuilder.ToString();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(finalQuery, connection))
                    {
                        command.CommandText = finalQuery;
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 120;
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> row = new Dictionary<string, object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.GetValue(i);
                                }

                                resultList.Add(row);
                            }
                        }
                    }



                }





                if (resultList == null || resultList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;
                ExcelUtilities_TransactionSummary obj = new ExcelUtilities_TransactionSummary();


                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(resultList);



                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;


                fname = "CardRetain_" + DateTime.Now.ToString("yyyyMMdd");

                strPathDesc = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname + ".xlsx";


                if (obj.FileSaveAsXlsxFormat != null)
                {

                    if (System.IO.File.Exists(strPathDesc))
                        System.IO.File.Delete(strPathDesc);

                    if (!System.IO.File.Exists(strPathDesc))
                    {
                        System.IO.File.Copy(strPathSource, strPathDesc);
                        System.IO.File.Delete(strPathSource);
                    }
                    strSuccess = "S";
                    strErr = "";
                }
                else
                {
                    fname = "";
                    strSuccess = "F";
                    strErr = "Data Not Found";
                }

                ViewBag.ErrorMsg = "Error";
                return Json(new { success = strSuccess, filename = fname, errstr = strErr });
            }
            catch (Exception ex)
            {
                ViewBag.resultListag.ErrorMsg = ex.Message;
                return Json(new { success = "F", filename = "", errstr = ex.Message.ToString() });
            }
        }



        [HttpGet]
        public ActionResult DownloadExportFile_CardRetain2(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            string tsDate = "";
            string teDate = "";
            try
            {

                fname = "CardRetain_" + DateTime.Now.ToString("yyyyMMdd");

                switch (rpttype.ToLower())
                {
                    case "csv":
                        fname = fname + ".csv";
                        break;
                    case "pdf":
                        fname = fname + ".pdf";
                        break;
                    case "xlsx":
                        fname = fname + ".xlsx";
                        break;
                }

                tempPath = Path.GetFullPath(Environment.CurrentDirectory + _myConfiguration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname);




                if (rpttype.ToLower().EndsWith("s") == true)
                    return File(tempPath + "xml", "application/vnd.openxmlformats-officedocument.spreadsheetml", fname);
                else if (rpttype.ToLower().EndsWith("f") == true)
                    return File(tempPath + "xml", "application/pdf", fname);
                else  //(rpttype.ToLower().EndsWith("v") == true)
                    return PhysicalFile(tempPath, "application/vnd.ms-excel", fname);



            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = "Download Method : " + ex.Message;
                return Json(new
                {
                    success = false,
                    fname
                });
            }
        }
        #endregion

        #region Transactions

        [HttpGet]
        public IActionResult Transactions()
        {

            int pageSize = 20;
            int? maxRows = 20;
            pageSize = maxRows.HasValue ? maxRows.Value : 100;
            ViewBag.maxRows = pageSize;
            ViewBag.CurrentTID = GetDeviceInfoALL();
            ViewBag.pageSize = pageSize;
            List<SDataValueModel> trxTypeData = GetDataBySdatagroup("TRX_TYPE");
            ViewBag.TrxTypeData = trxTypeData;
            List<SDataValueModel> rcData = GetDataBySdatagroup("S_REMARK");
            ViewBag.RCData = rcData;
            ViewBag.CurrentFr = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
            ViewBag.CurrentTo = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
            return View();
        }
        [HttpGet]
        public IActionResult TransactionsFetchData(string terminalno, string row, string page, string search, string sort, string order, string status, string trxtype, string rc, string todate, string fromdate)
        {
            int _page;
            string transactiontable = _myConfiguration.GetValue<string>("ProjectSetting:transactiontable") ?? "ejlog_history";

            if (page == null || search == "search")
            {
                _page = 1;
            }
            else
            {
                _page = int.Parse(page);
            }
            if (search == "next")
            {
                _page++;
            }
            else if (search == "prev")
            {
                _page--;
            }
            int _row;
            if (row == null)
            {
                _row = 20;
            }
            else
            {
                _row = int.Parse(row);
            }
            terminalno = terminalno ?? "";
            sort = sort ?? "trxdatetime";
            order = order ?? "asc";
            status = status ?? "";
            trxtype = trxtype ?? "";
            rc = rc ?? "";
            fromdate = fromdate ?? DateTime.Now.ToString("yyyy-MM-dd");
            todate = todate ?? DateTime.Now.ToString("yyyy-MM-dd");
            List<TransactionModel> jsonData = new List<TransactionModel>();

            if (search == "search")
            {
                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    connection.Open();
                    // Modify the SQL query to use the 'input' parameter for filtering
                    string query = " SELECT seq,trx_datetime,trx_type,bankcode,s_other,pan_no,fr_accno,to_accno,trx_status,amt1,fee_amt1,retract_amt1,CASE WHEN S_RC = 'N' AND S_REMARK = '' THEN S_REMARK ELSE S_REMARK END AS rc,case when billCounter is null then '' else billCounter end as billcounter FROM " + transactiontable + " where trx_datetime is not null and trx_datetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' ";
                    if (terminalno != "")
                    {
                        query += " and terminalid like '%" + terminalno + "%' ";
                    }
                    if (status != "")
                    {
                        query += " and trx_status = '" + status + "' ";
                    }
                    if (trxtype != "")
                    {
                        query += " and trx_type = '" + trxtype + "' ";
                    }
                    if (trxtype != "")
                    {
                        query += " and trx_type = '" + trxtype + "' ";
                    }
                    if (rc != "")
                    {
                        query += " AND (S_RC = '" + rc + "' OR S_REMARK = '" + rc + "') ";
                    }
                    switch (sort)
                    {
                        case "Datetime":
                            query += " ORDER BY trx_datetime " + order + " ; ";
                            break;
                        case "TransactionType":
                            query += " ORDER BY trx_type " + order + " , trx_datetime " + order + " ; ";
                            break;
                        case "ResponseCode":
                            query += " ORDER BY (S_RC AND S_REMARK) " + order + ", trx_datetime " + order + " ; ";
                            break;
                        default:
                            query += " ORDER BY trx_datetime " + order + " ; ";
                            break;
                    }
                    MySqlCommand command = new MySqlCommand(query, connection);
                    int id_row = 0;
                    string _trxdatetime = "";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            id_row += 1;
                            if (reader["trx_datetime"] != DBNull.Value && DateTime.TryParse(reader["trx_datetime"].ToString(), out DateTime trxDateTime))
                            {
                                _trxdatetime = trxDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                _trxdatetime = "-";
                            }
                            jsonData.Add(new TransactionModel
                            {

                                no = (id_row).ToString(),
                                seq = reader["seq"].ToString(),
                                trx_datetime = _trxdatetime,
                                trx_type = reader["trx_type"].ToString(),
                                bankcode = reader["bankcode"].ToString(),
                                s_other = reader["s_other"].ToString(),
                                pan_no = reader["pan_no"].ToString(),
                                fr_accno = reader["fr_accno"].ToString(),
                                to_accno = reader["to_accno"].ToString(),
                                trx_status = reader["trx_status"].ToString(),
                                amt1 = reader["amt1"].ToString(),
                                fee_amt1 = reader["fee_amt1"].ToString(),
                                retract_amt1 = reader["retract_amt1"].ToString(),
                                rc = reader["rc"].ToString(),
                                billcounter = FormatString(reader["billcounter"].ToString()),
                            });
                        }
                    }
                }
            }
            else
            {
                jsonData = transaction_dataList;
            }
            transaction_dataList = jsonData;
            bankname_ej = _myConfiguration.GetValue<string>("ProjectSetting:Bank") ?? "Bank";
            fromtodate_ej = fromdate + " 00:00:00 - " + todate + " 23:59:59";
            sortby_ej = sort;
            orderby_ej = (order == "asc") ? "น้อยไปมาก" : (order == "desc") ? "มากไปน้อย" : "น้อยไปมาก";
            term_ej = terminalno;
            branchname_ej = GetBranchName(terminalno);
            status_ej = (status == "") ? "เลือกทั้งหมด" : status;
            totaltransaction_ej = jsonData.Count().ToString();
            trxtype_ej = (trxtype == "") ? "เลือกทั้งหมด" : trxtype;
            rc_ej = (rc == "") ? "เลือกทั้งหมด" : rc;
            int pages = (int)Math.Ceiling((double)jsonData.Count() / _row);
            List<TransactionModel> filteredData = RangeFilter_tr(jsonData, _page, _row);
            var response = new DataResponse_Transaction
            {
                JsonData = filteredData,
                Page = pages,
                currentPage = _page,
                TotalTerminal = jsonData.Count(),
            };
            return Json(response);
        }

        private static string FormatString(string input)
        {
            input = input.Trim();
            if (input.Length % 3 != 0)
            {
                throw new ArgumentException("Input string length must be divisible by 3.");
            }

            string formattedString = "";

            for (int i = 0; i < input.Length; i += 3)
            {
                formattedString += input.Substring(i, 3);
                if (i + 3 < input.Length)
                {
                    formattedString += "-";
                }
            }

            return formattedString;
        }
        private string GetBranchName(string termid)
        {
            string result = "";
            using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
            {
                connection.Open();

                // Assuming your table is named "YourTableName"
                string query = "SELECT * FROM fv_device_info WHERE TERM_ID = @term_id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@term_id", termid);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Map the database columns to your model properties
                            deviceinfoModel data = new deviceinfoModel
                            {
                                term_name = reader["TERM_NAME"].ToString(),
                                // Map other properties accordingly
                            };

                            result = data.term_name;
                        }
                    }
                }
            }
            return result;
        }
        private List<SDataValueModel> GetDataBySdatagroup(string sdatagroup)
        {
            List<SDataValueModel> result = new List<SDataValueModel>();

            using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
            {
                connection.Open();

                // Assuming your table is named "YourTableName"
                string query = "SELECT * FROM ejlog_masterdata WHERE sdatagroup = @sdatagroup";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@sdatagroup", sdatagroup);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Map the database columns to your model properties
                            SDataValueModel data = new SDataValueModel
                            {
                                Sdatavalue = reader["sdatavalue"].ToString(),
                                // Map other properties accordingly
                            };

                            result.Add(data);
                        }
                    }
                }
            }

            return result;
        }

        static List<TransactionModel> RangeFilter_tr<TransactionModel>(List<TransactionModel> inputList, int page, int row)
        {
            int start_row;
            int end_row;
            if (page == 1)
            {
                start_row = 0;
            }
            else
            {
                start_row = (page - 1) * row;
            }
            end_row = start_row + row - 1;
            if (inputList.Count < end_row)
            {
                end_row = inputList.Count - 1;
            }
            return inputList.Skip(start_row).Take(row).ToList();
        }

        public class SDataValueModel
        {
            public string Sdatavalue { get; set; }
        }
        public class deviceinfoModel
        {
            public string term_id { get; set; }
            public string term_seq { get; set; }
            public string term_name { get; set; }
            public string type_id { get; set; }
        }
        public class TransactionModel
        {
            public string no { get; set; }
            public string seq { get; set; }
            public string trx_datetime { get; set; }
            public string trx_type { get; set; }
            public string bankcode { get; set; }
            public string s_other { get; set; }
            public string pan_no { get; set; }
            public string fr_accno { get; set; }
            public string to_accno { get; set; }
            public string trx_status { get; set; }
            public string amt1 { get; set; }
            public string fee_amt1 { get; set; }
            public string retract_amt1 { get; set; }
            public string rc { get; set; }
            public string billcounter { get; set; }

        }
        public class DataResponse_Transaction
        {
            public List<TransactionModel> JsonData { get; set; }
            public int Page { get; set; }
            public int currentPage { get; set; }
            public int TotalTerminal { get; set; }
        }
        #endregion

        #region Excel LastTransaction

        [HttpPost]
        public ActionResult LastTransaction_ExportExc()
        {
            string fname = "";
            string tsDate = "";
            string teDate = "";
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;

            try
            {

                if (lasttransaction_dataList == null || lasttransaction_dataList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;


                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }
                ExcelUtilities_LastTransaction obj = new ExcelUtilities_LastTransaction();
                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(lasttransaction_dataList);



                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;



                fname = "LastTransaction_" + DateTime.Now.ToString("yyyyMMdd");

                strPathDesc = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname + ".xlsx";


                if (obj.FileSaveAsXlsxFormat != null)
                {

                    if (System.IO.File.Exists(strPathDesc))
                        System.IO.File.Delete(strPathDesc);

                    if (!System.IO.File.Exists(strPathDesc))
                    {
                        System.IO.File.Copy(strPathSource, strPathDesc);
                        System.IO.File.Delete(strPathSource);
                    }
                    strSuccess = "S";
                    strErr = "";
                }
                else
                {
                    fname = "";
                    strSuccess = "F";
                    strErr = "Data Not Found";
                }

                ViewBag.ErrorMsg = "Error";
                return Json(new { success = strSuccess, filename = fname, errstr = strErr });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = ex.Message;
                return Json(new { success = "F", filename = "", errstr = ex.Message.ToString() });
            }
        }



        [HttpGet]
        public ActionResult LastTransaction_DownloadExportFile(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            string tsDate = "";
            string teDate = "";
            try
            {




                fname = "LastTransaction_" + DateTime.Now.ToString("yyyyMMdd");

                switch (rpttype.ToLower())
                {
                    case "csv":
                        fname = fname + ".csv";
                        break;
                    case "pdf":
                        fname = fname + ".pdf";
                        break;
                    case "xlsx":
                        fname = fname + ".xlsx";
                        break;
                }

                tempPath = Path.GetFullPath(Environment.CurrentDirectory + _myConfiguration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname);




                if (rpttype.ToLower().EndsWith("s") == true)
                    return File(tempPath + "xml", "application/vnd.openxmlformats-officedocument.spreadsheetml", fname);
                else if (rpttype.ToLower().EndsWith("f") == true)
                    return File(tempPath + "xml", "application/pdf", fname);
                else  //(rpttype.ToLower().EndsWith("v") == true)
                    return PhysicalFile(tempPath, "application/vnd.ms-excel", fname);



            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = "Download Method : " + ex.Message;
                return Json(new
                {
                    success = false,
                    fname
                });
            }
        }
        #endregion

        #region Excel CardRetain

        [HttpPost]
        public ActionResult CardRetain_ExportExc()
        {
            string fname = "";
            string tsDate = "";
            string teDate = "";
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;

            try
            {

                if (cardretain_dataList == null || cardretain_dataList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;


                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }
                ExcelUtilities_CardRetain obj = new ExcelUtilities_CardRetain();
                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(cardretain_dataList);



                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;



                fname = "CardRetain_" + DateTime.Now.ToString("yyyyMMdd");

                strPathDesc = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname + ".xlsx";


                if (obj.FileSaveAsXlsxFormat != null)
                {

                    if (System.IO.File.Exists(strPathDesc))
                        System.IO.File.Delete(strPathDesc);

                    if (!System.IO.File.Exists(strPathDesc))
                    {
                        System.IO.File.Copy(strPathSource, strPathDesc);
                        System.IO.File.Delete(strPathSource);
                    }
                    strSuccess = "S";
                    strErr = "";
                }
                else
                {
                    fname = "";
                    strSuccess = "F";
                    strErr = "Data Not Found";
                }

                ViewBag.ErrorMsg = "Error";
                return Json(new { success = strSuccess, filename = fname, errstr = strErr });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = ex.Message;
                return Json(new { success = "F", filename = "", errstr = ex.Message.ToString() });
            }
        }



        [HttpGet]
        public ActionResult CardRetain_DownloadExportFile(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            string tsDate = "";
            string teDate = "";
            try
            {




                fname = "CardRetain_" + DateTime.Now.ToString("yyyyMMdd");

                switch (rpttype.ToLower())
                {
                    case "csv":
                        fname = fname + ".csv";
                        break;
                    case "pdf":
                        fname = fname + ".pdf";
                        break;
                    case "xlsx":
                        fname = fname + ".xlsx";
                        break;
                }

                tempPath = Path.GetFullPath(Environment.CurrentDirectory + _myConfiguration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname);




                if (rpttype.ToLower().EndsWith("s") == true)
                    return File(tempPath + "xml", "application/vnd.openxmlformats-officedocument.spreadsheetml", fname);
                else if (rpttype.ToLower().EndsWith("f") == true)
                    return File(tempPath + "xml", "application/pdf", fname);
                else  //(rpttype.ToLower().EndsWith("v") == true)
                    return PhysicalFile(tempPath, "application/vnd.ms-excel", fname);



            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = "Download Method : " + ex.Message;
                return Json(new
                {
                    success = false,
                    fname
                });
            }
        }
        #endregion


        private static List<DeviceFirmwareModel> devicefirmware_dataList = new List<DeviceFirmwareModel>();
        [HttpGet]
        public IActionResult DeviceFirmware()
        {
            int pageSize = 100;
            int? maxRows = 100;
            pageSize = maxRows.HasValue ? maxRows.Value : 100;
            ViewBag.maxRows = pageSize;
            ViewBag.CurrentTID = GetDeviceInfoALL();
            ViewBag.pageSize = pageSize;
            return View();
        }
        public class DeviceFirmwareModel
        {
            public string no { get; set; }
            public string term_id { get; set; }
            public string term_seq { get; set; }
            public string term_name { get; set; }
            public string pin_version { get; set; }
            public string idc_version { get; set; }
            public string ptr_version { get; set; }
            public string bcr_version { get; set; }
            public string siu_version { get; set; }
            public string update_date { get; set; }

        }
        [HttpGet]
        public IActionResult DeviceFirmwareFetchData(string terminalno, string row, string page, string search, string sort, string terminaltype)
        {
            int _page;

            if (page == null || search == "search")
            {
                _page = 1;
            }
            else
            {
                _page = int.Parse(page);
            }
            if (search == "next")
            {
                _page++;
            }
            else if (search == "prev")
            {
                _page--;
            }
            int _row;
            if (row == null)
            {
                _row = 20;
            }
            else
            {
                _row = int.Parse(row);
            }
            terminalno = terminalno ?? "";
            terminaltype = terminaltype ?? "";
            sort = sort ?? "terminalno";
            List<DeviceFirmwareModel> jsonData = new List<DeviceFirmwareModel>();
            if (search == "search" || search == "paging")
            {
                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    connection.Open();
                    string query = @"SELECT t.TERM_ID, dih.TERM_SEQ, dih.TERM_NAME, 
                        t.CRM_VERSION, t.PIN_VERSION, t.IDC_VERSION, 
                        t.PTR_VERSION, t.BCR_VERSION, t.SIU_VERSION, 
                        DATE(t.UPDATE_DATE)
                 FROM gsb_logview.devfwversion_record t
                 LEFT JOIN device_info_history dih ON t.TERM_ID = dih.TERM_ID
                 WHERE 1 = 1"; // Add 'WHERE 1=1' to facilitate appending further conditions

                    // Filter by terminal number if provided
                    if (!string.IsNullOrEmpty(terminalno))
                    {
                        query += " AND t.TERM_ID = '" + terminalno + "'";
                    }

                    // Filter by terminal type if provided
                    if (!string.IsNullOrEmpty(terminaltype))
                    {
                        query += " AND dih.TYPE_ID = '" + terminaltype + "'";
                    }

                    // Sorting logic
                    switch (sort)
                    {

                        case "term_id":
                            query += " ORDER BY t.TERM_ID ASC, t.UPDATE_DATE ASC;";
                            break;
                        case "branch_id":
                            query += " ORDER BY SUBSTRING_INDEX(t.TERM_ID, 'B', -1) ASC, t.UPDATE_DATE ASC;";
                            break;
                        case "term_seq":
                            query += " ORDER BY dih.TERM_SEQ ASC, t.UPDATE_DATE ASC;";
                            break;
                        default:
                            query += " ORDER BY t.TERM_ID ASC, t.UPDATE_DATE ASC;";
                            break;
                    }
                    MySqlCommand command = new MySqlCommand(query, connection);
                    int id_row = 0;
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            id_row += 1;
                            jsonData.Add(new DeviceFirmwareModel
                            {
                                no = (id_row).ToString(),
                                term_seq = reader["term_seq"].ToString(),
                                term_id = reader["term_id"].ToString(),
                                term_name = reader["term_name"].ToString(),
                                pin_version = reader["pin_version"].ToString(),
                                idc_version = reader["idc_version"].ToString(),
                                ptr_version = reader["ptr_version"].ToString(),
                                bcr_version = reader["bcr_version"].ToString(),
                                siu_version = reader["siu_version"].ToString(),
                                update_date = reader["DATE(t.UPDATE_DATE)"].ToString()

                            });

                        }
                    }
                }
            }
            devicefirmware_dataList = jsonData;
            int pages = (int)Math.Ceiling((double)jsonData.Count() / _row);
            List<DeviceFirmwareModel> filteredData = RangeFilter_dvfw(jsonData, _page, _row);
            var response = new DataResponse_DeviceFirmware
            {
                JsonData = filteredData,
                Page = pages,
                currentPage = _page,
                TotalTerminal = jsonData.Count(),
            };
            return Json(response);
        }
        static List<DeviceFirmwareModel> RangeFilter_dvfw<DeviceFirmwareModel>(List<DeviceFirmwareModel> inputList, int page, int row)
        {
            int start_row;
            int end_row;
            if (page == 1)
            {
                start_row = 0;
            }
            else
            {
                start_row = (page - 1) * row;
            }
            end_row = start_row + row - 1;
            if (inputList.Count < end_row)
            {
                end_row = inputList.Count - 1;
            }
            return inputList.Skip(start_row).Take(row).ToList();
        }
        public class DataResponse_DeviceFirmware
        {
            public List<DeviceFirmwareModel> JsonData { get; set; }
            public int Page { get; set; }
            public int currentPage { get; set; }
            public int TotalTerminal { get; set; }
        }
        #region Excel DeviceFirmware
        public class DeviceFirmwareExportRequest
        {
            public string Exparams { get; set; }
            public string Terminalno { get; set; }
            public string Terminaltype { get; set; }
        }
        [HttpPost]
        public ActionResult DeviceFirmware_ExportExc([FromBody] DeviceFirmwareExportRequest request)
        {
            string exparams = request.Exparams;
            string terminalno = request.Terminalno ?? "";
            string terminaltype = request.Terminaltype ?? "";
            string webRootPath = _webHostEnvironment.WebRootPath;
            string folderPath = Path.Combine(webRootPath, "RegulatorExcel", "excelfiles");
            string strSuccess = "F";
            string strErr = "Data Not Found";
            string fname = "";
            string strPathDesc = "";
            List<DeviceFirmwareModel> jsonData = new List<DeviceFirmwareModel>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    connection.Open();
                    string query = @"SELECT t.TERM_ID, dih.TERM_SEQ, dih.TERM_NAME, 
                        t.CRM_VERSION, t.PIN_VERSION, t.IDC_VERSION, 
                        t.PTR_VERSION, t.BCR_VERSION, t.SIU_VERSION, 
                        DATE(t.UPDATE_DATE)
                 FROM gsb_logview.devfwversion_record t
                 LEFT JOIN device_info_history dih ON t.TERM_ID = dih.TERM_ID
                 WHERE 1 = 1";

                    // Filter by terminal number if provided
                    if (!string.IsNullOrEmpty(terminalno))
                    {
                        query += " AND t.TERM_ID = '" + terminalno + "'";
                    }

                    // Filter by terminal type if provided
                    if (!string.IsNullOrEmpty(terminaltype))
                    {
                        query += " AND dih.TYPE_ID = '" + terminaltype + "'";
                    }

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@terminalno", "%" + terminalno + "%");
                    command.Parameters.AddWithValue("@terminaltype", terminaltype);

                    int id_row = 0;
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            id_row += 1;
                            jsonData.Add(new DeviceFirmwareModel
                            {
                                no = id_row.ToString(),
                                term_seq = reader["TERM_SEQ"].ToString(),
                                term_id = reader["TERM_ID"].ToString(),
                                term_name = reader["TERM_NAME"].ToString(),
                                pin_version = reader["PIN_VERSION"].ToString(),
                                idc_version = reader["IDC_VERSION"].ToString(),
                                ptr_version = reader["PTR_VERSION"].ToString(),
                                bcr_version = reader["BCR_VERSION"].ToString(),
                                siu_version = reader["SIU_VERSION"].ToString(),
                                update_date = reader["DATE(t.UPDATE_DATE)"].ToString()
                            });
                        }
                    }
                }

                if (jsonData == null || jsonData.Count == 0)
                {
                    return Json(new { success = "F", filename = "", errstr = "Data not found!" });
                }
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                // Create Excel File using EPPlus
                using (var package = new ExcelPackage())
                {

                    var worksheet = package.Workbook.Worksheets.Add("DeviceFirmwareData");

                    // Set headers
                    worksheet.Cells[1, 1].Value = "No.";
                    worksheet.Cells[1, 2].Value = "Serial No.";
                    worksheet.Cells[1, 3].Value = "Terminal ID";
                    worksheet.Cells[1, 4].Value = "Terminal Name";
                    worksheet.Cells[1, 5].Value = "PIN Version";
                    worksheet.Cells[1, 6].Value = "IDC Version";
                    worksheet.Cells[1, 7].Value = "PTR Version";
                    worksheet.Cells[1, 8].Value = "BCR Version";
                    worksheet.Cells[1, 9].Value = "SIU Version";
                    worksheet.Cells[1, 10].Value = "Update Date";

                    // Fill data
                    for (int i = 0; i < jsonData.Count; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = jsonData[i].no;
                        worksheet.Cells[i + 2, 2].Value = jsonData[i].term_seq;
                        worksheet.Cells[i + 2, 3].Value = jsonData[i].term_id;
                        worksheet.Cells[i + 2, 4].Value = jsonData[i].term_name;
                        worksheet.Cells[i + 2, 5].Value = jsonData[i].pin_version;
                        worksheet.Cells[i + 2, 6].Value = jsonData[i].idc_version;
                        worksheet.Cells[i + 2, 7].Value = jsonData[i].ptr_version;
                        worksheet.Cells[i + 2, 8].Value = jsonData[i].bcr_version;
                        worksheet.Cells[i + 2, 9].Value = jsonData[i].siu_version;
                        // Apply date format for Update Date
                        DateTime updateDate;
                        if (DateTime.TryParse(jsonData[i].update_date, out updateDate))
                        {
                            worksheet.Cells[i + 2, 10].Value = updateDate;
                            worksheet.Cells[i + 2, 10].Style.Numberformat.Format = "yyyy-MM-dd";
                        }
                        else
                        {
                            worksheet.Cells[i + 2, 10].Value = jsonData[i].update_date;
                        }
                    }
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    try
                    {

                        folderPath = Path.Combine(webRootPath, "RegulatorExcel", "excelfiles");
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        // Define the file name and path
                        fname = "DeviceFirmware_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
                        strPathDesc = Path.Combine(folderPath, fname);

                        // Log or print the file path to ensure it's correct
                        Console.WriteLine("Attempting to save file at: " + strPathDesc);

                        // Save the Excel file
                        System.IO.File.WriteAllBytes(strPathDesc, package.GetAsByteArray());

                        // Log success message
                        Console.WriteLine("File saved successfully at: " + strPathDesc);

                        strSuccess = "S";
                        strErr = "";  // No errors
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error saving file: {ex.Message}");

                        // Set the error message to be returned
                        strSuccess = "F";
                        strErr = "Error saving file: " + ex.Message;
                    }
                }

                strSuccess = "S";
                strErr = "";
            }
            catch (Exception ex)
            {
                strErr = ex.Message;
            }

            return Json(new { success = strSuccess, filename = fname, errstr = strErr });
        }

        #endregion




        [HttpGet]
        public ActionResult DeviceFirmware_DownloadExportFile(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            string tsDate = "";
            string teDate = "";
            try
            {




                fname = "DeviceFirmware_" + DateTime.Now.ToString("yyyyMMdd");

                switch (rpttype.ToLower())
                {
                    case "csv":
                        fname = fname + ".csv";
                        break;
                    case "pdf":
                        fname = fname + ".pdf";
                        break;
                    case "xlsx":
                        fname = fname + ".xlsx";
                        break;
                }

                tempPath = Path.GetFullPath(Environment.CurrentDirectory + _myConfiguration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname);




                if (rpttype.ToLower().EndsWith("s") == true)
                    return File(tempPath + "xml", "application/vnd.openxmlformats-officedocument.spreadsheetml", fname);
                else if (rpttype.ToLower().EndsWith("f") == true)
                    return File(tempPath + "xml", "application/pdf", fname);
                else  //(rpttype.ToLower().EndsWith("v") == true)
                    return PhysicalFile(tempPath, "application/vnd.ms-excel", fname);



            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = "Download Method : " + ex.Message;
                return Json(new
                {
                    success = false,
                    fname
                });
            }
        }

        #region Excel Transaction

        [HttpPost]
        public ActionResult Transaction_ExportExc()
        {
            string fname = "";
            string tsDate = "";
            string teDate = "";
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;

            try
            {

                if (transaction_dataList == null || transaction_dataList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;


                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }
                ExcelUtilities_Transaction obj = new ExcelUtilities_Transaction();
                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(transaction_dataList);



                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;



                fname = "Transaction_" + DateTime.Now.ToString("yyyyMMdd");

                strPathDesc = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname + ".xlsx";


                if (obj.FileSaveAsXlsxFormat != null)
                {

                    if (System.IO.File.Exists(strPathDesc))
                        System.IO.File.Delete(strPathDesc);

                    if (!System.IO.File.Exists(strPathDesc))
                    {
                        System.IO.File.Copy(strPathSource, strPathDesc);
                        System.IO.File.Delete(strPathSource);
                    }
                    strSuccess = "S";
                    strErr = "";
                }
                else
                {
                    fname = "";
                    strSuccess = "F";
                    strErr = "Data Not Found";
                }

                ViewBag.ErrorMsg = "Error";
                return Json(new { success = strSuccess, filename = fname, errstr = strErr });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = ex.Message;
                return Json(new { success = "F", filename = "", errstr = ex.Message.ToString() });
            }
        }



        [HttpGet]
        public ActionResult Transaction_DownloadExportFile(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            string tsDate = "";
            string teDate = "";
            try
            {




                fname = "Transaction_" + DateTime.Now.ToString("yyyyMMdd");

                switch (rpttype.ToLower())
                {
                    case "csv":
                        fname = fname + ".csv";
                        break;
                    case "pdf":
                        fname = fname + ".pdf";
                        break;
                    case "xlsx":
                        fname = fname + ".xlsx";
                        break;
                }

                tempPath = Path.GetFullPath(Environment.CurrentDirectory + _myConfiguration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname);




                if (rpttype.ToLower().EndsWith("s") == true)
                    return File(tempPath + "xml", "application/vnd.openxmlformats-officedocument.spreadsheetml", fname);
                else if (rpttype.ToLower().EndsWith("f") == true)
                    return File(tempPath + "xml", "application/pdf", fname);
                else  //(rpttype.ToLower().EndsWith("v") == true)
                    return PhysicalFile(tempPath, "application/vnd.ms-excel", fname);



            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = "Download Method : " + ex.Message;
                return Json(new
                {
                    success = false,
                    fname
                });
            }
        }
        #endregion
        #region EJLogMonitor


        [HttpGet]
        public IActionResult EJLogMonitor()
        {

            int pageSize = 20;
            int? maxRows = 20;
            pageSize = maxRows.HasValue ? maxRows.Value : 100;
            ViewBag.maxRows = pageSize;
            ViewBag.CurrentTID = GetDeviceInfoALL();
            ViewBag.pageSize = pageSize;
            return View();
        }
        [HttpGet]
        public IActionResult EjreportMonitorFetchData(string terminalno, string row, string page, string search, string sort, string terminaltype, string date, string status)
        {
            int _page;

            if (page == null || search == "search")
            {
                _page = 1;
            }
            else
            {
                _page = int.Parse(page);
            }
            if (search == "next")
            {
                _page++;
            }
            else if (search == "prev")
            {
                _page--;
            }
            int _row;
            if (row == null)
            {
                _row = 20;
            }
            else
            {
                _row = int.Parse(row);
            }
            terminalno = terminalno ?? "";
            terminaltype = terminaltype ?? "";
            sort = sort ?? "term_id";
            date = date ?? DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
            List<EJReportMonitorModel> jsonData = new List<EJReportMonitorModel>();
            if (search == "search")
            {
                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    connection.Open();
                    // Modify the SQL query to use the 'input' parameter for filtering
                    string query = "SELECT t1.terminalId,fdi.TERM_SEQ,fdi.TERM_NAME,date,t1.transaction_history,t1.transaction_monitoring,t1.transaction_history - t1.transaction_monitoring AS diff,  CASE WHEN t1.transaction_history = t1.transaction_monitoring THEN 'Complete' WHEN t1.transaction_history > t1.transaction_monitoring THEN 'N/A' WHEN t1.transaction_history < t1.transaction_monitoring THEN 'Incomplete' END AS status FROM ejlog_monitoring_report t1 left join fv_device_info fdi on t1.terminalId = fdi.TERM_ID where id is not null ";
                    if (terminalno != "")
                    {
                        query += " and fdi.TERM_ID like '%" + terminalno + "%' ";
                    }
                    if (terminaltype != "")
                    {
                        query += " and fdi.TYPE_ID = '" + terminaltype + "' ";
                    }
                    if (!string.IsNullOrEmpty(date))
                    {
                        query += " AND t1.date = '" + date + "' ";
                    }
                    switch (status)
                    {
                        case "Complete":
                            query += " AND t1.transaction_history = t1.transaction_monitoring ";
                            break;
                        case "N/A":
                            query += " AND t1.transaction_history > t1.transaction_monitoring ";
                            break;
                        case "Incomplete":
                            query += " AND t1.transaction_history < t1.transaction_monitoring ";
                            break;
                        default: break;
                    }
                    switch (sort)
                    {
                        case "diff":
                            query += " ORDER BY t1.transaction_history - t1.transaction_monitoring desc";
                            break;
                        case "term_id":
                            query += " ORDER BY fdi.term_id asc";
                            break;
                        case "branch_id":
                            query += " ORDER BY SUBSTRING_INDEX(fdi.term_id, 'B', -1) asc";
                            break;
                        case "term_seq":
                            query += " ORDER BY fdi.term_seq asc";
                            break;
                        default:
                            query += " ORDER BY fdi.term_id asc";
                            break;
                    }
                    MySqlCommand command = new MySqlCommand(query, connection);
                    int id_row = 0;
                    string _trxdatetime = "";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            id_row += 1;
                            string _trxdate;
                            if (reader["date"] != DBNull.Value && DateTime.TryParse(reader["date"].ToString(), out DateTime trxDate))
                            {
                                _trxdate = trxDate.ToString("yyyy-MM-dd");
                            }
                            else
                            {
                                _trxdate = "-";
                            }
                            jsonData.Add(new EJReportMonitorModel
                            {
                                TerminalId = reader["terminalId"]?.ToString() ?? "-",
                                TermSeq = reader["TERM_SEQ"]?.ToString() ?? "-",
                                TermName = reader["TERM_NAME"]?.ToString() ?? "-",
                                Date = _trxdate,
                                TransactionHistory = reader["transaction_history"]?.ToString() ?? "-",
                                TransactionMonitoring = reader["transaction_monitoring"]?.ToString() ?? "-",
                                Diff = Convert.ToInt32(reader["diff"]),
                                Status = reader["status"]?.ToString() ?? "-"
                            });
                        }
                    }

                }
            }
            else
            {
                jsonData = ejmonitor_dataList;
            }
            ejmonitor_dataList = jsonData;
            int pages = (int)Math.Ceiling((double)jsonData.Count() / _row);
            List<EJReportMonitorModel> filteredData = RangeFilter_ejr(jsonData, _page, _row);
            var response = new DataResponse_EJReportMonitor
            {
                JsonData = filteredData,
                Page = pages,
                currentPage = _page,
                TotalTerminal = jsonData.Count(),
            };
            return Json(response);
        }

        static List<EJReportMonitorModel> RangeFilter_ejr<EJReportMonitorModel>(List<EJReportMonitorModel> inputList, int page, int row)
        {
            int start_row;
            int end_row;
            if (page == 1)
            {
                start_row = 0;
            }
            else
            {
                start_row = (page - 1) * row;
            }
            end_row = start_row + row - 1;
            if (inputList.Count < end_row)
            {
                end_row = inputList.Count - 1;
            }
            return inputList.Skip(start_row).Take(row).ToList();
        }

        public class EJReportMonitorModel
        {
            public string TerminalId { get; set; }
            public string TermSeq { get; set; }
            public string TermName { get; set; }
            public string Date { get; set; }
            public string TransactionHistory { get; set; }
            public string TransactionMonitoring { get; set; }
            public int Diff { get; set; }
            public string Status { get; set; }
        }

        public class DataResponse_EJReportMonitor
        {
            public List<EJReportMonitorModel> JsonData { get; set; }
            public int Page { get; set; }
            public int currentPage { get; set; }
            public int TotalTerminal { get; set; }
        }
        #endregion
        #region Excel EJLogMonitor

        public class ExportParams_EJLogMonitor
        {
            public string Terminalno { get; set; }
            public string Terminaltype { get; set; }
            public string Date { get; set; }
            public string Status { get; set; }
        }

        [HttpPost]
        public ActionResult EJLogMonitor_ExportExc([FromBody] ExportParams_EJLogMonitor exportParams)
        {
            string fname = "";
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;

            try
            {
                // Define SQL query for filtering data
                string query = @"
            SELECT 
                t1.terminalId,
                fdi.TERM_SEQ,
                fdi.TERM_NAME,
                t1.date,
                t1.transaction_history,
                t1.transaction_monitoring,
                t1.transaction_history - t1.transaction_monitoring AS diff,
                CASE
                    WHEN t1.transaction_history = t1.transaction_monitoring THEN 'Complete'
                    WHEN t1.transaction_history > t1.transaction_monitoring THEN 'N/A'
                    ELSE 'Incomplete'
                END AS status
            FROM ejlog_monitoring_report t1
            LEFT JOIN fv_device_info fdi ON t1.terminalId = fdi.TERM_ID
            WHERE 1=1 ";

                // Apply filters
                if (!string.IsNullOrEmpty(exportParams.Terminalno))
                {
                    query += " AND fdi.TERM_ID LIKE @TerminalNo ";
                }
                if (!string.IsNullOrEmpty(exportParams.Terminaltype))
                {
                    query += " AND fdi.TYPE_ID = @TerminalType ";
                }
                if (!string.IsNullOrEmpty(exportParams.Date))
                {
                    query += " AND t1.date = @Date ";
                }
                switch (exportParams.Status)
                {
                    case "Complete":
                        query += " AND t1.transaction_history = t1.transaction_monitoring ";
                        break;
                    case "N/A":
                        query += " AND t1.transaction_history > t1.transaction_monitoring ";
                        break;
                    case "Incomplete":
                        query += " AND t1.transaction_history < t1.transaction_monitoring ";
                        break;
                    default: break;
                }
                // Execute query and fetch data
                List<EJReportMonitorModel> filteredData = new List<EJReportMonitorModel>();
                using (MySqlConnection conn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(exportParams.Terminalno))
                            cmd.Parameters.AddWithValue("@TerminalNo", "%" + exportParams.Terminalno + "%");
                        if (!string.IsNullOrEmpty(exportParams.Terminaltype))
                            cmd.Parameters.AddWithValue("@TerminalType", exportParams.Terminaltype);
                        if (!string.IsNullOrEmpty(exportParams.Date))
                            cmd.Parameters.AddWithValue("@Date", exportParams.Date);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                filteredData.Add(new EJReportMonitorModel
                                {
                                    TerminalId = reader["terminalId"].ToString(),
                                    TermSeq = reader["TERM_SEQ"].ToString(),
                                    TermName = reader["TERM_NAME"].ToString(),
                                    Date = reader["date"].ToString(),
                                    TransactionHistory = reader["transaction_history"].ToString(),
                                    TransactionMonitoring = reader["transaction_monitoring"].ToString(),
                                    Diff = Convert.ToInt32(reader["diff"]),
                                    Status = reader["status"].ToString()
                                });
                            }
                        }
                    }
                }

                // Validate data
                if (filteredData == null || filteredData.Count == 0)
                    return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                // Generate Excel file
                string strPath = Environment.CurrentDirectory;
                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");

                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                ExcelUtilities_EJReportMonitor obj = new ExcelUtilities_EJReportMonitor
                {
                    PathDefaultTemplate = folder_name
                };
                obj.GatewayOutput(filteredData);

                // Define source and destination paths
                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;

                fname = "EJLogMonitor_" + DateTime.Now.ToString("yyyyMMdd");
                strPathDesc = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname + ".xlsx";

                if (obj.FileSaveAsXlsxFormat != null)
                {
                    if (System.IO.File.Exists(strPathDesc))
                        System.IO.File.Delete(strPathDesc);

                    if (!System.IO.File.Exists(strPathDesc))
                    {
                        System.IO.File.Copy(strPathSource, strPathDesc);
                        System.IO.File.Delete(strPathSource);
                    }

                    strSuccess = "S";
                    strErr = "";
                }
                else
                {
                    fname = "";
                    strSuccess = "F";
                    strErr = "Data Not Found";
                }

                return Json(new { success = strSuccess, filename = fname, errstr = strErr });
            }
            catch (Exception ex)
            {
                return Json(new { success = "F", filename = "", errstr = ex.Message.ToString() });
            }
        }




        [HttpGet]
        public ActionResult EJLogMonitor_DownloadExportFile(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            string tsDate = "";
            string teDate = "";
            try
            {




                fname = "EJLogMonitor_" + DateTime.Now.ToString("yyyyMMdd");

                switch (rpttype.ToLower())
                {
                    case "csv":
                        fname = fname + ".csv";
                        break;
                    case "pdf":
                        fname = fname + ".pdf";
                        break;
                    case "xlsx":
                        fname = fname + ".xlsx";
                        break;
                }

                tempPath = Path.GetFullPath(Environment.CurrentDirectory + _myConfiguration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname);




                if (rpttype.ToLower().EndsWith("s") == true)
                    return File(tempPath + "xml", "application/vnd.openxmlformats-officedocument.spreadsheetml", fname);
                else if (rpttype.ToLower().EndsWith("f") == true)
                    return File(tempPath + "xml", "application/pdf", fname);
                else  //(rpttype.ToLower().EndsWith("v") == true)
                    return PhysicalFile(tempPath, "application/vnd.ms-excel", fname);



            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = "Download Method : " + ex.Message;
                return Json(new
                {
                    success = false,
                    fname
                });
            }
        }
        #endregion
    }
}
