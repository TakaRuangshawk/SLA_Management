using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SLA_Management.Commons;
using SLA_Management.Data;
using SLA_Management.Data.ExcelUtilitie;
using SLA_Management.Models.ManagementModel;
using SLA_Management.Models.OperationModel;
using SLA_Management.Models.ReportModel;
using SLA_Management.Models.TermProbModel;
using SLA_Management.Services;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using static Services.ReportCassetteBoxService;


namespace SLA_Management.Controllers
{
    public class ManagementController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly CassetteService _cassetteService;
        private readonly ReportCasesCheckService _reportCasesCheckService;

        private static regulator_seek param = new regulator_seek();
        private static List<TicketManagement> ticket_dataList = new List<TicketManagement>();
        private static List<TicketManagement> recordset_ticketManagement = new List<TicketManagement>();
        public ManagementController(IConfiguration configuration, CassetteService cassetteService, ReportCasesCheckService reportCaesCheckService)
        {
            _configuration = configuration;
            _cassetteService = cassetteService;
            _reportCasesCheckService = reportCaesCheckService;
        }

        private static List<Device_info_record> GetDeviceInfoFeelview(string _bank, IConfiguration _myConfiguration)
        {

            ConnectMySQL db_mysql = new ConnectMySQL(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + _bank));
            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM device_info order by TERM_SEQ;";
            DataTable testss = db_mysql.GetDatatable(com);

            List<Device_info_record> test = ConvertDataTableToModel.ConvertDataTable<Device_info_record>(testss);

