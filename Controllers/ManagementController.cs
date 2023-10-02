using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace SLA_Management.Controllers
{
    public class ManagementController : Controller
    {
        private readonly IConfiguration _configuration;

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
    }
}
