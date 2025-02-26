﻿using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SLA_Management.Data;
using SLA_Management.Commons;
using SLA_Management.Models.OperationModel;
using System.Data;
using SLA_Management.Data.ExcelUtilitie;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Text;
using System.Security.Cryptography;
using Renci.SshNet;
using NuGet.DependencyResolver;
using SLA_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using SLA_Management.Models.TermProbModel;
using PagedList;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SLA_Management.Data.TermProb;
using System.Linq;

namespace SLA_Management.Controllers
{
    public class MaintenanceController : Controller
    {
        private IConfiguration _myConfiguration;
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");
        private static ConnectMySQL db_fv;
        private static List<InventoryMaintenanceModel> Inventory_dataList = new List<InventoryMaintenanceModel>();
        private static List<WhitelistFilterTemplateModel> WhitelistFilterTemplates_datalist = new List<WhitelistFilterTemplateModel>();

        private static ej_trandada_seek param = new ej_trandada_seek();


        public MaintenanceController(IConfiguration myConfiguration)
        {

            _myConfiguration = myConfiguration;
            db_fv = new ConnectMySQL(myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection"));
        }
        public IActionResult Index()
        {
            return View();
        }
        #region Inventory

        [HttpGet]
        public IActionResult InventoryMonitor(string bankcode, string termid, string ticket, DateTime? todate, DateTime? fromdate, string mainproblem, string terminaltype, string jobno, string cmdButton)
        {
            ViewBag.maxRows = 50;
            bankcode = bankcode ?? "";
            if (bankcode != "")
            {
                ViewBag.CurrentTID = GetDeviceInfoFeelview(bankcode, _myConfiguration);
                ViewBag.CurrentTSeq = GetSerialNo(bankcode, _myConfiguration);
                ViewBag.CurrentCC = GetCounterCode(bankcode, _myConfiguration);
                ViewBag.CurrentST = GetServiceType(bankcode, _myConfiguration);
                ViewBag.CurrentType = GetTerminalType(bankcode, _myConfiguration);
            }
            else
            {
                ViewBag.CurrentTID = new List<Device_info_record>();
                ViewBag.CurrentTSeq = new List<SerialNo>();
                ViewBag.CurrentCC = new List<COUNTERCODE>();
                ViewBag.CurrentST = new List<SERVICETYPE>();
                ViewBag.CurrentType = new List<TERMINALTYPE>();
            }
            ViewBag.bankcode = bankcode;
            string FrDate = DateTime.Now.ToString("yyyy-MM-dd");
            string ToDate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.CurrentFr = FrDate;
            ViewBag.CurrentTo = ToDate;
            ViewBag.TERM_TYPE = terminaltype;

            return View();
        }
        [HttpGet]
        public IActionResult InventoryFetchData(string bankcode, string terminalseq, string terminalno, string terminaltype, string connencted, string servicetype, string countertype, string status, string row, string page, string search, string fromdate, string todate, string currentlyinuse)
        {
            int _page;
            string filterquery = string.Empty;
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
            terminalseq = terminalseq ?? "";
            terminaltype = terminaltype ?? "";
            connencted = connencted ?? "";
            servicetype = servicetype ?? "";
            countertype = countertype ?? "";
            fromdate = fromdate ?? "";
            todate = todate ?? "";
            status = status ?? "";
            bankcode = bankcode ?? string.Empty;
            if (terminalno != "")
            {
                filterquery += " and di.TERM_ID like '%" + terminalno + "%' ";
            }
            if (terminalseq != "")
            {
                filterquery += " and di.TERM_SEQ = '" + terminalseq + "' ";

            }
            if (terminaltype != "")
            {
                filterquery += " and di.TYPE_ID = '" + terminaltype + "' ";
            }
            if (status == "use")
            {
                filterquery += " and (di.STATUS = 'use' or di.STATUS ='roustop') ";
            }
            else if (status == "notuse")
            {
                filterquery += " and di.STATUS = 'no' ";
            }

            if (servicetype != "")
            {
                filterquery += " and CONCAT(di.SERVICE_TYPE, ' ', di.BUSINESS_BEGINTIME, ' - ', di.BUSINESS_ENDTIME) = '" + servicetype + "' ";
            }
            if (countertype != "")
            {
                filterquery += " and di.COUNTER_CODE = '" + countertype + "' ";
            }
            if (fromdate != "" && todate != "")
            {
                filterquery += " AND (STR_TO_DATE(di.SERVICE_ENDDATE, '%Y-%m-%d') BETWEEN '" + fromdate + "' AND '" + todate + "' ";
                filterquery += " OR (LENGTH(TRIM(di.SERVICE_ENDDATE)) = 0 AND (STR_TO_DATE(di.SERVICE_BEGINDATE, '%Y-%m-%d') < '" + todate + "' ";
                filterquery += " OR STR_TO_DATE(di.SERVICE_BEGINDATE, '%Y/%m/%d') < '" + todate + "'))) ";
            }
            else
            {
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                filterquery += " AND (STR_TO_DATE(di.SERVICE_ENDDATE, '%Y-%m-%d') BETWEEN '2020-05-01' AND '" + currentDate + "' ";
                filterquery += "OR (LENGTH(TRIM(di.SERVICE_ENDDATE)) = 0 AND (STR_TO_DATE(di.SERVICE_BEGINDATE, '%Y-%m-%d') < '" + currentDate + "' ";
                filterquery += "OR STR_TO_DATE(di.SERVICE_BEGINDATE, '%Y/%m/%d') < '" + currentDate + "'))) ";
            }
            if (currentlyinuse == "no")
            {
                filterquery += " and di.TERM_SEQ IN (SELECT TERM_SEQ FROM device_info GROUP BY TERM_SEQ HAVING COUNT(DISTINCT status) = 1 AND MAX(status) = 'no') ";
            }
            else if (currentlyinuse == "yes")
            {
                filterquery += " and di.TERM_SEQ NOT IN (SELECT TERM_SEQ FROM device_info GROUP BY TERM_SEQ HAVING COUNT(DISTINCT status) = 1 AND MAX(status) = 'no') ";
            }
            List<InventoryMaintenanceModel> jsonData = new List<InventoryMaintenanceModel>();

            using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + bankcode)))
            {
                connection.Open();

                // Modify the SQL query to use the 'input' parameter for filtering
                string query = @" SELECT di.DEVICE_ID,di.TERM_SEQ,di.TYPE_ID,di.TERM_ID,di.TERM_NAME,di.TERM_IP,
                CASE WHEN SERVICE_ENDDATE IS NULL OR SERVICE_ENDDATE = '' THEN 'Active' ELSE 'Inactive' END AS Status,
                di.COUNTER_CODE,CONCAT(di.SERVICE_TYPE, ' ', di.BUSINESS_BEGINTIME, ' - ', di.BUSINESS_ENDTIME) as ServiceType,
                di.TERM_LOCATION,di.LATITUDE,di.LONGITUDE,di.CONTROL_BY,di.PROVINCE,DATE_FORMAT(
                     COALESCE(
                         STR_TO_DATE(di.SERVICE_BEGINDATE, '%Y-%m-%d'),
                         STR_TO_DATE(di.SERVICE_BEGINDATE, '%Y/%m/%d')
                     ), 
                     '%Y-%m-%d'
                 ) AS SERVICE_BEGINDATE,
				di.SERVICE_ENDDATE,di.VERSION_AGENT
				FROM device_info di
                where di.TERM_ID is not null ";


                query += filterquery + " order by di.TERM_SEQ asc,DATE_FORMAT(COALESCE(STR_TO_DATE(di.SERVICE_BEGINDATE, '%Y-%m-%d'),STR_TO_DATE(di.SERVICE_BEGINDATE, '%Y/%m/%d')),'%Y-%m-%d') asc";

                MySqlCommand command = new MySqlCommand(query, connection);

                int id_row = 0;
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        id_row += 1;
                        jsonData.Add(new InventoryMaintenanceModel
                        {
                            ID = (id_row).ToString(),
                            DEVICE_ID = reader["device_id"].ToString(),
                            TERM_SEQ = reader["term_seq"].ToString(),
                            TYPE_ID = reader["type_id"].ToString(),
                            TERM_ID = reader["TERM_ID"].ToString(),
                            TERM_NAME = reader["TERM_NAME"].ToString(),
                            Status = reader["status"].ToString(),
                            COUNTER_CODE = reader["COUNTER_CODE"].ToString(),
                            ServiceType = reader["servicetype"].ToString(),
                            TERM_LOCATION = reader["term_location"].ToString(),
                            LATITUDE = reader["latitude"].ToString(),
                            LONGITUDE = reader["longitude"].ToString(),
                            CONTROL_BY = reader["control_by"].ToString(),
                            PROVINCE = reader["province"].ToString(),
                            SERVICE_BEGINDATE = reader["service_begindate"].ToString(),
                            SERVICE_ENDDATE = reader["service_enddate"].ToString(),
                            VERSION_AGENT = reader["version_agent"].ToString(),
                            TERM_IP = reader["TERM_IP"].ToString(),
                        });
                    }
                }
            }
            Inventory_dataList = jsonData;
            int pages = (int)Math.Ceiling((double)jsonData.Count() / _row);
            List<InventoryMaintenanceModel> filteredData = RangeFilter(jsonData, _page, _row);
            var response = new DataResponse_InventoryMaintenanceModel
            {
                JsonData = filteredData,
                Page = pages,
                currentPage = _page,
                TotalTerminal = jsonData.Count(),
            };
            return Json(response);
        }

        static List<InventoryMaintenanceModel> RangeFilter<InventoryMaintenanceModel>(List<InventoryMaintenanceModel> inputList, int page, int row)
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

        public class InventoryMaintenanceModel
        {
            public string ID { get; set; }
            public string DEVICE_ID { get; set; }
            public string TERM_SEQ { get; set; }
            public string TYPE_ID { get; set; }
            public string TERM_ID { get; set; }
            public string TERM_NAME { get; set; }
            public string Connected { get; set; }
            public string Status { get; set; }
            public string COUNTER_CODE { get; set; }
            public string ServiceType { get; set; }
            public string TERM_LOCATION { get; set; }
            public string LATITUDE { get; set; }
            public string LONGITUDE { get; set; }
            public string CONTROL_BY { get; set; }
            public string PROVINCE { get; set; }
            public string SERVICE_BEGINDATE { get; set; }
            public string SERVICE_ENDDATE { get; set; }
            public string VERSION_MASTER { get; set; }
            public string VERSION { get; set; }
            public string VERSION_AGENT { get; set; }
            public string TERM_IP { get; set; }
        }
        public class DataResponse_InventoryMaintenanceModel
        {
            public List<InventoryMaintenanceModel> JsonData { get; set; }
            public int Page { get; set; }
            public int currentPage { get; set; }
            public int TotalTerminal { get; set; }
        }
        #endregion
        #region WhitelistFilterTemplate


        [HttpGet]
        public IActionResult WhitelistFilterTemplate(string termid, string ticket, DateTime? todate, DateTime? fromdate, string mainproblem, string terminaltype, string jobno, string cmdButton)
        {
            ViewBag.maxRows = 500;
            return View();
        }
        [HttpGet]
        public IActionResult WhitelistFilterTemplateFetchData(string keyword, string status, string row, string page, string search)
        {
            int _page;
            string filterquery = string.Empty;

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
                _row = 50;
            }
            else
            {
                _row = int.Parse(row);
            }
            keyword = keyword ?? "";
            status = status ?? "";

            if (keyword != "")
            {
                filterquery += " and POLICY_DESC like '%" + keyword + "%' ";
            }

            switch (status)
            {
                case "USE":
                    filterquery += "and (UPDATE_STATUS = 'X') ";
                    break;
                case "NOTUSE":
                    filterquery += "and (UPDATE_STATUS is null or UPDATE_STATUS = '') ";
                    break;
                default:
                    break;
            }
            List<WhitelistFilterTemplateModel> jsonData = new List<WhitelistFilterTemplateModel>();

            using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
            {
                connection.Open();

                // Modify the SQL query to use the 'input' parameter for filtering
                string query = @" SELECT ID,POLICY_DESC,UPDATE_STATUS,UPDATE_DATE FROM operation_whitelist_template where ID is not null ";


                query += filterquery + " order by UPDATE_DATE desc";

                MySqlCommand command = new MySqlCommand(query, connection);

                int id_row = 0;
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        id_row += 1;
                        jsonData.Add(new WhitelistFilterTemplateModel
                        {
                            NO = (id_row).ToString(),
                            ID = reader["ID"].ToString(),
                            POLICY_DESC = reader["POLICY_DESC"].ToString(),
                            UPDATE_STATUS = reader["UPDATE_STATUS"].ToString(),
                            UPDATE_DATE = ((DateTime)reader["UPDATE_DATE"]).ToString("yyyy-MM-dd HH:mm:ss"),

                        });
                    }
                }
            }
            WhitelistFilterTemplates_datalist = jsonData;
            int pages = (int)Math.Ceiling((double)jsonData.Count() / _row);
            List<WhitelistFilterTemplateModel> filteredData = RangeFilter_wlft(jsonData, _page, _row);
            var response = new DataResponse_WhitelistFilterTemplateModel
            {
                JsonData = filteredData,
                Page = pages,
                currentPage = _page,
                TotalTerminal = jsonData.Count(),
            };
            return Json(response);
        }

        static List<WhitelistFilterTemplateModel> RangeFilter_wlft<WhitelistFilterTemplateModel>(List<WhitelistFilterTemplateModel> inputList, int page, int row)
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
        [HttpPost]
        public ActionResult UpdateDatabase(bool isChecked, string id)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    connection.Open();

                    // Create the command
                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        // Construct the SQL query based on isChecked and id
                        string sql = isChecked ? "UPDATE operation_whitelist_template SET UPDATE_STATUS = 'X' WHERE Id = @id"
                                               : "UPDATE operation_whitelist_template SET UPDATE_STATUS = '' WHERE Id = @id";

                        command.CommandText = sql;
                        command.Parameters.AddWithValue("@id", id);

                        // Execute the command
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Return success response
                            return Json(new { success = true });
                        }
                        else
                        {
                            // No rows affected (entity with the provided id not found)
                            return Json(new { success = false, message = "Entity not found." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as required
                // For now, returning an error response
                return Json(new { success = false, message = "An error occurred while updating the database." });
            }
        }
        public class WhitelistFilterTemplateModel
        {
            public string NO { get; set; }
            public string ID { get; set; }
            public string POLICY_DESC { get; set; }
            public string UPDATE_STATUS { get; set; }
            public string UPDATE_DATE { get; set; }
        }
        public class DataResponse_WhitelistFilterTemplateModel
        {
            public List<WhitelistFilterTemplateModel> JsonData { get; set; }
            public int Page { get; set; }
            public int currentPage { get; set; }
            public int TotalTerminal { get; set; }
        }

        #endregion
        #region Get Device Info
        private static List<Device_info_record> GetDeviceInfoFeelview(string _bank, IConfiguration _myConfiguration)
        {

            ConnectMySQL db_mysql = new ConnectMySQL(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + _bank));
            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM device_info order by TERM_SEQ;";
            DataTable testss = db_mysql.GetDatatable(com);

            List<Device_info_record> test = ConvertDataTableToModel.ConvertDataTable<Device_info_record>(testss);

            return test;
        }
        public class SerialNo
        {
            public string TERM_SEQ { get; set; }
        }
        private static List<SerialNo> GetSerialNo(string _bank, IConfiguration _myConfiguration)
        {

            ConnectMySQL db_mysql = new ConnectMySQL(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + _bank));
            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT TERM_SEQ FROM device_info group by TERM_SEQ;";
            DataTable testss = db_mysql.GetDatatable(com);

            List<SerialNo> test = ConvertDataTableToModel.ConvertDataTable<SerialNo>(testss);

            return test;
        }
        public class COUNTERCODE
        {
            public string COUNTER_CODE { get; set; }
        }
        private static List<COUNTERCODE> GetCounterCode(string _bank, IConfiguration _myConfiguration)
        {
            ConnectMySQL db_mysql = new ConnectMySQL(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + _bank));
            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT COUNTER_CODE FROM device_info group by COUNTER_CODE;";
            DataTable testss = db_mysql.GetDatatable(com);

            List<COUNTERCODE> test = ConvertDataTableToModel.ConvertDataTable<COUNTERCODE>(testss);

            return test;
        }
        public class SERVICETYPE
        {
            public string ServiceType { get; set; }
        }
        private static List<SERVICETYPE> GetServiceType(string _bank, IConfiguration _myConfiguration)
        {
            ConnectMySQL db_mysql = new ConnectMySQL(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + _bank));
            MySqlCommand com = new MySqlCommand();
            com.CommandText = @"SELECT CONCAT(di.SERVICE_TYPE, ' ', di.BUSINESS_BEGINTIME, ' - ', di.BUSINESS_ENDTIME) as ServiceType 
                              FROM device_info di
                              group by CONCAT(di.SERVICE_TYPE, ' ', di.BUSINESS_BEGINTIME, ' - ', di.BUSINESS_ENDTIME)";
            DataTable testss = db_mysql.GetDatatable(com);

            List<SERVICETYPE> test = ConvertDataTableToModel.ConvertDataTable<SERVICETYPE>(testss);

            return test;
        }
        public class TERMINALTYPE
        {
            public string TYPE_ID { get; set; }
        }
        private static List<TERMINALTYPE> GetTerminalType(string _bank, IConfiguration _myConfiguration)
        {
            ConnectMySQL db_mysql = new ConnectMySQL(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + _bank));
            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT TYPE_ID FROM device_info group by TYPE_ID;";
            DataTable testss = db_mysql.GetDatatable(com);

            List<TERMINALTYPE> test = ConvertDataTableToModel.ConvertDataTable<TERMINALTYPE>(testss);

            return test;
        }
        #endregion
        #region Excel Ticket
        public class InventoryExportModel
        {
            public string TerminalSeq { get; set; }
            public string TerminalNo { get; set; }
            public string TerminalType { get; set; }
            public string ServiceType { get; set; }
            public string CounterType { get; set; }
            public string Status { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string BankCode { get; set; }
            public string CurrentlyInUse { get; set; }
        }
        [HttpPost]
        public ActionResult Inventory_ExportExc(InventoryExportModel model)
        {
            string fname = "";
            string tsDate = "";
            string teDate = "";
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;
            string filterquery = string.Empty;

            try
            {
                string terminalseq = model.TerminalSeq ?? "";
                string terminalno = model.TerminalNo ?? "";
                string terminaltype = model.TerminalType ?? "";
                string servicetype = model.ServiceType ?? "";
                string countertype = model.CounterType ?? "";
                string status = model.Status ?? "";
                string fromdate = model.FromDate ?? "";
                string todate = model.ToDate ?? "";
                string bankcode = model.BankCode ?? "";
                string currentlyinuse = model.CurrentlyInUse ?? "";
                if (bankcode == "")
                {
                    return Json(new { success = "F", filename = "", errstr = "Bank not found!" });
                }
                if (terminalno != "")
                {
                    filterquery += " and di.TERM_ID like '%" + terminalno + "%' ";
                }
                if (terminalseq != "")
                {
                    filterquery += " and di.TERM_SEQ = '" + terminalseq + "' ";

                }
                if (terminaltype != "")
                {
                    filterquery += " and di.TYPE_ID = '" + terminaltype + "' ";
                }
                if (status == "use")
                {
                    filterquery += " and (di.STATUS = 'use' or di.STATUS ='roustop') ";
                }
                else if (status == "notuse")
                {
                    filterquery += " and di.STATUS = 'no' ";
                }

                if (servicetype != "")
                {
                    filterquery += " and CONCAT(di.SERVICE_TYPE, ' ', di.BUSINESS_BEGINTIME, ' - ', di.BUSINESS_ENDTIME) = '" + servicetype + "' ";
                }
                if (countertype != "")
                {
                    filterquery += " and di.COUNTER_CODE = '" + countertype + "' ";
                }
                if (fromdate != "" && todate != "")
                {
                    filterquery += " and (STR_TO_DATE(di.SERVICE_ENDDATE, '%Y-%m-%d') between '" + fromdate + "' and '" + todate + "'";
                    filterquery += "or(LENGTH(di.SERVICE_ENDDATE) = 0 and STR_TO_DATE(di.SERVICE_BEGINDATE, '%Y-%m-%d') < '" + todate + "'))";
                }
                else
                {
                    filterquery += " and (STR_TO_DATE(di.SERVICE_ENDDATE, '%Y-%m-%d') between '2020-05-01' and '" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
                    filterquery += " or(LENGTH(di.SERVICE_ENDDATE) = 0 and STR_TO_DATE(di.SERVICE_BEGINDATE, '%Y-%m-%d') < '" + DateTime.Now.ToString("yyyy-MM-dd") + "')) ";
                }
                if (currentlyinuse == "no")
                {
                    filterquery += " and di.TERM_SEQ IN (SELECT TERM_SEQ FROM device_info GROUP BY TERM_SEQ HAVING COUNT(DISTINCT status) = 1 AND MAX(status) = 'no') ";
                }
                else if (currentlyinuse == "yes")
                {
                    filterquery += " and di.TERM_SEQ NOT IN (SELECT TERM_SEQ FROM device_info GROUP BY TERM_SEQ HAVING COUNT(DISTINCT status) = 1 AND MAX(status) = 'no') ";
                }
                List<InventoryMaintenanceModel> jsonData = new List<InventoryMaintenanceModel>();

                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + bankcode)))
                {
                    connection.Open();

                    // Modify the SQL query to use the 'input' parameter for filtering
                    string query = @" SELECT di.DEVICE_ID,di.TERM_SEQ,di.TYPE_ID,di.TERM_ID,di.TERM_NAME,di.TERM_IP,
                CASE WHEN SERVICE_ENDDATE IS NULL OR SERVICE_ENDDATE = '' THEN 'Active' ELSE 'Inactive' END AS Status,
                di.COUNTER_CODE,CONCAT(di.SERVICE_TYPE, ' ', di.BUSINESS_BEGINTIME, ' - ', di.BUSINESS_ENDTIME) as ServiceType,
                di.TERM_LOCATION,di.LATITUDE,di.LONGITUDE,di.CONTROL_BY,di.PROVINCE,di.SERVICE_BEGINDATE,
				di.SERVICE_ENDDATE,di.VERSION_AGENT
				FROM device_info di
                where di.TERM_ID is not null ";


                    query += filterquery + " order by di.TERM_SEQ asc,STR_TO_DATE(di.SERVICE_BEGINDATE, '%Y-%m-%d') asc";

                    MySqlCommand command = new MySqlCommand(query, connection);

                    int id_row = 0;
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            id_row += 1;
                            jsonData.Add(new InventoryMaintenanceModel
                            {
                                ID = (id_row).ToString(),
                                DEVICE_ID = reader["device_id"].ToString(),
                                TERM_SEQ = reader["term_seq"].ToString(),
                                TYPE_ID = reader["type_id"].ToString(),
                                TERM_ID = reader["TERM_ID"].ToString(),
                                TERM_NAME = reader["TERM_NAME"].ToString(),
                                Status = reader["status"].ToString(),
                                COUNTER_CODE = reader["COUNTER_CODE"].ToString(),
                                ServiceType = reader["servicetype"].ToString(),
                                TERM_LOCATION = reader["term_location"].ToString(),
                                LATITUDE = reader["latitude"].ToString(),
                                LONGITUDE = reader["longitude"].ToString(),
                                CONTROL_BY = reader["control_by"].ToString(),
                                PROVINCE = reader["province"].ToString(),
                                SERVICE_BEGINDATE = reader["service_begindate"].ToString(),
                                SERVICE_ENDDATE = reader["service_enddate"].ToString(),
                                VERSION_AGENT = reader["version_agent"].ToString(),
                                TERM_IP = reader["TERM_IP"].ToString(),
                            });
                        }
                    }
                }
                Inventory_dataList = jsonData;
                if (Inventory_dataList == null || Inventory_dataList.Count == 0)
                {
                    return Json(new { success = "F", filename = "", errstr = "Data not found!" });
                }

                string strPath = Environment.CurrentDirectory;
                ExcelUtilities_Inventory obj = new ExcelUtilities_Inventory();

                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(Inventory_dataList, fromdate, todate);



                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;



                fname = "Inventory_" + DateTime.Now.ToString("yyyyMMdd");

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
        public ActionResult Inventory_DownloadExportFile(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            string tsDate = "";
            string teDate = "";
            try
            {




                fname = "Inventory_" + DateTime.Now.ToString("yyyyMMdd");

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
        public class TerminalUpdateModel
        {
            public string DeviceID { get; set; }
            public string TerminalNo { get; set; }
            public string SerialNo { get; set; }
            public string TerminalIP { get; set; }
            public string TerminalName { get; set; }
            public string Location { get; set; }
            public string CounterCode { get; set; }
            public string Latitude { get; set; }
            public string Longitude { get; set; }
            public string ControlBy { get; set; }
            public string Province { get; set; }
            public string ServiceBeginDate { get; set; }
            public string ServiceEndDate { get; set; }
            public string CurrTerminalNo { get; set; }
            public string BankCode { get; set; }
            // Add other properties as needed
        }
        [HttpPost]
        public IActionResult UpdateTerminal(TerminalUpdateModel model)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + model.BankCode)))
                {
                    connection.Open();

                    string query = @"UPDATE device_info 
                        SET 
                        TERM_ID = @TerminalNo,
                        TERM_SEQ = @SerialNo, 
                        TERM_IP = @TerminalIP, 
                        TERM_NAME = @TerminalName, 
                        TERM_LOCATION = @Location, 
                        COUNTER_CODE = @CounterCode, 
                        LATITUDE = @Latitude, 
                        LONGITUDE = @Longitude, 
                        CONTROL_BY = @ControlBy, 
                        PROVINCE = @Province, 
                        SERVICE_BEGINDATE = @ServiceBeginDate, 
                        SERVICE_ENDDATE = @ServiceEndDate 
                        WHERE DEVICE_ID = @DeviceID AND TERM_ID = @CurrTerminalNo";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TerminalNo", model.TerminalNo ?? "");
                        command.Parameters.AddWithValue("@SerialNo", model.SerialNo ?? "");
                        command.Parameters.AddWithValue("@TerminalIP", model.TerminalIP ?? "");
                        command.Parameters.AddWithValue("@TerminalName", model.TerminalName ?? "");
                        command.Parameters.AddWithValue("@Location", model.Location ?? "");
                        command.Parameters.AddWithValue("@CounterCode", model.CounterCode ?? "");
                        command.Parameters.AddWithValue("@Latitude", model.Latitude ?? "");
                        command.Parameters.AddWithValue("@Longitude", model.Longitude ?? "");
                        command.Parameters.AddWithValue("@ControlBy", model.ControlBy ?? "");
                        command.Parameters.AddWithValue("@Province", model.Province ?? "");
                        command.Parameters.AddWithValue("@ServiceBeginDate", model.ServiceBeginDate ?? "");
                        command.Parameters.AddWithValue("@ServiceEndDate", model.ServiceEndDate ?? "");
                        command.Parameters.AddWithValue("@CurrTerminalNo", model.CurrTerminalNo ?? "");
                        command.Parameters.AddWithValue("@DeviceID", model.DeviceID ?? "");
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok("Data updated successfully.");
                        }
                        else
                        {
                            return NotFound("No record found for the given id.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        static string generateID()
        {
            string ID;
            using (SHA256 sha256Hash = SHA256.Create())
            {
                var time = DateTime.Now.ToString();
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(time));
                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                ID = string.Format("{0}4{1:N}", "", Guid.NewGuid()).Substring(0, 32);
            }
            return ID;
        }
        public class AddTerminal
        {
            public string? DEVICE_ID { get; set; }
            public string? TYPE_ID { get; set; }
            public string? TERM_ID { get; set; }
            public string? TERM_SEQ { get; set; }
            public string? TERM_IP { get; set; }
            public string? TERM_NAME { get; set; }
            public string? TERM_LOCATION { get; set; }
            public string? COUNTER_CODE { get; set; }
            public string? SERVICETYPE { get; set; }
            public string? LATITUDE { get; set; }
            public string? LONGITUDE { get; set; }
            public string? CONTROL_BY { get; set; }
            public string? PROVINCE { get; set; }
            public DateTime? SERVICE_BEGINDATE { get; set; }
            public DateTime? SERVICE_ENDDATE { get; set; }
            public string? BANK { get; set; }
        }

        [HttpPost]
        public IActionResult InsertData(AddTerminal data)
        {
            try
            {
                data.DEVICE_ID = generateID();
                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + data.BANK)))
                {
                    connection.Open();

                    // Prepare your MySQL insert statement
                    string query = @"INSERT INTO device_info 
                            (DEVICE_ID, TYPE_ID, TERM_ID, TERM_SEQ, TERM_IP, TERM_NAME, 
                             TERM_LOCATION, COUNTER_CODE, SERVICE_TYPE, LATITUDE, LONGITUDE, 
                             CONTROL_BY, PROVINCE, SERVICE_BEGINDATE, SERVICE_ENDDATE,STATUS) 
                             VALUES 
                             (@DeviceId, @TypeId, @TermId, @TermSeq, @TermIp, @TermName, 
                             @TermLocation, @CounterCode, @ServiceType, @Latitude, @Longitude, 
                             @ControlBy, @Province, @ServiceBeginDate, @ServiceEndDate,'use')"
                    ;

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Add parameters to the command
                        command.Parameters.AddWithValue("@DeviceId", data.DEVICE_ID);
                        command.Parameters.AddWithValue("@TypeId", data.TYPE_ID);
                        command.Parameters.AddWithValue("@TermId", data.TERM_ID);
                        command.Parameters.AddWithValue("@TermSeq", data.TERM_SEQ);
                        command.Parameters.AddWithValue("@TermIp", data.TERM_IP);
                        command.Parameters.AddWithValue("@TermName", data.TERM_NAME);
                        command.Parameters.AddWithValue("@TermLocation", data.TERM_LOCATION);
                        command.Parameters.AddWithValue("@CounterCode", data.COUNTER_CODE);
                        command.Parameters.AddWithValue("@ServiceType", data.SERVICETYPE);
                        command.Parameters.AddWithValue("@Latitude", data.LATITUDE);
                        command.Parameters.AddWithValue("@Longitude", data.LONGITUDE);
                        command.Parameters.AddWithValue("@ControlBy", data.CONTROL_BY);
                        command.Parameters.AddWithValue("@Province", data.PROVINCE);
                        command.Parameters.AddWithValue("@ServiceBeginDate", data.SERVICE_BEGINDATE);
                        command.Parameters.AddWithValue("@ServiceEndDate", data.SERVICE_ENDDATE);

                        // Execute the command
                        command.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Data inserted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        private static string GenerateUniqueID()
        {
            return Guid.NewGuid().ToString();
        }

        private static string FormatFileLength(long lengthInBytes)
        {
            double length = lengthInBytes;
            string[] units = { "B", "KB", "MB", "GB", "TB" };

            int unitIndex = 0;
            while (length >= 1024 && unitIndex < units.Length - 1)
            {
                length /= 1024;
                unitIndex++;
            }

            return $"{Math.Round(length, 2)} {units[unitIndex]}"; // แสดงขนาดไฟล์ในหน่วยที่เข้าใจง่าย
        }

        //private static async Task<(List<string> terminals, List<EJournalModel> journalList)> ExecuteSftpProcessAsync(string host,string username,string password,string remoteFilePath ,string TermID ,DateTime startDateTemp,DateTime endDateTemp )
        //{
        //    List<EJournalModel> journalList = new List<EJournalModel>();
        //    List<string> terminals = new List<string>();
        //    try
        //    {
        //        using (var sftpClient = new SftpClient(host, username, password))
        //        {
        //            sftpClient.Connect();

        //            if (sftpClient.Exists(remoteFilePath))
        //            {
        //                var files = sftpClient.ListDirectory(remoteFilePath);

        //                #region Loop add terminal name from file server
        //                foreach (var file in files)
        //                {
        //                    if (file.IsDirectory && file.Name != "." && file.Name != "..")
        //                    {
        //                        terminals.Add(file.Name.Replace('.', ' '));
        //                    }
        //                }
        //                #endregion

        //                // Process each terminal in parallel (asynchronous)
        //                var tasks = terminals.Select(async terminal =>
        //                {
        //                    if (TermID != null)
        //                    {
        //                        if (terminal != TermID) return;
        //                    }


        //                    string terminalPath = Path.Combine(remoteFilePath, terminal);
        //                    var yearPath = sftpClient.ListDirectory(terminalPath);

        //                    foreach (var fileYear in yearPath)
        //                    {
        //                        if (fileYear.IsDirectory && fileYear.Name != "." && fileYear.Name != "..")
        //                        {
        //                            DateTime checkDate = new DateTime(int.Parse(fileYear.Name), 1, 1);

        //                            if (!(checkDate >= startDateTemp && checkDate <= endDateTemp))
        //                                continue;

        //                            string yearStr = Path.GetFileName(fileYear.Name);
        //                            string yearPathStr = Path.Combine(terminalPath, yearStr);

        //                            var monthPath = sftpClient.ListDirectory(yearPathStr);

        //                            foreach (var fileMonth in monthPath)
        //                            {
        //                                if (fileMonth.IsDirectory && fileMonth.Name != "." && fileMonth.Name != "..")
        //                                {
        //                                    checkDate = new DateTime(int.Parse(fileYear.Name), int.Parse(fileMonth.Name), 1);

        //                                    if (!(checkDate >= startDateTemp && checkDate <= endDateTemp))
        //                                        continue;

        //                                    string monthStr = Path.GetFileName(fileMonth.Name);
        //                                    string monthPathStr = Path.Combine(yearPathStr, monthStr);

        //                                    var dayPath = sftpClient.ListDirectory(monthPathStr);

        //                                    foreach (var fileDay in dayPath)
        //                                    {
        //                                        if (!fileDay.IsDirectory && fileDay.Name != "." && fileDay.Name != "..")
        //                                        {
        //                                            string dateFromFile = fileDay.Name.Substring(2, 8);
        //                                            checkDate = DateTime.ParseExact(dateFromFile, "yyyyMMdd", null);

        //                                            if (!(checkDate >= startDateTemp && checkDate <= endDateTemp))
        //                                                continue;

        //                                            string dayPathStr = Path.Combine(monthPathStr, fileDay.Name);

        //                                            if (fileDay.Name.EndsWith(".txt"))
        //                                            {
        //                                                var journal = new EJournalModel
        //                                                {
        //                                                    ID = GenerateUniqueID(),
        //                                                    FileName = fileDay.Name,
        //                                                    FileContent = "",
        //                                                    TerminalID = terminal,
        //                                                    UpdateDate = fileDay.LastWriteTime.ToString("yyyy-MM-dd"),
        //                                                    LastUploadingTime = fileDay.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
        //                                                    pathOfFile = fileDay.FullName,
        //                                                    FileLength = FormatFileLength(fileDay.Length),
        //                                                    UploadStatus = "Success"
        //                                                };

        //                                                // Add to journal list
        //                                                journalList.Add(journal);
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                });

        //                // Run the tasks concurrently
        //                await Task.WhenAll(tasks);
        //            }

        //            sftpClient.Disconnect();
        //        }

        //        return (terminals, journalList);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"EJournalMenu error: {ex.Message}");
        //        return (new List<string>(), new List<EJournalModel>());
        //    }
        //}


        public async Task<IActionResult> EJournalMenu(string cmdButton, string TermID, string FrDate, string ToDate, string FrTime, string ToTime
      , string currTID, string currFr, string currTo, string currFrTime, string currToTime, string lstPageSize
      , string currPageSize, int? page, string maxRows, string terminalType, string startDate, string bankName)
        {

            string host = "10.98.10.31";
            string username = "root";
            string password = "P@ssw0rd";
            string remoteFilePath = "/opt/FileServerBAAC/EJ/";

            List<string> terminals = new List<string>();

            List<EJournalModel> journalListResult = new List<EJournalModel>();


            List<Device_info_record> device_Info_Records = new List<Device_info_record>();

            DBService_TermProb dBService = new DBService_TermProb(_myConfiguration, _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_baac"));

            device_Info_Records = dBService.GetDeviceInfoFeelview();



            if (startDate == null || startDate == "")
                startDate = DateTime.Now.ToString("yyyy-MM-dd");

            ViewBag.startDate = startDate;

            int pageNum = 1;

            try
            {


                if (cmdButton == "Clear")
                    return RedirectToAction("EJournalMenu");

                if (null == TermID && null == FrDate && null == ToDate && null == page)
                {

                    FrDate = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
                    ToDate = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);

                    page = 1;
                }
                else
                {
                    // Return temp value back to it own variable
                    FrDate = (FrDate ?? currFr);
                    ToDate = (ToDate ?? currTo);
                    FrTime = (FrTime ?? currFrTime);
                    ToTime = (ToTime ?? currToTime);
                    TermID = (TermID ?? currTID);

                }





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



                DateTime startDateTemp = DateTime.Parse(FrDate);
                DateTime endDateTemp = DateTime.Parse(ToDate);

                DateTime checkDate;

                #region Get EJlog operation

                //(terminals, journalListResult) = await ExecuteSftpProcessAsync(
                //host, username, password, remoteFilePath, TermID, startDateTemp, endDateTemp
                // );

                try
                {

                    using (var sftpClient = new SftpClient(host, username, password))
                    {

                        sftpClient.Connect();


                        if (sftpClient.Exists(remoteFilePath))
                        {

                            var files = sftpClient.ListDirectory(remoteFilePath);

                            #region Loop add terminal name from file server

                            foreach (var file in files)
                            {

                                if (file.IsDirectory && file.Name != "." && file.Name != "..")
                                {

                                    terminals.Add(file.Name.Replace('.', ' '));
                                }

                            }

                            #endregion

                            #region #region Loop folder of Terminal -> /opt/FileServerBAAC/EJ/T021B034B992P001

                            foreach (var terminal in terminals) //Terminal
                            {

                                if (TermID != null)
                                {
                                    if (terminal != TermID) continue;
                                }

                                string termianlPath = Path.Combine(remoteFilePath, terminal);

                                var yearPath = sftpClient.ListDirectory(termianlPath);

                                #region Loop folder of Month -> /opt/FileServerBAAC/EJ/T021B034B992P001/2025

                                foreach (var fileYear in yearPath) //Year
                                {

                                    if (fileYear.IsDirectory && fileYear.Name != "." && fileYear.Name != "..") // && fileYear.Name == "2025"
                                    {

                                        checkDate = new DateTime(int.Parse(fileYear.Name), 1, 1);

                                        if (checkDate.Year < startDateTemp.Year || checkDate.Year > endDateTemp.Year)
                                        {
                                            continue;
                                        }

                                        string yearStr = Path.GetFileName(fileYear.Name);

                                        string yearPathStr = termianlPath + "/" + yearStr;


                                        var monthPath = sftpClient.ListDirectory(yearPathStr);

                                        #region Loop folder of Month -> /opt/FileServerBAAC/EJ/T021B034B992P001/2025/01
                                        foreach (var fileMonth in monthPath) //Month
                                        {


                                            if (fileMonth.IsDirectory && fileMonth.Name != "." && fileMonth.Name != "..") //&& fileMonth.Name == "01"
                                            {

                                                checkDate = new DateTime(int.Parse(fileYear.Name), int.Parse(fileMonth.Name), 1);


                                                if (checkDate.Year < startDateTemp.Year || (checkDate.Year == startDateTemp.Year && checkDate.Month < startDateTemp.Month) ||
                                                    checkDate.Year > endDateTemp.Year || (checkDate.Year == endDateTemp.Year && checkDate.Month > endDateTemp.Month))
                                                {
                                                    continue;
                                                }

                                                string monthStr = Path.GetFileName(fileMonth.Name);

                                                string monthPathStr = yearPathStr + "/" + monthStr;


                                                var dayPath = sftpClient.ListDirectory(monthPathStr);

                                                #region Get file EJYYYYMMDD.txt

                                                foreach (var fileDay in dayPath) //Day
                                                {
                                                    if (!fileDay.IsDirectory && fileDay.Name != "." && fileDay.Name != "..")
                                                    {

                                                        string dateFromFile = fileDay.Name.Substring(2, 8);

                                                        checkDate = DateTime.ParseExact(dateFromFile, "yyyyMMdd", null);


                                                        if (checkDate < startDateTemp || checkDate > endDateTemp)
                                                        {
                                                            continue;
                                                        }

                                                        string dayPathStr = monthPathStr + "/" + fileDay.Name;

                                                        Console.WriteLine("Reading from path: " + monthPath);


                                                        if (fileDay.Name.EndsWith(".txt"))
                                                        {


                                                            //string content = reader.ReadToEnd();
                                                            Device_info_record filteredRecordsTemp = device_Info_Records
                                                             .FirstOrDefault(device => device.TERM_ID == terminal);


                                                            var journal = new EJournalModel
                                                            {
                                                                ID = GenerateUniqueID(),
                                                                SerialNo = filteredRecordsTemp.TERM_SEQ,
                                                                TerminalName = filteredRecordsTemp.TERM_NAME,
                                                                TerminalType = filteredRecordsTemp.COUNTER_CODE,
                                                                FileName = fileDay.Name,
                                                                FileContent = "",
                                                                TerminalID = terminal,
                                                                UpdateDate = fileDay.LastWriteTime.ToString("yyyy-MM-dd"),
                                                                LastUploadingTime = fileDay.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                                pathOfFile = fileDay.FullName,
                                                                FileLength = FormatFileLength(fileDay.Length),
                                                                UploadStatus = "Success"

                                                            };


                                                            journalListResult.Add(journal);


                                                        }

                                                    }

                                                }

                                                #endregion
                                            }


                                        }

                                        #endregion

                                    }

                                }

                                #endregion


                            }

                            #endregion


                        }
                        else
                        {

                        }

                        sftpClient.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"EJournalMenu error : {ex.Message}");
                }


                #endregion





                var additionalItems = device_Info_Records.Select(x => x.COUNTER_CODE).Distinct();


                var item = new List<string> {  };


                ViewBag.probTermStr = new SelectList(additionalItems.Concat(item).ToList());



                List<Device_info_record> filteredRecords = device_Info_Records
                        .Where(device => terminals.Contains(device.TERM_ID))
                        .ToList();

                ViewBag.CurrentTID = filteredRecords;
                ViewBag.TermID = TermID;




                #region Set page
                long recCnt = 0;

                if (String.IsNullOrEmpty(maxRows))
                    ViewBag.maxRows = "50";
                else
                    ViewBag.maxRows = maxRows;

                //if (journalList.Count > 0)
                //{
                //    if (bankName == "BAAC")
                //    {
                //        switch (terminalType)
                //        {
                //            case "LOT572":
                //                journalList.RemoveAll(item => item.Terminal_Type == "LOT587");
                //                break;
                //            case "LOT587":
                //                journalList.RemoveAll(item => item.Terminal_Type == "LOT572");
                //                break;
                //            default:
                //                break;
                //        }
                //    }


                //    if (TermID != null)
                //    {
                //        journalList.RemoveAll(item => item.Terminal_ID != TermID);
                //    }




                //}

                if (journalListResult.Count > 0)
                {

                    if (terminalType != null)
                    {
                        journalListResult = journalListResult.Where(z => z.TerminalType.Contains(terminalType)).OrderBy(x => x.FileName).ToList();
                    }
                    else
                    {
                        journalListResult = journalListResult.OrderBy(x => x.FileName).ToList();
                    }


                }



                if (null == journalListResult || journalListResult.Count <= 0)
                {
                    ViewBag.NoData = "true";

                }
                else
                {
                    recCnt = journalListResult.Count;

                    param.PAGESIZE = journalListResult.Count;
                }




                if (recCnt > 0)
                {
                    ViewBag.Records = String.Format("{0:#,##0}", recCnt.ToString());
                }
                else
                    ViewBag.Records = "0";

                pageNum = (page ?? 1);

                int amountrecordset = journalListResult.Count();

                if (amountrecordset > 5000)
                {
                    journalListResult.RemoveRange(5000, amountrecordset - 5000);
                }
                #endregion

               


            }
            catch (Exception)
            {

            }




            return View(journalListResult.ToPagedList(pageNum, (int)param.PAGESIZE));

        }






        [HttpPost]
        public ActionResult GetFileContent([FromBody] string pathOfFile)
        {
            if (!string.IsNullOrEmpty(pathOfFile))
            {
                // ดึงข้อมูลไฟล์จาก SFTP โดยใช้ pathOfFile
                var fileContent = GetFileContentFromSFTP(pathOfFile);

                if (!string.IsNullOrEmpty(fileContent))
                {
                    return Content(fileContent); // ส่งเนื้อหาของไฟล์กลับไป
                }
                else
                {
                    return Content("Error: Unable to read file.");
                }
            }
            else
            {
                return Content("Invalid file path.");
            }
        }


        private static string GetFileContentFromSFTP(string path)
        {
           
            string host = "10.98.10.31";
            string username = "root";
            string password = "P@ssw0rd";

            using (var sftp = new SftpClient(host, username, password))
            {
                try
                {
                    sftp.Connect(); 
                    if (!sftp.Exists(path))
                    {
                        return $"Error: File '{path}' not found on the server.";
                    }

                    var fileContent = string.Empty;

                  
                    using (var fileStream = new MemoryStream())
                    {
                        sftp.DownloadFile(path, fileStream); 
                        fileStream.Position = 0;

                        using (var reader = new StreamReader(fileStream))
                        {
                            fileContent = reader.ReadToEnd(); 
                        }
                    }

                    return fileContent; 
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
                finally
                {
                    sftp.Disconnect(); 
                }
            }
        }




    }
}
