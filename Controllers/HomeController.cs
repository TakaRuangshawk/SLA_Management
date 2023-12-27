using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SLA_Management.Data;
using SLA_Management.Models;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;


namespace SLA_Management.Controllers
{
    public class HomeController : Controller
    {
        
        
        private  string _error = "";
        private  string _complete = "";      
        readonly DecryptConfig decryptConfig = new DecryptConfig();
        private readonly string dbFullName;

        readonly Loger log = new Loger();

        public HomeController(IConfiguration myConfiguration)
        {
            IConfiguration _myConfiguration;
            _myConfiguration = myConfiguration;
            dbFullName = decryptConfig.DecryptString(_myConfiguration.GetValue<string>("ConnectString_Gateway:FullNameConnection") ?? "", decryptConfig.EnsureKeySize("boom"));
           

        }

        
        public IActionResult Login()
        {
            if (HttpContext.Session.TryGetValue("username", out byte[]? usernameBytes))
            {
                return RedirectToAction("Transaction", "Gateway");
                
            }
            else
            {
                ViewBag.complete = _complete;
                ViewBag.error = _error;
                return View();

            }
            
        }
      
        public IActionResult Logout()
        {
            
            HttpContext.Session.Clear();
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, post-check=0, pre-check=0";
            Response.Headers["Expires"] = "0";
            Response.Headers["Pragma"] = "no-cache";
           
            return RedirectToAction("Login", "Home");
        }
        public IActionResult CheckSession()
        {
           
            bool sessionIsValid = HttpContext.Session.TryGetValue("username", out _);
            return Json(sessionIsValid);
        }
        [HttpPost]
        public IActionResult GetLogin(string username, string password)
        {
        
            log.WriteLogFile("Login : " + username);

            _complete = "";
            try
            {
                using (var connection = new MySqlConnection(dbFullName))
                {
                    connection.Open();

                    using (var command = new MySqlCommand("SELECT PasswordHash, Salt,Role FROM Users WHERE Username = @Username and Status = 'Active'", connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader["PasswordHash"].ToString() ?? "";
                                string salt = reader["Salt"].ToString() ?? "";
                                int role = Convert.ToInt32(reader["Role"]);

                                
                                string hashedPassword = HashPassword(password, salt);
                                reader.Close();
                               
                                if (storedHash == hashedPassword)
                                {
                                    
                                    HttpContext.Session.SetString("username", username);
                                    HttpContext.Session.SetInt32("role", role);
                                    UpdateUserLastLogin(username, connection); 
                                    _error = "";
                                    log.WriteLogFile("username : " + username + " login complete");
                                    return RedirectToAction("Transaction", "Gateway");
                                }
                            }
                        }
                    }
                }

                
                
                _error = "Invalid username or password";
                log.WriteLogFile(username+ " : " + _error);
            }
            catch (Exception ex)
            {
                _error = "Something went wrong";
                log.WriteErrLog("GetLogin Error : " + ex.ToString());
                return RedirectToAction("Login", "Home");
            }
            return RedirectToAction("Login", "Home");
        }
        [HttpPost]
        public IActionResult ResetPassword(string username)
        {
          

            try
            {

            
            using (var connection = new MySqlConnection(dbFullName))
                {
                    connection.Open();

                   
                    string newSalt = GenerateSalt();
                    string hashedPassword = HashPassword("111111", newSalt);

                    
                    using (var command = new MySqlCommand("UPDATE Users SET PasswordHash = @PasswordHash, Salt = @Salt WHERE Username = @Username", connection))
                    {
                        command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        command.Parameters.AddWithValue("@Salt", newSalt);
                        command.Parameters.AddWithValue("@Username", username);

                        command.ExecuteNonQuery();
                    }
                    UpdateUserLastLogin(username, connection);
                }
            }
            catch (Exception ex)
            {

                log.WriteErrLog("ResetPassword Error : " + ex.ToString());
                return Json(new { success = false, message = "Something went wrong!" });
            }
            return Json(new { success = true, message = "Password reset successfully!" });
        }
        [HttpPost]
        public IActionResult DeleteUser(string username)
        {
          

            try
            {

            
            using (var connection = new MySqlConnection(dbFullName))
            {
                connection.Open();

                
                using (var command = new MySqlCommand("UPDATE Users SET Status = 'Inactive' WHERE Username = @Username", connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    command.ExecuteNonQuery();
                }
                UpdateUserLastLogin(username, connection);
            }
            }
            catch (Exception ex)
            {

                log.WriteErrLog("DeleteUser Error : " + ex.ToString());
                return Json(new { success = false, message = "Something went wrong!" });

            }
            return Json(new { success = true, message = "User deleted successfully!" });
        }
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        private static void UpdateUserLastLogin(string username, MySqlConnection connection)
        {
            
          
            using (var updateCommand = new MySqlCommand("UPDATE Users SET LastLogin = NOW() WHERE Username = @Username", connection))
            {
                updateCommand.Parameters.AddWithValue("@Username", username);
                updateCommand.ExecuteNonQuery();
            }
        }

        public static string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] passwordWithSaltBytes = new byte[passwordBytes.Length + saltBytes.Length];

