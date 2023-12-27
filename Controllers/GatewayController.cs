using EncryptData.Net;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SLA_Management.Data;
using SLA_Management.Data.ExcelUtilitie;
using System.Data;


namespace SLA_Management.Controllers
{
    public class GatewayController : Controller
    {
        
        #region Action page
        private readonly IConfiguration _myConfiguration;
        private string tmp_term = "";
        private string tmp_fromdate = "";
        private string tmp_todate = "";
        private readonly string rawConfig;
        readonly DecryptConfig decryptConfig = new DecryptConfig();
        readonly Loger log = new Loger();
        public class UserDetail
        {
            public int Id { get; set; }
            public string? Username { get; set; }
         
        }
        public IActionResult Transaction()
        {
            
            if (HttpContext.Session.TryGetValue("username", out byte[]? usernameBytes))
            {
                ViewBag.CurrentFr = DateTime.Now.ToString("yyyy-MM-dd");
                ViewBag.CurrentTo = DateTime.Now.ToString("yyyy-MM-dd");
                int? userRole = HttpContext.Session.GetInt32("role");
                if (userRole.HasValue && userRole.Value <= 1)
                {
                    List<UserDetail> userList = GetUserListFromMySQL();
                    ViewBag.UserList = userList;
                    return View();
                }
                else
                {
                    ViewBag.UserList = new List<UserDetail>();
                    return View();
                }
            }
            else
            {
               
                return RedirectToAction("Login","Home");
            }
            
        }
        #endregion
        #region Constructor

        public GatewayController(IConfiguration myConfiguration)
        {

            _myConfiguration = myConfiguration;

            AesEncryption encryptData = new AesEncryption();
            rawConfig = encryptData.DecryptConnectionString(_myConfiguration.GetValue<string>("ConnectString_Gateway:FullNameConnection") ?? "") ;

        }



        #endregion
      
        public List<UserDetail> GetUserListFromMySQL()
        {
            List<UserDetail> userList = new List<UserDetail>();

            using (var connection = new MySqlConnection(rawConfig))
            {
                connection.Open();

                using (var command = new MySqlCommand("SELECT id, Username FROM Users where Status = 'Active'", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new UserDetail
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Username = reader["Username"].ToString()
                            };

                            userList.Add(user);
                        }
                    }
                }
            }

