using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SLA_Management.Commons;
using SLA_Management.Data;
using SLA_Management.Data.ExcelUtilitie;
using SLA_Management.Models.OperationModel;
using SLA_Management.Models.TermProbModel;
using SLA_Management.Services;
using System;
using System.Data;

namespace SLA_Management.Controllers
{
    public class ReportController : Controller
    {
        private IConfiguration _myConfiguration;
        private BalancingService _balancingService;
        private static ConnectMySQL db_fv;
        private static ConnectMySQL db_all;
        List<WhitelistReportModel> whitelistreport_dataList = new List<WhitelistReportModel>();
        private List<BalancingReportModel> balancingreport_dataList = new List<BalancingReportModel>();
        private List<HardwareReportWebModel> hardwarereport_dataList = new List<HardwareReportWebModel>();
        public ReportController(IConfiguration myConfiguration, BalancingService balancingService)
        {

            _myConfiguration = myConfiguration;
            _balancingService = balancingService;
            db_fv = new ConnectMySQL(myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection"));
            db_all = new ConnectMySQL(myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection"));
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult WhitelistReport()
        {
            ViewBag.maxRows = 50;
            ViewBag.SelectedMonth = DateTime.Now.AddMonths(-1).ToString("yyyy-MM");
            return View();
        }
        public IActionResult WhiteListFetchData(string row, string page, string search, string month)
        {
            int _page;
            string filterquery = string.Empty;
            string fromdate = string.Empty;
            string todate = string.Empty;
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

            month = month ?? DateTime.Now.ToString("yyyy-MM");
            DateTime _fromdate = DateTime.Parse(month + "-01");
            DateTime _todate = _fromdate.AddMonths(1).AddDays(-1);
            fromdate = _fromdate.ToString("yyyy-MM-dd");
            todate = _todate.ToString("yyyy-MM-dd");
            if (month != "")
            {
                filterquery += " and WARN_DATE between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' ";
            }
            else
            {
                filterquery += " and WARN_DATE between '" + DateTime.Now.ToString("yyyy-MM") + " 00:00:00' and '" + DateTime.Now.AddMonths(1).AddDays(-1).ToString("yyyy-MM") + " 23:59:59' ";
            }
            List<WhitelistReportModel> jsonData = new List<WhitelistReportModel>();

            using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
            {
                connection.Open();

                // Modify the SQL query to use the 'input' parameter for filtering
                string query = @"SELECT CAST(WARN_DATE AS DATE) AS eventdate, 
                CASE 
                    WHEN warn_type = 93 THEN 'Network access deny'
                    WHEN warn_type = 96 THEN 'Insert the USB disk illegally'
                    WHEN warn_type = 97 THEN 'Illegal to modify the registry'
                    WHEN warn_type = 98 THEN 'File illegally modified'
                    WHEN warn_type = 99 THEN 'Illegal to boot'
                    WHEN warn_type = 102 THEN 'Operating system configuration changes'
                    ELSE 'Unknown'
                END AS warn_type, 
                CASE 
                    WHEN SEVERITY_FLAG = 0 THEN 'Critical'
                    WHEN SEVERITY_FLAG = 1 THEN 'High'
                    WHEN SEVERITY_FLAG = 3 THEN 'Low'
                    ELSE 'Unknown'
                END AS severity_level, 
                COUNT(warn_type) AS total 
                FROM client_warning_record 
                WHERE WARN_DATE IS NOT NULL ";

                query = query.Replace("\r\n", "");

                query += filterquery + " group by CAST(WARN_DATE AS DATE), warn_type, SEVERITY_FLAG order by CAST(WARN_DATE AS DATE)  ";

                MySqlCommand command = new MySqlCommand(query, connection);

                int id_row = 0;
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        id_row += 1;
                        DateTime eventDate = (DateTime)reader["eventdate"];
                        jsonData.Add(new WhitelistReportModel
                        {
                            ID = (id_row).ToString(),
                            eventdate = eventDate.ToString("yyyy-MM-dd"),
                            warn_type = reader["warn_type"].ToString(),
                            severity_level = reader["severity_level"].ToString(),
                            total = reader["total"].ToString()
                        });
                    }
                }
            }
            whitelistreport_dataList = jsonData;
            int pages = (int)Math.Ceiling((double)jsonData.Count() / _row);
            List<WhitelistReportModel> filteredData = RangeFilter(jsonData, _page, _row);


            List<WhitelistPivotModel> pivotData = new List<WhitelistPivotModel>();
            using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
            {
                connection.Open();

                // Modify the SQL query to use the 'input' parameter for filtering
                string query = @" SELECT CASE 
                    WHEN warn_type = 93 THEN 'Network access deny'
                    WHEN warn_type = 96 THEN 'Insert the USB disk illegally'
                    WHEN warn_type = 97 THEN 'Illegal to modify the registry'
                    WHEN warn_type = 98 THEN 'File illegally modified'
                    WHEN warn_type = 99 THEN 'Illegal to boot'
                    WHEN warn_type = 102 THEN 'Operating system configuration changes'
                    ELSE 'Unknown'
                    END AS warn_type,
	                SUM(CASE WHEN SEVERITY_FLAG = 0 THEN 1 ELSE 0 END) AS critical,
                    SUM(CASE WHEN SEVERITY_FLAG = 1 THEN 1 ELSE 0 END) AS high,
                    SUM(CASE WHEN SEVERITY_FLAG = 3 THEN 1 ELSE 0 END) AS low FROM client_warning_record   WHERE WARN_DATE IS NOT NULL ";


                query += filterquery + " group by WARN_TYPE";

                MySqlCommand command = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pivotData.Add(new WhitelistPivotModel
                        {
                            warn_type = reader["warn_type"].ToString(),
                            critical = reader["critical"].ToString(),
                            high = reader["high"].ToString(),
                            low = reader["low"].ToString()
                        });
                    }
                }
            }
            var response = new DataResponse_WhitelistReportModel
            {
                JsonData = filteredData,
                PivotData = pivotData,
                Page = pages,
                currentPage = _page,
                TotalTerminal = jsonData.Count(),
            };
            return Json(response);
        }

