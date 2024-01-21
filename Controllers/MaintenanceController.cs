using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SLA_Management.Data;
using SLAManagement.Data;
using System.Drawing;
using System;
using static SLA_Management.Controllers.OperationController;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
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
        public IActionResult InventoryMonitor(string termid, string ticket, DateTime? todate, DateTime? fromdate, string mainproblem, string terminaltype, string jobno, string cmdButton)
        {
            ViewBag.maxRows = null;
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
        public IActionResult InventoryFetchData(string terminalseq,string terminalno,string terminaltype, string connencted,string servicetype,string countertype,string status, string row, string page, string search,string fromdate,string todate)
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
                filterquery += " and di.STATUS = 'use' ";
            }
            else if(status == "notuse")
            {
                filterquery += " and di.STATUS != 'use' ";
            }
            else
            {
                filterquery += "";
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
            else
            {
                filterquery += "";
            }
            if(servicetype != "")
            {
                filterquery += " and CONCAT(di.SERVICE_TYPE, ' ', di.BUSINESS_BEGINTIME, ' - ', di.BUSINESS_ENDTIME) = '"+ servicetype +"' ";
            }
            if (countertype != ""){
                filterquery += " and di.COUNTER_CODE = '"+ countertype +"' ";
            }
            if(fromdate != "")
            {
                filterquery += "and di.SERVICE_BEGINDATE >= '"+ fromdate +"'";
            }
            else
            {
                filterquery += " and di.SERVICE_BEGINDATE >= '2020-05-01' ";
            }
            if(todate != "")
            {
                filterquery += " and (di.SERVICE_ENDDATE <= '"+ todate +"' or SERVICE_ENDDATE = 'เครื่องไม่เปิดให้บริการ' or SERVICE_ENDDATE = 'เครื่องยังเปิดให้บริการ') ";
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
                string query = @" SELECT di.DEVICE_ID,di.TERM_SEQ,di.TYPE_ID,di.TERM_ID,di.TERM_NAME,
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
                FROM gsb_adm_fv.device_info di
				LEFT JOIN gsb_adm_fv.project_version pv ON di.TERM_ID = pv.TERM_ID
                left join device_status_info dsi on pv.TERM_ID = dsi.TERM_ID
                left join device_inner_event die on dsi.CONN_STATUS_EVENT_ID = die.EVENT_ID 
                where di.TERM_ID is not null ";


                query += filterquery + " order by di.TERM_SEQ asc,di.STATUS asc";

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
        }
        public class DataResponse_InventoryMaintenanceModel
        {
            public List<InventoryMaintenanceModel> JsonData { get; set; }
            public int Page { get; set; }
            public int currentPage { get; set; }
            public int TotalTerminal { get; set; }
        }
        #endregion

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


                    query += filterquery + " order by di.TERM_SEQ asc,di.STATUS asc";

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
    }
}
