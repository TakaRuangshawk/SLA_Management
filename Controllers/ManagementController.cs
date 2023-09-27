using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

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
    }
}
