using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SLA_Management.Data;
using SLA_Management.Commons;
using SLA_Management.Models.OperationModel;
using System.Data;
using SLA_Management.Data.ExcelUtilitie;
using Microsoft.AspNetCore.Mvc.Rendering;
using PagedList;
using Renci.SshNet;
using SLA_Management.Data.TermProb;
using SLA_Management.Models;
using SLA_Management.Models.TermProbModel;
using System.Globalization;
using static System.Net.WebRequestMethods;
using System.Collections.Concurrent;
using System.Configuration;
using System.Security.AccessControl;
using SLA_Management.Models.LogMonitorModel;
using System.Text.RegularExpressions;

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
        #region Inventory History
        [HttpGet]
        public IActionResult InventoryMonitorHistory(string termid, string ticket, DateTime? todate, DateTime? fromdate, string mainproblem, string terminaltype, string jobno, string cmdButton)
        {
            ViewBag.maxRows = 50;
            ViewBag.CurrentTID = GetDeviceInfoFeelview();
            ViewBag.CurrentTSeq = GetSerialNo();
            ViewBag.CurrentCC = GetCounterCode();
            ViewBag.CurrentST = GetServiceType();
            string FrDate = "2020-05-01";
            string ToDate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.CurrentFr = FrDate;
            ViewBag.CurrentTo = ToDate;

            return View();
        }
        [HttpGet]
        public IActionResult InventoryHistoryFetchData(string terminalseq, string terminalno, string terminaltype, string connencted, string servicetype, string countertype, string status, string row, string page, string search, string fromdate, string todate, string currentlyinuse)
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
                filterquery += " and di.TERM_SEQ IN (SELECT TERM_SEQ FROM device_info_history GROUP BY TERM_SEQ HAVING COUNT(DISTINCT status) = 1 AND MAX(status) = 'no') ";
            }
            else if (currentlyinuse == "yes")
            {
                filterquery += " and di.TERM_SEQ NOT IN (SELECT TERM_SEQ FROM device_info_history GROUP BY TERM_SEQ HAVING COUNT(DISTINCT status) = 1 AND MAX(status) = 'no') ";
            }
            List<InventoryMaintenanceModel> jsonData = new List<InventoryMaintenanceModel>();

            using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
            {
                connection.Open();

                // Modify the SQL query to use the 'input' parameter for filtering
                string query = @"  SELECT di.DEVICE_ID,di.TERM_SEQ,di.TYPE_ID,di.TERM_ID,di.TERM_NAME,di.TERM_IP,di.TERM_LOCATION,di.LATITUDE,di.LONGITUDE,di.CONTROL_BY,di.PROVINCE,di.VERSION_AGENT,di.SERVICE_BEGINDATE,di.SERVICE_ENDDATE
                FROM device_info_history di
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

        #endregion
        #region Inventory

        [HttpGet]
        public IActionResult InventoryMonitor(string termid, string ticket, DateTime? todate, DateTime? fromdate, string mainproblem, string terminaltype, string jobno, string cmdButton)
        {
            ViewBag.maxRows = 50;
            ViewBag.CurrentTID = GetDeviceInfoFeelview();
            ViewBag.CurrentTSeq = GetSerialNo();
            ViewBag.CurrentCC = GetCounterCode();
            ViewBag.CurrentST = GetServiceType();
            string FrDate = "2020-05-01";
            string ToDate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.CurrentFr = FrDate;
            ViewBag.CurrentTo = ToDate;

            return View();
        }
        [HttpGet]
        public IActionResult InventoryFetchData(string terminalseq, string terminalno, string terminaltype, string connencted, string servicetype, string countertype, string status, string row, string page, string search, string fromdate, string todate, string currentlyinuse)
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

            if (terminalno != "")
            {
                filterquery += " and pv.TERM_ID like '%" + terminalno.Trim() + "%' ";
            }
            if (terminalseq != "")
            {
                filterquery += " and di.TERM_SEQ = '" + terminalseq.Trim() + "' ";

            }
            if (terminaltype != "")
            {
                filterquery += " and di.TYPE_ID = '" + terminaltype.Trim() + "' ";
            }
            if (status == "use")
            {
                filterquery += " and (di.STATUS = 'use' or di.STATUS ='roustop') ";
            }
            else if (status == "notuse")
            {
                filterquery += " and di.STATUS = 'no' ";
            }
            if (connencted == "0")
            {
                filterquery += " and die.CONN_STATUS_ID = '0' ";
            }
            else if (connencted == "2")
            {
                filterquery += " and die.CONN_STATUS_ID != '0' ";
            }
            else if (connencted == "1")
            {
                filterquery += " and die.CONN_STATUS_ID is null and di.STATUS = 'no' ";
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

            using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection")))
            {
                connection.Open();

                // Modify the SQL query to use the 'input' parameter for filtering
                string query = @" SELECT di.DEVICE_ID,di.TERM_SEQ,di.TYPE_ID,di.TERM_ID,di.TERM_NAME,di.TERM_IP,
                CASE WHEN die.CONN_STATUS_ID = 0 THEN 'Online' 
                WHEN die.CONN_STATUS_ID is null and di.STATUS = 'no' THEN 'Unknown' 
                ELSE 'Offline' END AS Connected,
                CASE WHEN di.STATUS = 'use' or di.STATUS = 'roustop'  THEN 'Active'
                ELSE 'Inactive' END AS Status,
                di.COUNTER_CODE,CONCAT(di.SERVICE_TYPE, ' ', di.BUSINESS_BEGINTIME, ' - ', di.BUSINESS_ENDTIME) as ServiceType,
                di.TERM_LOCATION,di.LATITUDE,di.LONGITUDE,di.CONTROL_BY,di.PROVINCE,di.SERVICE_BEGINDATE,
                CASE WHEN STATUS = 'no' AND (dsi.CONN_STATUS_ID IS NULL)AND LENGTH(di.SERVICE_ENDDATE) = 0 THEN 'เครื่องไม่เปิดให้บริการ' 
                WHEN STATUS != 'no' AND LENGTH(di.SERVICE_ENDDATE) = 0 THEN 'เครื่องยังเปิดให้บริการ' ELSE di.SERVICE_ENDDATE
                END AS SERVICE_ENDDATE,
                pv.VERSION_MASTER,pv.VERSION,di.VERSION_AGENT
                FROM gsb_adm_fv.device_info di
				LEFT JOIN gsb_adm_fv.project_version pv ON di.TERM_ID = pv.TERM_ID
                left join device_status_info dsi on pv.TERM_ID = dsi.TERM_ID
                left join device_inner_event die on dsi.CONN_STATUS_EVENT_ID = die.EVENT_ID 
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
                            Connected = reader["connected"].ToString(),
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
                            VERSION_MASTER = reader["version_master"].ToString(),
                            VERSION = reader["version"].ToString(),
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
        private static List<Device_info_record> GetDeviceInfoFeelview()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM device_info order by TERM_SEQ;";
            DataTable testss = db_fv.GetDatatable(com);

            List<Device_info_record> test = ConvertDataTableToModel.ConvertDataTable<Device_info_record>(testss);

            return test;
        }
        public class SerialNo
        {
            public string TERM_SEQ { get; set; }
        }
        private static List<SerialNo> GetSerialNo()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT TERM_SEQ FROM device_info group by TERM_SEQ;";
            DataTable testss = db_fv.GetDatatable(com);

            List<SerialNo> test = ConvertDataTableToModel.ConvertDataTable<SerialNo>(testss);

            return test;
        }
        public class COUNTERCODE
        {
            public string COUNTER_CODE { get; set; }
        }
        private static List<COUNTERCODE> GetCounterCode()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT COUNTER_CODE FROM device_info group by COUNTER_CODE;";
            DataTable testss = db_fv.GetDatatable(com);

            List<COUNTERCODE> test = ConvertDataTableToModel.ConvertDataTable<COUNTERCODE>(testss);

            return test;
        }
        public class SERVICETYPE
        {
            public string ServiceType { get; set; }
        }
        private static List<SERVICETYPE> GetServiceType()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = @"SELECT CONCAT(di.SERVICE_TYPE, ' ', di.BUSINESS_BEGINTIME, ' - ', di.BUSINESS_ENDTIME) as ServiceType 
                              FROM gsb_adm_fv.device_info di
                              group by CONCAT(di.SERVICE_TYPE, ' ', di.BUSINESS_BEGINTIME, ' - ', di.BUSINESS_ENDTIME)";
            DataTable testss = db_fv.GetDatatable(com);

            List<SERVICETYPE> test = ConvertDataTableToModel.ConvertDataTable<SERVICETYPE>(testss);

            return test;
        }
        #endregion
        #region Excel Ticket

        [HttpPost]
        public ActionResult Inventory_ExportExc(string terminalseq, string terminalno, string terminaltype, string connencted, string servicetype, string countertype, string status, string fromdate, string todate)
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
                terminalno = terminalno ?? "";
                terminalseq = terminalseq ?? "";
                terminaltype = terminaltype ?? "";
                connencted = connencted ?? "";
                servicetype = servicetype ?? "";
                countertype = countertype ?? "";
                fromdate = fromdate ?? "";
                todate = todate ?? "";
                status = status ?? "";

                if (terminalno != "")
                {
                    filterquery += " and pv.TERM_ID like '%" + terminalno + "%' ";
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
                    filterquery += " and di.STATUS = 'use' ";
                }
                else if (status == "notuse")
                {
                    filterquery += " and di.STATUS != 'use' ";
                }
                else
                {
                    filterquery += "";
                }
                if (connencted == "0")
                {
                    filterquery += " and die.CONN_STATUS_ID = '0' ";
                }
                else if (connencted == "2")
                {
                    filterquery += " and die.CONN_STATUS_ID != '0' ";
                }
                else if (connencted == "1")
                {
                    filterquery += " and die.CONN_STATUS_ID is null and di.STATUS = 'no' ";
                }
                else
                {
                    filterquery += "";
                }
                if (servicetype != "")
                {
                    filterquery += " and CONCAT(di.SERVICE_TYPE, ' ', di.BUSINESS_BEGINTIME, ' - ', di.BUSINESS_ENDTIME) = '" + servicetype + "' ";
                }
                if (countertype != "")
                {
                    filterquery += " and di.COUNTER_CODE = '" + countertype + "' ";
                }
                if (fromdate != "")
                {
                    filterquery += "and di.SERVICE_BEGINDATE >= '" + fromdate + "'";
                }
                else
                {
                    filterquery += " and di.SERVICE_BEGINDATE >= '2020-05-01' ";
                }
                if (todate != "")
                {
                    filterquery += " and (di.SERVICE_ENDDATE <= '" + todate + "' or SERVICE_ENDDATE = 'เครื่องไม่เปิดให้บริการ' or SERVICE_ENDDATE = 'เครื่องยังเปิดให้บริการ') ";
                }
                else
                {
                    filterquery += " and (di.SERVICE_ENDDATE <= '" + "2099-12-31" + "' or SERVICE_ENDDATE = 'เครื่องไม่เปิดให้บริการ' or SERVICE_ENDDATE = 'เครื่องยังเปิดให้บริการ') ";
                }
                List<InventoryMaintenanceModel> jsonData = new List<InventoryMaintenanceModel>();

                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection")))
                {
                    connection.Open();

                    // Modify the SQL query to use the 'input' parameter for filtering
                    string query = @" SELECT di.DEVICE_ID,di.TERM_SEQ,di.TYPE_ID,pv.TERM_ID,di.TERM_NAME,
                CASE WHEN die.CONN_STATUS_ID = 0 THEN 'Online' 
                WHEN die.CONN_STATUS_ID is null and di.STATUS = 'no' THEN 'Unknown' 
                ELSE 'Offline' END AS Connected,
                CASE WHEN di.STATUS = 'use' THEN 'Active'
                ELSE 'Inactive' END AS Status,
                di.COUNTER_CODE,CONCAT(di.SERVICE_TYPE, ' ', di.BUSINESS_BEGINTIME, ' - ', di.BUSINESS_ENDTIME) as ServiceType,
                di.TERM_LOCATION,di.LATITUDE,di.LONGITUDE,di.CONTROL_BY,di.PROVINCE,di.SERVICE_BEGINDATE,
                CASE WHEN STATUS = 'no' AND (dsi.CONN_STATUS_ID IS NULL OR dsi.CONN_STATUS_ID = 0)AND LENGTH(di.SERVICE_ENDDATE) = 0 THEN 'เครื่องไม่เปิดให้บริการ' 
                WHEN STATUS != 'no' AND LENGTH(di.SERVICE_ENDDATE) = 0 THEN 'เครื่องยังเปิดให้บริการ' ELSE di.SERVICE_ENDDATE
                END AS SERVICE_ENDDATE,
                pv.VERSION_MASTER,pv.VERSION,di.VERSION_AGENT
                FROM gsb_adm_fv.project_version pv
                left join device_info di on pv.TERM_ID = di.TERM_ID
                left join device_status_info dsi on pv.TERM_ID = dsi.TERM_ID
                left join device_inner_event die on dsi.CONN_STATUS_EVENT_ID = die.EVENT_ID 
                where pv.TERM_ID is not null ";


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
                                Connected = reader["connected"].ToString(),
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
                                VERSION_MASTER = reader["version_master"].ToString(),
                                VERSION = reader["version"].ToString(),
                                VERSION_AGENT = reader["version_agent"].ToString(),
                            });
                        }
                    }
                }

                if (Inventory_dataList == null || Inventory_dataList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

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
            // Add other properties as needed
        }
        [HttpPost]
        public IActionResult UpdateTerminal(TerminalUpdateModel model)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection")))
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


        private static List<string> ListFilesRecursively(SftpClient sftpClient, string path)
        {
            List<string> directories = new List<string>();

            // List the files in the current directory
            var files = sftpClient.ListDirectory(path);

            // Use a ConcurrentBag to ensure thread-safety when adding to the result list
            ConcurrentBag<string> result = new ConcurrentBag<string>();

            // Process files in parallel
            Parallel.ForEach(files, (file) =>
            {
                if (file.Name != "." && file.Name != "..")
                {
                    if (file.IsDirectory)
                    {
                        // Recursively list files in subdirectories (in parallel as well)
                        var subDirectoryFiles = ListFilesRecursively(sftpClient, file.FullName);
                        foreach (var subFile in subDirectoryFiles)
                        {
                            result.Add(subFile);
                        }
                    }
                    else
                    {
                        result.Add(file.FullName);
                    }
                }
            });

            // Convert the ConcurrentBag to a List and return it
            return result.ToList();
        }


        [HttpPost]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files, string Job)
        {
            string uploadPath = @"C:\Temp";

            try
            {
                string dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
               
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                
                if (files == null || files.Count == 0)
                {
                    return BadRequest("No files were uploaded.");
                }

                int countFile = files.Count();

                foreach (var file in files)
                {
                    
                    if (file.ContentType != "text/plain")
                    {
                        return BadRequest("Only .txt files are allowed.");
                    }

                   
                    string fileName = Path.GetFileName(file.FileName);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

                    if (!fileNameWithoutExtension.StartsWith("EJ") || fileNameWithoutExtension.Length != 10 || !DateTime.TryParseExact(fileNameWithoutExtension.Substring(2), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out _))
                    {
                        return BadRequest("File name must start with 'EJ' followed by a valid date in 'yyyyMMdd' format.");
                    }

                   
                    if (file.Length > 10 * 1024 * 1024) // 10MB
                    {
                        return BadRequest("File size exceeds the limit of 10MB.");
                    }

                    string jobId = Job;
                    uploadPath = @"D:\logs\" + jobId;  

                   
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath); 
                    }

                  
                    string filePath = Path.Combine(uploadPath, file.FileName); 

                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream); 
                    }

                }

                DBService_TermProb dBService = new DBService_TermProb(_myConfiguration, _myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection"));
                dBService.InsertDataToJobEJ(Job, HttpContext.Session.GetString("Username"), countFile);

                return Ok("Files uploaded successfully.");
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadFilesText(List<IFormFile> files, string Job)
        {
            string uploadPath = @"C:\Temp";

            try
            {
                string dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");

               
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

               
                if (files == null || files.Count == 0)
                {
                    return BadRequest("No files were uploaded.");
                }

                int countFile = files.Count();

                foreach (var file in files)
                {
                   
                    if (file.ContentType != "text/plain")
                    {
                        return BadRequest("Only .txt files are allowed.");
                    }

                   
                    string fileName = Path.GetFileName(file.FileName);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

                  
                    string pattern = @"^[A-Za-z0-9]+_\d{8}_FileViewer$";  // terminalNo_yyyyMMdd_FileViewer
                    if (!Regex.IsMatch(fileNameWithoutExtension, pattern))
                    {
                        return BadRequest("File name must follow the format 'terminalNo_yyyyMMdd_FileViewer'.");
                    }

                   
                    if (file.Length > 10 * 1024 * 1024) // 10MB
                    {
                        return BadRequest("File size exceeds the limit of 10MB.");
                    }

                    string jobId = Job;
                    uploadPath = @"D:\logMonitor\" + jobId;  

                   
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath); 
                    }

                 
                    string filePath = Path.Combine(uploadPath, file.FileName); 

                   
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream); 
                    }

                }

                DBService_TermProb dBService = new DBService_TermProb(_myConfiguration, _myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection"));
                dBService.InsertDataToJobLogMonitor(Job, HttpContext.Session.GetString("Username"), countFile);

                return Ok("Files uploaded successfully.");
            }
            catch (Exception ex)
            {
           
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet]
        public JsonResult GetFileLogMonitoringCycleIds(string fileLogMonitoringCycleId)
        {
            try
            {
               
                DBService_TermProb dBService = new DBService_TermProb(
                    _myConfiguration,
                    _myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")
                );

                
                var cycleIds = dBService.GetFileLogMonitoringCycleIdsByStatusFalseAndCycleId(fileLogMonitoringCycleId);

               
                if (cycleIds == null || !cycleIds.Any())
                {
                    return Json(new { success = false, message = "No data found", data = new List<string>() });
                }

                
                return Json(new { success = true, data = cycleIds });
            }
            catch (Exception ex)
            {
               
                return Json(new { success = false, message = "An error occurred while fetching data.", error = ex.Message });
            }
        }


        public async Task<IActionResult> JobLogMonitorMenu(string cmdButton, string TermID, string FrDate, string ToDate, string FrTime, string ToTime
, string currTID, string currFr, string currTo, string currFrTime, string currToTime, string lstPageSize
, string currPageSize, int? page, string maxRows, string terminalType, string startDate, string status)
        {


            List<logmonitorjob> logmonitorjob = new List<logmonitorjob>();

            List<Device_info_record> device_Info_Records = new List<Device_info_record>();

            DBService_TermProb dBService = new DBService_TermProb(_myConfiguration, _myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection"));




            if (startDate == null || startDate == "")
                startDate = DateTime.Now.ToString("yyyy-MM-dd");

            ViewBag.startDate = startDate;

            int pageNum = 1;

            try
            {


                if (cmdButton == "Clear")
                    return RedirectToAction("JobLogMonitorMenu");

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

                device_Info_Records = dBService.GetDeviceInfoFeelview();


                var additionalItems = device_Info_Records.Select(x => x.COUNTER_CODE).Distinct();



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

                logmonitorjob = dBService.GetJobLogMonitor(FrDate + " 00:00:00 ", ToDate + " 23:59:59", status);





                //var result = dBService.GetAllCountLogMonitoringFail(TermID);

                //Console.WriteLine($"ejLog False Count: {result.ejLogFalseCount}");
                //Console.WriteLine($"eCatLog False Count: {result.eCatLogFalseCount}");
                //Console.WriteLine($"comLog False Count: {result.comLogFalseCount}");
                //Console.WriteLine($"imageLog False Count: {result.imageLogFalseCount}");



                #region Set page
                long recCnt = 0;

                if (String.IsNullOrEmpty(maxRows))
                    ViewBag.maxRows = "50";
                else
                    ViewBag.maxRows = maxRows;


                if (null == logmonitorjob || logmonitorjob.Count <= 0)
                {
                    ViewBag.NoData = "true";

                }
                else
                {
                    recCnt = logmonitorjob.Count;

                    param.PAGESIZE = logmonitorjob.Count;
                }




                if (recCnt > 0)
                {
                    ViewBag.Records = String.Format("{0:#,##0}", recCnt.ToString());
                }
                else
                    ViewBag.Records = "0";

                pageNum = (page ?? 1);

                int amountrecordset = logmonitorjob.Count();

                if (amountrecordset > 5000)
                {
                    logmonitorjob.RemoveRange(5000, amountrecordset - 5000);
                }
                #endregion




            }
            catch (Exception ex)
            {
                Console.WriteLine("ex : " + ex);
            }




            return View(logmonitorjob.ToPagedList(pageNum, (int)param.PAGESIZE));

        }




        public async Task<IActionResult> LogMonitorMenu(string cmdButton, string TermID, string FrDate, string ToDate, string FrTime, string ToTime
 , string currTID, string currFr, string currTo, string currFrTime, string currToTime, string lstPageSize
 , string currPageSize, int? page, string maxRows, string terminalType, string startDate, string bankName)
        {


            List<fileLog_monitoring_report> fileLog_monitoring_report = new List<fileLog_monitoring_report>();

            List<Device_info_record> device_Info_Records = new List<Device_info_record>();

            DBService_TermProb dBService = new DBService_TermProb(_myConfiguration, _myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection"));




            if (startDate == null || startDate == "")
                startDate = DateTime.Now.ToString("yyyy-MM-dd");

            ViewBag.startDate = startDate;

            int pageNum = 1;

            try
            {


                if (cmdButton == "Clear")
                    return RedirectToAction("LogMonitorMenu");

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

                device_Info_Records = dBService.GetDeviceInfoFeelview();


                var additionalItems = device_Info_Records.Select(x => x.COUNTER_CODE).Distinct();


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




                fileLog_monitoring_report = dBService.GetFileLogMonitoringReports(FrDate + " 00:00:00 ", ToDate + " 23:59:59");

                fileLog_monitoring_report = fileLog_monitoring_report
    .OrderByDescending(report => report.date) 
    .ToList();


                foreach (var report in fileLog_monitoring_report)
                {

                    var deviceInfo = device_Info_Records.FirstOrDefault(d => d.TERM_ID == report.terminal_id);

                    if (deviceInfo != null)
                    {

                        report.terminal_Name = deviceInfo.TERM_NAME;
                        report.serial_No = deviceInfo.TERM_SEQ;


                    }
                }


                //var result = dBService.GetAllCountLogMonitoringFail(TermID);

                //Console.WriteLine($"ejLog False Count: {result.ejLogFalseCount}");
                //Console.WriteLine($"eCatLog False Count: {result.eCatLogFalseCount}");
                //Console.WriteLine($"comLog False Count: {result.comLogFalseCount}");
                //Console.WriteLine($"imageLog False Count: {result.imageLogFalseCount}");



                #region Set page
                long recCnt = 0;

                if (String.IsNullOrEmpty(maxRows))
                    ViewBag.maxRows = "50";
                else
                    ViewBag.maxRows = maxRows;


                if (null == fileLog_monitoring_report || fileLog_monitoring_report.Count <= 0)
                {
                    ViewBag.NoData = "true";

                }
                else
                {
                    recCnt = fileLog_monitoring_report.Count;

                    param.PAGESIZE = fileLog_monitoring_report.Count;
                }




                if (recCnt > 0)
                {
                    ViewBag.Records = String.Format("{0:#,##0}", recCnt.ToString());
                }
                else
                    ViewBag.Records = "0";

                pageNum = (page ?? 1);

                int amountrecordset = fileLog_monitoring_report.Count();

                if (amountrecordset > 5000)
                {
                    fileLog_monitoring_report.RemoveRange(5000, amountrecordset - 5000);
                }
                #endregion




            }
            catch (Exception ex)
            {
                Console.WriteLine("ex : " + ex);
            }




            return View(fileLog_monitoring_report.ToPagedList(pageNum, (int)param.PAGESIZE));

        }






        public async Task<IActionResult> EJournalMenu_Rerun(string cmdButton, string TermID, string FrDate, string ToDate, string FrTime, string ToTime
  , string currTID, string currFr, string currTo, string currFrTime, string currToTime, string lstPageSize
  , string currPageSize, int? page, string maxRows, string terminalType, string startDate, string bankName , string status)
        {

            string host = _myConfiguration.GetValue<string>("FileServer:IP");
            string username = _myConfiguration.GetValue<string>("FileServer:Username");
            string password = _myConfiguration.GetValue<string>("FileServer:Password");
            string remoteFilePath = _myConfiguration.GetValue<string>("FileServer:partLinuxUploadFileBackUp_BK");


            List<string> terminals = new List<string>();

            List<EJ_Job> ej_Job = new List<EJ_Job>();



            List<Device_info_record> device_Info_Records = new List<Device_info_record>();

            DBService_TermProb dBService = new DBService_TermProb(_myConfiguration, _myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection"));



            device_Info_Records = dBService.GetDeviceInfoFeelview();



            if (startDate == null || startDate == "")
                startDate = DateTime.Now.ToString("yyyy-MM-dd");

            ViewBag.startDate = startDate;

            int pageNum = 1;

            try
            {


                if (cmdButton == "Clear")
                    return RedirectToAction("EJournalMenu_Rerun");

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

                ej_Job = dBService.SelectDataFromJobEJ(FrDate, ToDate);

                var additionalItems = device_Info_Records.Select(x => x.COUNTER_CODE).Distinct();


                var item = new List<string> { };


                ViewBag.probTermStr = new SelectList(additionalItems.Concat(item).ToList());



                List<Device_info_record> filteredRecords = device_Info_Records
                        .Where(device => terminals.Contains(device.TERM_ID))
                        .ToList();

                ViewBag.CurrentTID = filteredRecords;
                ViewBag.TermID = TermID;

                if(status != null && status != "")
                {
                    ej_Job = ej_Job.Where(job => job.Status == status).ToList();
                    ViewBag.status = status;

                }
              

                    #region Set page
                    long recCnt = 0;

                if (String.IsNullOrEmpty(maxRows))
                    ViewBag.maxRows = "50";
                else
                    ViewBag.maxRows = maxRows;


                if (null == ej_Job || ej_Job.Count <= 0)
                {
                    ViewBag.NoData = "true";

                }
                else
                {
                    recCnt = ej_Job.Count;

                    param.PAGESIZE = ej_Job.Count;
                }




                if (recCnt > 0)
                {
                    ViewBag.Records = String.Format("{0:#,##0}", recCnt.ToString());
                }
                else
                    ViewBag.Records = "0";

                pageNum = (page ?? 1);

                int amountrecordset = ej_Job.Count();

                if (amountrecordset > 5000)
                {
                    ej_Job.RemoveRange(5000, amountrecordset - 5000);
                }
                #endregion




            }
            catch (Exception)
            {

            }




            return View(ej_Job.ToPagedList(pageNum, (int)param.PAGESIZE));

        }


        public async Task<IActionResult> EJournalMenu_BK(string cmdButton, string TermID, string FrDate, string ToDate, string FrTime, string ToTime
   , string currTID, string currFr, string currTo, string currFrTime, string currToTime, string lstPageSize
   , string currPageSize, int? page, string maxRows, string terminalType, string startDate, string bankName)
        {

            string host = _myConfiguration.GetValue<string>("FileServer:IP");
            string username = _myConfiguration.GetValue<string>("FileServer:Username");
            string password = _myConfiguration.GetValue<string>("FileServer:Password");
            string remoteFilePath = _myConfiguration.GetValue<string>("FileServer:partLinuxUploadFileBackUp_BK");


            List<string> terminals = new List<string>();

            List<EJournalModel> journalListResult = new List<EJournalModel>();


            List<Device_info_record> device_Info_Records = new List<Device_info_record>();

            DBService_TermProb dBService = new DBService_TermProb(_myConfiguration, _myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection"));

            device_Info_Records = dBService.GetDeviceInfoFeelview();



            if (startDate == null || startDate == "")
                startDate = DateTime.Now.ToString("yyyy-MM-dd");

            ViewBag.startDate = startDate;

            int pageNum = 1;

            try
            {


                if (cmdButton == "Clear")
                    return RedirectToAction("EJournalMenu_BK");

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

                            Parallel.ForEach(terminals, terminal =>
                            {
                                if (TermID != null)
                                {
                                    if (terminal != TermID) return; // Skip if terminal does not match TermID

                                    string termianlPath = Path.Combine(remoteFilePath, terminal);

                                    // Get the directories in parallel as well
                                    var directories = ListFilesRecursively(sftpClient, termianlPath);

                                    // Use Parallel.ForEach for processing each directory
                                    Parallel.ForEach(directories, directory =>
                                    {
                                        var fileInfo = sftpClient.Get(directory);
                                        try
                                        {
                                            string dateFromFile = fileInfo.Name.Substring(2, 8);
                                            DateTime checkDateTemp;

                                            if (DateTime.TryParseExact(dateFromFile, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out checkDateTemp))
                                            {
                                                if (checkDateTemp < startDateTemp || checkDateTemp > endDateTemp)
                                                {
                                                    return; // Skip if the date is out of range
                                                }

                                                if (fileInfo.Name.EndsWith(".txt"))
                                                {
                                                    Device_info_record filteredRecordsTemp = device_Info_Records
                                                        .FirstOrDefault(device => device.TERM_ID == terminal);

                                                    var journal = new EJournalModel
                                                    {
                                                        ID = GenerateUniqueID(),
                                                        SerialNo = filteredRecordsTemp.TERM_SEQ,
                                                        TerminalName = filteredRecordsTemp.TERM_NAME,
                                                        TerminalType = filteredRecordsTemp.COUNTER_CODE,
                                                        FileName = fileInfo.Name,
                                                        FileContent = "",  // If you want to read file content, do it here
                                                        TerminalID = terminal,
                                                        UpdateDate = fileInfo.LastWriteTime.ToString("yyyy-MM-dd"),
                                                        LastUploadingTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                        pathOfFile = fileInfo.FullName,
                                                        FileLength = FormatFileLength(fileInfo.Length),
                                                        UploadStatus = "Success"
                                                    };

                                                    journalListResult.Add(journal); // Add to thread-safe collection
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Error while processing file {fileInfo.Name} in directory {directory}: {ex.Message}");
                                        }
                                    });
                                }
                            });

                            #region Can be use 
                            //foreach (var terminal in terminals) //Terminal
                            //{

                            //    if (TermID != null)
                            //    {
                            //        if (terminal != TermID) continue;

                            //        string termianlPath = Path.Combine(remoteFilePath, terminal);

                            //        List<string> directories = ListFilesRecursively(sftpClient, termianlPath);


                            //        foreach (var directory in directories)
                            //        {

                            //            var fileInfo = sftpClient.Get(directory);
                            //            try
                            //            {

                            //                string dateFromFile = fileInfo.Name.Substring(2, 8);
                            //                DateTime checkDateTemp;


                            //                if (DateTime.TryParseExact(dateFromFile, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out checkDateTemp))
                            //                {

                            //                    if (checkDateTemp < startDateTemp || checkDateTemp > endDateTemp)
                            //                    {
                            //                        continue; 
                            //                    }


                            //                    if (fileInfo.Name.EndsWith(".txt"))
                            //                    {



                            //                        Device_info_record filteredRecordsTemp = device_Info_Records
                            //                            .FirstOrDefault(device => device.TERM_ID == terminal);


                            //                        var journal = new EJournalModel
                            //                        {
                            //                            ID = GenerateUniqueID(),
                            //                            SerialNo = filteredRecordsTemp.TERM_SEQ,
                            //                            TerminalName = filteredRecordsTemp.TERM_NAME,
                            //                            TerminalType = filteredRecordsTemp.COUNTER_CODE,
                            //                            FileName = fileInfo.Name,
                            //                            FileContent = "",  // ถ้าต้องการอ่านเนื้อหาของไฟล์ สามารถทำได้ที่นี่
                            //                            TerminalID = terminal,
                            //                            UpdateDate = fileInfo.LastWriteTime.ToString("yyyy-MM-dd"),
                            //                            LastUploadingTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            //                            pathOfFile = fileInfo.FullName,
                            //                            FileLength = FormatFileLength(fileInfo.Length),
                            //                            UploadStatus = "Success"
                            //                        };


                            //                        journalListResult.Add(journal);
                            //                    }
                            //                }
                            //            }
                            //            catch (Exception ex)
                            //            {
                            //                Console.WriteLine($"Error while processing file {fileInfo.Name} in directory {directory}: {ex.Message}");
                            //            }


                            //        }


                            //        #region parallel
                            //        //List<string> directories = ListFilesRecursively(sftpClient, termianlPath);

                            //        //Parallel.ForEach(directories, (directory) =>
                            //        //{
                            //        //    try
                            //        //    {
                            //        //        try
                            //        //        {
                            //        //            var files = sftpClient.ListDirectory(directory).Where(file => !file.Name.StartsWith(".")).ToList();
                            //        //        }
                            //        //        catch (Exception ex)
                            //        //        {
                            //        //            Console.WriteLine($"Error while listing directory {directory}: {ex.Message}");
                            //        //        }
                            //        //        foreach (var file in files)
                            //        //        {

                            //        //            string dateFromFile = file.Name.Substring(2, 8);
                            //        //            DateTime checkDate;

                            //        //            if (DateTime.TryParseExact(dateFromFile, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out checkDate))
                            //        //            {
                            //        //                if (checkDate < startDateTemp || checkDate > endDateTemp)
                            //        //                {
                            //        //                    continue;
                            //        //                }

                            //        //                if (file.Name.EndsWith(".txt"))
                            //        //                {

                            //        //                    var fileInfo = file;


                            //        //                    Device_info_record filteredRecordsTemp = device_Info_Records
                            //        //                        .FirstOrDefault(device => device.TERM_ID == terminal);

                            //        //                    var journal = new EJournalModel
                            //        //                    {
                            //        //                        ID = GenerateUniqueID(),
                            //        //                        SerialNo = filteredRecordsTemp.TERM_SEQ,
                            //        //                        TerminalName = filteredRecordsTemp.TERM_NAME,
                            //        //                        TerminalType = filteredRecordsTemp.COUNTER_CODE,
                            //        //                        FileName = file.Name,
                            //        //                        FileContent = "",
                            //        //                        TerminalID = terminal,
                            //        //                        UpdateDate = file.LastWriteTime.ToString("yyyy-MM-dd"),
                            //        //                        LastUploadingTime = file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            //        //                        pathOfFile = file.FullName,
                            //        //                        FileLength = FormatFileLength(file.Length),
                            //        //                        UploadStatus = "Success"
                            //        //                    };

                            //        //                    journalListResult.Add(journal);
                            //        //                }
                            //        //            }
                            //        //        }

                            //        //    }
                            //        //    catch (Exception ex)
                            //        //    {

                            //        //    }

                            //        //});
                            //        #endregion

                            //        #region Get file EJYYYYMMDD.txt

                            //        //foreach (var fileDay in dayPath)
                            //        //{
                            //        //    if (!fileDay.IsDirectory && fileDay.Name != "." && fileDay.Name != "..")
                            //        //    {



                            //        //        string dateFromFile = fileDay.Name.Substring(2, 8);

                            //        //        checkDate = DateTime.ParseExact(dateFromFile, "yyyyMMdd", null);


                            //        //        if (checkDate < startDateTemp || checkDate > endDateTemp)
                            //        //        {
                            //        //            continue;
                            //        //        }

                            //        //        string dayPathStr = termianlPath + "/" + fileDay.Name;

                            //        //        Console.WriteLine("Reading from path: " + termianlPath);


                            //        //        if (fileDay.Name.EndsWith(".txt"))
                            //        //        {


                            //        //            //string content = reader.ReadToEnd();
                            //        //            Device_info_record filteredRecordsTemp = device_Info_Records
                            //        //             .FirstOrDefault(device => device.TERM_ID == terminal);


                            //        //            var journal = new EJournalModel
                            //        //            {
                            //        //                ID = GenerateUniqueID(),
                            //        //                SerialNo = filteredRecordsTemp.TERM_SEQ,
                            //        //                TerminalName = filteredRecordsTemp.TERM_NAME,
                            //        //                TerminalType = filteredRecordsTemp.COUNTER_CODE,
                            //        //                FileName = fileDay.Name,
                            //        //                FileContent = "",
                            //        //                TerminalID = terminal,
                            //        //                UpdateDate = fileDay.LastWriteTime.ToString("yyyy-MM-dd"),
                            //        //                LastUploadingTime = fileDay.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            //        //                pathOfFile = fileDay.FullName,
                            //        //                FileLength = FormatFileLength(fileDay.Length),
                            //        //                UploadStatus = "Success"

                            //        //            };


                            //        //            journalListResult.Add(journal);


                            //        //        }

                            //        //    }

                            //        //}

                            //        #endregion
                            //    }







                            //}

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


                #endregion


                var additionalItems = device_Info_Records.Select(x => x.COUNTER_CODE).Distinct();


                var item = new List<string> { };


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


        public async Task<IActionResult> EJournalMenu(string cmdButton, string TermID, string FrDate, string ToDate, string FrTime, string ToTime
    , string currTID, string currFr, string currTo, string currFrTime, string currToTime, string lstPageSize
    , string currPageSize, int? page, string maxRows, string terminalType, string startDate, string bankName)
        {

            string host = _myConfiguration.GetValue<string>("FileServer:IP");
            string username = _myConfiguration.GetValue<string>("FileServer:Username");
            string password = _myConfiguration.GetValue<string>("FileServer:Password");
            string remoteFilePath = _myConfiguration.GetValue<string>("FileServer:partLinuxUploadFileBackUp");


            List<string> terminals = new List<string>();

            List<EJournalModel> journalListResult = new List<EJournalModel>();


            List<Device_info_record> device_Info_Records = new List<Device_info_record>();

            DBService_TermProb dBService = new DBService_TermProb(_myConfiguration, _myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection"));

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

                            Parallel.ForEach(terminals, terminal =>
                            {
                                if (TermID != null)
                                {
                                    if (terminal != TermID) return; // Skip if terminal does not match TermID

                                    string termianlPath = Path.Combine(remoteFilePath, terminal);

                                    // Get the directories in parallel as well
                                    var directories = ListFilesRecursively(sftpClient, termianlPath);

                                    // Use Parallel.ForEach for processing each directory
                                    Parallel.ForEach(directories, directory =>
                                    {
                                        var fileInfo = sftpClient.Get(directory);
                                        try
                                        {
                                            string dateFromFile = fileInfo.Name.Substring(2, 8);
                                            DateTime checkDateTemp;

                                            if (DateTime.TryParseExact(dateFromFile, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out checkDateTemp))
                                            {
                                                if (checkDateTemp < startDateTemp || checkDateTemp > endDateTemp)
                                                {
                                                    return; // Skip if the date is out of range
                                                }

                                                if (fileInfo.Name.EndsWith(".txt"))
                                                {
                                                    Device_info_record filteredRecordsTemp = device_Info_Records
                                                        .FirstOrDefault(device => device.TERM_ID == terminal);

                                                    var journal = new EJournalModel
                                                    {
                                                        ID = GenerateUniqueID(),
                                                        SerialNo = filteredRecordsTemp.TERM_SEQ,
                                                        TerminalName = filteredRecordsTemp.TERM_NAME,
                                                        TerminalType = filteredRecordsTemp.COUNTER_CODE,
                                                        FileName = fileInfo.Name,
                                                        FileContent = "",  // If you want to read file content, do it here
                                                        TerminalID = terminal,
                                                        UpdateDate = fileInfo.LastWriteTime.ToString("yyyy-MM-dd"),
                                                        LastUploadingTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                        pathOfFile = fileInfo.FullName,
                                                        FileLength = FormatFileLength(fileInfo.Length),
                                                        UploadStatus = "Success"
                                                    };

                                                    journalListResult.Add(journal); // Add to thread-safe collection
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Error while processing file {fileInfo.Name} in directory {directory}: {ex.Message}");
                                        }
                                    });
                                }
                            });

                            #region Can be use 
                            //foreach (var terminal in terminals) //Terminal
                            //{

                            //    if (TermID != null)
                            //    {
                            //        if (terminal != TermID) continue;

                            //        string termianlPath = Path.Combine(remoteFilePath, terminal);

                            //        List<string> directories = ListFilesRecursively(sftpClient, termianlPath);


                            //        foreach (var directory in directories)
                            //        {

                            //            var fileInfo = sftpClient.Get(directory);
                            //            try
                            //            {

                            //                string dateFromFile = fileInfo.Name.Substring(2, 8);
                            //                DateTime checkDateTemp;


                            //                if (DateTime.TryParseExact(dateFromFile, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out checkDateTemp))
                            //                {

                            //                    if (checkDateTemp < startDateTemp || checkDateTemp > endDateTemp)
                            //                    {
                            //                        continue; 
                            //                    }


                            //                    if (fileInfo.Name.EndsWith(".txt"))
                            //                    {



                            //                        Device_info_record filteredRecordsTemp = device_Info_Records
                            //                            .FirstOrDefault(device => device.TERM_ID == terminal);


                            //                        var journal = new EJournalModel
                            //                        {
                            //                            ID = GenerateUniqueID(),
                            //                            SerialNo = filteredRecordsTemp.TERM_SEQ,
                            //                            TerminalName = filteredRecordsTemp.TERM_NAME,
                            //                            TerminalType = filteredRecordsTemp.COUNTER_CODE,
                            //                            FileName = fileInfo.Name,
                            //                            FileContent = "",  // ถ้าต้องการอ่านเนื้อหาของไฟล์ สามารถทำได้ที่นี่
                            //                            TerminalID = terminal,
                            //                            UpdateDate = fileInfo.LastWriteTime.ToString("yyyy-MM-dd"),
                            //                            LastUploadingTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            //                            pathOfFile = fileInfo.FullName,
                            //                            FileLength = FormatFileLength(fileInfo.Length),
                            //                            UploadStatus = "Success"
                            //                        };


                            //                        journalListResult.Add(journal);
                            //                    }
                            //                }
                            //            }
                            //            catch (Exception ex)
                            //            {
                            //                Console.WriteLine($"Error while processing file {fileInfo.Name} in directory {directory}: {ex.Message}");
                            //            }


                            //        }


                            //        #region parallel
                            //        //List<string> directories = ListFilesRecursively(sftpClient, termianlPath);

                            //        //Parallel.ForEach(directories, (directory) =>
                            //        //{
                            //        //    try
                            //        //    {
                            //        //        try
                            //        //        {
                            //        //            var files = sftpClient.ListDirectory(directory).Where(file => !file.Name.StartsWith(".")).ToList();
                            //        //        }
                            //        //        catch (Exception ex)
                            //        //        {
                            //        //            Console.WriteLine($"Error while listing directory {directory}: {ex.Message}");
                            //        //        }
                            //        //        foreach (var file in files)
                            //        //        {

                            //        //            string dateFromFile = file.Name.Substring(2, 8);
                            //        //            DateTime checkDate;

                            //        //            if (DateTime.TryParseExact(dateFromFile, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out checkDate))
                            //        //            {
                            //        //                if (checkDate < startDateTemp || checkDate > endDateTemp)
                            //        //                {
                            //        //                    continue;
                            //        //                }

                            //        //                if (file.Name.EndsWith(".txt"))
                            //        //                {

                            //        //                    var fileInfo = file;


                            //        //                    Device_info_record filteredRecordsTemp = device_Info_Records
                            //        //                        .FirstOrDefault(device => device.TERM_ID == terminal);

                            //        //                    var journal = new EJournalModel
                            //        //                    {
                            //        //                        ID = GenerateUniqueID(),
                            //        //                        SerialNo = filteredRecordsTemp.TERM_SEQ,
                            //        //                        TerminalName = filteredRecordsTemp.TERM_NAME,
                            //        //                        TerminalType = filteredRecordsTemp.COUNTER_CODE,
                            //        //                        FileName = file.Name,
                            //        //                        FileContent = "",
                            //        //                        TerminalID = terminal,
                            //        //                        UpdateDate = file.LastWriteTime.ToString("yyyy-MM-dd"),
                            //        //                        LastUploadingTime = file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            //        //                        pathOfFile = file.FullName,
                            //        //                        FileLength = FormatFileLength(file.Length),
                            //        //                        UploadStatus = "Success"
                            //        //                    };

                            //        //                    journalListResult.Add(journal);
                            //        //                }
                            //        //            }
                            //        //        }

                            //        //    }
                            //        //    catch (Exception ex)
                            //        //    {

                            //        //    }

                            //        //});
                            //        #endregion

                            //        #region Get file EJYYYYMMDD.txt

                            //        //foreach (var fileDay in dayPath)
                            //        //{
                            //        //    if (!fileDay.IsDirectory && fileDay.Name != "." && fileDay.Name != "..")
                            //        //    {



                            //        //        string dateFromFile = fileDay.Name.Substring(2, 8);

                            //        //        checkDate = DateTime.ParseExact(dateFromFile, "yyyyMMdd", null);


                            //        //        if (checkDate < startDateTemp || checkDate > endDateTemp)
                            //        //        {
                            //        //            continue;
                            //        //        }

                            //        //        string dayPathStr = termianlPath + "/" + fileDay.Name;

                            //        //        Console.WriteLine("Reading from path: " + termianlPath);


                            //        //        if (fileDay.Name.EndsWith(".txt"))
                            //        //        {


                            //        //            //string content = reader.ReadToEnd();
                            //        //            Device_info_record filteredRecordsTemp = device_Info_Records
                            //        //             .FirstOrDefault(device => device.TERM_ID == terminal);


                            //        //            var journal = new EJournalModel
                            //        //            {
                            //        //                ID = GenerateUniqueID(),
                            //        //                SerialNo = filteredRecordsTemp.TERM_SEQ,
                            //        //                TerminalName = filteredRecordsTemp.TERM_NAME,
                            //        //                TerminalType = filteredRecordsTemp.COUNTER_CODE,
                            //        //                FileName = fileDay.Name,
                            //        //                FileContent = "",
                            //        //                TerminalID = terminal,
                            //        //                UpdateDate = fileDay.LastWriteTime.ToString("yyyy-MM-dd"),
                            //        //                LastUploadingTime = fileDay.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            //        //                pathOfFile = fileDay.FullName,
                            //        //                FileLength = FormatFileLength(fileDay.Length),
                            //        //                UploadStatus = "Success"

                            //        //            };


                            //        //            journalListResult.Add(journal);


                            //        //        }

                            //        //    }

                            //        //}

                            //        #endregion
                            //    }







                            //}

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


                #endregion


                var additionalItems = device_Info_Records.Select(x => x.COUNTER_CODE).Distinct();


                var item = new List<string> { };


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


        public async Task<IActionResult> EJournalMenu_Compare(string cmdButton, string TermID, string FrDate, string ToDate, string FrTime, string ToTime
, string currTID, string currFr, string currTo, string currFrTime, string currToTime, string lstPageSize
, string currPageSize, int? page, string maxRows, string terminalType, string startDate, string status)
        {

            string host = _myConfiguration.GetValue<string>("FileServer:IP");
            string username = _myConfiguration.GetValue<string>("FileServer:Username");
            string password = _myConfiguration.GetValue<string>("FileServer:Password");
            string remoteFilePath = _myConfiguration.GetValue<string>("FileServer:partLinuxUploadFileBackUp");


            List<string> terminals = new List<string>();

            List<ejlog_compare> ejlog_compare = new List<ejlog_compare>();


            List<Device_info_record> device_Info_Records = new List<Device_info_record>();

            DBService_TermProb dBService = new DBService_TermProb(_myConfiguration, _myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection"));

            device_Info_Records = dBService.GetDeviceInfoFeelview();



            if (startDate == null || startDate == "")
                startDate = DateTime.Now.ToString("yyyy-MM-dd");

            ViewBag.startDate = startDate;

            int pageNum = 1;

            try
            {


                if (cmdButton == "Clear")
                    return RedirectToAction("EJournalMenu_Compare");

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

                ejlog_compare = dBService.GetAllEjLogCompare();

                 var additionalItems = device_Info_Records.Select(x => x.COUNTER_CODE).Distinct();


                var item = new List<string> { };


                ViewBag.probTermStr = new SelectList(additionalItems.Concat(item).ToList());



                //List<Device_info_record> filteredRecords = device_Info_Records                     
                //        .ToList();

                ViewBag.CurrentTID = device_Info_Records;
                ViewBag.TermID = TermID;

                ejlog_compare = (from log in ejlog_compare
                                 join dev in device_Info_Records on log.terminalid equals dev.TERM_ID into gj
                                 from dev in gj.DefaultIfEmpty()
                                 select new ejlog_compare
                                 {
                                     idejlog_compare = log.idejlog_compare,
                                     updatedate = log.updatedate,
                                     ejlog_name = log.ejlog_name,
                                     ejlog_datetime = log.ejlog_datetime,
                                     status = log.status,
                                     terminalid = log.terminalid,
                                     remark = log.remark,
                                     SerialNo = dev?.TERM_SEQ ?? "",            // fallback เป็นค่าว่างถ้าไม่เจอ
                                     TerminalName = dev?.TERM_NAME ?? ""
                                 }).ToList();



                #region Set page
                long recCnt = 0;

                if (String.IsNullOrEmpty(maxRows))
                    ViewBag.maxRows = "50";
                else
                    ViewBag.maxRows = maxRows;

                if (ejlog_compare.Count > 0)
                {

                    if (!string.IsNullOrEmpty(TermID))
                    {
                        ejlog_compare = ejlog_compare
                            .Where(z => z.terminalid.Contains(TermID))
                            .ToList();
                    }

                    if (!string.IsNullOrEmpty(FrDate) && !string.IsNullOrEmpty(ToDate))
                    {
                        if (DateTime.TryParse(FrDate, out DateTime fromDate) && DateTime.TryParse(ToDate, out DateTime toDate))
                        {
                            ejlog_compare = ejlog_compare
                                .Where(x =>
                                {
                                    if (DateTime.TryParse(x.ejlog_datetime, out DateTime logDate))
                                    {
                                        return logDate.Date >= fromDate.Date && logDate.Date <= toDate.Date;
                                    }
                                    return false;
                                })
                                .ToList();
                        }
                    }

                    if (!string.IsNullOrEmpty(status))
                    {
                        ejlog_compare = ejlog_compare
                            .Where(x => x.status.Equals(status, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                    }

                    ejlog_compare = ejlog_compare
                        .OrderBy(x =>
                        {
                            if (DateTime.TryParse(x.updatedate, out DateTime updated))
                                return updated;
                            else
                                return DateTime.MinValue; // ถ้าแปลงไม่ได้ให้เลื่อนไปท้าย
                        })
                        .ToList();


                }



                if (null == ejlog_compare || ejlog_compare.Count <= 0)
                {
                    ViewBag.NoData = "true";

                }
                else
                {
                    recCnt = ejlog_compare.Count;

                    param.PAGESIZE = ejlog_compare.Count;
                }




                if (recCnt > 0)
                {
                    ViewBag.Records = String.Format("{0:#,##0}", recCnt.ToString());
                }
                else
                    ViewBag.Records = "0";

                pageNum = (page ?? 1);

                int amountrecordset = ejlog_compare.Count();

                if (amountrecordset > 5000)
                {
                    ejlog_compare.RemoveRange(5000, amountrecordset - 5000);
                }
                #endregion




            }
            catch (Exception)
            {

            }




            return View(ejlog_compare.ToPagedList(pageNum, (int)param.PAGESIZE));

        }




        [HttpPost]
        public ActionResult GetFileContent([FromBody] string pathOfFile)
        {
            if (!string.IsNullOrEmpty(pathOfFile))
            {
               
                var fileContent = GetFileContentFromSFTP(pathOfFile);

                if (!string.IsNullOrEmpty(fileContent))
                {
                    return Content(fileContent); 
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

            return $"{Math.Round(length, 2)} {units[unitIndex]}"; 
        }

    }
}