            return userList;
        }

        #region Excel

        [HttpGet]
        public ActionResult Gateway_ExportExc(string terminalno, string fromdate,string todate,string acctnoto, string transtype,string status)
        {
            string fname = "";
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;
            terminalno = terminalno ?? string.Empty;
            fromdate = fromdate ?? string.Empty;
            todate = todate ?? string.Empty;
            transtype = transtype ?? string.Empty;
            acctnoto = acctnoto ?? string.Empty;
            status = status ?? string.Empty;
            try
            {
                List<GatewayModel> jsonData = new List<GatewayModel>();
                using (MySqlConnection connection = new MySqlConnection(rawConfig))
                {
                    connection.Open();


                    string query = " SELECT  ROW_NUMBER() OVER (ORDER BY Id) AS Id,SeqNo,ThaiID,PhoneOTP,AcctNoTo,FromBank,TransType,TransDateTime,TerminalNo,Amount,UpdateStatus,ErrorCode FROM bank_transaction WHERE TransDateTime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' ";
                    if (terminalno != "")
                    {
                        query += " and TerminalNo like '%" + terminalno + "%'";
                    }
                    if (acctnoto != "")
                    {
                        query += " and AcctNoTo like '%" + acctnoto + "%'";
                    }
                    if (transtype != "")
                    {
                        query += " and TransType = '" + transtype + "'";
                    }
                    if (status != "")
                    {
                        query += " and UpdateStatus = '" + status + "'";
                    }
                    else
                    {
                        query += " order by TransDateTime asc";
                    }
                    MySqlCommand command = new MySqlCommand(query, connection);


                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            jsonData.Add(new GatewayModel
                            {
                                Id = reader["Id"].ToString() ?? "",
                                SeqNo = reader["SeqNo"].ToString() ?? "",
                                ThaiID = reader["ThaiID"].ToString() ?? "",
                                PhoneOTP = reader["PhoneOTP"].ToString() ?? "",
                                AcctNoTo = reader["AcctNoTo"].ToString() ?? "",
                                FromBank = reader["FromBank"].ToString() ?? "",
                                TransType = reader["TransType"].ToString() ?? "",
                                TransDateTime = Convert.ToDateTime(reader["TransDateTime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                TerminalNo = reader["TerminalNo"].ToString() ?? "",
                                Amount = string.Format("{0:N0}", reader.GetInt32(reader.GetOrdinal("Amount"))),
                                UpdateStatus = reader["UpdateStatus"].ToString() ?? "",
                                ErrorCode = reader["ErrorCode"].ToString() ?? ""
                            });
                        }
                    }
                }
               
                if (jsonData == null || jsonData.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;
                ExcelUtilitiesgateway obj = new ExcelUtilitiesgateway();

                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderGatewayTemplate_Excel");

                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(jsonData, terminalno, fromdate,todate,tmp_term,tmp_fromdate,tmp_todate);

                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;

                fname = "GatewayTransaction_" + DateTime.Now.ToString("yyyyMMdd");

                strPathDesc = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderGateway_Excel") + fname + ".xlsx";

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
        public ActionResult DownloadExportFile(string rpttype)
        {
            string fname = "";
            string tempPath = "";
          
            try
            {

                fname = "GatewayTransaction_" + DateTime.Now.ToString("yyyyMMdd");

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

                tempPath = Path.GetFullPath(Environment.CurrentDirectory + _myConfiguration.GetValue<string>("Collection_path:FolderGateway_Excel") + fname);
                if (rpttype.ToLower().EndsWith('s'))
                    return File(tempPath + "xml", "application/vnd.openxmlformats-officedocument.spreadsheetml", fname);
                else if (rpttype.ToLower().EndsWith('f'))
                    return File(tempPath + "xml", "application/pdf", fname);
                else  
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

        [HttpGet]
        public IActionResult GatewayFetchData(string terminalno,string acctnoto,string transtype,string todate,string fromdate,string status,string row,string page,string search,string sort)
        {
            int _page;
            if (page == null||search == "search")
            {
                _page = 1;
            }
            else
            {
                _page = int.Parse(page);
            }
            if (search == "next")
            {
                _page++;
            }
            else if(search == "prev")
            {
                _page--;
            }
            int _row;
            if (row == null)
            {
                _row = 20;
            }
            else
            {
                _row = int.Parse(row);
            }
            terminalno = terminalno ?? "";
            acctnoto = acctnoto ?? "";
            transtype = transtype ?? "";
            status = status ?? "";
            tmp_fromdate = fromdate;
            tmp_term = terminalno;
            tmp_todate = todate;

            List<GatewayModel> jsonData = new List<GatewayModel>();
            using (MySqlConnection connection = new MySqlConnection(rawConfig))
            {
                connection.Open();

                
                string query = " SELECT  ROW_NUMBER() OVER (ORDER BY Id) AS Id,SeqNo,ThaiID,PhoneOTP,AcctNoTo,FromBank,TransType,TransDateTime,TerminalNo,Amount,UpdateStatus,ErrorCode FROM bank_transaction WHERE TransDateTime between '" + fromdate + " 00:00:00' and '"+ todate +" 23:59:59' ";
                if(terminalno != "")
                {
                    query += " and TerminalNo like '%" + terminalno + "%'";
                }
                if (acctnoto != "")
                {
                    query += " and AcctNoTo like '%" + acctnoto + "%'";
                }
                if (transtype != "")
                {
                    query += " and TransType = '" + transtype + "'";
                }
                if (status != "")
                {
                    query += " and UpdateStatus = '" + status + "'";
                }
                if(sort != "")
                {
                    query += " order by TransDateTime " + sort;
                }
                else
                {
                    query += " order by TransDateTime asc" ;
                }
                MySqlCommand command = new MySqlCommand(query, connection);
            

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        jsonData.Add(new GatewayModel
                        {
                            Id = reader["Id"].ToString() ?? "",
                            SeqNo = reader["SeqNo"].ToString() ?? "",
                            ThaiID = reader["ThaiID"].ToString() ?? "",
                            PhoneOTP = reader["PhoneOTP"].ToString() ?? "",
                            AcctNoTo = reader["AcctNoTo"].ToString() ?? "",
                            FromBank = reader["FromBank"].ToString() ?? "",
                            TransType = reader["TransType"].ToString() ?? "",
                            TransDateTime = Convert.ToDateTime(reader["TransDateTime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                            TerminalNo = reader["TerminalNo"].ToString() ?? "",
                            Amount = string.Format("{0:N0}", reader.GetInt32(reader.GetOrdinal("Amount"))),
                            UpdateStatus = reader["UpdateStatus"].ToString() ?? "",
                            ErrorCode = reader["ErrorCode"].ToString() ?? ""
                        });
                    }
                }
            }
             
            int pages = (int)Math.Ceiling((double)jsonData.Count / _row);
            List<GatewayModel> filteredData = RangeFilter(jsonData, _page, _row);
            var response = new DataResponse
            {
                JsonData = filteredData,
                Page = pages,
                currentPage = _page
            };

            log.WriteLogFile("response : " + response);
            return Json(response);
        }
        
        static List<GatewayModel> RangeFilter<GatewayModel>(List<GatewayModel> inputList, int page, int row)
        {
            int start_row;
           
            if (page == 1)
            {
                start_row = 0;
            }
            else
            {
                start_row = (page - 1) * row;
            }
           
            return inputList.Skip(start_row).Take(row).ToList();
        }
    }
    public class GatewayModel
    {
        public string? Id { get; set; }
        public string? SeqNo { get; set; }
        public string? ThaiID { get; set; }
        public string? PhoneOTP { get; set; }
        public string? AcctNoTo { get; set; }
        public string? FromBank { get; set; }
        public string? TransType { get; set; }
        public string? TransDateTime { get; set; }
        public string? TerminalNo { get; set; }
        public string? Amount { get; set; }
        public string? UpdateStatus { get; set; }
        public string? ErrorCode { get; set; }

    }
    public class DataResponse
    {
        public List<GatewayModel>? JsonData { get; set; }
        public int Page { get; set; }
        public int currentPage { get; set; }
    }
}
