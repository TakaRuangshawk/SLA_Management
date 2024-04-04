using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using SLA_Management.Commons;
using SLA_Management.Data;
using SLA_Management.Models.OperationModel;
using SLA_Management.Models.ReportModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace SLA_Management.Controllers
{
    public class ManagementController : Controller
    {
        private readonly IConfiguration _configuration;
        private static List<TicketManagement> ticket_dataList = new List<TicketManagement>();
        private static List<TicketManagement> recordset_ticketManagement = new List<TicketManagement>();
        public ManagementController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
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
    }
}
