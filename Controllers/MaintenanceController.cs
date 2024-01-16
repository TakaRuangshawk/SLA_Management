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

namespace SLA_Management.Controllers
{
    public class MaintenanceController : Controller
    {
        private IConfiguration _myConfiguration;
        private static ConnectMySQL db_fv;
        private static List<AdminCardModel> admincard_dataList = new List<AdminCardModel>();
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
            ViewBag.CurrentTSeq = null;
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
                filterquery += " di.STATUS = 'use' ";
            }
            else if(status == "notuse")
            {
                filterquery += " di.STATUS != 'use' ";
            }
            else
            {
                filterquery += "";
            }
            if(connencted == "0")
            {
                filterquery += " and die.CONN_STATUS_ID = '0' ";
            }
            else if (connencted == "1")
            {
                filterquery += " and die.CONN_STATUS_ID != '0' ";
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
            List<AdminCardModel> jsonData = new List<AdminCardModel>();

            using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection")))
            {
                connection.Open();

                // Modify the SQL query to use the 'input' parameter for filtering
                string query = @" SELECT di.DEVICE_ID,di.TERM_SEQ,di.TYPE_ID,pv.TERM_ID,
                CASE WHEN die.CONN_STATUS_ID = 0 THEN 'Online' 
                WHEN die.CONN_STATUS_ID is null and di.STATUS = 'no' THEN 'Unknown' 
                ELSE 'Offline' END AS Connected,
                CASE WHEN di.STATUS = 'use' THEN 'Active'
                ELSE 'Inactive' END AS Status,
                di.COUNTER_CODE,CONCAT(di.SERVICE_TYPE, ' ', di.BUSINESS_BEGINTIME, ' - ', di.BUSINESS_ENDTIME) as ServiceType,
                di.TERM_LOCATION,di.LATITUDE,di.LONGITUDE,di.CONTROL_BY,di.PROVINCE,di.SERVICE_BEGINDATE,
                CASE WHEN STATUS = 'no' AND (dsi.CONN_STATUS_ID IS NULL OR dsi.CONN_STATUS_ID = 0) THEN 'เครื่องไม่เปิดให้บริการ' 
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
                        jsonData.Add(new AdminCardModel
                        {
                            id = (id_row).ToString(),
                            term_seq = reader["term_seq"].ToString(),
                            term_id = reader["term_id"].ToString(),
                            term_name = reader["term_name"].ToString(),
                            admin_card_master = reader["admin_card_master"].ToString(),
                            admin_password_digits = reader["admin_password_digits"].ToString(),
                            status = reader["status"].ToString(),
                            update_date = Convert.ToDateTime(reader["update_date"]).ToString("yyyy-MM-dd HH:mm:ss"),
                        });
                    }
                }
            }
            admincard_dataList = jsonData;
            int pages = (int)Math.Ceiling((double)jsonData.Count() / _row);
            List<AdminCardModel> filteredData = RangeFilter(jsonData, _page, _row);
            var response = new DataResponse
            {
                JsonData = filteredData,
                Page = pages,
                currentPage = _page,
                TotalTerminal = jsonData.Count(),
            };
            return Json(response);
        }

        static List<AdminCardModel> RangeFilter<AdminCardModel>(List<AdminCardModel> inputList, int page, int row)
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

        public class AdminCardModel
        {
            public string id { get; set; }
            public string term_id { get; set; }
            public string term_seq { get; set; }
            public string term_name { get; set; }
            public string admin_card_master { get; set; }
            public string admin_password_digits { get; set; }
            public string status { get; set; }
            public string update_date { get; set; }
        }
        public class DataResponse
        {
            public List<AdminCardModel> JsonData { get; set; }
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
    }
}