            return test;
        }
        public class IssueName
        {
            public string Issue_Name { get; set; }
        }
        private static List<IssueName> GetIssue_Name(string _bank, IConfiguration _myConfiguration)
        {
            ConnectMySQL db_mysql = new ConnectMySQL(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + _bank));
            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT Issue_Name FROM reportcases  group by Issue_Name order by Issue_Name;";
            DataTable testss = db_mysql.GetDatatable(com);

            List<IssueName> test = ConvertDataTableToModel.ConvertDataTable<IssueName>(testss);

            return test;
        }
        public class StatusName
        {
            public string Status_Name { get; set; }
        }
        private static List<StatusName> GetStatus_Name(string _bank, IConfiguration _myConfiguration)
        {
            ConnectMySQL db_mysql = new ConnectMySQL(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + _bank));
            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT Status_Name FROM reportcases  group by Status_Name order by Status_Name;";
            DataTable testss = db_mysql.GetDatatable(com);

            List<StatusName> test = ConvertDataTableToModel.ConvertDataTable<StatusName>(testss);

            return test;
        }

        #region Holiday
        public IActionResult Holiday()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetEvents()
        {
            List<object> eventsList = new List<object>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT Holiday as Title, CONVERT(DATE, Date) as StartDate FROM t_msd_Holiday";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var title = reader["Title"].ToString();
                            var start = DateTime.Parse(reader["StartDate"].ToString()).ToString("yyyy-MM-dd");

                            eventsList.Add(new { title, start });
                        }
                    }
                }
            }


            return Json(eventsList);
        }
        [HttpPost]
        public IActionResult CreateEvent(string title, DateTime start, string user)
        {
            TimeZoneInfo thaiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            start = TimeZoneInfo.ConvertTimeFromUtc(start, thaiTimeZone);
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var sqlCommand = new SqlCommand("INSERT INTO t_msd_Holiday (Date, Holiday,CreatedBy,CreatedDate,UpdatedDate) VALUES (@start, @title,@CreatedBy,@CreatedDate,@UpdatedDate)", connection);

                    sqlCommand.Parameters.AddWithValue("@start", start.Date);
                    sqlCommand.Parameters.AddWithValue("@title", title);
                    sqlCommand.Parameters.AddWithValue("@CreatedBy", user);
                    sqlCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    sqlCommand.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);
                    sqlCommand.ExecuteNonQuery();
                }

                return Ok("Event created successfully.");
            }
            catch (Exception ex)
            {
                // Handle any errors or exceptions here
                return BadRequest("Failed to create event: " + ex.Message);
            }
        }
        [HttpPost]
        public IActionResult DeleteEvent(string title, DateTime start)
        {
            TimeZoneInfo thaiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            start = TimeZoneInfo.ConvertTimeFromUtc(start, thaiTimeZone);
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            //start = start.AddDays(1).Date;
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Define the DELETE SQL command with a WHERE clause
                    var sqlCommand = new SqlCommand("DELETE FROM t_msd_Holiday WHERE Holiday = @Holiday AND Date = @start", connection);

                    // Add parameters for the WHERE clause
                    sqlCommand.Parameters.AddWithValue("@Holiday", title);
                    sqlCommand.Parameters.AddWithValue("@start", start);

                    // Execute the DELETE command
                    int rowsAffected = sqlCommand.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok("User deleted successfully.");
                    }
                    else
                    {
                        return NotFound("User not found or deletion failed.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors or exceptions here
                return BadRequest("Failed to create event: " + ex.Message);
            }
        }
        [HttpPost]
        public IActionResult CheckUser(string username, string email)
        {
            //string _connectionString = _configuration.GetConnectionString("ConnectString_FVMySQL");
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_configuration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    connection.Open();

                    using (var cmd = new MySqlCommand("SELECT CASE WHEN COUNT(*) > 0 THEN 'yes' ELSE 'no' END AS _check FROM fv_system_users WHERE ACCOUNT = @UserId AND EMAIL = @Email", connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", username);
                        cmd.Parameters.AddWithValue("@Email", email);

                        var result = cmd.ExecuteScalar();

                        // Convert the result to a string
                        var checkResult = result != null ? result.ToString() : "no";

                        return Ok(checkResult);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to create event: " + ex.Message);
            }



        }

        #endregion

        #region Report Cases Management ( Check )

        public IActionResult ReportCasesCheck(
   string bankName,
   string Filter_FromDate,
   string Filter_ToDate
   )
        {

            ViewBag.bankName = bankName;

            ViewBag.Filter_FromDate = Filter_FromDate;
            ViewBag.Filter_ToDate = Filter_ToDate;
            

            return View();
        }

       

        [HttpGet]
        public async Task<JsonResult> FetchReportCasesCheck(DateTime? fromdate, DateTime? todate, string bankName, int row = 50, int page = 1, string status = "")

        {
            if (string.IsNullOrEmpty(bankName))
            {
                return Json(new
                {
                    jsonData = new List<ReportCaseStatusModel>(),
                    currentPage = page,
                    totalPages = 0,
                    totalCases = 0,
                    terminalDropdown = new List<Device_info_record>()
                });
            }

            // 📦 preload dropdown (ถ้ามีในอนาคต)
            var terminalList = GetDeviceInfoFeelview(bankName.ToLower(), _configuration);

            // ✅ ดึงข้อมูลทั้งหมดจาก Service
            var allCases = await _reportCasesCheckService.GetReportCasesAsync(fromdate, todate, bankName);

            // ✅ กรองตาม Status ถ้ามีการระบุ
            if (!string.IsNullOrEmpty(status))
            {
                allCases = allCases
                    .Where(x => string.Equals(x.Status, status, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            int totalCases = allCases.Count;
            int totalPages = (int)Math.Ceiling((double)totalCases / row);

            // ✅ Paging
            var pagedData = allCases
                .OrderByDescending(r => r.DateInform)
                .Skip((page - 1) * row)
                .Take(row)
                .ToList();

            // ✅ ถ้าไม่พบเลย  
            if (pagedData.Count == 0)
            {
                pagedData.Add(new ReportCaseStatusModel
                {
                    DateInform = null,
                    Status = "NotFound"
                });
            }

            return Json(new
            {
                jsonData = pagedData,
                currentPage = page,
                totalPages,
                totalCases,
                terminalDropdown = terminalList
            });
        }

        [HttpGet]
        public async Task<IActionResult> ExportReportCasesCheckToExcel(DateTime? fromdate, DateTime? todate, string bankName, string status)
        {
            var data = await _reportCasesCheckService.GetReportCasesAsync(fromdate, todate, bankName);

            if (!string.IsNullOrEmpty(status))
            {
                data = data.Where(d => d.Status == status).ToList();
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("ReportCases");

            string displayDate = fromdate.HasValue && todate.HasValue
                ? $"{fromdate.Value:dd-MM-yyyy} ถึง {todate.Value:dd-MM-yyyy}"
                : "ทั้งหมด";

            ws.Cells["A1"].Value = $"📅 ช่วงวันที่ข้อมูล: {displayDate}";
            ws.Cells["A1"].Style.Font.Bold = true;

            var headers = new[] { "No.", "Date", "Status" };
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
                ws.Cells[row, 2].Value = item.DateInform?.ToString("dd-MM-yyyy");
                ws.Cells[row, 3].Value = item.Status;
                row++;
            }

            string safeFrom = fromdate?.ToString("yyyyMMdd") ?? "Start";
            string safeTo = todate?.ToString("yyyyMMdd") ?? "End";
            string fileName = $"ReportCasesCheck_{safeFrom}_to_{safeTo}.xlsx";

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }


        #endregion

        #region Cassette Status

        [HttpPost]
        public async Task<IActionResult> UploadCassetteEventMultiple(List<IFormFile> files, string _bank)
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

            var connStr = _cassetteService.GetConnectionStringByBank(_bank);
            var result = InsertReport.Insert(streamList, connStr, userName);

            return Ok(new
            {
                success = true,
                message = result.Inserted
                    ? $"✅ Upload สำเร็จ: เพิ่มข้อมูลจำนวน {result.CassetteCount + result.TerminalCassetteCount} รายการ"
                    : "⚠️ Upload สำเร็จ แต่ไม่มีการเพิ่มข้อมูล"
            });
        }

        [HttpGet]
        public async Task<JsonResult> FetchCassetteStatus(string terminalID, string lotType, DateTime? fromdate, string bankName, int row = 50, int page = 1)
        {
            if (string.IsNullOrEmpty(bankName))
            {
                return Json(new
                {
                    jsonData = new List<report_display>(),
                    currentPage = page,
                    totalPages = 0,
                    totalCases = 0,
                    latestUpdateDate = "",
                    updatedBy = "",
                    terminalDropdown = new List<Device_info_record>(),
                    lotTypeDropdown = new List<string>()
                });
            }


            var terminalList = GetDeviceInfoFeelview(bankName.ToLower(), _configuration);
            var lotTypeList = _cassetteService.GetLotTypes(bankName.ToLower());

            var reportCassette = await _cassetteService.GetCassetteStatusAsync(terminalID, lotType, fromdate, bankName);

            int sum1000A = 0, sum1000B = 0, sum500 = 0, sum100 = 0;
            foreach (var item in reportCassette)
            {
                sum1000A += CassetteService.ExtractNumber(item.cassette1000A);
                sum1000B += CassetteService.ExtractNumber(item.cassette1000B);
                sum500 += CassetteService.ExtractNumber(item.cassette500);
                sum100 += CassetteService.ExtractNumber(item.cassette100);
            }


            int totalCases = reportCassette.Count;
            int totalPages = (int)Math.Ceiling((double)totalCases / row);
            var pagedData = reportCassette.Skip((page - 1) * row).Take(row).ToList();

            var (latestDate, updatedBy) = _cassetteService.GetLatestUpdate(bankName);

            return Json(new
            {
                jsonData = pagedData,
                currentPage = page,
                totalPages,
                totalCases,
                latestUpdateDate = latestDate,
                updatedBy = updatedBy,
                terminalDropdown = terminalList,
                lotTypeDropdown = lotTypeList,
                sum1000A,
                sum1000B,
                sum500,
                sum100
            });
        }

        [HttpGet]
        public async Task<IActionResult> ExportCassetteStatusToExcel(string terminalID, string lotType, string fromdate, string bankName)
        {
            DateTime? parsedDate = null;
            if (DateTime.TryParse(fromdate, out var dt))
                parsedDate = dt;

            var data = await _cassetteService.GetCassetteStatusAsync(terminalID, lotType, parsedDate, bankName);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("CassetteStatus");

            string displayDate = parsedDate.HasValue ? parsedDate.Value.ToString("yyyy-MM-dd") : "ทั้งหมด";
            ws.Cells["A1"].Value = $"📅 วันที่ข้อมูล: {displayDate}";
            ws.Cells["A1"].Style.Font.Bold = true;

            var headers = new[] { "No.", "TerminalNo", "Lot Type", "1000A", "1000B", "500", "100" };
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
                ws.Cells[row, 2].Value = item.terminalNo;
                ws.Cells[row, 3].Value = item.lotType;
                ws.Cells[row, 4].Value = item.cassette1000A;
                ws.Cells[row, 5].Value = item.cassette1000B;
                ws.Cells[row, 6].Value = item.cassette500;
                ws.Cells[row, 7].Value = item.cassette100;
                row++;
            }

            string safeDate = parsedDate.HasValue ? parsedDate.Value.ToString("yyyyMMdd") : "AllDates";
            string fileName = $"CassetteStatus_{safeDate}.xlsx";

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public IActionResult CassetteStatus(
    string bankName,
    string Filter_TerminalID,
    string Filter_LotType,
    string Filter_DataDate)
        {

            if (!string.IsNullOrEmpty(bankName))
            {
                ViewBag.CurrentTID = GetDeviceInfoFeelview(bankName.ToLower(), _configuration);

                ViewBag.LotTypes = _cassetteService.GetLotTypes(bankName.ToLower());
            }
            else
            {
                ViewBag.CurrentTID = new List<Device_info_record>();

                ViewBag.LotTypes = new List<string>();
            }


            ViewBag.bankName = bankName;

            ViewBag.Filter_TerminalID_Current = Filter_TerminalID;
            ViewBag.Filter_LotType_Current = Filter_LotType;
            ViewBag.Filter_DataDate_Current = Filter_DataDate;

            return View();
        }

        #endregion

        #region ReportCases
        public IActionResult ReportCases(string bankName)
        {

            if (bankName != null)
            {
                ViewBag.CurrentTID = GetDeviceInfoFeelview(bankName.ToLower(), _configuration);
                ViewBag.Issue_Name = GetIssue_Name(bankName.ToLower(), _configuration);
                ViewBag.Status_Name = GetStatus_Name(bankName.ToLower(), _configuration);
            }
            else
            {
                ViewBag.CurrentTID = new List<Device_info_record>();
                ViewBag.Issue_Name = new List<IssueName>();
                ViewBag.Status_Name = new List<StatusName>();
            }

            ViewBag.bankName = bankName;

            SetLatestUpdateViewBag();
            // For now, just return the empty view
            return View();
        }

        [HttpGet]
        public JsonResult FetchReportCases(string terminalID, string placeInstall, string issueName, DateTime? fromdate, DateTime? todate, string statusName, string bankName, int row = 50, int page = 1)
        {
            List<ReportCase> reportCases = new List<ReportCase>();
            int totalCases = 0;
            int totalPages = 0;


            string connectionDB = "";

            switch (bankName)
            {
                case "BAAC":
                    connectionDB = "ConnectString_NonOutsource:FullNameConnection_baac";
                    break;
                case "ICBC":
                    connectionDB = "ConnectString_NonOutsource:FullNameConnection_icbc";

                    break;
                case "BOC":
                    connectionDB = "ConnectString_NonOutsource:FullNameConnection_boct";
                    break;
                default:
                    connectionDB = "";
                    break;
            }


            if (connectionDB == "")
            {
                return Json(new
                {
                    jsonData = reportCases,
                    currentPage = page,
                    totalPages = totalPages,
                    totalCases = totalCases,
                    latestUpdateDate = ViewBag.LatestUpdateDate,
                    updatedBy = ViewBag.UpdatedBy
                });
            }

            bool hasWhere = false;

            using (MySqlConnection conn = new MySqlConnection(_configuration.GetValue<string>(connectionDB)))
            {
                conn.Open();

                string query = @"
    SELECT 
        r.Case_Error_No, r.Terminal_ID, r.Place_Install, r.Issue_Name, r.Date_Inform, 
        r.Status_Name, r.Branch_name_pb, r.Repair1, r.Repair2, r.Repair3, r.Repair4, r.Repair5, 
        r.Incident_No, r.Date_Close_Pb, r.Type_Project, r.Update_Date, r.Update_By, r.Remark,
        j.Problem_Detail, j.Solving_Program , j.Status as AService_Status
    FROM baac_logview.reportcases r
    LEFT JOIN baac_logview.t_tsd_jobdetail j 
    ON j.Job_No = r.Repair2 
    OR j.Job_No = r.Repair3 
    OR j.Job_No = r.Repair4 
    OR j.Job_No = r.Incident_No";


                if (!string.IsNullOrEmpty(terminalID))
                {
                    query += hasWhere ? " AND" : " WHERE";
                    query += " Terminal_ID = @TerminalID";
                    hasWhere = true;
                }
                if (!string.IsNullOrEmpty(placeInstall))
                {
                    query += hasWhere ? " AND" : " WHERE";
                    query += " Place_Install LIKE @PlaceInstall";
                    hasWhere = true;
                }
                if (!string.IsNullOrEmpty(issueName))
                {
                    query += hasWhere ? " AND" : " WHERE";
                    query += " Issue_Name LIKE @IssueName";
                    hasWhere = true;
                }
                if (fromdate.HasValue)
                {
                    query += hasWhere ? " AND" : " WHERE";
                    query += " DATE(Date_Inform) >= @FromDate";
                    hasWhere = true;
                }
                if (todate.HasValue)
                {
                    query += hasWhere ? " AND" : " WHERE";
                    query += " DATE(Date_Inform) <= @ToDate";
                    hasWhere = true;
                }
                if (!string.IsNullOrEmpty(statusName))
                {
                    query += hasWhere ? " AND" : " WHERE";
                    query += " Status_Name = @StatusName";
                    hasWhere = true;
                }

                //query += " ORDER BY Date_Inform ASC ";
                query += " ORDER BY Date_Inform ASC, Case_Error_No ASC ";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TerminalID", terminalID);
                cmd.Parameters.AddWithValue("@PlaceInstall", "%" + placeInstall + "%");
                cmd.Parameters.AddWithValue("@IssueName", "%" + issueName + "%");
                cmd.Parameters.AddWithValue("@FromDate", fromdate);
                cmd.Parameters.AddWithValue("@ToDate", todate);
                cmd.Parameters.AddWithValue("@StatusName", statusName);
                cmd.Parameters.AddWithValue("@Offset", (page - 1) * row);


                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reportCases.Add(new ReportCase
                        {
                            CaseErrorNo = reader.GetInt64("Case_Error_No"),
                            TerminalID = reader["Terminal_ID"] != DBNull.Value ? reader.GetString("Terminal_ID") : string.Empty,
                            PlaceInstall = reader["Place_Install"] != DBNull.Value ? reader.GetString("Place_Install") : string.Empty,
                            IssueName = reader["Issue_Name"] != DBNull.Value ? reader.GetString("Issue_Name") : string.Empty,
                            DateInform = reader["Date_Inform"] != DBNull.Value ? reader.GetDateTime("Date_Inform").ToString("dd/MM/yyyy") : string.Empty,
                            StatusName = reader["Status_Name"] != DBNull.Value ? reader.GetString("Status_Name") : string.Empty,
                            BranchName = reader["Branch_name_pb"] != DBNull.Value ? reader.GetString("Branch_name_pb") : string.Empty,
                            Repair1 = reader["Repair1"] != DBNull.Value ? reader.GetString("Repair1") : string.Empty,
                            Repair2 = reader["Repair2"] != DBNull.Value ? reader.GetString("Repair2") : string.Empty,
                            Repair3 = reader["Repair3"] != DBNull.Value ? reader.GetString("Repair3") : string.Empty,
                            Repair4 = reader["Repair4"] != DBNull.Value ? reader.GetString("Repair4") : string.Empty,
                            Repair5 = reader["Repair5"] != DBNull.Value ? reader.GetString("Repair5") : string.Empty,
                            IncidentNo = reader["Incident_No"] != DBNull.Value ? reader.GetString("Incident_No") : string.Empty,
                            DateClosePb = reader["Date_Close_Pb"] != DBNull.Value ? reader.GetDateTime("Date_Close_Pb").ToString("dd/MM/yyyy") : string.Empty,
                            TypeProject = reader["Type_Project"] != DBNull.Value ? reader.GetString("Type_Project") : string.Empty,
                            UpdateDate = reader["Update_Date"] != DBNull.Value ? reader.GetDateTime("Update_Date").ToString("dd/MM/yyyy HH:mm") : string.Empty,
                            UpdateBy = reader["Update_By"] != DBNull.Value ? reader.GetString("Update_By") : string.Empty,
                            Remark = reader["Remark"] != DBNull.Value ? reader.GetString("Remark") : string.Empty,
                            ProblemDetail = reader["Problem_Detail"] != DBNull.Value ? reader.GetString("Problem_Detail") : string.Empty,
                            SolvingProgram = reader["Solving_Program"] != DBNull.Value ? reader.GetString("Solving_Program") : string.Empty,
                            AServiceStatus = reader["AService_Status"] != DBNull.Value ? reader.GetString("AService_Status") : string.Empty,
                        });

                    }
                    if (reader.NextResult() && reader.Read())
                    {
                        totalCases = reader.GetInt32(0);
                    }
                }
            }

            totalPages = (int)Math.Ceiling((double)totalCases / row);

            SetLatestUpdateViewBag();

            return Json(new
            {
                jsonData = reportCases,
                currentPage = page,
                totalPages = totalPages,
                totalCases = totalCases,
                latestUpdateDate = ViewBag.LatestUpdateDate,
                updatedBy = ViewBag.UpdatedBy
            });
        }
        public IActionResult ExportReportCasesToExcel(string terminalID, string placeInstall, string issueName, DateTime? fromdate, DateTime? todate, string statusName)
        {
            using (var connection = new MySqlConnection(_configuration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_BAAC")))
            {
                connection.Open();

                string query = @"
SELECT 
    r.Case_Error_No, r.Terminal_ID, r.Place_Install, r.Issue_Name, r.Date_Inform, 
    r.Status_Name, r.Branch_name_pb, r.Repair1, r.Repair2, r.Repair3, r.Repair4, r.Repair5, 
    r.Incident_No, r.Date_Close_Pb, r.Type_Project, r.Update_Date, r.Update_By, r.Remark,
    j.Problem_Detail, j.Solving_Program , j.Status as AService_Status 
FROM reportcases r
LEFT JOIN baac_logview.t_tsd_jobdetail j 
    ON j.Job_No = r.Repair2 
    OR j.Job_No = r.Repair3 
    OR j.Job_No = r.Repair4 
    OR j.Job_No = r.Incident_No
";

                bool hasWhere = false;

                if (!string.IsNullOrEmpty(terminalID))
                {
                    query += hasWhere ? " AND" : " WHERE";
                    query += " r.Terminal_ID = @TerminalID";
                    hasWhere = true;
                }
                if (!string.IsNullOrEmpty(placeInstall))
                {
                    query += hasWhere ? " OR" : " WHERE";
                    query += " r.Place_Install LIKE @PlaceInstall";
                    hasWhere = true;
                }
                if (!string.IsNullOrEmpty(issueName))
                {
                    query += hasWhere ? " AND" : " WHERE";
                    query += " r.Issue_Name LIKE @IssueName";
                    hasWhere = true;
                }
                if (fromdate.HasValue)
                {
                    query += hasWhere ? " AND" : " WHERE";
                    query += " r.Date_Inform >= @FromDate";
                    hasWhere = true;
                }
                if (todate.HasValue)
                {
                    query += hasWhere ? " AND" : " WHERE";
                    query += " r.Date_Inform <= @ToDate";
                    hasWhere = true;
                }
                if (!string.IsNullOrEmpty(statusName))
                {
                    query += hasWhere ? " AND" : " WHERE";
                    query += " r.Status_Name = @StatusName";
                    hasWhere = true;
                }

                query += " ORDER BY r.Date_Inform ASC, r.Case_Error_No ASC";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TerminalID", terminalID);
                    command.Parameters.AddWithValue("@PlaceInstall", "%" + placeInstall + "%");
                    command.Parameters.AddWithValue("@IssueName", "%" + issueName + "%");
                    command.Parameters.AddWithValue("@FromDate", fromdate?.Date);
                    command.Parameters.AddWithValue("@ToDate", todate?.Date);
                    command.Parameters.AddWithValue("@StatusName", statusName);

                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (var reader = command.ExecuteReader())
                    {
                        using (var package = new ExcelPackage())
                        {
                            var worksheet = package.Workbook.Worksheets.Add("ReportCases");

                            // Header
                            string[] headers = new[]
                            {
                        "Case No", "Terminal ID", "Place Install", "Issue Name", "Date Inform", "BAAC Status", "Branch Name",
                        "Repair 1", "Repair 2", "Repair 3", "Repair 4", "Repair 5", "Incident No", "Date Close Pb",
                        "Type Project", "Update Date", "Update By", "Remark", "Problem Detail", "Solving Program", "Aservice Status"
                    };

                            for (int i = 0; i < headers.Length; i++)
                            {
                                worksheet.Cells[1, i + 1].Value = headers[i];
                            }

                            using (var range = worksheet.Cells[1, 1, 1, headers.Length])
                            {
                                range.Style.Font.Bold = true;
                                range.Style.Font.Size = 14;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            }

                            int row = 2;
                            while (reader.Read())
                            {
                                worksheet.Cells[row, 1].Value = reader["Case_Error_No"] != DBNull.Value ? reader["Case_Error_No"] : null;
                                worksheet.Cells[row, 2].Value = reader["Terminal_ID"] != DBNull.Value ? reader["Terminal_ID"] : null;
                                worksheet.Cells[row, 3].Value = reader["Place_Install"] != DBNull.Value ? reader["Place_Install"] : null;
                                worksheet.Cells[row, 4].Value = reader["Issue_Name"] != DBNull.Value ? reader["Issue_Name"] : null;
                                worksheet.Cells[row, 5].Value = reader["Date_Inform"] != DBNull.Value ? Convert.ToDateTime(reader["Date_Inform"]).ToString("dd/MM/yyyy") : null;
                                worksheet.Cells[row, 6].Value = reader["Status_Name"] != DBNull.Value ? reader["Status_Name"] : null;
                                worksheet.Cells[row, 7].Value = reader["Branch_name_pb"] != DBNull.Value ? reader["Branch_name_pb"] : null;
                                worksheet.Cells[row, 8].Value = reader["Repair1"] != DBNull.Value ? reader["Repair1"] : null;
                                worksheet.Cells[row, 9].Value = reader["Repair2"] != DBNull.Value ? reader["Repair2"] : null;
                                worksheet.Cells[row, 10].Value = reader["Repair3"] != DBNull.Value ? reader["Repair3"] : null;
                                worksheet.Cells[row, 11].Value = reader["Repair4"] != DBNull.Value ? reader["Repair4"] : null;
                                worksheet.Cells[row, 12].Value = reader["Repair5"] != DBNull.Value ? reader["Repair5"] : null;
                                worksheet.Cells[row, 13].Value = reader["Incident_No"] != DBNull.Value ? reader["Incident_No"] : null;
                                worksheet.Cells[row, 14].Value = reader["Date_Close_Pb"] != DBNull.Value ? Convert.ToDateTime(reader["Date_Close_Pb"]).ToString("dd/MM/yyyy") : null;
                                worksheet.Cells[row, 15].Value = reader["Type_Project"] != DBNull.Value ? reader["Type_Project"] : null;
                                worksheet.Cells[row, 16].Value = reader["Update_Date"] != DBNull.Value ? Convert.ToDateTime(reader["Update_Date"]).ToString("dd/MM/yyyy HH:mm") : null;
                                worksheet.Cells[row, 17].Value = reader["Update_By"] != DBNull.Value ? reader["Update_By"] : null;
                                worksheet.Cells[row, 18].Value = reader["Remark"] != DBNull.Value ? reader["Remark"] : null;
                                worksheet.Cells[row, 19].Value = reader["Problem_Detail"] != DBNull.Value ? reader["Problem_Detail"] : null;
                                worksheet.Cells[row, 20].Value = reader["Solving_Program"] != DBNull.Value ? reader["Solving_Program"] : null;
                                worksheet.Cells[row, 21].Value = reader["AService_Status"] != DBNull.Value ? reader["AService_Status"] : null;
                                row++;
                            }

                            worksheet.Cells.AutoFitColumns();

                            string excelName = "ReportCases";
                            if (!string.IsNullOrEmpty(terminalID)) excelName += $"_Terminal_{terminalID}";
                            if (!string.IsNullOrEmpty(placeInstall)) excelName += $"_Place_{placeInstall.Replace(" ", "_")}";
                            if (!string.IsNullOrEmpty(issueName)) excelName += $"_Issue_{issueName.Replace(" ", "_")}";
                            if (fromdate.HasValue) excelName += $"_From_{fromdate.Value:yyyyMMdd}";
                            if (todate.HasValue) excelName += $"_To_{todate.Value:yyyyMMdd}";
                            if (!string.IsNullOrEmpty(statusName)) excelName += $"_Status_{statusName.Replace(" ", "_")}";

                            excelName += ".xlsx";

                            var stream = new MemoryStream();
                            package.SaveAs(stream);
                            stream.Position = 0;

                            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
                        }
                    }
                }
            }
        }

        public JsonResult GetReportCases(string termID, string issueName, string statusName, DateTime? fromDate, DateTime? toDate)
        {
            List<ReportCase> reportCases = new List<ReportCase>();

            using (MySqlConnection conn = new MySqlConnection(_configuration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_BAAC")))
            {
                conn.Open();

                string query = "SELECT Case_Error_No, Terminal_ID, Place_Install, Issue_Name, Date_Inform, Status_Name " +
                               "FROM reportcases WHERE 1=1";

                if (!string.IsNullOrEmpty(termID))
                {
                    query += " AND Terminal_ID = @TermID";
                }
                if (!string.IsNullOrEmpty(issueName))
                {
                    query += " AND Issue_Name = @IssueName";
                }
                if (!string.IsNullOrEmpty(statusName))
                {
                    query += " AND Status_Name = @StatusName";
                }
                if (fromDate.HasValue)
                {
                    query += " AND DATE(Date_Inform) >= DATE(@FromDate)";
                }
                if (toDate.HasValue)
                {
                    query += " AND DATE(Date_Inform) <= DATE(@ToDate)";
                }

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TermID", termID);
                cmd.Parameters.AddWithValue("@IssueName", issueName);
                cmd.Parameters.AddWithValue("@StatusName", statusName);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reportCases.Add(new ReportCase
                        {
                            CaseErrorNo = reader.GetInt64("Case_Error_No"),
                            TerminalID = reader.GetString("Terminal_ID"),
                            PlaceInstall = reader.GetString("Place_Install"),
                            IssueName = reader.GetString("Issue_Name"),
                            DateInform = reader.GetDateTime("Date_Inform").ToString("dd/MM/yyyy"),
                            StatusName = reader.GetString("Status_Name")
                        });
                    }
                }
            }

            return Json(reportCases);
        }

        [HttpPost]
        public IActionResult UpdateRemark(string caseErrorNo, string remark)
        {
            try
            {
                string connectionString = _configuration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_BAAC") ?? "";
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "UPDATE reportcases SET Remark = @Remark WHERE Case_Error_No = @CaseErrorNo";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Remark", remark);
                        command.Parameters.AddWithValue("@CaseErrorNo", caseErrorNo);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { success = true, message = "Remark updated successfully" });
                        }
                        else
                        {
                            return NotFound(new { success = false, message = "Case not found" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        private void SetLatestUpdateViewBag()
        {
            try
            {
                ConnectMySQL db_mysql = new ConnectMySQL(_configuration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_BAAC"));
                MySqlCommand com = new MySqlCommand();
                com.CommandText = "SELECT Update_Date, Update_By FROM reportcases ORDER BY Update_Date DESC LIMIT 1;";
                DataTable dt = db_mysql.GetDatatable(com);
                List<LatestUpdateData_record> result = ConvertDataTableToModel.ConvertDataTable<LatestUpdateData_record>(dt);
                LatestUpdateData_record latestUpdate = result.FirstOrDefault();

                if (latestUpdate != null)
                {
                    ViewBag.LatestUpdateDate = latestUpdate.Update_Date.ToString("dd/MM/yyyy HH:mm");
                    ViewBag.UpdatedBy = latestUpdate.Update_By;
                }
                else
                {
                    ViewBag.LatestUpdateDate = "-";
                    ViewBag.UpdatedBy = "-";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching latest update: {ex.Message}");
                ViewBag.LatestUpdateDate = "-";
                ViewBag.UpdatedBy = "-";
            }
        }
        #endregion

        #region ticket
        private DateTime SetTime(DateTime date, int hour, int minute, int second)
        {

            return new DateTime(date.Year, date.Month, date.Day, hour, minute, second);
        }
        [HttpGet]
        public IActionResult TicketManagement(string termid, string ticket, DateTime? todate, DateTime? fromdate, string mainproblem, string terminaltype, string jobno, int? maxRows, string cmdButton, string bank)
        {
            int pageSize = 20;
            recordset_ticketManagement = new List<TicketManagement>();
            pageSize = maxRows.HasValue ? maxRows.Value : 100;
            termid = termid ?? "";
            mainproblem = mainproblem ?? "";
            terminaltype = terminaltype ?? "";
            jobno = jobno ?? "";
            bank = bank ?? "";
            DateTime job_fromdate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
            DateTime job_todate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            if (cmdButton == "Clear")
            {
                ViewBag.CurrentJobNo = new List<TicketJob>();
                ViewBag.CurrentTermID = new List<Device_info_record>();
                return View();
            }


            ViewBag.JobNo = jobno;
            ViewBag.countrow = recordset_ticketManagement.Count;
            ViewBag.maxRows = pageSize;
            if (bank != "")
            {
                ViewBag.CurrentJobNo = GetJobNumberMysql(job_fromdate.ToString("yyyy-MM-dd"), job_todate.ToString("yyyy-MM-dd"), bank, _configuration);
                ViewBag.CurrentTermID = GetDeviceInfoMysql(bank, _configuration);
                if (fromdate.HasValue && todate.HasValue)
                {
                    if (fromdate <= todate)
                    {
                        if ((DateTime)todate > DateTime.Now)
                        {

                            todate = SetTime(DateTime.Now, 23, 59, 59);
                        }
                        DateTime _fromdate = (DateTime)fromdate;
                        DateTime _todate = (DateTime)todate;
                        recordset_ticketManagement = GetTicketManagementFromMySql(_fromdate.ToString("yyyy-MM-dd"), _todate.ToString("yyyy-MM-dd"), termid, mainproblem, jobno, terminaltype, bank);
                        ticket_dataList = recordset_ticketManagement;
                    }
                }
            }
            else
            {
                ViewBag.CurrentJobNo = new List<TicketJob>();
                ViewBag.CurrentTermID = new List<Device_info_record>();
            }
            ViewBag.pageSize = pageSize;
            ViewBag.TERM_ID = termid;
            ViewBag.terminaltype = terminaltype;
            ViewBag.mainproblem = mainproblem;
            ViewBag.bankcode = bank;
            if (fromdate != null)
            {
                ViewBag.CurrentFr = fromdate;
            }
            else
            {
                ViewBag.CurrentFr = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1).ToString("yyyy-MM-dd");
            }
            if (todate != null)
            {
                ViewBag.CurrentTo = todate;
            }
            else
            {
                ViewBag.CurrentTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)).ToString("yyyy-MM-dd");
            }

            return View(recordset_ticketManagement);
        }

        public List<TicketManagement> GetTicketManagementFromMySql(string fromdate, string todate, string termid, string mainproblem, string jobno, string terminaltype, string _bank)

        {
            List<TicketManagement> dataList = new List<TicketManagement>();
            string sqlQuery = " SELECT a.Open_Date,a.Appointment_Date,a.Closed_Repair_Date,a.Down_Time,a.Actual_Open_Date,a.Actual_Appointment_Date, ";
            sqlQuery += " a.Actual_Closed_Repair_Date,a.Actual_Down_Time,a.Status,b.TERM_ID,a.Serial_No,b.TERM_NAME,b.MODEL_ID,b.PROVINCE,a.Problem_Detail,a.Solving_Program, ";
            sqlQuery += " a.Service_Team,a.Contact_Name_Branch_CIT,a.Open_By,a.Remark,a.Job_No,a.Aservice_Status,a.Service_Type,a.Open_Name,a.Assign_By, ";
            sqlQuery += " a.Zone_Area,a.Main_Problem,a.Sub_Problem,a.Main_Solution,a.Sub_Solution,a.Part_of_use,a.TechSupport,a.CIT_Request,a.Terminal_Status ";
            sqlQuery += " FROM t_tsd_JobDetail a left join device_info b on a.Serial_No  = b.TERM_SEQ  WHERE ";
            sqlQuery += " a.Open_Date between '" + DateTime.Parse(fromdate).ToString("yyyy-MM-dd") + " 00:00:00' and '" + DateTime.Parse(todate).ToString("yyyy-MM-dd") + " 23:59:59' ";

            if (termid != "")
            {
                sqlQuery += " and b.TERM_ID = '" + termid + "' ";
            }
            if (jobno != "")
            {
                sqlQuery += " and a.Job_No ='" + jobno + "' ";
            }
            if (mainproblem != "")
            {
                sqlQuery += " and a.Main_Problem like '%" + mainproblem + "%' ";
            }
            if (terminaltype != "")
            {
                sqlQuery += " and b.TERM_ID like '%" + terminaltype + "%' ";
            }
            sqlQuery += " order by a.Open_Date asc";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_configuration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + _bank)))
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dataList.Add(GetTicketManagementFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return dataList;
        }
        protected virtual TicketManagement GetTicketManagementFromReader(IDataReader reader)
        {
            TicketManagement record = new TicketManagement();
            if (reader["Open_Date"] != DBNull.Value)
            {
                DateTime xValue = Convert.ToDateTime(reader["Open_Date"]);
                record.Open_Date = xValue.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                record.Open_Date = "-";
            }
            if (reader["Appointment_Date"] != DBNull.Value)
            {
                DateTime xValue = Convert.ToDateTime(reader["Appointment_Date"]);
                record.Appointment_Date = xValue.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                record.Appointment_Date = "-";
            }
            if (reader["Closed_Repair_Date"] != DBNull.Value)
            {
                DateTime xValue = Convert.ToDateTime(reader["Closed_Repair_Date"]);
                record.Closed_Repair_Date = xValue.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                record.Closed_Repair_Date = "-";
            }
            record.Down_Time = reader["Down_Time"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Down_Time"].ToString()) ? "-" : reader["Down_Time"].ToString();
            if (reader["Actual_Open_Date"] != DBNull.Value)
            {
                DateTime xValue = Convert.ToDateTime(reader["Actual_Open_Date"]);
                record.Actual_Open_Date = xValue.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                record.Actual_Open_Date = "-";
            }
            if (reader["Actual_Appointment_Date"] != DBNull.Value)
            {
                DateTime xValue = Convert.ToDateTime(reader["Actual_Appointment_Date"]);
                record.Actual_Appointment_Date = xValue.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                record.Actual_Appointment_Date = "-";
            }
            if (reader["Actual_Closed_Repair_Date"] != DBNull.Value)
            {
                DateTime xValue = Convert.ToDateTime(reader["Actual_Closed_Repair_Date"]);
                record.Actual_Appointment_Date = xValue.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                record.Actual_Closed_Repair_Date = "-";
            }
            record.Actual_Down_Time = reader["Actual_Down_Time"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Actual_Down_Time"].ToString()) ? "-" : reader["Actual_Down_Time"].ToString();
            record.Status = reader["Status"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Status"].ToString()) ? "-" : reader["Status"].ToString();
            record.TERM_ID = reader["TERM_ID"] is DBNull ? "-" : string.IsNullOrEmpty(reader["TERM_ID"].ToString()) ? "-" : reader["TERM_ID"].ToString();
            record.TERM_SEQ = reader["Serial_No"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Serial_No"].ToString()) ? "-" : reader["Serial_No"].ToString();
            record.MODEL_NAME = reader["MODEL_ID"] is DBNull ? "-" : string.IsNullOrEmpty(reader["MODEL_ID"].ToString()) ? "-" : reader["MODEL_ID"].ToString();
            record.PROVINCE = reader["PROVINCE"] is DBNull ? "-" : string.IsNullOrEmpty(reader["PROVINCE"].ToString()) ? "-" : reader["PROVINCE"].ToString();
            record.TERM_NAME = reader["TERM_NAME"] is DBNull ? "-" : string.IsNullOrEmpty(reader["TERM_NAME"].ToString()) ? "-" : reader["TERM_NAME"].ToString();
            record.Problem_Detail = reader["Problem_Detail"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Problem_Detail"].ToString()) ? "-" : reader["Problem_Detail"].ToString().Replace("\n", "").Replace("\r", "").Replace(",", "|").Replace("/", "|");
            record.Solving_Program = reader["Solving_Program"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Solving_Program"].ToString()) ? "-" : reader["Solving_Program"].ToString().Replace("\n", "").Replace("\r", "").Replace(",", "|").Replace("/", "|");
            record.Service_Team = reader["Service_Team"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Service_Team"].ToString()) ? "-" : reader["Service_Team"].ToString();
            record.Contact_Name_Branch_CIT = reader["Contact_Name_Branch_CIT"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Contact_Name_Branch_CIT"].ToString()) ? "-" : reader["Contact_Name_Branch_CIT"].ToString().Replace("\n", "").Replace("\r", "").Replace(",", "|").Replace("/", "|");
            record.Open_By = reader["Open_By"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Open_By"].ToString()) ? "-" : reader["Open_By"].ToString().Replace("\n", "").Replace("\r", "").Replace(",", "|").Replace("/", "|");
            record.Remark = reader["Remark"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Remark"].ToString()) ? "-" : reader["Remark"].ToString();
            record.Job_No = reader["Job_No"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Job_No"].ToString()) ? "-" : reader["Job_No"].ToString();
            record.Aservice_Status = reader["Aservice_Status"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Aservice_Status"].ToString()) ? "-" : reader["Aservice_Status"].ToString();
            record.Service_Type = reader["Service_Type"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Service_Type"].ToString()) ? "-" : reader["Service_Type"].ToString();
            record.Open_Name = reader["Open_Name"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Open_Name"].ToString()) ? "-" : reader["Open_Name"].ToString();
            record.Assign_By = reader["Assign_By"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Assign_By"].ToString()) ? "-" : reader["Assign_By"].ToString();
            record.Zone_Area = reader["Zone_Area"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Zone_Area"].ToString()) ? "-" : reader["Zone_Area"].ToString();
            record.Main_Problem = reader["Main_Problem"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Main_Problem"].ToString()) ? "-" : reader["Main_Problem"].ToString();
            record.Sub_Problem = reader["Sub_Problem"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Sub_Problem"].ToString()) ? "-" : reader["Sub_Problem"].ToString();
            record.Main_Solution = reader["Main_Solution"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Main_Solution"].ToString()) ? "-" : reader["Main_Solution"].ToString();
            record.Sub_Solution = reader["Sub_Solution"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Sub_Solution"].ToString()) ? "-" : reader["Sub_Solution"].ToString();
            record.Part_of_use = reader["Part_of_use"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Part_of_use"].ToString()) ? "-" : reader["Part_of_use"].ToString();
            record.TechSupport = reader["TechSupport"] is DBNull ? "-" : string.IsNullOrEmpty(reader["TechSupport"].ToString()) ? "-" : reader["TechSupport"].ToString();
            record.CIT_Request = reader["CIT_Request"] is DBNull ? "-" : string.IsNullOrEmpty(reader["CIT_Request"].ToString()) ? "-" : reader["CIT_Request"].ToString();
            record.Terminal_Status = reader["Terminal_Status"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Terminal_Status"].ToString()) ? "-" : reader["Terminal_Status"].ToString();
            return record;
        }
        private static List<Device_info_record> GetDeviceInfoMysql(string _bank, IConfiguration _myConfiguration)
        {
            ConnectMySQL db_mysql = new ConnectMySQL(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + _bank));
            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM device_info order by TERM_SEQ;";
            DataTable testss = db_mysql.GetDatatable(com);

            List<Device_info_record> test = ConvertDataTableToModel.ConvertDataTable<Device_info_record>(testss);

            return test;
        }
        private static List<TicketJob> GetJobNumberMysql(string frommonth, string tomonth, string _bank, IConfiguration _myConfiguration)
        {

            ConnectMySQL db_mysql = new ConnectMySQL(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + _bank));
            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT Job_No FROM t_tsd_JobDetail where Open_Date between '" + frommonth + " 00:00:00' and '" + tomonth + " 23:59:59'"; ;
            DataTable testss = db_mysql.GetDatatable(com);

            List<TicketJob> test = ConvertDataTableToModel.ConvertDataTable<TicketJob>(testss);

            return test;
        }
        #endregion

        #region Excel Ticket

        [HttpPost]
        public ActionResult Ticket_ExportExc()
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

                if (ticket_dataList == null || ticket_dataList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;
                ExcelUtilities_Regulator obj = new ExcelUtilities_Regulator(param);


                // Session["PrefixRep"] = "EJAddTran";

                string folder_name = strPath + _configuration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(ticket_dataList);



                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;



                fname = "Ticket_" + DateTime.Now.ToString("yyyyMMdd");

                strPathDesc = strPath + _configuration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname + ".xlsx";


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
        public ActionResult Ticket_DownloadExportFile(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            string tsDate = "";
            string teDate = "";
            try
            {




                fname = "Ticket_" + DateTime.Now.ToString("yyyyMMdd");

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

                tempPath = Path.GetFullPath(Environment.CurrentDirectory + _configuration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname);




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
