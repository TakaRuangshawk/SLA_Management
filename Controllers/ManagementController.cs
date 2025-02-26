using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.ManagementModel;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SLA_Management.Commons;
using SLA_Management.Data;
using SLA_Management.Data.ExcelUtilitie;
using SLA_Management.Models.ManagementModel;
using SLA_Management.Models.OperationModel;
using SLA_Management.Models.ReportModel;
using SLA_Management.Models.TermProbModel;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq.Expressions;

namespace SLA_Management.Controllers
{
    public class ManagementController : Controller
    {
        private readonly IConfiguration _configuration;
        private static regulator_seek param = new regulator_seek();
        private static List<TicketManagement> ticket_dataList = new List<TicketManagement>();
        private static List<TicketManagement> recordset_ticketManagement = new List<TicketManagement>();
        public ManagementController(IConfiguration configuration)
        {
            _configuration = configuration;
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
        public IActionResult CreateEvent(string title, DateTime start,string user)
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
                    sqlCommand.Parameters.AddWithValue ("@CreatedBy", user);
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
            catch(Exception ex)
            {
                return BadRequest("Failed to create event: " + ex.Message);
            }
            
           

        }

        #endregion

        #region ReportCases
        public IActionResult ReportCases()
        {
            ViewBag.CurrentTID = GetDeviceInfoFeelview("BAAC", _configuration);
            ViewBag.Issue_Name = GetIssue_Name("BAAC", _configuration);
            ViewBag.Status_Name = GetStatus_Name("BAAC", _configuration);
            SetLatestUpdateViewBag();
            // For now, just return the empty view
            return View();
        }

