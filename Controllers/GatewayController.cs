

using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Serilog;
using SLA_Management.Data;
using SLA_Management.Data.ExcelUtilitie;
using SLA_Management.Models.TermProbModel;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace SLA_Management.Controllers
{
    public class GatewayController : Controller
    {
        private readonly IConfiguration _configuration;
        #region Action page

        private CultureInfo _cultureEnInfo = new CultureInfo("en-US");
        private IConfiguration _myConfiguration;

        private static List<GatewayModel> gatewaytransaction_dataList = new List<GatewayModel>();
        private static gateway_seek param = new gateway_seek();
        private static string tmp_term = "";
        private static string tmp_fromdate = "";
        private static string tmp_todate = "";
        
        private int pageNum = 1;
        private long recCnt = 0;
        public class User
        {
            public int Id { get; set; }
            public string Username { get; set; }
         
        }
        public IActionResult Transaction()
        {
            if (HttpContext.Session.TryGetValue("username", out byte[] usernameBytes))
            {
                ViewBag.CurrentFr = DateTime.Now.ToString("yyyy-MM-dd");
                ViewBag.CurrentTo = DateTime.Now.ToString("yyyy-MM-dd");
                int? userRole = HttpContext.Session.GetInt32("role");
                if (userRole.HasValue && userRole.Value <= 1)
                {
                    // Assuming you have a method to fetch user data from the MySQL table
                    List<User> userList = GetUserListFromMySQL();

                    // Store the user list in ViewBag
                    ViewBag.UserList = userList;

                    return View();
                }
                else
                {
                    ViewBag.UserList = new List<User>();
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
        }



        #endregion
        public List<User> GetUserListFromMySQL()
        {
            List<User> userList = new List<User>();

            using (var connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_Gateway:FullNameConnection")))
            {
                connection.Open();

                using (var command = new MySqlCommand("SELECT id, Username FROM Users where Status = 'Active'", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new User
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

        #region Excel

        [HttpPost]
        public ActionResult Gateway_ExportExc(string terminal,string fromdate,string todate)
        {
            string fname = "";
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;
            terminal = terminal ?? "";
            fromdate = fromdate ?? "";
            todate = todate ?? "";
            try
            {

                if (gatewaytransaction_dataList == null || gatewaytransaction_dataList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;
                ExcelUtilities_gateway obj = new ExcelUtilities_gateway(param);


                // Session["PrefixRep"] = "EJAddTran";

                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderGatewayTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(gatewaytransaction_dataList,terminal,fromdate,todate);



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
            string tsDate = "";
            string teDate = "";
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




                if (rpttype.ToLower().EndsWith("s") == true)
                    return File(tempPath + "xml", "application/vnd.openxmlformats-officedocument.spreadsheetml", fname);
                else if (rpttype.ToLower().EndsWith("f") == true)
                    return File(tempPath + "xml", "application/pdf", fname);
                else  //(rpttype.ToLower().EndsWith("v") == true)
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
            using var log = new LoggerConfiguration().WriteTo.Console().WriteTo.File("Logs/log" + DateTime.Now.ToString("yyyyMMdd") + ".txt").CreateLogger();

            try
            {

            }
            catch (Exception ex)
            {

                log.Error("Error : " + ex.ToString());
                return Json(new { success = false, message = "Something went wrong!" });
            }
            using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_Gateway:FullNameConnection")))
            {
                connection.Open();

                // Modify the SQL query to use the 'input' parameter for filtering
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
                            Id = reader["Id"].ToString(),
                            SeqNo = reader["SeqNo"].ToString(),
                            ThaiID = reader["ThaiID"].ToString(),
                            PhoneOTP = reader["PhoneOTP"].ToString(),
                            AcctNoTo = reader["AcctNoTo"].ToString(),
                            FromBank = reader["FromBank"].ToString(),
                            TransType = reader["TransType"].ToString(),
                            TransDateTime = Convert.ToDateTime(reader["TransDateTime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                            TerminalNo = reader["TerminalNo"].ToString(),
                            Amount = string.Format("{0:N0}", reader.GetInt32(reader.GetOrdinal("Amount"))),
                            UpdateStatus = reader["UpdateStatus"].ToString(),
                            ErrorCode = reader["ErrorCode"].ToString()
                        });
                    }
                }
            }
            gatewaytransaction_dataList = jsonData;
            int pages = (int)Math.Ceiling((double)jsonData.Count() / _row);
            List<GatewayModel> filteredData = RangeFilter(jsonData, _page, _row);
            var response = new DataResponse
            {
                JsonData = filteredData,
                Page = pages,
                currentPage = _page
            };
            return Json(response);
        }
        
        static List<GatewayModel> RangeFilter<GatewayModel>(List<GatewayModel> inputList, int page, int row)
        {
            int start_row;
            int end_row;
            if (page == 1)
            {
                start_row = 0;
            }
            else
            {
                start_row = (page - 1) * row;
            }
            end_row = start_row + row - 1;
            if (inputList.Count < end_row)
            {
                end_row = inputList.Count - 1;
            }
            return inputList.Skip(start_row).Take(row).ToList();
        }
    }
    public class GatewayModel
    {
        public string Id { get; set; }
        public string SeqNo { get; set; }
        public string ThaiID { get; set; }
        public string PhoneOTP { get; set; }
        public string AcctNoTo { get; set; }
        public string FromBank { get; set; }
        public string TransType { get; set; }
        public string TransDateTime { get; set; }
        public string TerminalNo { get; set; }
        public string Amount { get; set; }
        public string UpdateStatus { get; set; }
        public string ErrorCode { get; set; }

    }
    public class DataResponse
    {
        public List<GatewayModel> JsonData { get; set; }
        public int Page { get; set; }
        public int currentPage { get; set; }
    }
}
