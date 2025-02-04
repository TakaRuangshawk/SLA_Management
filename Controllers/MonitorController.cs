using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using OfficeOpenXml;
using PagedList;
using SLA_Management.Commons;
using SLA_Management.Data;
using SLA_Management.Data.ExcelUtilitie;
using SLA_Management.Data.HealthCheck;
using SLA_Management.Data.Monitor;
using SLA_Management.Data.RecurringCasesMonitor;
using SLA_Management.Data.TermProb;
using SLA_Management.Models;
using SLA_Management.Models.Monitor;
using SLA_Management.Models.OperationModel;
using SLA_Management.Models.RecurringCasesMonitor;
using SLA_Management.Models.TermProbModel;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using static SLA_Management.Controllers.MaintenanceController;
using static SLA_Management.Controllers.ReportController;

namespace SLA_Management.Controllers
{
    public class MonitorController : Controller
    {
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");
        private IConfiguration _myConfiguration;
        private static ConnectMySQL db_fv;
        private static ConnectMySQL db_all;
        private static List<LastTransactionModel> lasttransaction_dataList = new List<LastTransactionModel>();
        private static List<CardRetainModel> cardretain_dataList = new List<CardRetainModel>();
        private static List<TransactionModel> transaction_dataList = new List<TransactionModel>();
        private static ej_trandada_seek param = new ej_trandada_seek();
        private static List<HealthCheckModel> healthCheck_dataList = new List<HealthCheckModel>();
        RecurringCasesMonitorViewModel vm = new RecurringCasesMonitorViewModel();
        RecurringCasesDataContext dbContext;
        private static List<LogAnalysisModel> logAnalysis_dataList = new List<LogAnalysisModel>();


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
        public MonitorController(IConfiguration myConfiguration)
        {

            _myConfiguration = myConfiguration;
            db_fv = new ConnectMySQL(myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection"));
            db_all = new ConnectMySQL(myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection"));

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


        #region Health Check by boom

        [HttpGet]
        public IActionResult HealthCheck(string cmdButton, string TermID, string FrDate, string ToDate, string FrTime, string ToTime
        , string currTID, string currFr, string currTo, string currFrTime, string currToTime, string lstPageSize
        , string ddlProbMaster, string currProbMaster, string MessErrKeyWord, string currMessErrKeyWord
        , string currPageSize, int? page, string maxRows, string KeyWordList, string terminalType, string startDate, string bankName)
        {

            List<HealthCheckModel> recordset = new List<HealthCheckModel>();
            DBService_HealthCheck dBService;

            bool noBankSelect = false;

            //if (bankName == null) bankName = "BAAC";

            switch (bankName)
            {
                case "BAAC":
                    dBService = new DBService_HealthCheck(_myConfiguration, _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_baac"));
                    break;
                case "ICBC":
                    dBService = new DBService_HealthCheck(_myConfiguration, _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_icbc"));
                    break;
                case "BOC":
                    dBService = new DBService_HealthCheck(_myConfiguration, _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_boct"));
                    break;
                default:
                    noBankSelect = true;
                    dBService = new DBService_HealthCheck(_myConfiguration);
                    break;
            }

            if (startDate == null || startDate == "")
                startDate = DateTime.Now.ToString("yyyy-MM-dd");

            ViewBag.startDate = startDate;



            healthCheck_dataList.Clear();

            int pageNum = 1;

            try
            {


                if (cmdButton == "Clear")
                    return RedirectToAction("HealthCheck");

                if (null == TermID && null == FrDate && null == ToDate && null == page)
                {

                    FrDate = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
                    ToDate = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);

                    page = 1;
                }
                else
                {

                    FrDate = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
                    ToDate = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
                    // Return temp value back to it own variable

                    FrTime = (FrTime ?? currFrTime);
                    ToTime = (ToTime ?? currToTime);

                    FrDate = (FrDate ?? currFr);
                    ToDate = (ToDate ?? currTo);
                    FrTime = (FrTime ?? currFrTime);
                    ToTime = (ToTime ?? currToTime);
                    TermID = (TermID ?? currTID);
                    ddlProbMaster = (ddlProbMaster ?? currProbMaster);
                    MessErrKeyWord = (MessErrKeyWord ?? currMessErrKeyWord);
                }

                List<Device_info_record> device_Info_Records = new List<Device_info_record>();


                if (DBService.CheckDatabase() && !noBankSelect)
                {

                    recordset = dBService.GetAllTerminalHaveErrorSLA445(FrDate + " 00:00:00", ToDate + " 23:59:59");

                    device_Info_Records = dBService.GetDeviceInfoFeelview();


                    ViewBag.ConnectDB = "true";
                }
                else
                {
                    recordset = recordset ?? new List<HealthCheckModel>();
                    ViewBag.ConnectDB = "false";
                }

                var additionalItems = device_Info_Records.Select(x => x.TYPE_ID).Distinct();
                if (bankName == "BAAC")
                {
                    additionalItems = device_Info_Records.Select(x => x.COUNTER_CODE).Distinct();
                }

                var item = new List<string> { "All" };


                ViewBag.probTermStr = new SelectList(additionalItems.Concat(item).ToList());


                ViewBag.CurrentTID = device_Info_Records;
                ViewBag.TermID = TermID;
                ViewBag.CurrentFr = (FrDate ?? currFr);
                ViewBag.CurrentTo = (ToDate ?? currTo);
                ViewBag.CurrentPageSize = (lstPageSize ?? currPageSize);

                #region Set param
                bool chk_date = false;
                if (null == TermID)
                    param.TERMID = currTID == null ? "" : currTID;
                else
                    param.TERMID = TermID == null ? "" : TermID;

                if ((FrDate == null && currFr == null) && (FrDate == "" && currFr == ""))
                {
                    param.FRDATE = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
                    chk_date = false;
                }
                else
                {
                    if ((FrTime == "" && currFrTime == "") || (FrTime == null && currFrTime == null) ||
                        (FrTime == null && currFrTime == "") || (FrTime == "" && currFrTime == null))
                        param.FRDATE = FrDate + " 00:00:00";
                    else
                        param.FRDATE = FrDate + " " + FrTime;
                    chk_date = true;
                }

                if ((ToDate == null && currTo == null) && (ToDate == "" && currTo == ""))
                {
                    param.TODATE = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
                }
                else
                {
                    if ((ToTime == "" && currToTime == "") || (ToTime == null && currToTime == null) ||
                        (ToTime == null && currToTime == "") || (ToTime == "" && currToTime == null))
                        param.TODATE = ToDate + " 23:59:59";
                    else
                        param.TODATE = ToDate + " " + ToTime;
                }

                if (ddlProbMaster == null && currProbMaster == null)
                    param.PROBNAME = "All";
                else
                    param.PROBNAME = ddlProbMaster == null ? currProbMaster : ddlProbMaster;

                if (MessErrKeyWord == null && currMessErrKeyWord == null)
                    param.PROBKEYWORD = "";
                else
                    param.PROBKEYWORD = MessErrKeyWord == null ? currMessErrKeyWord : MessErrKeyWord;

                if (null != lstPageSize || null != currPageSize)
                {
                    param.PAGESIZE = String.IsNullOrEmpty(lstPageSize) == true ?
                        int.Parse(currPageSize) : int.Parse(lstPageSize);
                }
                else
                    param.PAGESIZE = 20;


                param.MONTHPERIOD = "";
                param.YEARPERIOD = "";
                param.TRXTYPE = "";

                #endregion

                #region Set page
                long recCnt = 0;

                if (String.IsNullOrEmpty(maxRows))
                    ViewBag.maxRows = "50";
                else
                    ViewBag.maxRows = maxRows;

                if (recordset.Count > 0)
                {
                    if (bankName == "BAAC")
                    {
                        switch (terminalType)
                        {
                            case "LOT572":
                                recordset.RemoveAll(item => item.Terminal_Type == "LOT587");
                                break;
                            case "LOT587":
                                recordset.RemoveAll(item => item.Terminal_Type == "LOT572");
                                break;
                            default:
                                break;
                        }
                    }


                    if (TermID != null)
                    {
                        recordset.RemoveAll(item => item.Terminal_ID != TermID);
                    }




                }


                if (recordset.Count <= 0)
                {
                    ViewBag.NoData = "true";
                    param.PAGESIZE = 1;
                }
                else
                {
                    recCnt = recordset.Count;
                    healthCheck_dataList = recordset;
                    param.PAGESIZE = recordset.Count;
                }




                if (recCnt > 0)
                {
                    ViewBag.Records = String.Format("{0:#,##0}", recCnt.ToString());
                }
                else
                    ViewBag.Records = "0";

                pageNum = (page ?? 1);

                int amountrecordset = recordset.Count();

                if (amountrecordset > 5000)
                {
                    recordset.RemoveRange(5000, amountrecordset - 5000);
                }
                #endregion

                if (recordset.Count > 0)
                {


                    recordset = recordset
     .OrderBy(x => x.Status == "true" ? 0 : (x.Status == "N/A" ? 1 : 2))
     .ThenByDescending(x => x.Status == "false")
     .ThenBy(x => x.Transaction_DateTime)
     .ToList();

                }



            }
            catch (Exception)
            {

            }
            return View(recordset.ToPagedList(pageNum, (int)param.PAGESIZE == 0 ? 1 : (int)param.PAGESIZE));
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
            string finalQuery = string.Empty;
            List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();
            fromDate = DateTime.Parse(fromDateStr);
            toDate = DateTime.Parse(toDateStr);
            if (terminalStr != "")
            {
                terminalQuery += " and terminalid = '" + terminalStr + "' ";
            }
            if (terminaltypeStr != "")
            {
                terminalQuery += " and terminalid like '%" + terminaltypeStr + "' ";
            }
            switch (trxtpyeStr)
            {
                case "Deposit":
                    terminalQuery += " AND trx_type IN ('DEP_DCA' , 'DEP_DCC' , 'DEP_P00', 'DEP_P01','RFT_DCA') ";
                    break;
                case "Withdraw":
                    terminalQuery += " AND trx_type IN ('FAS' , 'MCASH' , 'WDL','CL_WDL') ";
                    break;
                case "":
                    terminalQuery += " AND trx_type IN ('DEP_DCA' , 'DEP_DCC' , 'DEP_P00', 'DEP_P01','RFT_DCA','FAS' , 'MCASH' , 'WDL','CL_WDL') ";
                    break;
                default:
                    terminalQuery += " AND trx_type IN ('DEP_DCA' , 'DEP_DCC' , 'DEP_P00', 'DEP_P01','RFT_DCA','FAS' , 'MCASH' , 'WDL','CL_WDL') ";
                    break;
            }
            StringBuilder queryBuilder = new StringBuilder();
            if (fromDate.ToString("yyyy-MM-dd") == toDate.ToString("yyyy-MM-dd"))
            {
                queryBuilder.AppendLine(@"  SELECT  (@row_number:=@row_number + 1) AS No,_" + fromDate.ToString("yyyyMMdd") + ".terminalid as TerminalNo,fdi.TERM_NAME as TerminalName,fdi.TYPE_ID as TerminalType,fdi.TERM_SEQ as DeviceSerialNo, COALESCE(_" + fromDate.ToString("yyyyMMdd") + "._" + fromDate.ToString("yyyyMMdd") + ",0) as _" + fromDate.ToString("yyyyMMdd") + "");
            }
            else
            {
                queryBuilder.AppendLine(@"  SELECT  (@row_number:=@row_number + 1) AS No,_" + fromDate.ToString("yyyyMMdd") + ".terminalid as TerminalNo,fdi.TERM_NAME as TerminalName,fdi.TYPE_ID as TerminalType,fdi.TERM_SEQ as DeviceSerialNo, COALESCE(_" + fromDate.ToString("yyyyMMdd") + "._" + fromDate.ToString("yyyyMMdd") + ",0) as _" + fromDate.ToString("yyyyMMdd") + ",");
            }

            for (DateTime date = fromDate.AddDays(1); date <= toDate; date = date.AddDays(1))
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

            queryBuilder.AppendLine(@"FROM
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
                terminalid) AS _" + fromDate.ToString("yyyyMMdd"));

            for (DateTime date = fromDate.AddDays(1); date <= toDate; date = date.AddDays(1))
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
                _" + fromDate.ToString("yyyyMMdd") + ".terminalid = _" + dateStr + @".terminalid");
            }

            finalQuery = queryBuilder.ToString();
            finalQuery += @" left JOIN fv_device_info fdi on _" + fromDate.ToString("yyyyMMdd") + ".terminalid = fdi.TERM_ID " +
                " join (SELECT @row_number:=0) AS t";

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
                    string query = " SELECT adi.term_seq as term_seq, t1.terminalid AS term_id, adi.term_name as term_name, SUBSTRING_INDEX(SUBSTRING_INDEX(UPPER(t1.remark), 'CARD NUMBER : ', -1), ' ', 4) AS card_number, t2.trxdatetime AS trxdatetime_ejreport FROM ejlog_devicetermprob_ejreport t1 LEFT JOIN ejlog_devicetermprob t2 ON t1.terminalid = t2.terminalid AND t1.trxdatetime BETWEEN DATE_SUB(t2.trxdatetime, INTERVAL 3 minute) AND t2.trxdatetime join fv_device_info adi on t1.terminalid = adi.term_id WHERE t1.probcode = 'LASTTRANS_14' AND (t2.probcode = 'DEVICE05' OR t2.probcode = 'DEVICE14') AND (t2.remark LIKE '%RETAIN CARD%' or t2.remark like '%CARD RETAINED FAILED%') and t1.trxdatetime between '" + DateTime.Now.AddDays(-120).ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") + "' ";
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

        #region RecurringCases
        [HttpGet]
        public IActionResult RecurringCasesMonitor(string bankName,string termID, string frDate,string toDate,string terminalType,int page, int maxRows,string orderBy)
        {
            
            if (bankName == null) bankName = "BAAC";

            switch (bankName.ToUpper())
            {
                case "BAAC":
                    dbContext = new RecurringCasesDataContext(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_baac"));
                    break;
                case "ICBC":
                    dbContext = new RecurringCasesDataContext(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_icbc"));
                    break;
                case "BOC":
                    dbContext = new RecurringCasesDataContext(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_boct"));
                    break;
                default:
                    dbContext = new RecurringCasesDataContext("");
                    break;
            }
            vm.Device_Info_Records = dbContext.GetDeviceInfoFeelview();
            
            if (bankName.ToUpper() == "BAAC")
            {
                vm.TerminalTypes = vm.Device_Info_Records
                                   .Where(x => !string.IsNullOrWhiteSpace(x.COUNTER_CODE))
                                   .DistinctBy(x => x.COUNTER_CODE)
                                   .Select(x => new TerminalType { Terminal_Type = x.COUNTER_CODE }).ToList();
            }
            else
            {
                vm.TerminalTypes = vm.Device_Info_Records
                               .Where(x => !string.IsNullOrWhiteSpace(x.TYPE_ID))
                               .DistinctBy(x => x.TYPE_ID)
                               .Select(x => new TerminalType { Terminal_Type = x.TYPE_ID }).ToList();
            }
            vm.RecurringCases = dbContext.GetRecurringTerminalList(termID,frDate,toDate,terminalType,orderBy);
            if (vm.RecurringCases != null) 
            {
                //Apply pagination
                int recurCount = vm.RecurringCases.Count;
                int totalPages = (int)Math.Ceiling((double)recurCount / maxRows);
                if (page > totalPages)
                {
                    page = 1;  // Reset to first page to show available data
                }
                var paginatedRecurringList = vm.RecurringCases.Skip((page - 1) * maxRows).Take(maxRows).ToList();
                vm.TotalRecords = recurCount;
                vm.RecurringCases = paginatedRecurringList;
                vm.CurrentPage = page;
                vm.TotalPages = totalPages;
                vm.PageSize = maxRows;
            }
            vm.frDate = frDate;
            vm.toDate = toDate;
            vm.bank = bankName;
            vm.selectedTerminal = termID;
            vm.selectedTerminalType = terminalType;
            return View(vm);
        }

        //getting Detail
        [HttpGet]
        public async Task<IActionResult> GetRecurringCasesList(string bankName,string termID,string frDate,string toDate,string TermName,string SerialNo)
        {
            List<RecurringCaseDetail> detailList = new List<RecurringCaseDetail>();
            vm.RecurringCase = new RecurringCase();
            switch (bankName.ToUpper())
            {
                case "BAAC":
                    dbContext = new RecurringCasesDataContext(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_baac"));
                    break;
                case "ICBC":
                    dbContext = new RecurringCasesDataContext(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_icbc"));
                    break;
                case "BOC":
                    dbContext = new RecurringCasesDataContext(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_boct"));
                    break;
                default:
                    dbContext = new RecurringCasesDataContext("");
                    break;
            }
            detailList = dbContext.GetRecurringCaseDetailsList(termID,frDate,toDate);
            vm.RecurringCaseDetails = detailList;
            vm.RecurringCase.Terminal_Id = termID;
            vm.RecurringCase.Terminal_Name= TermName;
            vm.RecurringCase.Serial_No= SerialNo;
            return PartialView("_PartialRecurringCasesDetail", vm);
        }

        //export Excel
        [HttpPost]
        public ActionResult RecurringCases_ExportXlsx([FromBody] ExportModel exportModel)
        {
            string fname = "";
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;
            string conn ="";
            try
            {
                if (exportModel.bankName == null) exportModel.bankName = "BAAC";
                
                switch (exportModel.bankName.ToUpper())
                {
                    case "BAAC":
                        dbContext = new RecurringCasesDataContext(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_baac"));
                        break;
                    case "ICBC":
                        dbContext = new RecurringCasesDataContext(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_icbc"));
                        break;
                    case "BOC":
                        dbContext = new RecurringCasesDataContext(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_boct"));
                        break;
                    default:
                        dbContext = new RecurringCasesDataContext("");
                        break;
                }
                vm.RecurringCases = dbContext.GetRecurringTerminalList(exportModel.termID, exportModel.frDate, exportModel.toDate, exportModel.terminalType, exportModel.orderBy);
                if (vm.RecurringCases == null || vm.RecurringCases.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;
                ExcelUtilities_RecurringCases obj = new ExcelUtilities_RecurringCases();
                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(vm.RecurringCases);


                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;



                fname = "RecurringCases_" + DateTime.Now.ToString("yyyyMMdd");

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
        public ActionResult RecurringCases_DownloadExportFile(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            try
            {




                fname = "RecurringCases_" + DateTime.Now.ToString("yyyyMMdd");

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

        #region LogAnalysisMonitor
        [HttpGet]

        public IActionResult LogAnalysis(string bankName, string TermID, string FrDate, string ToDate, string FrTime, string ToTime, string counterCode, string currTID, string clearButton,
            int? page, string currFr, string currTo, string currFrTime, string currToTime, string categoryType, string lstPageSize, string currPageSize, string maxRows)
        {
            List<LogAnalysisModel> recordset = new List<LogAnalysisModel>();
            DBService_LogAnalysis dBService;
            if (bankName == null) bankName = "BAAC";

            switch (bankName)
            {
                case "BAAC":
                    dBService = new DBService_LogAnalysis(_myConfiguration, _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_baac") ?? "");
                    break;
                case "ICBC":
                    dBService = new DBService_LogAnalysis(_myConfiguration, _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_icbc") ?? "");
                    break;
                case "BOC":
                    dBService = new DBService_LogAnalysis(_myConfiguration, _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_boct") ?? "");
                    break;
                default:
                    dBService = new DBService_LogAnalysis(_myConfiguration);
                    break;
            }

            logAnalysis_dataList.Clear();
            int pageNum = 1;

            try
            {
                if (DBService.CheckDatabase())
                {
                    recordset = dBService.FetchAllData(TermID, FrDate + " 00:00:00", ToDate + " 23:59:59", categoryType, counterCode);

                    ViewBag.ConnectDB = "true";
                }
                else
                {
                    ViewBag.ConnectDB = "false";
                }

                List<Device_info_record> device_Info_Records = dBService.GetDeviceInfo();
                var additionalItems = device_Info_Records.Select(x => x.COUNTER_CODE).Where(x => !string.IsNullOrEmpty(x)).Distinct();
                var item = new List<string> { "ALL" }.Concat(additionalItems).ToList();
                ViewBag.counterCode = new SelectList(item.Select(x => new { Value = x, Text = x }), "Value", "Text");

                List<Sisbu_analysis_record> sisbu_Analysis_Records = dBService.GetCategory();
                var additionalCategory = sisbu_Analysis_Records.Select(x => x.incident_name).Distinct();
                var categoryitem = new List<string> { "ALL" }.Concat(additionalCategory).ToList();
                ViewBag.category = new SelectList(categoryitem.Select(x => new { Value = x, Text = x }), "Value", "Text");


                List<TotalCase_record> TotalCase_Records = dBService.FetchTotalCaseData(TermID, FrDate + " 00:00:00", ToDate + " 23:59:59", categoryType, counterCode);
                ViewBag.TotalCaseData = TotalCase_Records;
                ViewBag.incident_name = TotalCase_Records.Select(x => x.incident_name);
                ViewBag.analyst_01 = TotalCase_Records.Select(x => x.analyst_01);
                ViewBag.TotalCount = TotalCase_Records.Select(x => x.TotalCount);


                ViewBag.CurrentTID = device_Info_Records;


                ViewBag.TermID = TermID;
                ViewBag.CurrentFr = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
                ViewBag.CurrentTo = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
                ViewBag.CurrentPageSize = (lstPageSize ?? currPageSize);
                ViewBag.BankCode = bankName;

                #region Set param
                bool chk_date = false;
                if (null == TermID)
                    param.TERMID = currTID == null ? "" : currTID;
                else
                    param.TERMID = TermID == null ? "" : TermID;

                if ((FrDate == null && currFr == null) && (FrDate == "" && currFr == ""))
                {
                    param.FRDATE = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
                    chk_date = false;
                }
                else
                {
                    if ((FrTime == "" && currFrTime == "") || (FrTime == null && currFrTime == null) ||
                        (FrTime == null && currFrTime == "") || (FrTime == "" && currFrTime == null))
                        param.FRDATE = FrDate + " 00:00:00";
                    else
                        param.FRDATE = FrDate + " " + FrTime;
                    chk_date = true;
                }

                if ((ToDate == null && currTo == null) && (ToDate == "" && currTo == ""))
                {
                    param.TODATE = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
                }
                else
                {
                    if ((ToTime == "" && currToTime == "") || (ToTime == null && currToTime == null) ||
                        (ToTime == null && currToTime == "") || (ToTime == "" && currToTime == null))
                        param.TODATE = ToDate + " 23:59:59";
                    else
                        param.TODATE = ToDate + " " + ToTime;
                }

                if (null != lstPageSize || null != currPageSize)
                {
                    param.PAGESIZE = String.IsNullOrEmpty(lstPageSize) == true ?
                        int.Parse(currPageSize) : int.Parse(lstPageSize);
                }
                else
                    param.PAGESIZE = 20;

                #endregion

                #region Set page
                long recCnt = 0;

                if (String.IsNullOrEmpty(maxRows))
                    ViewBag.maxRows = "50";
                else
                    ViewBag.maxRows = maxRows;

                if (recordset.Count > 0)
                {
                    if (bankName == "BAAC")
                    {
                        switch (counterCode)
                        {
                            case "LOT572":
                                recordset.RemoveAll(item => item.Counter_Code == "LOT587");
                                break;
                            case "LOT587":
                                recordset.RemoveAll(item => item.Counter_Code == "LOT572");
                                break;
                            default:
                                break;
                        }
                    }

                    if (TermID != null)
                    {
                        recordset.RemoveAll(item => item.Terminal_ID != TermID);
                    }

                }

                if (null == recordset || recordset.Count <= 0)
                {
                    ViewBag.NoData = "true";

                }
                else
                {
                    recCnt = recordset.Count;
                    logAnalysis_dataList = recordset;
                    param.PAGESIZE = recordset.Count;
                }

                if (recCnt > 0)
                {
                    ViewBag.Records = String.Format("{0:#,##0}", recCnt.ToString());
                }
                else
                    ViewBag.Records = "0";

                pageNum = (page ?? 1);

                int amountrecordset = recordset.Count();

                if (amountrecordset > 5000)
                {
                    recordset.RemoveRange(5000, amountrecordset - 5000);
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching data.", ex);
            }
            return View(recordset.ToPagedList(pageNum, (int)param.PAGESIZE == 0 ? 1 : (int)param.PAGESIZE));
        }

        [HttpPost]
        public ActionResult LogAnalysis_ExportExc(LogAnalysisModel model)
        {
            string fname = "";
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;
            string filterquery = string.Empty;

            try
            {
                string bankCode = model.BankCode;
                string terminalId = model.Terminal_ID;
                string fromDate = model.FromDate;
                string toDate = model.ToDate;
                string category = model.Category;
                string countercode = model.Counter_Code;

                if (!string.IsNullOrEmpty(terminalId))
                {
                    filterquery += " a.TERM_ID LIKE '" + terminalId + "' AND";
                }
                if (!string.IsNullOrEmpty(category))
                {
                    filterquery += " a.incident_name LIKE '" + category + "' AND";
                }

                if (!string.IsNullOrEmpty(countercode))
                {
                    filterquery += " b.COUNTER_CODE LIKE '" + countercode + "' AND";
                }

                if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                {
                    filterquery += " (STR_TO_DATE(a.incident_date, '%Y-%m-%d') between '" + fromDate + "' and '" + toDate + "')";
                }
                else
                {
                    filterquery += " a.incident_date IS NULL";
                }

                List<LogAnalysisModel> jsonData = new List<LogAnalysisModel>();
                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + bankCode)))
                {
                    connection.Open();

                    string query = @"SELECT DISTINCT b.TERM_SEQ, a.TERM_ID, b.TERM_NAME,b.COUNTER_CODE, 
                    a.incident_name, CONVERT(COUNT(a.incident_name), CHAR) AS TotalCase                     
                    FROM sisbu_analysis a
                    INNER JOIN device_info b
                    ON a.TERM_ID = b.TERM_ID                    
                    WHERE ";

                    query += filterquery + " GROUP BY b.TERM_SEQ, a.TERM_ID, b.TERM_NAME,b.COUNTER_CODE,a.incident_name;";

                    MySqlCommand command = new MySqlCommand(query, connection);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            jsonData.Add(new LogAnalysisModel
                            {
                                Terminal_SEQ = reader["TERM_SEQ"].ToString() ?? "",
                                Terminal_ID = reader["TERM_ID"].ToString() ?? "",
                                Terminal_NAME = reader["TERM_NAME"].ToString() ?? "",
                                Category = reader["incident_name"].ToString() ?? "",
                                TotalCase = reader["TotalCase"].ToString() ?? "",
                            });
                        }
                    }
                }

                logAnalysis_dataList = jsonData;

                if (jsonData.Count == 0)
                {
                    return Json(new { success = "F", filename = "", errstr = "Data not found!" });
                }
                string strPath = Environment.CurrentDirectory;
                ExcelUtilities_LogAnalysis obj = new ExcelUtilities_LogAnalysis();

                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");

                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                // Generate the Excel file
                obj.LogAnalysisExcelOutput(logAnalysis_dataList);

                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;
                fname = "LogAnalysis_" + DateTime.Now.ToString("yyyyMMdd");

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
        public ActionResult LogAnalysis_DownloadExportFile(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            try
            {
                fname = "LogAnalysis_" + DateTime.Now.ToString("yyyyMMdd");

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

        [HttpPost]
        public IActionResult UpdateLogAnalysis(UpdateLogAnalysis model)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + model.BankCode)))
                {
                    connection.Open();

                    string query = @"UPDATE sisbu_analysis 
                        SET 
                        incident_name = @Category,
                        incident_date = @Incident_Date, 
                        analyst_01 = @SubCategory, 
                        analyst_02 = @Analyst_Info, 
                        inform_by = @Inform_By,   
                        update_date = @Update_date,
                        update_by = @Update_by
                        WHERE TERM_ID = @Terminal_ID AND incident_no = @Incident_No";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Category", model.Category ?? "");
                        if (DateTime.TryParse(model.Incident_Date, out DateTime incidentDate))
                        {
                            cmd.Parameters.AddWithValue("@Incident_Date", incidentDate.ToString("yyyy-MM-dd"));
                        }
                        cmd.Parameters.AddWithValue("@SubCategory", model.SubCategory ?? "");
                        cmd.Parameters.AddWithValue("@Analyst_Info", model.Analyst_Info ?? "");
                        cmd.Parameters.AddWithValue("@Inform_By", model.Inform_By ?? "");
                        cmd.Parameters.AddWithValue("@Terminal_ID", model.Terminal_ID ?? "");
                        cmd.Parameters.AddWithValue("@Update_date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Update_by", "System");
                        cmd.Parameters.AddWithValue("@Incident_No", model.Incident_No ?? "");
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true, message = "Data updated successfully." });
                        }
                        else
                        {
                            return Json(new { success = false, message = "No data found." });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult InsertTicketInfo(string bankCode, string addCategory, string addIncidentDate, string terminalId, string addSubCategory, string addAnalystInfo2, string addInform)
        {
            try
            {
                using (var connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + bankCode)))
                {
                    connection.Open();
                    int incident_no = 0;
                    string query = "SELECT MAX(incident_no) as incident_no from sisbu_analysis";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                incident_no = Convert.ToInt32(reader["incident_no"]);
                            }
                        }
                    }
                    string insertQuery = "INSERT INTO sisbu_analysis(incident_no,incident_name,incident_date,term_id,analyst_01,analyst_02,inform_by,update_date,update_by)" +
                        "values (@incident_no,@addCategory,@addIncidentDate,@terminalId,@addSubCategory,@addAnalystInfo2,@addInform,@updateDate,@updateBy)";
                    using (var cmd = new MySqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@incident_no", incident_no + 1);
                        cmd.Parameters.AddWithValue("@addCategory", addCategory);
                        cmd.Parameters.AddWithValue("@addIncidentDate", addIncidentDate);
                        cmd.Parameters.AddWithValue("@terminalId", terminalId);
                        cmd.Parameters.AddWithValue("@addSubCategory", addSubCategory);
                        cmd.Parameters.AddWithValue("@addAnalystInfo2", addAnalystInfo2);
                        cmd.Parameters.AddWithValue("@addInform", addInform);
                        cmd.Parameters.AddWithValue("@updateDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@updateBy", "System");
                        cmd.ExecuteNonQuery();
                    }
                }
                ViewBag.SuccessMessage = "Data Insert Successfully.";
            }
            catch
            {
                ViewBag.SuccessMessage = "Insert Fail!";
            }

            return Json(new { success = true, message = ViewBag.SuccessMessage });
        }

        #endregion

        #region Device Fireware
        [HttpGet]
        public IActionResult DeviceFirmware(string termID, string terminalType, string sort, int page, int maxRows)
        {
            if (maxRows == 0)
            {
                return RedirectToAction("DeviceFirmware", new { termID, terminalType, sort, page = 1, maxRows = 50 });
            }
            DeviceFirmwareViewModel viewModel = new DeviceFirmwareViewModel();
            List<DeviceFirmware> devices = new List<DeviceFirmware>();
            string conn = _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_baac");
            dbContext = new RecurringCasesDataContext(conn);

            viewModel.Device_Info_Records = dbContext.GetDeviceInfoFeelview();
            viewModel.TerminalTypes = viewModel.Device_Info_Records
                                   .Where(x => !string.IsNullOrWhiteSpace(x.COUNTER_CODE))
                                   .DistinctBy(x => x.COUNTER_CODE)
                                   .Select(x => new TermType { Terminal_Type = x.COUNTER_CODE }).ToList();
            using (var connection = new MySqlConnection(conn))
            {
                connection.OpenAsync();
                string procName = "GetDeviceFirmware";
                using (MySqlCommand cmd = new MySqlCommand(procName, connection))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@terminal", termID);
                    cmd.Parameters.AddWithValue("@counter_code", terminalType == "ALL" ? null : terminalType);
                    cmd.Parameters.AddWithValue("@sort", sort);

                    using (var reader = cmd.ExecuteReader())
                    {
                        int recordCount = 1;
                        while (reader.Read())
                        {
                            var devFirmware = new DeviceFirmware
                            {
                                No = recordCount,
                                Term_ID = reader["TERM_ID"].ToString(),
                                Term_SEQ = reader["TERM_SEQ"].ToString(),
                                Term_Name = reader["TERM_NAME"].ToString(),
                                PIN_Ver = reader["PIN_VERSION"].ToString(),
                                IDC_Ver = reader["IDC_VERSION"].ToString(),
                                PTR_Ver = reader["PTR_VERSION"].ToString(),
                                BCR_Ver = reader["BCR_VERSION"].ToString(),
                                SIU_Ver = reader["SIU_VERSION"].ToString(),
                                Update_Date = reader["UPDATE_DATE"].ToString()
                            };
                            devices.Add(devFirmware);
                            recordCount++;

                        }
                    }
                }

            }
            viewModel.DeviceFirmwareList = devices;
            viewModel.selectedTerminal = termID;
            viewModel.selectedTerminalType = terminalType;
            if (viewModel.DeviceFirmwareList != null)
            {
                //Apply pagination
                int devFirmwareCount = viewModel.DeviceFirmwareList.Count;
                int totalPages = (int)Math.Ceiling((double)devFirmwareCount / maxRows);
                if (page > totalPages)
                {
                    page = 1;  // Reset to first page to show available data
                }
                var paginatedRecurringList = viewModel.DeviceFirmwareList.Skip((page - 1) * maxRows).Take(maxRows).ToList();
                viewModel.TotalRecords = devFirmwareCount;
                viewModel.DeviceFirmwareList = paginatedRecurringList;
                viewModel.CurrentPage = page;
                viewModel.TotalPages = totalPages;
                viewModel.PageSize = maxRows;
            }
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult DeviceFirmware_ExportXlsx([FromBody] DeviceFirmwareExport export)
        {
            string fname = "";
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;
            string conn = _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_baac");
            DeviceFirmwareViewModel viewModel = new DeviceFirmwareViewModel();
            List<DeviceFirmware> devices = new List<DeviceFirmware>();
            try
            {
                using (var connection = new MySqlConnection(conn))
                {
                    connection.OpenAsync();
                    string procName = "GetDeviceFirmware";
                    using (MySqlCommand cmd = new MySqlCommand(procName, connection))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@terminal", export.termID);
                        cmd.Parameters.AddWithValue("@counter_code", export.terminalType == "ALL" ? null : export.terminalType);
                        cmd.Parameters.AddWithValue("@sort", export.sort);

                        using (var reader = cmd.ExecuteReader())
                        {
                            int recordCount = 1;
                            while (reader.Read())
                            {
                                var devFirmware = new DeviceFirmware
                                {
                                    No = recordCount,
                                    Term_ID = reader["TERM_ID"].ToString(),
                                    Term_SEQ = reader["TERM_SEQ"].ToString(),
                                    Term_Name = reader["TERM_NAME"].ToString(),
                                    PIN_Ver = reader["PIN_VERSION"].ToString(),
                                    IDC_Ver = reader["IDC_VERSION"].ToString(),
                                    PTR_Ver = reader["PTR_VERSION"].ToString(),
                                    BCR_Ver = reader["BCR_VERSION"].ToString(),
                                    SIU_Ver = reader["SIU_VERSION"].ToString(),
                                    Update_Date = Convert.ToDateTime(reader["UPDATE_DATE"]).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                                };
                                devices.Add(devFirmware);
                                recordCount++;

                            }
                        }
                    }

                }
                viewModel.DeviceFirmwareList = devices;
                if (viewModel.DeviceFirmwareList == null || viewModel.DeviceFirmwareList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });
                string strPath = Environment.CurrentDirectory;
                ExcelUtilities_DeviceFirmware obj = new ExcelUtilities_DeviceFirmware();
                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(viewModel.DeviceFirmwareList);


                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;



                fname = "DeviceFirmware_" + DateTime.Now.ToString("yyyyMMdd");

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
        public ActionResult DeviceFirmware_DownloadExportFile(string rpttype)
        {
            string fname = "";
            string tempPath = "";
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
        #endregion


    }
}