                Buffer.BlockCopy(passwordBytes, 0, passwordWithSaltBytes, 0, passwordBytes.Length);
                Buffer.BlockCopy(saltBytes, 0, passwordWithSaltBytes, passwordBytes.Length, saltBytes.Length);

                byte[] hashBytes = sha256.ComputeHash(passwordWithSaltBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
        [HttpPost]
        public IActionResult ChangePassword(string username, string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                return Json(new { success = false, message = "Please fill in all the required fields." });
            }
            if (oldPassword == newPassword)
            {
                return Json(new { success = false, message = "Password cannot be duplicated." });
            }

            try
            {
                using (var connection = new MySqlConnection(dbFullName))
                {
                    connection.Open();

                    var checkPasswordQuery = "SELECT PasswordHash, Salt FROM Users WHERE Username = @Username";
                    using (var checkPasswordCommand = new MySqlCommand(checkPasswordQuery, connection))
                    {
                        checkPasswordCommand.Parameters.AddWithValue("@Username", username);

                        using (var reader = checkPasswordCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader["PasswordHash"].ToString() ?? "";
                                string salt = reader["Salt"].ToString() ?? "";

                               
                                string hashedOldPassword = HashPassword(oldPassword, salt);

                               
                                if (storedHash != hashedOldPassword)
                                {
                                    return Json(new { success = false, message = "Old password does not match." });
                                }
                            }
                            else
                            {
                                return Json(new { success = false, message = "User not found." });
                            }
                        }
                    }

                    
                    string newSalt = GenerateSalt();

                   
                    string hashedNewPassword = HashPassword(newPassword, newSalt);

                    
                    var updatePasswordQuery = "UPDATE Users SET PasswordHash = @NewPassword, Salt = @NewSalt WHERE Username = @Username";
                    using (var updatePasswordCommand = new MySqlCommand(updatePasswordQuery, connection))
                    {
                        updatePasswordCommand.Parameters.AddWithValue("@NewPassword", hashedNewPassword);
                        updatePasswordCommand.Parameters.AddWithValue("@NewSalt", newSalt);
                        updatePasswordCommand.Parameters.AddWithValue("@Username", username);

                        updatePasswordCommand.ExecuteNonQuery();
                    }
                    UpdateUserLastLogin(username, connection);
                }

               
                _complete = "Password changed successfully!";
                return Json(new { success = true, message = "Password changed successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
        [HttpPost]
        public IActionResult CreateUser(string username, string password, string role)
        {
            username = username ?? "";
            password = password ?? "";
            role = role ?? "";
            if(username != "" && password != "" && role != "")
            {
                try
                {

                    using (var connection = new MySqlConnection(dbFullName))
                    {
                        connection.Open();


                        string salt = GenerateSalt();
                        string hashedPassword = HashPassword(password, salt);


                        using (var command = new MySqlCommand("INSERT INTO Users (Username, PasswordHash, Salt, Role, UpdateDate, Status) VALUES (@Username, @PasswordHash, @Salt, @Role, NOW(), 'Active')", connection))
                        {
                            command.Parameters.AddWithValue("@Username", username);
                            command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                            command.Parameters.AddWithValue("@Salt", salt);
                            command.Parameters.AddWithValue("@Role", role);

                            command.ExecuteNonQuery();
                        }
                    }


                    return Json(new { success = true, message = "User created successfully!" });
                }
                catch (Exception ex)
                {

                    log.WriteErrLog("Error : " + ex.ToString());
                    return Json(new { success = false, message = "Something went wrong!" });
                }
            }
            else
            {
                return Json(new { success = false, message = "Please fill out the form!" });
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
      
       
    }
}