        static List<WhitelistReportModel> RangeFilter<WhitelistReportModel>(List<WhitelistReportModel> inputList, int page, int row)
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

        public class WhitelistReportModel
        {
            public string ID { get; set; }
            public string eventdate { get; set; }
            public string warn_type { get; set; }
            public string severity_level { get; set; }
            public string total { get; set; }
        }
        public class WhitelistPivotModel
        {
            public string warn_type { get; set; }
            public string critical { get; set; }
            public string high { get; set; }
            public string low { get; set; }
        }
        public class DataResponse_WhitelistReportModel
        {
            public List<WhitelistReportModel> JsonData { get; set; }
            public List<WhitelistPivotModel> PivotData { get; set; }
            public int Page { get; set; }
            public int currentPage { get; set; }
            public int TotalTerminal { get; set; }
        }

        #region Hardware
        public class HardwareReportQueryModel
        {
            public string ID { get; set; }
            public string TERM_ID { get; set; }
            public string EVENT_ID { get; set; }
            public string END_TIME { get; set; }
        }
        public class HardwareReportWebModel
        {
            public string problem_name { get; set; }
            public int problem_count { get; set; }
        }
        public class DataResponse_HardwareReport
        {
            public List<HardwareReportWebModel> JsonData { get; set; }
            public int Page { get; set; }
            public int currentPage { get; set; }
            public int TotalTerminal { get; set; }
        }
        public IActionResult HardwareReport()
        {
            ViewBag.CurrentTID = GetDeviceInfoALL();
            return View();
        }
        public IActionResult HardwareReportFetchData(string terminalno, string row, string page, string search, string todate, string fromdate)
        {
            int _page;
            int id_row = 0;
            string filterquery = string.Empty;
            string connectionstring = string.Empty;
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
            fromdate = fromdate ?? DateTime.Now.ToString("yyyy-MM-dd");
            todate = todate ?? DateTime.Now.ToString("yyyy-MM-dd");
            List<HardwareReportQueryModel> datas = new List<HardwareReportQueryModel>();
            if (terminalno.Length > 0)
            {
                char firstLetter = terminalno[0];

                switch (firstLetter)
                {
                    case 'A':
                        connectionstring = _myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection");
                        break;

                    case 'R':
                        connectionstring = _myConfiguration.GetValue<string>("ConnectString_MySQL_FV_CDM:FullNameConnection");
                        break;
                    case 'L':
                        connectionstring = _myConfiguration.GetValue<string>("ConnectString_MySQL_FV_LRM:FullNameConnection");
                        break;


                    default:
                        connectionstring = _myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection");
                        break;
                }
            }

            using (MySqlConnection connection = new MySqlConnection(connectionstring))
            {
                string transationdate_row = "";
                connection.Open();

                if (terminalno != "")
                {
                    filterquery += " and TERM_ID = '" + terminalno + "' ";
                }
                string query = @"SELECT TERM_ID,EVENT_ID,END_TIME FROM mt_caseflow_record_his ";

                query += " WHERE END_TIME BETWEEN '" + fromdate + " 00:00:00' AND '" + todate + " 23:59:59' " + filterquery;


                MySqlCommand command = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        datas.Add(new HardwareReportQueryModel
                        {

                            TERM_ID = reader["TERM_ID"].ToString(),
                            EVENT_ID = reader["EVENT_ID"].ToString(),
                            END_TIME = reader["END_TIME"].ToString(),

                        });
                    }
                }
            }
            List<HardwareReportWebModel> problemList = new List<HardwareReportWebModel>
            {
                new HardwareReportWebModel { problem_name = "Terminal Maintenance Mode", problem_count = datas.Count(x => x.EVENT_ID == "E1002") },
                new HardwareReportWebModel { problem_name = "Terminal OffLineMode", problem_count = datas.Count(x => x.EVENT_ID == "E1005") },
                new HardwareReportWebModel { problem_name = "Terminal StopService", problem_count = datas.Count(x => x.EVENT_ID == "E1006") },
                new HardwareReportWebModel { problem_name = "Cash Dispenser Error", problem_count = datas.Count(x => x.EVENT_ID == "E1010") },
                new HardwareReportWebModel { problem_name = "Cash Acceptor Error", problem_count = datas.Count(x => x.EVENT_ID == "E1012") },
                new HardwareReportWebModel { problem_name = "Cash Acceptor Note Jam", problem_count = datas.Count(x => x.EVENT_ID == "E1020") },
                new HardwareReportWebModel { problem_name = "Card Reader Error", problem_count = datas.Count(x => x.EVENT_ID == "E1036") },
                new HardwareReportWebModel { problem_name = "Thai ID Card Reader Error", problem_count = datas.Count(x => x.EVENT_ID == "E1038") },
                new HardwareReportWebModel { problem_name = "EPP Keypad Error", problem_count = datas.Count(x => x.EVENT_ID == "E1046") },
                new HardwareReportWebModel { problem_name = "Cassettes Error", problem_count = datas.Count(x => x.EVENT_ID == "E1127") },
                new HardwareReportWebModel { problem_name = "CardRetractBin Full", problem_count = datas.Count(x => x.EVENT_ID == "E1136") },
                new HardwareReportWebModel { problem_name = "Withdrawal Hardware Fault", problem_count = datas.Count(x => x.EVENT_ID == "E1149") },
                new HardwareReportWebModel { problem_name = "Withdrawal Cassette Issue", problem_count = datas.Count(x => x.EVENT_ID == "E1150") },
                new HardwareReportWebModel { problem_name = "Withdrawal No Cassette", problem_count = datas.Count(x => x.EVENT_ID == "E1151") },
                new HardwareReportWebModel { problem_name = "Withdrawal Retract Reject Full", problem_count = datas.Count(x => x.EVENT_ID == "E1152") },
                new HardwareReportWebModel { problem_name = "Withdrawal No CashReplenishment", problem_count = datas.Count(x => x.EVENT_ID == "E1153") },
                new HardwareReportWebModel { problem_name = "Communication Interrupt", problem_count = datas.Count(x => x.EVENT_ID == "E1156") },
                new HardwareReportWebModel { problem_name = "Vibration Alarm", problem_count = datas.Count(x => x.EVENT_ID == "E1179") },
                new HardwareReportWebModel { problem_name = "Box Alarm", problem_count = datas.Count(x => x.EVENT_ID == "E1182") },
                new HardwareReportWebModel { problem_name = "AntiJump Alarm", problem_count = datas.Count(x => x.EVENT_ID == "E1183") },
                new HardwareReportWebModel { problem_name = "Heat Alarm", problem_count = datas.Count(x => x.EVENT_ID == "E1185") },
                new HardwareReportWebModel { problem_name = "FaceCamera Error", problem_count = datas.Count(x => x.EVENT_ID == "E1217") },
                new HardwareReportWebModel { problem_name = "ShutterCamera Error", problem_count = datas.Count(x => x.EVENT_ID == "E1220") },
                new HardwareReportWebModel { problem_name = "CardReader Error Card Jammed", problem_count = datas.Count(x => x.EVENT_ID == "E1283") },
                new HardwareReportWebModel { problem_name = "Receipt Printer Error", problem_count = datas.Count(x => x.EVENT_ID == "E1375") },
                new HardwareReportWebModel { problem_name = "RPRXPaper Low", problem_count = datas.Count(x => x.EVENT_ID == "E2197") },
                new HardwareReportWebModel { problem_name = "RPRXPaper Out", problem_count = datas.Count(x => x.EVENT_ID == "E2198") },
                new HardwareReportWebModel { problem_name = "RPRXPaper NotSupport", problem_count = datas.Count(x => x.EVENT_ID == "E2199") },
                new HardwareReportWebModel { problem_name = "RPRXPaper Unknown", problem_count = datas.Count(x => x.EVENT_ID == "E2200") },
                new HardwareReportWebModel { problem_name = "RPRXPaper Jammed", problem_count = datas.Count(x => x.EVENT_ID == "E2201") },
                new HardwareReportWebModel { problem_name = "Cash Dispensing Shutter Jammed", problem_count = datas.Count(x => x.EVENT_ID == "E2204") },
                new HardwareReportWebModel { problem_name = "Cash Dispensing Shutter Unknown", problem_count = datas.Count(x => x.EVENT_ID == "E2205") },
                new HardwareReportWebModel { problem_name = "Cash Dispensing Shutter Not Support", problem_count = datas.Count(x => x.EVENT_ID == "E2206") },
                new HardwareReportWebModel { problem_name = "Cash Acceptor Shutter Jammed", problem_count = datas.Count(x => x.EVENT_ID == "E2209") },
                new HardwareReportWebModel { problem_name = "Cash Acceptor Shutter Unknown", problem_count = datas.Count(x => x.EVENT_ID == "E2210") },
                new HardwareReportWebModel { problem_name = "Cash Acceptor Shutter Not Support", problem_count = datas.Count(x => x.EVENT_ID == "E2211") },
                new HardwareReportWebModel { problem_name = "Other Error", problem_count = datas.Count(x => x.EVENT_ID == "E9999") }

            };
            int totalProblemCount = problemList.Sum(item => item.problem_count);
            hardwarereport_dataList = problemList;
            var response = new DataResponse_HardwareReport
            {
                JsonData = hardwarereport_dataList,
                Page = 1,
                currentPage = _page,
                TotalTerminal = totalProblemCount,
            };
            return Json(response);
        }
        #endregion


        #region Balance

        public IActionResult BalancingReport(string bankName, string Filter_TerminalID, string Filter_FromDate, string Filter_ToDate)
        {
            ViewBag.BankName = bankName;

            if (!string.IsNullOrEmpty(bankName))
            {
                ViewBag.CurrentTID = GetDeviceInfoFeelview(bankName.ToLower(), _myConfiguration);
            }
            else
            {
                ViewBag.CurrentTID = new List<Device_info_record>();
            }

            ViewBag.Filter_TerminalID = Filter_TerminalID;
            ViewBag.Filter_FromDate = Filter_FromDate;
            ViewBag.Filter_ToDate = Filter_ToDate;

            return View();
        }

        private static List<Device_info_record> GetDeviceInfoFeelview(string _bank, IConfiguration _myConfiguration)
        {
            ConnectMySQL db_mysql = new ConnectMySQL(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + _bank));
            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM device_info order by TERM_SEQ;";
            DataTable testss = db_mysql.GetDatatable(com);

            return ConvertDataTableToModel.ConvertDataTable<Device_info_record>(testss);
        }

        public class BalancingReportFilter
        {
            public string TerminalID { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string BankName { get; set; }
            public int Page { get; set; } = 1;
            public int Row { get; set; } = 20;
        }

        [HttpGet]
        public async Task<JsonResult> BalancingReportFetchData(string terminalID, string fromdate, string todate, string bankName, int row = 20, int page = 1)
        {
            if (string.IsNullOrEmpty(bankName))
            {
                return Json(new
                {
                    jsonData = new List<report_display>(),
                    totalTerminal = 0,
                    terminalDropdown = new List<Device_info_record>(),
                    filter = new { terminalID, fromdate, todate, bankName }
                });
            }

            if (string.IsNullOrEmpty(terminalID) || string.IsNullOrEmpty(fromdate) || string.IsNullOrEmpty(todate))
            {
                var terminalList = GetDeviceInfoFeelview(bankName.ToLower(), _myConfiguration);
                return Json(new
                {
                    jsonData = new List<report_display>(),
                    totalTerminal = 0,
                    terminalDropdown = terminalList,
                    filter = new { terminalID, fromdate, todate, bankName }
                });
            }

            DateTime.TryParse(fromdate, out DateTime fromDate);
            DateTime.TryParse(todate, out DateTime toDate);

            var fullData = await _balancingService.GetBalancingReportAsync(
                terminalno: terminalID,
                fromDate: fromDate,
                toDate: toDate,
                bankName: bankName);

            int total = fullData.Count;
            int totalPages = (int)Math.Ceiling((double)total / row);
            var pagedData = fullData.Skip((page - 1) * row).Take(row).ToList();
            var terminalDropdown = GetDeviceInfoFeelview(bankName.ToLower(), _myConfiguration);

            return Json(new
            {
                jsonData = pagedData,
                totalTerminal = total,
                totalPages,
                currentPage = page,
                terminalDropdown,
                filter = new { terminalID, fromdate, todate, bankName }
            });
        }

        [HttpPost]
        public async Task<IActionResult> UploadBalanceFiles(List<IFormFile> files, string _bank)
        {
            string userName = HttpContext.Session.GetString("Username");

            if (files == null || files.Count == 0)
                return BadRequest("❌ ไม่พบไฟล์ที่อัปโหลด");

            if (files.Any(f => !new[] { ".txt" }.Contains(Path.GetExtension(f.FileName).ToLower())))
                return BadRequest("❌ รองรับเฉพาะไฟล์ .txt เท่านั้น");

            var streamList = new List<Stream>();
            foreach (var file in files)
            {
                var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                streamList.Add(memoryStream);
            }

            var connStr = _myConfiguration.GetConnectionString($"ConnectString_NonOutsource:FullNameConnection_{_bank.ToLower()}");
            var result = ReadLocalBalanceService.InsertLocalBalance.Insert(streamList, connStr, userName);

            return Ok(new
            {
                success = true,
                message = result.InsertedCount > 0
                    ? $"✅ Upload สำเร็จ: เพิ่มข้อมูล {result.InsertedCount} รายการ จาก {result.FileCount} ไฟล์"
                    : "⚠️ Upload สำเร็จ แต่ไม่มีข้อมูลใหม่ถูกเพิ่ม"
            });
        }

        [HttpGet]
        public async Task<IActionResult> ExportBalancingReportToExcel(string terminalno, string fromdate, string todate, string bankName)
        {
            DateTime? parsedFromDate = null;
            DateTime? parsedToDate = null;

            if (DateTime.TryParse(fromdate, out var dtFrom)) parsedFromDate = dtFrom;
            if (DateTime.TryParse(todate, out var dtTo)) parsedToDate = dtTo;

            var data = await _balancingService.GetBalancingReportAsync(
                terminalno: terminalno,
                fromDate: parsedFromDate,
                toDate: parsedToDate,
                bankName: bankName
            );

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("BalancingReport");

            string displayDateRange = parsedFromDate.HasValue && parsedToDate.HasValue
                ? $"{parsedFromDate:yyyy-MM-dd} ถึง {parsedToDate:yyyy-MM-dd}"
                : "ทั้งหมด";

            ws.Cells["A1"].Value = $"📅 วันที่ข้อมูล: {displayDateRange}";
            ws.Cells["A1"].Style.Font.Bold = true;

            var headers = new[]
            {
        "No.", "Terminal ID", "Terminal Name", "Terminal Seq", "Transaction Date",
        "C1 Inc", "C2 Inc", "C3 Inc",
        "C1 Dep", "C2 Dep", "C3 Dep",
        "C1 Out", "C2 Out", "C3 Out",
        "C1 End", "C2 End", "C3 End"
    };

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[3, i + 1].Value = headers[i];
                ws.Cells[3, i + 1].Style.Font.Bold = true;
                ws.Cells[3, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[3, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                ws.Column(i + 1).AutoFit();
            }

            int row = 4;
            int no = 1;
            foreach (var item in data)
            {
                ws.Cells[row, 1].Value = no++;
                ws.Cells[row, 2].Value = item.term_id;
                ws.Cells[row, 3].Value = item.term_name;
                ws.Cells[row, 4].Value = item.term_seq;
                ws.Cells[row, 5].Value = item.transationdate;
                ws.Cells[row, 6].Value = item.c1_inc;
                ws.Cells[row, 7].Value = item.c2_inc;
                ws.Cells[row, 8].Value = item.c3_inc;
                ws.Cells[row, 9].Value = item.c1_dep;
                ws.Cells[row, 10].Value = item.c2_dep;
                ws.Cells[row, 11].Value = item.c3_dep;
                ws.Cells[row, 12].Value = item.c1_out;
                ws.Cells[row, 13].Value = item.c2_out;
                ws.Cells[row, 14].Value = item.c3_out;
                ws.Cells[row, 15].Value = item.c1_end;
                ws.Cells[row, 16].Value = item.c2_end;
                ws.Cells[row, 17].Value = item.c3_end;
                row++;
            }

            string safeDate = parsedFromDate.HasValue ? parsedFromDate.Value.ToString("yyyyMMdd") : "AllDates";
            string fileName = $"BalancingReport_{safeDate}.xlsx";

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public class BalancingReportModel
        {
            public string no { get; set; }
            public string term_id { get; set; }
            public string term_seq { get; set; }
            public string term_name { get; set; }
            public string transationdate { get; set; }
            public string c1_inc { get; set; }
            public string c1_dec { get; set; }
            public string c1_out { get; set; }
            public string c1_end { get; set; }
            public string c2_inc { get; set; }
            public string c2_dec { get; set; }
            public string c2_out { get; set; }
            public string c2_end { get; set; }
            public string c3_inc { get; set; }
            public string c3_dec { get; set; }
            public string c3_out { get; set; }
            public string c3_end { get; set; }
            public string c1_dep { get; set; }
            public string c2_dep { get; set; }
            public string c3_dep { get; set; }
        }

        public class DataResponse_BalancingReport
        {
            public List<BalancingReportModel> JsonData { get; set; }
            public int Page { get; set; }
            public int currentPage { get; set; }
            public int TotalTerminal { get; set; }
        }


        #endregion

        #region anyfunction
        public class Device_info
        {
            public string TERM_ID { get; set; }
            public string TERM_SEQ { get; set; }
            public string TERM_NAME { get; set; }
        }
        private static List<Device_info> GetDeviceInfoALL()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM fv_device_info order by TERM_SEQ;";
            DataTable testss = db_all.GetDatatable(com);

            List<Device_info> test = ConvertDataTableToModel.ConvertDataTable<Device_info>(testss);

            return test;
        }
        #endregion
        #region Excel WhitelistReport

        [HttpGet]
        public ActionResult WhitelistReport_ExportExc(string month)
        {
            string fname = "";
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;
            month = month ?? DateTime.Now.ToString("yyyy-MM");
            string filterquery = string.Empty;
            string fromdate = string.Empty;
            string todate = string.Empty;
            try
            {
                month = month ?? DateTime.Now.ToString("yyyy-MM");
                DateTime _fromdate = DateTime.Parse(month + "-01");
                DateTime _todate = _fromdate.AddMonths(1).AddDays(-1);
                fromdate = _fromdate.ToString("yyyy-MM-dd");
                todate = _todate.ToString("yyyy-MM-dd");
                if (month != "")
                {
                    filterquery += " and WARN_DATE between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' ";
                }
                else
                {
                    filterquery += " and WARN_DATE between '" + DateTime.Now.ToString("yyyy-MM") + " 00:00:00' and '" + DateTime.Now.AddMonths(1).AddDays(-1).ToString("yyyy-MM") + " 23:59:59' ";
                }
                List<WhitelistReportModel> jsonData = new List<WhitelistReportModel>();

                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    connection.Open();

                    // Modify the SQL query to use the 'input' parameter for filtering
                    string query = @"SELECT CAST(WARN_DATE AS DATE) AS eventdate, 
                CASE 
                    WHEN warn_type = 93 THEN 'Network access deny'
                    WHEN warn_type = 96 THEN 'Insert the USB disk illegally'
                    WHEN warn_type = 97 THEN 'Illegal to modify the registry'
                    WHEN warn_type = 98 THEN 'File illegally modified'
                    WHEN warn_type = 99 THEN 'Illegal to boot'
                    WHEN warn_type = 102 THEN 'Operating system configuration changes'
                    ELSE 'Unknown'
                END AS warn_type, 
                CASE 
                    WHEN SEVERITY_FLAG = 0 THEN 'Critical'
                    WHEN SEVERITY_FLAG = 1 THEN 'High'
                    WHEN SEVERITY_FLAG = 3 THEN 'Low'
                    ELSE 'Unknown'
                END AS severity_level, 
                COUNT(warn_type) AS total 
                FROM client_warning_record 
                WHERE WARN_DATE IS NOT NULL ";

                    query = query.Replace("\r\n", "");

                    query += filterquery + " group by CAST(WARN_DATE AS DATE), warn_type, SEVERITY_FLAG order by CAST(WARN_DATE AS DATE)  ";

                    MySqlCommand command = new MySqlCommand(query, connection);

                    int id_row = 0;
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            id_row += 1;
                            DateTime eventDate = (DateTime)reader["eventdate"];
                            jsonData.Add(new WhitelistReportModel
                            {
                                ID = (id_row).ToString(),
                                eventdate = eventDate.ToString("yyyy-MM-dd"),
                                warn_type = reader["warn_type"].ToString(),
                                severity_level = reader["severity_level"].ToString(),
                                total = reader["total"].ToString()
                            });
                        }
                    }
                }
                whitelistreport_dataList = jsonData;

                List<WhitelistPivotModel> pivotData = new List<WhitelistPivotModel>();
                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    connection.Open();

                    // Modify the SQL query to use the 'input' parameter for filtering
                    string query = @" SELECT CASE 
                    WHEN warn_type = 93 THEN 'Network access deny'
                    WHEN warn_type = 96 THEN 'Insert the USB disk illegally'
                    WHEN warn_type = 97 THEN 'Illegal to modify the registry'
                    WHEN warn_type = 98 THEN 'File illegally modified'
                    WHEN warn_type = 99 THEN 'Illegal to boot'
                    WHEN warn_type = 102 THEN 'Operating system configuration changes'
                    ELSE 'Unknown'
                    END AS warn_type,
	                SUM(CASE WHEN SEVERITY_FLAG = 0 THEN 1 ELSE 0 END) AS critical,
                    SUM(CASE WHEN SEVERITY_FLAG = 1 THEN 1 ELSE 0 END) AS high,
                    SUM(CASE WHEN SEVERITY_FLAG = 3 THEN 1 ELSE 0 END) AS low FROM client_warning_record   WHERE WARN_DATE IS NOT NULL ";


                    query += filterquery + " group by WARN_TYPE";

                    MySqlCommand command = new MySqlCommand(query, connection);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pivotData.Add(new WhitelistPivotModel
                            {
                                warn_type = reader["warn_type"].ToString(),
                                critical = reader["critical"].ToString(),
                                high = reader["high"].ToString(),
                                low = reader["low"].ToString()
                            });
                        }
                    }
                }
                if (whitelistreport_dataList == null || whitelistreport_dataList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;
                ExcelUtilities_WhitelistReport obj = new ExcelUtilities_WhitelistReport();


                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(whitelistreport_dataList, pivotData);



                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;



                fname = "WhitelistReport_" + DateTime.Now.ToString("yyyyMMdd");

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
        public ActionResult DownloadExportFile_WhitelistReport(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            string tsDate = "";
            string teDate = "";
            try
            {




                fname = "WhitelistReport_" + DateTime.Now.ToString("yyyyMMdd");

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
