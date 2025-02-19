﻿using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SLA_Management.Data;
using SLA_Management.Commons;
using SLA_Management.Models.OperationModel;
using System.Data;
using SLA_Management.Data.ExcelUtilitie;

namespace SLA_Management.Controllers
{
    public class MaintenanceController : Controller
    {
        private IConfiguration _myConfiguration;
        private static ConnectMySQL db_fv;
        private static List<InventoryMaintenanceModel> Inventory_dataList = new List<InventoryMaintenanceModel>();
        private static List<WhitelistFilterTemplateModel> WhitelistFilterTemplates_datalist = new List<WhitelistFilterTemplateModel>();
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
        public IActionResult InventoryFetchData(string terminalseq,string terminalno,string terminaltype, string connencted,string servicetype,string countertype,string status, string row, string page, string search,string fromdate,string todate,string currentlyinuse)
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
                filterquery += " and pv.TERM_ID like '%" + terminalno + "%' ";
            }
            if (terminalseq != "")
            {
                filterquery += " and di.TERM_SEQ = '"+ terminalseq +"' ";

            }
            if(terminaltype != "")
            {
                filterquery += " and di.TYPE_ID = '" + terminaltype + "' ";
            }
            if (status == "use")
            {
                filterquery += " and (di.STATUS = 'use' or di.STATUS ='roustop') ";
            }
            else if(status == "notuse")
            {
                filterquery += " and di.STATUS = 'no' ";
            }        
            if(connencted == "0")
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
          
            if(servicetype != "")
            {
                filterquery += " and CONCAT(di.SERVICE_TYPE, ' ', di.BUSINESS_BEGINTIME, ' - ', di.BUSINESS_ENDTIME) = '"+ servicetype +"' ";
            }
            if (countertype != ""){
                filterquery += " and di.COUNTER_CODE = '"+ countertype +"' ";
            }
            if(fromdate != "" && todate != "")
            {
                filterquery += " and (STR_TO_DATE(di.SERVICE_ENDDATE, '%Y-%m-%d') between '" + fromdate + "' and '" + todate + "'";
                filterquery +=  "or(LENGTH(di.SERVICE_ENDDATE) = 0 and STR_TO_DATE(di.SERVICE_BEGINDATE, '%Y-%m-%d') < '" + todate + "'))";
            }
            else
            {
                filterquery += " and (STR_TO_DATE(di.SERVICE_ENDDATE, '%Y-%m-%d') between '2020-05-01' and '"+ DateTime.Now.ToString("yyyy-MM-dd") +"'";
                filterquery += " or(LENGTH(di.SERVICE_ENDDATE) = 0 and STR_TO_DATE(di.SERVICE_BEGINDATE, '%Y-%m-%d') < '" + DateTime.Now.ToString("yyyy-MM-dd") + "')) ";
            }
           if(currentlyinuse == "no")
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

                obj.GatewayOutput(Inventory_dataList, fromdate,todate);



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
    }
}
