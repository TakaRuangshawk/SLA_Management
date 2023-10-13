

using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PagedList;
using SLA_Management.Data;
using SLA_Management.Data.ExcelUtilitie;
using SLA_Management.Models.OperationModel;
using SLA_Management.Models.TermProbModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection.PortableExecutable;

namespace SLA_Management.Controllers
{
    public class GatewayController : Controller
    {
        private readonly IConfiguration _configuration;
        #region Action page

        private CultureInfo _cultureEnInfo = new CultureInfo("en-US");
        private static gateway_seek param = new gateway_seek();
        private IConfiguration _myConfiguration;
        private DBService dBService;
        private List<GatewayTransaction> recordset = new List<GatewayTransaction>();
        private List<terminalAndSeq> terminalIDAndSeqList = new List<terminalAndSeq>();
        private static List<GatewayTransaction> gateway_dataList = new List<GatewayTransaction>();
        private static List<GatewayModel> gatewaytransaction_dataList = new List<GatewayModel>();
        public static string tmp_term = "";
        public static string tmp_fromdate = "";
        public static string tmp_todate = "";
        private int pageNum = 1;
        private long recCnt = 0;
        public IActionResult Transaction()
        {
             ViewBag.CurrentFr = DateTime.Now.ToString("yyyy-MM-dd");
             ViewBag.CurrentTo = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }
        #endregion
        #region Constructor

        public GatewayController(IConfiguration myConfiguration)
        {

            _myConfiguration = myConfiguration;
            dBService = new DBService(_myConfiguration);
        }



        #endregion
        private List<GatewayTransaction> GetGateway_Database(gateway_seek model)
        {
            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL_gateway:FullNameConnection")))
                {
                    MySqlCommand cmd = new MySqlCommand("gatewaytransaction", cn);

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("?PhoneOTP", model.PhoneOTP));
                    cmd.Parameters.Add(new MySqlParameter("?acctnoto", model.acctnoto));
                    cmd.Parameters.Add(new MySqlParameter("?termid", model.TerminalNo));
                    cmd.Parameters.Add(new MySqlParameter("?trxtype", model.trxtype));
                    cmd.Parameters.Add(new MySqlParameter("?UpdateStatus", model.UpdateStatus));
                    cmd.Parameters.Add(new MySqlParameter("?FromDate", model.FRDATE));
                    cmd.Parameters.Add(new MySqlParameter("?ToDate", model.TODATE));
                    cn.Open();
                    return GetGatewayCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                string err = ex.Message;
                return null;
            }
        }
        private List<GatewayTransaction> GetGatewayCollectionFromReader(IDataReader reader)
        {
            string _seqNo = "";
            string terminalIDTemp = "";
            List<GatewayTransaction> resultList = new List<GatewayTransaction>();
            while (reader.Read())
            {
                    resultList.Add(GetGatewayFromReader(reader, _seqNo));
            }
            return resultList;
        }
        private GatewayTransaction GetGatewayFromReader(IDataReader reader, string pSeqNo)
        {
            GatewayTransaction record = new GatewayTransaction();
            
            record.seqno = reader["SeqNo"].ToString();
            record.terminalno = reader["terminalno"].ToString();
            record.acctnoto = reader["AcctNoTo"].ToString();
            record.frombank = reader["frombank"].ToString();
            record.transtype = reader["transtype"].ToString();
            record.amount = $"{reader["amount"]:n0}";
            record.updatestatus = reader["updatestatus"].ToString();
            record.transdatetime = reader["transdatetime"].ToString();
            record.phoneotp = reader["phoneotp"].ToString();
            return record;
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

        private string CheckAndGetSEQ(string terminalID, List<terminalAndSeq> terminalIDList)
        {
            string result = "";

            foreach (var terminalIDTemp in terminalIDList)
            {
                if (terminalIDTemp.terminalid.Equals(terminalID))
                {
                    result = terminalIDTemp.TERM_SEQ;
                    break;
                }
            }

            return result;
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
        public IActionResult GatewayFetchData(string terminalno,string acctnoto,string transtype,string todate,string fromdate,string status,string row,string page,string search)
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

            using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_Gateway:FullNameConnection")))
            {
                connection.Open();

                // Modify the SQL query to use the 'input' parameter for filtering
                string query = " SELECT Id,SeqNo,PhoneOTP,AcctNoTo,FromBank,TransType,TransDateTime,TerminalNo,Amount,UpdateStatus,ErrorCode FROM bank_transaction WHERE TransDateTime between '" + fromdate + " 00:00:00' and '"+ todate +" 23:59:59' ";
                if(terminalno != "")
                {
                    query += " and TerminalNo = '" + terminalno + "'";
                }
                if (acctnoto != "")
                {
                    query += " and AcctNoTo = '" + acctnoto + "'";
                }
                if (transtype != "")
                {
                    query += " and TransType = '" + transtype + "'";
                }
                if (status != "")
                {
                    query += " and UpdateStatus = '" + status + "'";
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
            int pages = jsonData.Count()/_row;
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
