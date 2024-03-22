using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SLA_Management.Commons;
using SLA_Management.Data;
using SLA_Management.Data.ExcelUtilitie;
using SLA_Management.Models.OperationModel;
using System;
using System.Data;
using System.Globalization;
using System.Text;
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
            string fromDateStr = model.fromDate.ToString("yyyy-MM-dd HH:mm:dd")??DateTime.Now.ToString("yyyy-MM-dd HH:mm:dd");
            string toDateStr = model.toDate.ToString("yyyy-MM-dd HH:mm:dd") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:dd");
            string terminalStr = model.terminalId??"";
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
            if(terminaltypeStr != "")
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
                            terminalid) AS _" + fromDate.ToString("yyyyMMdd") + " ON fdi.TERM_ID = _" + fromDate.ToString("yyyyMMdd")+".terminalid");

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
                            finalQuery += @" join (SELECT @row_number:=0) AS t   where fdi.TERM_ID is not null "+ terminalFinalQuery + " order by fdi.TERM_SEQ asc ";
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
                            terminalid) AS _" + fromDate.ToString("yyyyMMdd") + " ON fdi.TERM_ID = _" + fromDate.ToString("yyyyMMdd")+".terminalid");

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
                                       where fdi.TERM_ID is not null "+ terminalFinalQuery + " order by fdi.TERM_SEQ asc ";
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
            catch(Exception ex)
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
        #region Excel TransactionSummary

        [HttpGet]
        public ActionResult TransactionSummary_ExportExc(string terminalId, DateTime fromDate, DateTime toDate, string terminalType, string transactionType)
        {
            string fname = "";
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;
            string fromDateStr = fromDate.ToString("yyyy-MM-dd HH:mm:dd")??DateTime.Now.ToString("yyyy-MM-dd HH:mm:dd");
            string toDateStr = toDate.ToString("yyyy-MM-dd HH:mm:dd") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:dd");
            string terminalStr = terminalId??"";
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

                if(trxtpyeStr != "")
                {
                    trxtpyeStr = "_" + trxtpyeStr;
                }

                fname = "TransactionSummary_"+ DateTime.Now.ToString("yyyyMMdd") + trxtpyeStr;

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
        public ActionResult DownloadExportFile_TransactionSummary(string rpttype,string transactionType)
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

    }
}
