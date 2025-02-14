using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using X.PagedList;
using SLA_Management.Data;
using SLA_Management.Models;
using System.Security.Cryptography;
using System.Text;
using X.PagedList.Extensions;

namespace SLA_Management.Controllers
{
    public class LoginMainController : Controller
    {
        // GET: Login
        public ActionResult Login()
        {
            if (HttpContext.Session.GetString("Username") != null)
            {
                return RedirectToAction("Index", "Home");
            }
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


                    string hashedPassword = HashPassword(model.Password);

                    string query = "SELECT Role, Name FROM loginmain WHERE username = @Username AND password = @Password";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Username", model.Username);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string role = reader["Role"].ToString();
                            string name = reader["Name"].ToString();

                            HttpContext.Session.SetString("Username", model.Username);
                            HttpContext.Session.SetString("UserId", Guid.NewGuid().ToString());
                            HttpContext.Session.SetString("Password", model.Password);
                            HttpContext.Session.SetString("Name", name);
                            HttpContext.Session.SetString("Role", role);

                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid username or password.");
                        }
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
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("Name");


            return RedirectToAction("Login", "LoginMain");
        }

        public ActionResult EditUser()
        {

            var username = HttpContext.Session.GetString("Username");
            var password = HttpContext.Session.GetString("Password");
            var name = HttpContext.Session.GetString("Name");
            var role = HttpContext.Session.GetString("Role");
            var id = HttpContext.Session.GetString("UserId");

            if (username == null || role == null)
            {
                return RedirectToAction("Login");
            }

            var model = new LoginModel
            {
                Username = username,
                Password = password,
                Name = name,
                Role = role,
                ID = id
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(LoginModel model)
        {
            if (ModelState.IsValid)
            {

                HttpContext.Session.SetString("Username", model.Username);
                HttpContext.Session.SetString("Name", model.Name);
                HttpContext.Session.SetString("Role", model.Role);


                string hashedPassword = HashPassword(model.Password);

                HttpContext.Session.SetString("Password", model.Password);


                string connectionString = _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_BAAC");
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE loginmain SET password = @Password, name = @Name, role = @Role WHERE username = @Username";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Username", model.Username);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);
                    cmd.Parameters.AddWithValue("@Name", model.Name);
                    cmd.Parameters.AddWithValue("@Role", model.Role);

                    cmd.ExecuteNonQuery();
                }

                ViewBag.Message = "Your changes have been saved.";
                //return RedirectToAction("Index", "Home");
            }

            return View(model);
        }



        public ActionResult AddUser()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUser(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_BAAC");
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();


                    string checkQuery = "SELECT COUNT(*) FROM loginmain WHERE username = @Username";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@Username", model.Username);

                    int userCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (userCount > 0)
                    {

                        ModelState.AddModelError("Username", "Username already exists. Please choose another.");
                        return View(model);
                    }


                    string hashedPassword = HashPassword(model.Password);


                    string query = "INSERT INTO loginmain (username, password, name, role, updatedate) VALUES (@Username, @Password, @Name, @Role, @Updatedate)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Username", model.Username);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);
                    cmd.Parameters.AddWithValue("@Name", model.Name);
                    cmd.Parameters.AddWithValue("@Role", model.Role);
                    cmd.Parameters.AddWithValue("@Updatedate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    cmd.ExecuteNonQuery();

                    ViewBag.Message = "User has been added successfully.";
                    //return RedirectToAction("Index", "Home");
                }
            }

            return View(model);
        }


        public IActionResult ManageUser(int? page, int pageSize = 10)
        {
            List<LoginModel> users = new List<LoginModel>();

            string connectionString = _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_BAAC");

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT ID, Username, Name, Role FROM loginmain";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new LoginModel
                        {
                            ID = reader["ID"].ToString(),
                            Username = reader["Username"].ToString(),
                            Name = reader["Name"]?.ToString(),
                            Role = reader["Role"]?.ToString()
                        });
                    }
                }
            }

            int pageNumber = page ?? 1;
            ViewBag.PageSize = pageSize;

            IPagedList<LoginModel> pagedUsers = users.ToPagedList(pageNumber, pageSize);

            return View(pagedUsers);
        }

        [HttpPost]
        public IActionResult EditUser_ManageUser(LoginModel model)
        {

            string connectionString = _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_BAAC");

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE loginmain SET Username=@Username, Name=@Name, Role=@Role WHERE ID=@ID";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", model.ID);
                    cmd.Parameters.AddWithValue("@Username", model.Username);
                    cmd.Parameters.AddWithValue("@Name", model.Name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Role", model.Role ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("ManageUser");

        }


        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder hashString = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    hashString.Append(b.ToString("x2"));
                }
                return hashString.ToString();
            }
        }

    }
}