        [HttpGet]
        public JsonResult FetchReportCases(string terminalID, string placeInstall, string issueName, DateTime? fromdate, DateTime? todate, string statusName, int row = 50, int page = 1)
        {
            List<ReportCase> reportCases = new List<ReportCase>();
            int totalCases = 0;
            int totalPages = 0;
            using (MySqlConnection conn = new MySqlConnection(_configuration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_BAAC")))
            {
                conn.Open();

                string query = "SELECT Case_Error_No, Terminal_ID, Place_Install, Issue_Name, Date_Inform, Status_Name, Branch_name_pb, Repair1, Repair2, Repair3, Repair4, Repair5, Incident_No, Date_Close_Pb, Type_Project, Update_Date, Update_By, Remark " +
                               "FROM reportcases WHERE 1=1";

                if (!string.IsNullOrEmpty(terminalID))
                {
                    query += " AND Terminal_ID = @TerminalID";
                }
                if (!string.IsNullOrEmpty(placeInstall))
                {
                    query += " AND Place_Install LIKE @PlaceInstall";
                }
                if (!string.IsNullOrEmpty(issueName))
                {
                    query += " AND Issue_Name LIKE @IssueName";
                }
                if (fromdate.HasValue)
                {
                    query += " AND DATE(Date_Inform) >= @FromDate";
                }
                if (todate.HasValue)
                {
                    query += " AND DATE(Date_Inform) <= @ToDate";
                }
                if (!string.IsNullOrEmpty(statusName))
                {
                    query += " AND Status_Name = @StatusName";
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
                            Remark = reader["Remark"] != DBNull.Value ? reader.GetString("Remark") : string.Empty
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
                string query = "SELECT * FROM reportcases WHERE 1=1";

                if (!string.IsNullOrEmpty(terminalID))
                    query += " AND Terminal_ID = @TerminalID";
                if (!string.IsNullOrEmpty(placeInstall))
                    query += " AND Place_Install LIKE @PlaceInstall";
                if (!string.IsNullOrEmpty(issueName))
                    query += " AND Issue_Name LIKE @IssueName";
                if (fromdate.HasValue)
                    query += " AND Date_Inform >= @FromDate";
                if (todate.HasValue)
                    query += " AND Date_Inform <= @ToDate";
                if (!string.IsNullOrEmpty(statusName))
                    query += " AND Status_Name = @StatusName";

                query += " ORDER BY Date_Inform ASC, Case_Error_No ASC ";

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

                            // Add headers
                            worksheet.Cells[1, 1].Value = "Case No";
                            worksheet.Cells[1, 2].Value = "Terminal ID";
                            worksheet.Cells[1, 3].Value = "Place Install";
                            worksheet.Cells[1, 4].Value = "Issue Name";
                            worksheet.Cells[1, 5].Value = "Date Inform";
                            worksheet.Cells[1, 6].Value = "Status Name";
                            worksheet.Cells[1, 7].Value = "Branch Name";
                            worksheet.Cells[1, 8].Value = "Repair 1";
                            worksheet.Cells[1, 9].Value = "Repair 2";
                            worksheet.Cells[1, 10].Value = "Repair 3";
                            worksheet.Cells[1, 11].Value = "Repair 4";
                            worksheet.Cells[1, 12].Value = "Repair 5";
                            worksheet.Cells[1, 13].Value = "Incident No";
                            worksheet.Cells[1, 14].Value = "Date Close Pb";
                            worksheet.Cells[1, 15].Value = "Type Project";
                            worksheet.Cells[1, 16].Value = "Update Date";
                            worksheet.Cells[1, 17].Value = "Update By";
                            worksheet.Cells[1, 18].Value = "Remark";
                            using (var range = worksheet.Cells[1, 1, 1, 18]) // Apply to all header cells
                            {
                                range.Style.Font.Bold = true; // Bold font
                                range.Style.Font.Size = 14; // Larger font size
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid; // Set fill pattern to solid
                                range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue); // Set background color
                                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Center text
                                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center; // Center vertically
                            }
                            int row = 2;
                            while (reader.Read())
                            {
                                worksheet.Cells[row, 1].Value = reader["Case_Error_No"] != DBNull.Value ? reader["Case_Error_No"] : null;
                                worksheet.Cells[row, 2].Value = reader["Terminal_ID"] != DBNull.Value ? reader["Terminal_ID"] : null;
                                worksheet.Cells[row, 3].Value = reader["Place_Install"] != DBNull.Value ? reader["Place_Install"] : null;
                                worksheet.Cells[row, 4].Value = reader["Issue_Name"] != DBNull.Value ? reader["Issue_Name"] : null;
                                worksheet.Cells[row, 5].Value = reader["Date_Inform"] != DBNull.Value ? reader.GetDateTime("Date_Inform").ToString("dd/MM/yyyy") : null;
                                worksheet.Cells[row, 6].Value = reader["Status_Name"] != DBNull.Value ? reader["Status_Name"] : null;
                                worksheet.Cells[row, 7].Value = reader["Branch_name_pb"] != DBNull.Value ? reader["Branch_name_pb"] : null;
                                worksheet.Cells[row, 8].Value = reader["Repair1"] != DBNull.Value ? reader["Repair1"] : null;
                                worksheet.Cells[row, 9].Value = reader["Repair2"] != DBNull.Value ? reader["Repair2"] : null;
                                worksheet.Cells[row, 10].Value = reader["Repair3"] != DBNull.Value ? reader["Repair3"] : null;
                                worksheet.Cells[row, 11].Value = reader["Repair4"] != DBNull.Value ? reader["Repair4"] : null;
                                worksheet.Cells[row, 12].Value = reader["Repair5"] != DBNull.Value ? reader["Repair5"] : null;
                                worksheet.Cells[row, 13].Value = reader["Incident_No"] != DBNull.Value ? reader["Incident_No"] : null;
                                worksheet.Cells[row, 14].Value = reader["Date_Close_Pb"] != DBNull.Value ? reader.GetDateTime("Date_Close_Pb").ToString("dd/MM/yyyy") : null;
                                worksheet.Cells[row, 15].Value = reader["Type_Project"] != DBNull.Value ? reader["Type_Project"] : null;
                                worksheet.Cells[row, 16].Value = reader["Update_Date"] != DBNull.Value ? reader.GetDateTime("Update_Date").ToString("dd/MM/yyyy HH:mm") : null;
                                worksheet.Cells[row, 17].Value = reader["Update_By"] != DBNull.Value ? reader["Update_By"] : null;
                                worksheet.Cells[row, 18].Value = reader["Remark"] != DBNull.Value ? reader["Remark"] : null;


                                row++;
                            }

                            worksheet.Cells.AutoFitColumns();

                            // Build the filename based on filters
                            string excelName = "ReportCases";
                            if (!string.IsNullOrEmpty(terminalID))
                                excelName += $"_Terminal_{terminalID}";
                            if (!string.IsNullOrEmpty(placeInstall))
                                excelName += $"_Place_{placeInstall.Replace(" ", "_")}";
                            if (!string.IsNullOrEmpty(issueName))
                                excelName += $"_Issue_{issueName.Replace(" ", "_")}";
                            if (fromdate.HasValue)
                                excelName += $"_From_{fromdate.Value.ToString("yyyyMMdd")}";
                            if (todate.HasValue)
                                excelName += $"_To_{todate.Value.ToString("yyyyMMdd")}";
                            if (!string.IsNullOrEmpty(statusName))
                                excelName += $"_Status_{statusName.Replace(" ", "_")}";

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
        public IActionResult TicketManagement(string termid, string ticket, DateTime? todate, DateTime? fromdate, string mainproblem, string terminaltype, string jobno, int? maxRows, string cmdButton,string bank)
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
            if(bank != "") {
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
