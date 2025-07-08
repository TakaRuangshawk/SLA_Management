using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic.FileIO;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NuGet.DependencyResolver;
using PagedList;
using Renci.SshNet;
using SLA_Management.Commons;
using SLA_Management.Data;
using SLA_Management.Data.ExcelUtilitie;
using SLA_Management.Data.TermProb;
using SLA_Management.Models;
using SLA_Management.Models.Information;
using SLA_Management.Models.OperationModel;
using SLA_Management.Models.TermProbModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
            else if (status == "no")
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
                filterquery += $@"
        AND (
            di.SERVICE_ENDDATE IS NULL
            OR TRIM(di.SERVICE_ENDDATE) = ''
            OR COALESCE(
                STR_TO_DATE(di.SERVICE_ENDDATE, '%Y-%m-%d'),
                STR_TO_DATE(di.SERVICE_ENDDATE, '%Y/%m/%d')
            ) BETWEEN '{fromdate}' AND '{todate}'
        )";
            }
            else
            {
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                filterquery += $@"
        AND (
            di.SERVICE_ENDDATE IS NULL
            OR TRIM(di.SERVICE_ENDDATE) = ''
            OR COALESCE(
                STR_TO_DATE(di.SERVICE_ENDDATE, '%Y-%m-%d'),
                STR_TO_DATE(di.SERVICE_ENDDATE, '%Y/%m/%d')
            ) BETWEEN '2020-05-01' AND '{currentDate}'
        )";
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
               CASE 
    WHEN di.SERVICE_ENDDATE IS NULL OR TRIM(di.SERVICE_ENDDATE) = '' THEN 'Active'
    WHEN COALESCE(
            STR_TO_DATE(di.SERVICE_ENDDATE, '%Y-%m-%d'),
            STR_TO_DATE(di.SERVICE_ENDDATE, '%Y/%m/%d')
         ) > CURRENT_DATE THEN 'Active'
    ELSE 'Inactive'
  END AS Status,
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
        #region textfile upload
        [HttpPost]
        public async Task<IActionResult> UploadTerminal(IFormFile file, string bank)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please upload a valid text file.");
            }

            try
            {
                var connectionString = _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + bank);

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var stream = file.OpenReadStream())
                    using (var parser = new TextFieldParser(stream))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",");
                        parser.HasFieldsEnclosedInQuotes = false;

                        // ข้าม header row
                        if (!parser.EndOfData)
                            parser.ReadLine();

                        while (!parser.EndOfData)
                        {
                            var fields = parser.ReadFields();
                            if (fields == null || fields.Length < 70)
                                continue;

                            string counterCode = "";
                            if (file.FileName.Contains("587")) counterCode = "LOT587";
                            else if (file.FileName.Contains("572")) counterCode = "LOT572";
                            else if (file.FileName.Contains("362")) counterCode = "LOT362";

                            var terminal = new TerminalInformationModel
                            {
                                DEVICE_ID = CleanValue(fields[0]),
                                TERM_ID = CleanValue(fields[1]),
                                DEPT_ID = CleanValue(fields[2]),
                                TYPE_ID = CleanValue(fields[3]),
                                BRAND_ID = CleanValue(fields[4]),
                                MODEL_ID = CleanValue(fields[5]),
                                TERM_SEQ = CleanValue(fields[6]),
                                COUNTER_CODE = CleanValue(counterCode),
                                TERM_IP = CleanValue(fields[8]),
                                STATUS = CleanValue(fields[9]),
                                TERM_NAME = CleanValue(fields[10]),
                                TERM_ADDR = CleanValue(fields[11]),
                                TERM_LOCATION = CleanValue(fields[12]),
                                TERM_ZONE = CleanValue(fields[13]),
                                CONTROL_BY = CleanValue(fields[14]),
                                REPLENISH_BY = CleanValue(fields[15]),
                                POST = CleanValue(fields[16]),
                                INSTALL_DATE = CleanValue(fields[17]),
                                ACTIVE_DATE = CleanValue(fields[18]),
                                SERVICE_TYPE = CleanValue(fields[19]),
                                INSTALL_TYPE = CleanValue(fields[20]),
                                LAYOUT_TYPE = CleanValue(fields[21]),
                                MAN_ID = CleanValue(fields[22]),
                                SERVICEMAN_ID = CleanValue(fields[23]),
                                COMPANY_ID = CleanValue(fields[24]),
                                COMPANY_NAME = CleanValue(fields[25]),
                                SERVICE_BEGINDATE = CleanValue(fields[26]),
                                SERVICE_ENDDATE = CleanValue(fields[27]),
                                SERVICE_YEARS = CleanValue(fields[28]),
                                IS_CCTV = CleanValue(fields[29]),
                                IS_UPS = CleanValue(fields[30]),
                                IS_INTERNATIONAL = CleanValue(fields[31]),
                                BUSINESS_BEGINTIME = CleanValue(fields[32]),
                                BUSINESS_ENDTIME = CleanValue(fields[33]),
                                IS_VIP = CleanValue(fields[34]),
                                AREA_ID = CleanValue(fields[35]),
                                AREA_ADDR = CleanValue(fields[36]),
                                FUNCTION_TYPE = CleanValue(fields[37]),
                                LONGITUDE = CleanValue(fields[38]),
                                LATITUDE = CleanValue(fields[39]),
                                PROVINCE = CleanValue(fields[40]),
                                LOT_TYPE = CleanValue(fields[41]),
                                AUDITING = CleanValue(fields[42]),
                                CURRENT_IP = CleanValue(fields[43]),
                                VERSION_ATMC = CleanValue(fields[44]),
                                VERSION_SP = CleanValue(fields[45]),
                                VERSION_AGENT = CleanValue(fields[46]),
                                VERSION_MB = CleanValue(fields[47]),
                                FLAG_XFS = CleanValue(fields[48]),
                                FLAG_EJ = CleanValue(fields[49]),
                                FLAG_FSN = CleanValue(fields[50]),
                                EJ_FILES = CleanValue(fields[51]),
                                FSN_PATH = CleanValue(fields[52]),
                                TASK_PARA = CleanValue(fields[53]),
                                VERSION_AD = CleanValue(fields[54]),
                                MODIFY_USERID = CleanValue(fields[55]),
                                MODIFY_DATE = CleanValue(fields[56]),
                                ADD_USERID = CleanValue(fields[57]),
                                ADD_DATE = CleanValue(fields[58]),
                                ASSET_NO = CleanValue(fields[59]),
                                CASH_BOX_NUM = CleanValue(fields[60]),
                                SERVICE_SMS_TYPE = CleanValue(fields[61]),
                                EJ_OPEN_DATE = CleanValue(fields[62]),
                                ATMC_UPDATE_TIME = CleanValue(fields[63]),
                                SP_UPDATE_TIME = CleanValue(fields[64]),
                                AGENT_UPDATE_TIME = CleanValue(fields[65]),
                                VERSION_NV = CleanValue(fields[66]),
                                NV_UPDATE_TIME = CleanValue(fields[67]),
                                VERSION_MAIN = CleanValue(fields[68]),
                                MAIN_UPDATE_TIME = CleanValue(fields[69]),
                                LatestUpdatedBy = HttpContext.Session.GetString("Username"),
                                LatestUpdatedDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                            };

                            await InsertTextDatatoDB(connection, terminal);
                        }
                    }

                    return Ok("File processed and data inserted successfully.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private string CleanValue(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "null")
                return null;

            return new string(input
                .Where(c => !char.IsWhiteSpace(c)) // ลบทุกชนิดของ white space เช่น tab, space
                .ToArray());
        }

        private async Task InsertTextDatatoDB(MySqlConnection conn, TerminalInformationModel terminals)
        {

            try
            {
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO device_info
                                                (`DEVICE_ID`,`TERM_ID`,`DEPT_ID`,`TYPE_ID`,`BRAND_ID`,`MODEL_ID`,`TERM_SEQ`,`COUNTER_CODE`,`TERM_IP`,
                                                `STATUS`,`TERM_NAME`,`TERM_ADDR`,`TERM_LOCATION`,`TERM_ZONE`,`CONTROL_BY`,`REPLENISH_BY`,`POST`,
                                                `INSTALL_DATE`,`ACTIVE_DATE`,`SERVICE_TYPE`,`INSTALL_TYPE`,`LAYOUT_TYPE`,`MAN_ID`,`SERVICEMAN_ID`,
                                                `COMPANY_ID`,`COMPANY_NAME`,`SERVICE_BEGINDATE`,`SERVICE_ENDDATE`,`SERVICE_YEARS`,`IS_CCTV`,`IS_UPS`,
                                                `IS_INTERNATIONAL`,`BUSINESS_BEGINTIME`,`BUSINESS_ENDTIME`,`IS_VIP`,`AREA_ID`,`AREA_ADDR`,`FUNCTION_TYPE`,
                                                `LONGITUDE`,`LATITUDE`,`PROVINCE`,`LOT_TYPE`,`AUDITING`,`CURRENT_IP`,`VERSION_ATMC`,`VERSION_SP`,`VERSION_AGENT`,
                                                `VERSION_MB`,`FLAG_XFS`,`FLAG_EJ`,`FLAG_FSN`,`EJ_FILES`,`FSN_PATH`,`TASK_PARA`,`VERSION_AD`,`MODIFY_USERID`,
                                                `MODIFY_DATE`,`ADD_USERID`,`ADD_DATE`,`ASSET_NO`,`CASH_BOX_NUM`,`SERVICE_SMS_TYPE`,`EJ_OPEN_DATE`,
                                                `ATMC_UPDATE_TIME`,`SP_UPDATE_TIME`,`AGENT_UPDATE_TIME`,`VERSION_NV`,`NV_UPDATE_TIME`,`VERSION_MAIN`,`MAIN_UPDATE_TIME`)
                                                VALUES
                                                (@deviceid,@termid,@deptid,@typeid,@brandid,@modelid,@termseq,@countercode,@termip,@status,@termname,@termaddr,@termloc,
                                                @termzone,@controlby,@replishby,@post,@install,@active,@servicetype,@installtype,@layouttype,@manid,@serviceman,
                                                @company,@companyname,@sevicebdate,@serviceedate,@serviceyrs,@iscctv,@isups,
                                                @isinter,@businesstime,@bussinessendtime,@isvip,@areaid,@areaaddr,@function,@long,@lati,@prov,@lottype,@audit,@currentip,
                                                @veratmc,@versp,@veragent,@vermb,@flagxfs,@flagej,@flagfsn,@ejfile,@fsnpath,@task,@verad,@modifyid,@modifydate,@adduser,@adddate,
                                                @assetno,@cashbox,@servicesms,@ejopendate,@atmcupdatetime,@spupdatetime,@agentuptime,@vernv,@nvupdatetime,@vermain,@mainuptime)
                                                ON DUPLICATE KEY UPDATE 
                                                TERM_ID = @termid,DEPT_ID = @deptid,TYPE_ID = @typeid,BRAND_ID = @brandid,MODEL_ID = @modelid,TERM_SEQ = @termseq,
                                                COUNTER_CODE = @countercode,TERM_IP = @termip,STATUS = @status,TERM_NAME = @termname,TERM_ADDR = @termaddr,TERM_LOCATION = @termloc,
                                                TERM_ZONE = @termzone,CONTROL_BY = @controlby,REPLENISH_BY = @replishby,POST = @post,INSTALL_DATE = @install,ACTIVE_DATE = @active,
                                                SERVICE_TYPE = @servicetype,INSTALL_TYPE = @installtype,LAYOUT_TYPE = @layouttype,MAN_ID = @manid,SERVICEMAN_ID = @serviceman,
                                                COMPANY_ID = @company,COMPANY_NAME = @companyname,SERVICE_BEGINDATE = @sevicebdate,SERVICE_ENDDATE = @serviceedate,SERVICE_YEARS = @serviceyrs,
                                                IS_CCTV = @iscctv,IS_UPS = @isups,IS_INTERNATIONAL = @isinter,BUSINESS_BEGINTIME = @businesstime,BUSINESS_ENDTIME = @bussinessendtime,
                                                IS_VIP = @isvip,AREA_ID = @areaid,AREA_ADDR = @areaaddr,FUNCTION_TYPE = @function,LONGITUDE = @long,LATITUDE = @lati,
                                                PROVINCE = @prov,LOT_TYPE = @lottype,AUDITING = @audit,CURRENT_IP = @currentip,VERSION_ATMC = @veratmc,VERSION_SP = @versp,
                                                VERSION_AGENT = @veragent,VERSION_MB = @vermb,FLAG_XFS = @flagxfs,FLAG_EJ = @flagej,FLAG_FSN = @flagfsn,EJ_FILES = @ejfile,
                                                FSN_PATH = @fsnpath,TASK_PARA = @task,VERSION_AD = @verad,MODIFY_USERID = @modifyid,MODIFY_DATE = @modifydate,ADD_USERID = @adduser,
                                                ADD_DATE = @adddate,ASSET_NO = @assetno,CASH_BOX_NUM = @cashbox,SERVICE_SMS_TYPE = @servicesms,EJ_OPEN_DATE = @ejopendate,
                                                ATMC_UPDATE_TIME = @atmcupdatetime,SP_UPDATE_TIME = @spupdatetime,AGENT_UPDATE_TIME = @agentuptime,VERSION_NV = @vernv,
                                                NV_UPDATE_TIME = @nvupdatetime, VERSION_MAIN = @vermain,MAIN_UPDATE_TIME = @mainuptime";

                    #region
                    cmd.Parameters.AddWithValue("@deviceid", terminals.DEVICE_ID != "null" ? terminals.DEVICE_ID : DBNull.Value);
                    cmd.Parameters.AddWithValue("@termid", terminals.TERM_ID != "null" ? terminals.TERM_ID : DBNull.Value);
                    cmd.Parameters.AddWithValue("@deptid", terminals.DEPT_ID != "null" ? terminals.DEPT_ID : DBNull.Value);
                    cmd.Parameters.AddWithValue("@typeid", terminals.TYPE_ID != "null" ? terminals.TYPE_ID : DBNull.Value);
                    cmd.Parameters.AddWithValue("@brandid", terminals.BRAND_ID != "null" ? terminals.BRAND_ID : DBNull.Value);
                    cmd.Parameters.AddWithValue("@modelid", terminals.MODEL_ID != "null" ? terminals.MODEL_ID : DBNull.Value);
                    cmd.Parameters.AddWithValue("@termseq", terminals.TERM_SEQ != "null" ? terminals.TERM_SEQ : DBNull.Value);
                    cmd.Parameters.AddWithValue("@countercode", terminals.COUNTER_CODE != "null" ? terminals.COUNTER_CODE : DBNull.Value);
                    cmd.Parameters.AddWithValue("@termip", terminals.TERM_IP != "null" ? terminals.TERM_IP : DBNull.Value);
                    cmd.Parameters.AddWithValue("@status", terminals.STATUS != "null" ? terminals.STATUS : DBNull.Value);
                    cmd.Parameters.AddWithValue("@termname", terminals.TERM_NAME != "null" ? terminals.TERM_NAME : DBNull.Value);
                    cmd.Parameters.AddWithValue("@termaddr", terminals.TERM_ADDR != "null" ? terminals.TERM_ADDR : DBNull.Value);
                    cmd.Parameters.AddWithValue("@termloc", terminals.TERM_LOCATION != "null" ? terminals.TERM_LOCATION : DBNull.Value);
                    cmd.Parameters.AddWithValue("@termzone", terminals.TERM_ZONE != "null" ? terminals.TERM_ZONE : DBNull.Value);
                    cmd.Parameters.AddWithValue("@controlby", terminals.CONTROL_BY != "null" ? terminals.CONTROL_BY : DBNull.Value);
                    cmd.Parameters.AddWithValue("@replishby", terminals.REPLENISH_BY != "null" ? terminals.REPLENISH_BY : DBNull.Value);
                    cmd.Parameters.AddWithValue("@post", terminals.POST != "null" ? terminals.POST : DBNull.Value);
                    cmd.Parameters.AddWithValue("@install", terminals.INSTALL_DATE != "null" ? terminals.INSTALL_DATE : DBNull.Value);
                    cmd.Parameters.AddWithValue("@active", terminals.ACTIVE_DATE != "null" ? terminals.ACTIVE_DATE : DBNull.Value);
                    cmd.Parameters.AddWithValue("@servicetype", terminals.SERVICE_TYPE != "null" ? terminals.SERVICE_TYPE : DBNull.Value);
                    cmd.Parameters.AddWithValue("@installtype", terminals.INSTALL_TYPE != "null" ? terminals.INSTALL_TYPE : DBNull.Value);
                    cmd.Parameters.AddWithValue("@layouttype", terminals.LAYOUT_TYPE != "null" ? terminals.LAYOUT_TYPE : DBNull.Value);
                    cmd.Parameters.AddWithValue("@manid", terminals.MAN_ID != "null" ? terminals.MAN_ID : DBNull.Value);
                    cmd.Parameters.AddWithValue("@serviceman", terminals.SERVICEMAN_ID != "null" ? terminals.SERVICEMAN_ID : DBNull.Value);
                    cmd.Parameters.AddWithValue("@company", terminals.COMPANY_ID != "null" ? terminals.COMPANY_ID : DBNull.Value);
                    cmd.Parameters.AddWithValue("@companyname", terminals.COMPANY_NAME != "null" ? terminals.COMPANY_NAME : DBNull.Value);
                    cmd.Parameters.AddWithValue("@sevicebdate", terminals.SERVICE_BEGINDATE != "null" ? terminals.SERVICE_BEGINDATE : DBNull.Value);
                    cmd.Parameters.AddWithValue("@serviceedate", terminals.SERVICE_ENDDATE != "null" ? terminals.SERVICE_ENDDATE : DBNull.Value);
                    cmd.Parameters.AddWithValue("@serviceyrs", terminals.SERVICE_YEARS != "null" ? terminals.SERVICE_YEARS : DBNull.Value);
                    cmd.Parameters.AddWithValue("@iscctv", terminals.IS_CCTV != "null" ? terminals.IS_CCTV : DBNull.Value);
                    cmd.Parameters.AddWithValue("@isups", terminals.IS_UPS != "null" ? terminals.IS_UPS : DBNull.Value);
                    cmd.Parameters.AddWithValue("@isinter", terminals.IS_INTERNATIONAL != "null" ? terminals.IS_INTERNATIONAL : DBNull.Value);
                    cmd.Parameters.AddWithValue("@businesstime", terminals.BUSINESS_BEGINTIME != "null" ? terminals.BUSINESS_BEGINTIME : DBNull.Value);
                    cmd.Parameters.AddWithValue("@bussinessendtime", terminals.BUSINESS_ENDTIME != "null" ? terminals.BUSINESS_ENDTIME : DBNull.Value);
                    cmd.Parameters.AddWithValue("@isvip", terminals.IS_VIP != "null" ? terminals.IS_VIP : DBNull.Value);
                    cmd.Parameters.AddWithValue("@areaid", terminals.AREA_ID != "null" ? terminals.AREA_ID : DBNull.Value);
                    cmd.Parameters.AddWithValue("@areaaddr", terminals.AREA_ADDR != "null" ? terminals.AREA_ADDR : DBNull.Value);
                    cmd.Parameters.AddWithValue("@function", terminals.FUNCTION_TYPE != "null" ? terminals.FUNCTION_TYPE : DBNull.Value);
                    cmd.Parameters.AddWithValue("@long", terminals.LONGITUDE != "null" ? terminals.LONGITUDE : DBNull.Value);
                    cmd.Parameters.AddWithValue("@lati", terminals.LATITUDE != "null" ? terminals.LATITUDE : DBNull.Value);
                    cmd.Parameters.AddWithValue("@prov", terminals.PROVINCE != "null" ? terminals.PROVINCE : DBNull.Value);
                    cmd.Parameters.AddWithValue("@lottype", terminals.LOT_TYPE != "null" ? terminals.LOT_TYPE : DBNull.Value);
                    cmd.Parameters.AddWithValue("@audit", terminals.AUDITING != "null" ? terminals.AUDITING : DBNull.Value);
                    cmd.Parameters.AddWithValue("@currentip", terminals.CURRENT_IP != "null" ? terminals.CURRENT_IP : DBNull.Value);
                    cmd.Parameters.AddWithValue("@veratmc", terminals.VERSION_ATMC != "null" ? terminals.VERSION_ATMC : DBNull.Value);
                    cmd.Parameters.AddWithValue("@versp", terminals.VERSION_SP != "null" ? terminals.VERSION_SP : DBNull.Value);
                    cmd.Parameters.AddWithValue("@veragent", terminals.VERSION_AGENT != "null" ? terminals.VERSION_AGENT : DBNull.Value);
                    cmd.Parameters.AddWithValue("@vermb", terminals.VERSION_MB != "null" ? terminals.VERSION_MB : DBNull.Value);
                    cmd.Parameters.AddWithValue("@flagxfs", terminals.FLAG_XFS != "null" ? terminals.FLAG_XFS : DBNull.Value);
                    cmd.Parameters.AddWithValue("@flagej", terminals.FLAG_EJ != "null" ? terminals.FLAG_EJ : DBNull.Value);
                    cmd.Parameters.AddWithValue("@flagfsn", terminals.FLAG_FSN != "null" ? terminals.FLAG_FSN : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ejfile", terminals.EJ_FILES != "null" ? terminals.EJ_FILES : DBNull.Value);
                    cmd.Parameters.AddWithValue("@fsnpath", terminals.FSN_PATH != "null" ? terminals.FSN_PATH : DBNull.Value);
                    cmd.Parameters.AddWithValue("@task", terminals.TASK_PARA != "null" ? terminals.TASK_PARA : DBNull.Value);
                    cmd.Parameters.AddWithValue("@verad", terminals.VERSION_AD != "null" ? terminals.VERSION_AD : DBNull.Value);
                    cmd.Parameters.AddWithValue("@modifyid", terminals.MODIFY_USERID != "null" ? terminals.MODIFY_USERID : terminals.LatestUpdatedBy);
                    // Parse datetime or set to NULL
                    if (DateTime.TryParse(terminals.MODIFY_DATE, out var modifyDate))
                    {
                        cmd.Parameters.AddWithValue("@modifydate", modifyDate);
                    }
                    else
                    {
                        if (DateTime.TryParse(terminals.LatestUpdatedDate, out var latestDate))
                        {
                            cmd.Parameters.AddWithValue("@modifydate", latestDate); // Handle invalid or null dates
                        }
                    }
                    //cmd.Parameters.AddWithValue("@modifydate",terminals.MODIFY_DATE);
                    cmd.Parameters.AddWithValue("@adduser", terminals.ADD_USERID != "null" ? terminals.ADD_USERID : DBNull.Value);
                    // Parse datetime or set to NULL
                    if (DateTime.TryParse(terminals.ADD_DATE, out var addDate))
                    {
                        cmd.Parameters.AddWithValue("@adddate", addDate);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@adddate", DBNull.Value); // Handle invalid or null dates
                    }

                    cmd.Parameters.AddWithValue("@assetno", terminals.ASSET_NO != "null" ? terminals.ASSET_NO : DBNull.Value);
                    cmd.Parameters.AddWithValue("@cashbox", terminals.CASH_BOX_NUM != "null" ? terminals.CASH_BOX_NUM : DBNull.Value);
                    if (terminals.SERVICE_SMS_TYPE == "null")
                    {
                        cmd.Parameters.AddWithValue("@servicesms", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@servicesms", terminals.SERVICE_SMS_TYPE);
                    }

                    // Parse datetime or set to NULL
                    if (DateTime.TryParse(terminals.EJ_OPEN_DATE, out var ejOpenDate))
                    {
                        cmd.Parameters.AddWithValue("@ejopendate", ejOpenDate);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@ejopendate", DBNull.Value); // Handle invalid or null dates
                    }
                    // Parse datetime or set to NULL
                    if (DateTime.TryParse(terminals.ATMC_UPDATE_TIME, out var atmcUDate))
                    {
                        cmd.Parameters.AddWithValue("@atmcupdatetime", atmcUDate);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@atmcupdatetime", DBNull.Value); // Handle invalid or null dates
                    }
                    // Parse datetime or set to NULL
                    if (DateTime.TryParse(terminals.SP_UPDATE_TIME, out var spUDate))
                    {
                        cmd.Parameters.AddWithValue("@spupdatetime", spUDate);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@spupdatetime", DBNull.Value); // Handle invalid or null dates
                    }
                    // Parse datetime or set to NULL
                    if (DateTime.TryParse(terminals.AGENT_UPDATE_TIME, out var agentUDate))
                    {
                        cmd.Parameters.AddWithValue("@agentuptime", agentUDate);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@agentuptime", DBNull.Value); // Handle invalid or null dates
                    }
                    // Parse datetime or set to NULL
                    if (DateTime.TryParse(terminals.NV_UPDATE_TIME, out var nvUDate))
                    {
                        cmd.Parameters.AddWithValue("@nvupdatetime", nvUDate);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@nvupdatetime", DBNull.Value); // Handle invalid or null dates
                    }
                    // Parse datetime or set to NULL
                    if (DateTime.TryParse(terminals.MAIN_UPDATE_TIME, out var mainUDate))
                    {
                        cmd.Parameters.AddWithValue("@mainuptime", mainUDate);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@mainuptime", DBNull.Value); // Handle invalid or null dates
                    }



                    cmd.Parameters.AddWithValue("@vernv", terminals.VERSION_NV != "null" ? terminals.VERSION_NV : DBNull.Value);

                    cmd.Parameters.AddWithValue("@vermain", terminals.VERSION_MAIN != "null" ? terminals.VERSION_MAIN : DBNull.Value);

                    #endregion
                    await cmd.ExecuteNonQueryAsync();


                }
            }
            catch (Exception ex) { }


        }

        #endregion
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
