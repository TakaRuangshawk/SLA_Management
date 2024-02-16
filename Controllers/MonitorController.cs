using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SLA_Management.Commons;
using SLA_Management.Data;
using SLA_Management.Data.ExcelUtilitie;
using SLA_Management.Models.OperationModel;
using System.Data;
using System.Text;
using static SLA_Management.Controllers.ReportController;

namespace SLA_Management.Controllers
{
    public class MonitorController : Controller
    {
        private IConfiguration _myConfiguration;
        private static ConnectMySQL db_fv;
        public MonitorController(IConfiguration myConfiguration)
        {

            _myConfiguration = myConfiguration;
            db_fv = new ConnectMySQL(myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection"));
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
            string terminalQuery = "";
            DateTime fromDate = DateTime.Parse(fromDateStr);
            DateTime toDate = DateTime.Parse(toDateStr);
            if (terminalStr != "")
            {
                terminalQuery += " and terminalid = '" + terminalStr + "' ";
            }
            if(terminaltypeStr != "")
            {
                terminalQuery += " and terminalid like '%" + terminaltypeStr + "' ";
            }
            switch (trxtpyeStr)
            {
                case "Deposit":
                    terminalQuery += " AND trx_type IN ('DEP_DCA', 'DEP_DCC') ";
                    break;
                case "Withdraw":
                    terminalQuery += " AND trx_type IN ('FAS' , 'MCASH' , 'WDL','CL_WDL') ";
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
            finalQuery += @" left JOIN fv_device_info fdi on _"+ fromDate.ToString("yyyyMMdd") +".terminalid = fdi.TERM_ID " +
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
            string finalQuery = string.Empty;
            List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();
            fromDate = DateTime.Parse(fromDateStr);
            toDate = DateTime.Parse(toDateStr);
            if (terminalStr != "")
            {
                terminalQuery += " and terminalid = '" + terminalStr + "' ";
            }
            if(terminaltypeStr != "")
            {
                terminalQuery += " and terminalid like '%" + terminaltypeStr + "' ";
            }
            switch (trxtpyeStr)
            {
                case "Deposit":
                    terminalQuery += " AND trx_type IN ('DEP_DCA', 'DEP_DCC') ";
                    break;
                case "Withdraw":
                    terminalQuery += " AND trx_type IN ('FAS' , 'MCASH' , 'WDL','CL_WDL') ";
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
            finalQuery += @" left JOIN fv_device_info fdi on _"+ fromDate.ToString("yyyyMMdd") +".terminalid = fdi.TERM_ID " +
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



                fname = "TransactionSummary_"+ DateTime.Now.ToString("yyyyMMdd");

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
        public ActionResult DownloadExportFile_TransactionSummary(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            string tsDate = "";
            string teDate = "";
            try
            {




                fname = "TransactionSummary_" + DateTime.Now.ToString("yyyyMMdd");

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
