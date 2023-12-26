using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SLA_Management.Data;
using SLA_Management.Models;
using SLA_Management.Models.OperationModel;
using SLAManagement.Data;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Configuration;
using Serilog;

namespace SLA_Management.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _myConfiguration;
        private DBService dBService;
        SqlCommand com = new SqlCommand();
       
        private static ConnectMySQL db_fv;
        CultureInfo usaCulture = new CultureInfo("en-US");
        public static string _error = "";
        public static string _complete = "";      
        DecryptConfig decryptConfig = new DecryptConfig();
        private string dbFullName;
        public HomeController(IConfiguration myConfiguration)
        {

            _myConfiguration = myConfiguration;
            dBService = new DBService(_myConfiguration);
            
            

            dbFullName = decryptConfig.DecryptString(_myConfiguration.GetValue<string>("ConnectString_Gateway:FullNameConnection"), decryptConfig.EnsureKeySize("boom"));

            db_fv = new ConnectMySQL(dbFullName);
        }

        
        public IActionResult Login()
        {
            if (HttpContext.Session.TryGetValue("username", out byte[] usernameBytes))
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
            // Clear session
            HttpContext.Session.Clear();
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, post-check=0, pre-check=0";
            Response.Headers["Expires"] = "0";
            Response.Headers["Pragma"] = "no-cache";
            // Redirect to the login page
            return RedirectToAction("Login", "Home");
        }
        public IActionResult CheckSession()
        {
            // Check if the "username" session variable exists
            bool sessionIsValid = HttpContext.Session.TryGetValue("username", out _);
            return Json(sessionIsValid);
        }
        [HttpPost]
        public IActionResult GetLogin(string username, string password)
        {
            using var log = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("Logs/log" + DateTime.Now.ToString("yyyyMMdd") + ".txt")
            .CreateLogger();
            log.Information("Login : " + username);
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
                                string storedHash = reader["PasswordHash"].ToString();
                                string salt = reader["Salt"].ToString();
                                int role = Convert.ToInt32(reader["Role"]);

                                // Hash the entered password with the stored salt
                                string hashedPassword = HashPassword(password, salt);
                                reader.Close();
                                // Compare the hashed passwords
                                if (storedHash == hashedPassword)
                                {
                                    // Passwords match, set session and redirect
                                    HttpContext.Session.SetString("username", username);
                                    HttpContext.Session.SetInt32("role", role);
                                    UpdateUserLastLogin(username, connection, "LastLogin");
                                    _error = "";
                                    log.Information("username : " + username + " login complete");
                                    return RedirectToAction("Transaction", "Gateway");
                                }
                            }
                        }
                    }
                }

                // Invalid credentials, return to login view
                
                _error = "Invalid username or password";
                log.Information(username+ " : " + _error);
            }
            catch (Exception ex)
            {
                _error = "Something went wrong";
                log.Error("GetLogin Error : " + ex.ToString());
                return RedirectToAction("Login", "Home");
            }
            return RedirectToAction("Login", "Home");
        }
        [HttpPost]
        public IActionResult ResetPassword(string username)
        {
            using var log = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("Logs/log" + DateTime.Now.ToString("yyyyMMdd") + ".txt")
            .CreateLogger();

            try
            {

            
            using (var connection = new MySqlConnection(dbFullName))
                {
                    connection.Open();

                    // Generate a new salt and hash for the default password '111111'
                    string newSalt = GenerateSalt();
                    string hashedPassword = HashPassword("111111", newSalt);

                    // Update the password in the database
                    using (var command = new MySqlCommand("UPDATE Users SET PasswordHash = @PasswordHash, Salt = @Salt WHERE Username = @Username", connection))
                    {
                        command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        command.Parameters.AddWithValue("@Salt", newSalt);
                        command.Parameters.AddWithValue("@Username", username);

                        command.ExecuteNonQuery();
                    }
                    UpdateUserLastLogin(username, connection, "UpdateDate");
                }
            }
            catch (Exception ex)
            {

                log.Error("ResetPassword Error : " + ex.ToString());
                return Json(new { success = false, message = "Something went wrong!" });
            }
            return Json(new { success = true, message = "Password reset successfully!" });
        }
        [HttpPost]
        public IActionResult DeleteUser(string username)
        {
            using var log = new LoggerConfiguration().WriteTo.Console().WriteTo.File("Logs/log" + DateTime.Now.ToString("yyyyMMdd") + ".txt").CreateLogger();

            try
            {

            
            using (var connection = new MySqlConnection(dbFullName))
            {
                connection.Open();

                // Update the status to 'Inactive' in the database
                using (var command = new MySqlCommand("UPDATE Users SET Status = 'Inactive' WHERE Username = @Username", connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    command.ExecuteNonQuery();
                }
                UpdateUserLastLogin(username, connection, "UpdateDate");
            }
            }
            catch (Exception ex)
            {

                log.Error("DeleteUser Error : " + ex.ToString());
                return Json(new { success = false, message = "Something went wrong!" });

            }
            return Json(new { success = true, message = "User deleted successfully!" });
        }
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[16]; // You can adjust the size as needed
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        private void UpdateUserLastLogin(string username, MySqlConnection connection,string whichdate)
        {
            // Update 'updatedate' column to current timestamp
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
                                string storedHash = reader["PasswordHash"].ToString();
                                string salt = reader["Salt"].ToString();

                                // Hash the entered old password with the stored salt
                                string hashedOldPassword = HashPassword(oldPassword, salt);

                                // Compare the hashed old passwords
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

                    // Generate a new salt
                    string newSalt = GenerateSalt();

                    // Hash the new password with the new salt
                    string hashedNewPassword = HashPassword(newPassword, newSalt);

                    // Update the password
                    var updatePasswordQuery = "UPDATE Users SET PasswordHash = @NewPassword, Salt = @NewSalt WHERE Username = @Username";
                    using (var updatePasswordCommand = new MySqlCommand(updatePasswordQuery, connection))
                    {
                        updatePasswordCommand.Parameters.AddWithValue("@NewPassword", hashedNewPassword);
                        updatePasswordCommand.Parameters.AddWithValue("@NewSalt", newSalt);
                        updatePasswordCommand.Parameters.AddWithValue("@Username", username);

                        updatePasswordCommand.ExecuteNonQuery();
                    }
                    UpdateUserLastLogin(username, connection, "UpdateDate");
                }

                // Return a JSON response indicating that the password has been changed
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
            using var log = new LoggerConfiguration().WriteTo.Console().WriteTo.File("log" + DateTime.Now.ToString("yyyyMMdd") + ".txt").CreateLogger();
            try
            {
                // Perform MySQL operation to create user
                using (var connection = new MySqlConnection(dbFullName))
                {
                    connection.Open();

                    // Generate a salt and hash the password
                    string salt = GenerateSalt();
                    string hashedPassword = HashPassword(password, salt);

                    // Insert the new user into the database
                    using (var command = new MySqlCommand("INSERT INTO Users (Username, PasswordHash, Salt, Role, UpdateDate, Status) VALUES (@Username, @PasswordHash, @Salt, @Role, NOW(), 'Active')", connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        command.Parameters.AddWithValue("@Salt", salt);
                        command.Parameters.AddWithValue("@Role", role);

                        command.ExecuteNonQuery();
                    }
                }

                // Return success message
                return Json(new { success = true, message = "User created successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                log.Error("Error : " + ex.ToString());
                return Json(new { success = false, message = "Something went wrong!" });
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
        private IDataReader ExecuteReader(DbCommand cmd)
        {
            return ExecuteReader(cmd, CommandBehavior.Default);
        }
        private IDataReader ExecuteReader(DbCommand cmd, CommandBehavior behavior)
        {
            try
            {
                return cmd.ExecuteReader(behavior);
            }
            catch (MySqlException ex)
            {
                string err = "";
                err = "Inner message : " + ex.InnerException.Message;
                err += Environment.NewLine + "Message : " + ex.Message;
                return null;
            }
        }
    }
}