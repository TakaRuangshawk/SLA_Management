﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SLA_Management.Data;
using SLA_Management.Models;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using X.PagedList;
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
        Stopwatch stopwatch = new Stopwatch(); // เริ่มจับเวลา
        stopwatch.Start();

        string connectionString = _myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection");

        // ขั้นตอนเปิดการเชื่อมต่อฐานข้อมูล
        Stopwatch connStopwatch = new Stopwatch();
        connStopwatch.Start();

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open(); // เปิดการเชื่อมต่อ
            connStopwatch.Stop(); // หยุดจับเวลาเมื่อการเชื่อมต่อเสร็จ

            // ตรวจสอบเวลาในการเปิดการเชื่อมต่อ
            Console.WriteLine($"Database connection time: {connStopwatch.ElapsedMilliseconds} ms");

            string hashedPassword = HashPassword(model.Password);

            // ขั้นตอนการส่งคำสั่ง SQL
            Stopwatch queryStopwatch = new Stopwatch();
            queryStopwatch.Start();

            string query = "SELECT Role, Name FROM loginmain WHERE username = @Username AND password = @Password";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Username", model.Username);
            cmd.Parameters.AddWithValue("@Password", hashedPassword);

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                queryStopwatch.Stop(); // หยุดจับเวลาหลังจากคำสั่ง SQL ถูกประมวลผล

                // ตรวจสอบเวลาในการประมวลผลคำสั่ง SQL
                Console.WriteLine($"SQL query execution time: {queryStopwatch.ElapsedMilliseconds} ms");

                if (reader.Read())
                {
                    string role = reader["Role"].ToString();
                    string name = reader["Name"].ToString();

                    HttpContext.Session.SetString("Username", model.Username);
                    HttpContext.Session.SetString("UserId", Guid.NewGuid().ToString());
                    HttpContext.Session.SetString("Password", model.Password);
                    HttpContext.Session.SetString("Name", name);
                    HttpContext.Session.SetString("Role", role);

                    stopwatch.Stop(); // หยุดจับเวลาหลังจากการทำงานเสร็จ
                    Console.WriteLine($"Total login processing time: {stopwatch.ElapsedMilliseconds} ms");

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


                string connectionString = _myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection");
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
                string connectionString = _myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection");
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

            string connectionString = _myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection");

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

            if (string.IsNullOrEmpty(model.Username))
            {
                ModelState.AddModelError("Username", "Username is required.");
                return View("ManageUser");
            }

            string connectionString = _myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection");

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

        [HttpPost]
        public IActionResult ResetPassword(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "Invalid ID" });
            }

            // You can either use your model or fetch the user directly by ID
            string connectionString = _myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection");

            string hashedPassword = HashPassword("11111");

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Update query to set the password to "11111"
                string query = "UPDATE loginmain SET Password=@Password WHERE ID=@ID";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword); // Default password

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to reset password" });
                    }
                }
            }
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
