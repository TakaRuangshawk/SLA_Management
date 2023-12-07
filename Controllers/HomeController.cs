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
        private static List<feelviewstatus> recordset_homeshowstatus = new List<feelviewstatus>();
        private static List<comlogrecord> recordset_comlogrecord = new List<comlogrecord>();
        private static List<slatracking> recordset_slatracking = new List<slatracking>();
        private static List<secone> recordset_secone = new List<secone>();
        private static List<secone> recordset_secone_adm = new List<secone>();
        private DBService dBService;
        SqlCommand com = new SqlCommand();
        ConnectSQL_Server con;
        private static ConnectMySQL db_fv;
        CultureInfo usaCulture = new CultureInfo("en-US");
        public static string _error = "";
        public static string _complete = "";
        public HomeController(IConfiguration myConfiguration)
        {

            _myConfiguration = myConfiguration;
            dBService = new DBService(_myConfiguration);
            con = new ConnectSQL_Server(_myConfiguration["ConnectionStrings:DefaultConnection"]);
            db_fv = new ConnectMySQL(myConfiguration.GetValue<string>("ConnectString_Gateway:FullNameConnection"));
        }
        #region homeold
        public IActionResult Index()
        {
            recordset_homeshowstatus = GetHomeStatus();
            if(recordset_homeshowstatus != null)
            {
                foreach (var Data in recordset_homeshowstatus)
                {
                    ViewBag.onlineATM = Data.onlineATM;
                    ViewBag.onlineADM = Data.onlineADM;
                    ViewBag.offlineATM = Data.offlineATM;
                    ViewBag.offlineADM = Data.offlineADM;
                    ViewBag.onlineTotal = (Convert.ToInt32(Data.onlineATM) + Convert.ToInt32(Data.onlineADM)).ToString();
                    ViewBag.offlineTotal = (Convert.ToInt32(Data.offlineATM) + Convert.ToInt32(Data.offlineADM)).ToString();
                    ViewBag.FVTotal = ((Convert.ToInt32(Data.onlineATM) + Convert.ToInt32(Data.onlineADM)) + (Convert.ToInt32(Data.offlineATM) + Convert.ToInt32(Data.offlineADM))).ToString();
                }
            }
            else
            {
                ViewBag.onlineATM = "-";
                ViewBag.onlineADM = "-";
                ViewBag.offlineATM = "-";
                ViewBag.offlineADM = "-";
            }
            recordset_comlogrecord = GetComlogRecordFromSqlServer();
            recordset_slatracking = GetSlatrackingFromSqlServer();
            if(recordset_comlogrecord != null)
            {
                foreach (var Data in recordset_comlogrecord)
                {
                    if(Data.comlogADM != "" || Data.comlogATM != "")
                    {
                        ViewBag.comlogADM = Data.comlogADM;
                        ViewBag.comlogATM = Data.comlogATM;
                        ViewBag.comlogTotal = (Convert.ToInt32(Data.comlogADM) + Convert.ToInt32(Data.comlogATM)).ToString();
                    }
                    else
                    {
                        ViewBag.comlogATM = "-";
                        ViewBag.comlogADM = "-";
                        ViewBag.comlogTotal = "-";
                    }
                    
                }
            }
            else
            {
                ViewBag.comlogATM = "-";
                ViewBag.comlogADM = "-";
            }
            recordset_secone = GetSECOneStatus();
            if(recordset_secone != null)
            {
                foreach (var data in recordset_secone)
                {
                    ViewBag.secone_online = data._online;
                    ViewBag.secone_offline = data._offline;
                }
            }
            else
            {
                ViewBag.secone_online = "-";
                ViewBag.secone_offline = "-";
            }
            recordset_secone_adm = GetSECOneADMStatus();
            if(recordset_secone_adm != null)
            {
                foreach (var data in recordset_secone_adm)
                {
                    ViewBag.secone_adm_online = data._online;
                    ViewBag.secone_adm_offline = data._offline;
                }
            }
            else
            {
                ViewBag.secone_adm_online = "-";
                ViewBag.secone_adm_offline = "-";
            }
            if(recordset_secone != null && recordset_secone_adm != null)
            {
                ViewBag.TotalSECONE_online = (Convert.ToInt32(ViewBag.secone_adm_online)+Convert.ToInt32(ViewBag.secone_online)).ToString();
                ViewBag.TotalSECONE_offine = (Convert.ToInt32(ViewBag.secone_adm_offline) + Convert.ToInt32(ViewBag.secone_offline)).ToString();
                ViewBag.TotalSECONE = (Convert.ToInt32(ViewBag.TotalSECONE_online) + Convert.ToInt32(ViewBag.TotalSECONE_offine)).ToString();
            }
            ViewBag.DateNow = DateTime.Now.AddDays(-1).ToString("dd - MM - yyyy",usaCulture);
            return View(recordset_slatracking);
        }
        public List<comlogrecord> GetComlogRecordFromSqlServer()
        {
            List<comlogrecord> dataList = new List<comlogrecord>();

            
            string sqlQuery = " SELECT COUNT(CASE WHEN ERROR IS NULL  AND TERM_ID LIKE '%G262%' THEN 1 END) AS ComlogADM,COUNT(CASE WHEN ERROR IS NULL  AND TERM_ID like '%G165%' THEN 1 END) AS ComlogATM ";
            sqlQuery += " FROM comlog_record where COMLOGDATE between '" + DateTime.Now.Date.AddDays(-1).ToString("yyyy-MM-dd",usaCulture) + " 00:00:00' AND '" + DateTime.Now.Date.AddDays(-1).ToString("yyyy-MM-dd", usaCulture) + " 23:59:59'";
            try
            {
                using (SqlConnection connection = new SqlConnection(_myConfiguration.GetValue<string>("ConnectionStrings:DefaultConnection")))
                {

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dataList.Add(GetComlogRecordFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }
            

            return dataList;
        }
        public List<slatracking> GetSlatrackingFromSqlServer() {
            List<slatracking> dataList = new List<slatracking>();
            string sqlQuery = " SELECT [APPNAME],[STATUS],[UPDATE_DATE]FROM (SELECT [APPNAME],[STATUS],[UPDATE_DATE],ROW_NUMBER() OVER (PARTITION BY [APPNAME] ORDER BY [UPDATE_DATE] DESC) AS rn ";
            sqlQuery += "  FROM sla_tracking where APPNAME in ('appChangeAndUnzip','InsertFileCOMLog','Translator','NDCT','Downtime','SLA Report')) ranked WHERE rn = 1 and YEAR(UPDATE_DATE) = YEAR(GETDATE()) ";
            sqlQuery += " ORDER BY CASE WHEN [APPNAME] = 'appChangeAndUnzip' THEN 1 WHEN [APPNAME] = 'InsertFileCOMLog' THEN 2 WHEN [APPNAME] = 'Translator' THEN 3 WHEN [APPNAME] = 'NDCT' THEN 4 WHEN [APPNAME] = 'Downtime' THEN 5 WHEN [APPNAME] = 'SLA Report' THEN 6 ELSE 6 END; ";
            try
            {
                using (SqlConnection connection = new SqlConnection(_myConfiguration.GetValue<string>("ConnectionStrings:DefaultConnection")))
                {

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            int n = 1;
                            while (reader.Read())
                            {
                                dataList.Add(GetSlatrackingFromReader(reader,n));
                                n++;
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
        protected virtual slatracking GetSlatrackingFromReader(IDataReader reader,int n)
        {
            slatracking record = new slatracking();

            record.no = n.ToString();
            record.APPNAME = reader["APPNAME"].ToString();
            record.STATUS = reader["STATUS"].ToString();
            record.UPDATE_DATE = DateTime.Parse(reader["UPDATE_DATE"].ToString()).ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.DefaultThreadCurrentCulture);
            return record;
        }
        protected virtual comlogrecord GetComlogRecordFromReader(IDataReader reader)
        {
            comlogrecord record = new comlogrecord();

            record.comlogADM = reader["comlogADM"].ToString();
            record.comlogATM = reader["comlogATM"].ToString();
            return record;
        }
        public List<secone> GetSECOneStatus()
        {
            string _sql = string.Empty;

            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_SECOne:FullNameConnection")))
                {

                    _sql = "SELECT SUM(CASE WHEN AGENT_STATUS = 1 THEN 1 ELSE 0 END) AS _online, SUM(CASE WHEN AGENT_STATUS != 1 or AGENT_STATUS is null THEN 1 ELSE 0 END) AS _offline   ";
                    _sql += " FROM device_info  ";
                    cn.Open();

                    MySqlCommand cmd = new MySqlCommand(_sql, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                    return GetSECOneStatusCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                return null;
            }
        }
        protected virtual List<secone> GetSECOneStatusCollectionFromReader(IDataReader reader)
        {
            List<secone> recordlst = new List<secone>();
            while (reader.Read())
            {
                recordlst.Add(GetSECOneStatusFromReader(reader));
            }
            return recordlst;
        }
        protected virtual secone GetSECOneStatusFromReader(IDataReader reader)
        {
            secone record = new secone();

            record._online = reader["_online"].ToString();
            record._offline = reader["_offline"].ToString();
            return record;
        }
        public List<secone> GetSECOneADMStatus()
        {
            string _sql = string.Empty;

            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_SECOne_ADM:FullNameConnection")))
                {

                    _sql = "SELECT SUM(CASE WHEN AGENT_STATUS = 1 THEN 1 ELSE 0 END) AS _online, SUM(CASE WHEN AGENT_STATUS != 1 or AGENT_STATUS is null THEN 1 ELSE 0 END) AS _offline   ";
                    _sql += " FROM device_info  ";
                    cn.Open();

                    MySqlCommand cmd = new MySqlCommand(_sql, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                    return GetSECOneStatusADMCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                return null;
            }
        }
        protected virtual List<secone> GetSECOneStatusADMCollectionFromReader(IDataReader reader)
        {
            List<secone> recordlst = new List<secone>();
            while (reader.Read())
            {
                recordlst.Add(GetSECOneStatusADMFromReader(reader));
            }
            return recordlst;
        }
        protected virtual secone GetSECOneStatusADMFromReader(IDataReader reader)
        {
            secone record = new secone();

            record._online = reader["_online"].ToString();
            record._offline = reader["_offline"].ToString();
            return record;
        }
        public List<feelviewstatus> GetHomeStatus()
        {
            string _sql = string.Empty;

            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection")))
                {

                    _sql = "SELECT COUNT(CASE WHEN(SUBSTRING(TERM_ID, '14', 4) = '165'and DEVICE_STATUS_EVENT_ID != 'E1005' and DEVICE_STATUS_EVENT_ID != 'E1156' and DEVICE_STATUS_EVENT_ID != 'E1006' and DEVICE_STATUS_EVENT_ID != 'E1036') THEN 1 END) AS _onlineATM,  ";
                    _sql += " COUNT(CASE WHEN(SUBSTRING(TERM_ID, '14', 4) = '165'and (DEVICE_STATUS_EVENT_ID = 'E1005' or DEVICE_STATUS_EVENT_ID = 'E1156' or DEVICE_STATUS_EVENT_ID = 'E1006' or DEVICE_STATUS_EVENT_ID = 'E1036')) THEN 1 END) AS _offlineATM,  ";
                    _sql += " COUNT(CASE WHEN(SUBSTRING(TERM_ID, '14', 4) = '262'and DEVICE_STATUS_EVENT_ID != 'E1005' and DEVICE_STATUS_EVENT_ID != 'E1156' and DEVICE_STATUS_EVENT_ID != 'E1006' and DEVICE_STATUS_EVENT_ID != 'E1036') THEN 1 END) AS _onlineADM,  ";
                    _sql += " COUNT(CASE WHEN(SUBSTRING(TERM_ID, '14', 4) = '262'and (DEVICE_STATUS_EVENT_ID = 'E1005' or DEVICE_STATUS_EVENT_ID = 'E1156' or DEVICE_STATUS_EVENT_ID = 'E1006' or DEVICE_STATUS_EVENT_ID = 'E1036')) THEN 1 END) AS _offlineADM";
                    _sql += " FROM gsb_adm_fv.device_status_info  ";
                    //_sql += " left join device_info as b on a.TERM_ID = b.TERM_ID";

                    cn.Open();

                    MySqlCommand cmd = new MySqlCommand(_sql, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                    return GetHomeStatusCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                return null;
            }
        }
        protected virtual List<feelviewstatus> GetHomeStatusCollectionFromReader(IDataReader reader)
        {
            List<feelviewstatus> recordlst = new List<feelviewstatus>();
            while (reader.Read())
            {
                recordlst.Add(GetHomeStatusFromReader(reader));
            }
            return recordlst;
        }
        protected virtual feelviewstatus GetHomeStatusFromReader(IDataReader reader)
        {
            feelviewstatus record = new feelviewstatus();

            record.onlineADM = reader["_onlineADM"].ToString();
            record.onlineATM = reader["_onlineATM"].ToString();
            record.offlineADM = reader["_offlineADM"].ToString();
            record.offlineATM = reader["_offlineATM"].ToString();
            return record;
        }
        #endregion
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
            .WriteTo.File("log"+ DateTime.Now.ToString("yyyyMMdd") + ".txt")
            .CreateLogger();

            log.Information("Login : " + username);
            _complete = "";
            try
            {
                using (var connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_Gateway:FullNameConnection")))
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
            .WriteTo.File("log" + DateTime.Now.ToString("yyyyMMdd") + ".txt")
            .CreateLogger();

            try
            {

            
            using (var connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_Gateway:FullNameConnection")))
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
            using var log = new LoggerConfiguration().WriteTo.Console().WriteTo.File("log" + DateTime.Now.ToString("yyyyMMdd") + ".txt").CreateLogger();

            try
            {

            
            using (var connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_Gateway:FullNameConnection")))
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
        // Function to update the last login timestamp
        private void UpdateUserLastLogin(string username, MySqlConnection connection,string whichdate)
        {
            // Update 'updatedate' column to current timestamp
            using (var updateCommand = new MySqlCommand("UPDATE Users SET LastLogin = NOW() WHERE Username = @Username", connection))
            {
                updateCommand.Parameters.AddWithValue("@Username", username);
                updateCommand.ExecuteNonQuery();
            }
        }
        // Function to hash the password using SHA256 with salt
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
                using (var connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_Gateway:FullNameConnection")))
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
                using (var connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_Gateway:FullNameConnection")))
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