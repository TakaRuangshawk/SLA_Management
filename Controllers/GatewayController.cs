

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
namespace SLA_Management.Controllers
{
    public class GatewayController : Controller
    {
        #region Action page

        private CultureInfo _cultureEnInfo = new CultureInfo("en-US");
        private static gateway_seek param = new gateway_seek();
        private IConfiguration _myConfiguration;
        private DBService dBService;
        private List<GatewayTransaction> recordset = new List<GatewayTransaction>();
        private List<terminalAndSeq> terminalIDAndSeqList = new List<terminalAndSeq>();
        private static List<GatewayTransaction> gateway_dataList = new List<GatewayTransaction>();


        private int pageNum = 1;
        private long recCnt = 0;
        public IActionResult Transaction(string cmdButton, string TermID, string FrDate, string ToDate
            , string currTID, string currFr, string currTo,string lstPageSize
            , string currPageSize, int? page, string maxRows, string phoneotp, string acctnoto, string frombank, string transtype
            , string amount, string updatestatus)
        {


            if (String.IsNullOrEmpty(maxRows))
                ViewBag.maxRows = "20";
            else
                ViewBag.maxRows = maxRows;

            if (cmdButton == "Clear")
                return RedirectToAction("Transaction");

            try
            {
                

                if (null == TermID && null == FrDate && null == ToDate && null == page)
                {
                    //FrDate = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
                    FrDate = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
                    ToDate = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
                    page = 1;
                }
                else
                {
                    // Return temp value back to it own variable
                    FrDate = (FrDate ?? currFr);
                    ToDate = (ToDate ?? currTo);
                    TermID = (TermID ?? currTID);
                }

                // ViewBag.CurrentTID = (TermID ?? currTID);
                ViewBag.CurrentTerminalno = TermID;
                ViewBag.CurrentFr = (FrDate ?? currFr);
                ViewBag.CurrentTo = (ToDate ?? currTo);
                ViewBag.CurrentPageSize = (lstPageSize ?? currPageSize);
                ViewBag.CurrentPhoneotp = phoneotp;
                ViewBag.CurrentAcctnoto = acctnoto;
                ViewBag.CurrentFrombank = frombank;
                ViewBag.CurrentTranstype = transtype;
                ViewBag.CurrentAmount = amount;
                ViewBag.CurrentUpdatestatus = updatestatus;



                if (null == TermID)
                    param.TerminalNo = currTID == null ? "" : currTID;
                else
                    param.TerminalNo = TermID == null ? "" : TermID;

                if ((FrDate == null && currFr == null) && (FrDate == "" && currFr == ""))
                {
                    param.FRDATE = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
                }
                else
                {
                    param.FRDATE = FrDate + " 00:00:00";
                }

                if ((ToDate == null && currTo == null) && (ToDate == "" && currTo == ""))
                {
                    param.TODATE = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
                }
                else
                {
                    param.TODATE = ToDate + " 23:59:59";
                }


                if (null != lstPageSize || null != currPageSize)
                {
                    param.PAGESIZE = String.IsNullOrEmpty(lstPageSize) == true ?
                        int.Parse(currPageSize) : int.Parse(lstPageSize);
                }
                else
                {
                    param.PAGESIZE = 20;
                }
                param.TerminalNo = TermID ?? "";
                param.UpdateStatus = updatestatus ?? "";
                param.trxtype = transtype ?? "";
                param.PhoneOTP = phoneotp ?? "";
                param.acctnoto = acctnoto ?? "";
                recordset = GetGateway_Database(param);
                

                if (null == recordset || recordset.Count <= 0)
                {
                    ViewBag.NoData = "true";

                }

                else
                {
                    recCnt = recordset.Count;
                    gateway_dataList = recordset;
                    param.PAGESIZE = recordset.Count;
                }


                if (recCnt > 0)
                {
                    ViewBag.Records = String.Format("{0:#,##0}", recCnt.ToString());
                }
                else
                    ViewBag.Records = "0";

                pageNum = (page ?? 1);

            }
            catch (Exception ex)
            {

            }
            return View(recordset.ToPagedList(pageNum, (int)param.PAGESIZE));
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
        public ActionResult Gateway_ExportExc()
        {
            string fname = "";
            string tsDate = "";
            string teDate = "";
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;
            try
            {

                if (gateway_dataList == null || gateway_dataList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;
                ExcelUtilities_gateway obj = new ExcelUtilities_gateway(param);


                // Session["PrefixRep"] = "EJAddTran";

                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderGatewayTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(gateway_dataList);



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
    }
}
