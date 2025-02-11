using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SLA_Management.Data;
using SLA_Management.Models;

namespace SLA_Management.Controllers
{
    public class LoginMainController : Controller
    {
        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        private IConfiguration _myConfiguration;
        private DBService dBService;
        ConnectSQL_Server con;

        public LoginMainController(IConfiguration myConfiguration)
        {

            _myConfiguration = myConfiguration;
            dBService = new DBService(_myConfiguration);
            con = new ConnectSQL_Server(_myConfiguration["ConnectionStrings:DefaultConnection"]);
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_BAAC");
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Role FROM loginmain WHERE username = @Username AND password = @Password";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Username", model.Username);
                    cmd.Parameters.AddWithValue("@Password", model.Password);

                    object roleResult = cmd.ExecuteScalar(); 
                    if (roleResult != null)
                    {
                        string role = roleResult.ToString(); 

                        
                        HttpContext.Session.SetString("Username", model.Username);
                        HttpContext.Session.SetString("UserId", Guid.NewGuid().ToString());
                        HttpContext.Session.SetString("Role", role);

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid username or password.");
                    }
                }
            }

            return View(model);
        }

        public IActionResult Logout()
        {
           
            HttpContext.Session.Remove("Username");
            HttpContext.Session.Remove("Role");
            HttpContext.Session.Remove("Password");


            return RedirectToAction("Login", "LoginMain");
        }



    }
}
