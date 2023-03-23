using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using PagedList;
using SLA_Management.Data.TermProbDB;
using SLA_Management.Data.TermProbDB.ExcelUtilitie;
using SLA_Management.Models.TermProbModel;
using SLAManagement.Data;
using System.Data;
using System.Data.Common;
using System.Globalization;


namespace SLA_Management.Controllers
{
    public class EJAddTranProbTermController : Controller
    {

        #region Local Variable
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");
        SqlCommand com = new SqlCommand();
        ConnectSQL_Server con;
        static List<ej_trandeviceprob> ejLog_dataList = new List<ej_trandeviceprob>();
        static ej_trandada_seek param = new ej_trandada_seek();

        #endregion

        #region Initialize Variable



        //public EJAddTranProbTermController(IConfiguration myConfiguration)
        //{
        //    _myConfiguration = myConfiguration;
        //    con = new ConnectSQL_Server(_myConfiguration["ConnectionStrings:DefaultConnection"]);

        //    gatewayTable = _myConfiguration["Database:GatewayTable"];

        //    secondDatabase = _myConfiguration["Database:SecondDatabase"];

        //    secondTable = _myConfiguration["Database:SecondTable"];

        //    symmetricKey = _myConfiguration["Database:SymmetricKey"];

        //    certification = _myConfiguration["Database:Certification"];

        //    startQuery = "OPEN SYMMETRIC KEY " + symmetricKey + " DECRYPTION BY CERTIFICATE " + certification + "  SELECT ID,SeqNo,CONVERT(nvarchar, DecryptByKey(Citizen_Id)) AS 'Citizen_Id_Decrypt',CONVERT (nvarchar , DecryptByKey(Customer_title)) AS 'Customer_title' ,CONVERT(nvarchar, DecryptByKey(Customer_first_name)) AS 'Customer_first_name',CONVERT(nvarchar, DecryptByKey(Customer_middle_name)) AS 'Customer_middle_name',CONVERT(nvarchar, DecryptByKey(Customer_last_name)) AS 'Customer_last_name',CONVERT(nvarchar, DecryptByKey(English_customer_title)) AS 'English_customer_title',CONVERT(nvarchar, DecryptByKey(English_first_name)) AS 'English_first_name',CONVERT(nvarchar, DecryptByKey(English_middle_name)) AS 'English_middle_name',CONVERT(nvarchar, DecryptByKey(English_last_name)) AS 'English_last_name',CONVERT(datetime, CONVERT(nvarchar, DecryptByKey(Date_of_birth))) AS 'Date_of_birth',CONVERT(nvarchar, DecryptByKey(AcctNoFrom)) AS 'AcctNoFrom',CONVERT(nvarchar, DecryptByKey(AcctNoTo)) AS 'AcctNoTo',CONVERT(nvarchar, DecryptByKey(AcctName)) AS 'AcctName', TransType,TransDateTime,TerminalNo,Amount,UpdateStatus,ErrorDescription,device_info.TERM_NAME FROM " + gatewayTable + " left join " + secondDatabase + ".dbo." + secondTable + " On TERM_ID = TerminalNo ";

        //    loginSub = new LoginController(myConfiguration);
        //}

        #endregion


        public ActionResult EJAddTranProbTermAction(string cmdButton, string TermID, string FrDate, string ToDate, string FrTime, string ToTime
            , string currTID, string currFr, string currTo, string currFrTime, string currToTime, string lstPageSize
            , string ddlProbMaster, string currProbMaster, string MessErrKeyWord, string currMessErrKeyWord
            , string currPageSize, int? page)
        {

            List<ej_trandeviceprob> recordset = new List<ej_trandeviceprob>();
            List<ProblemMaster> ProdMasData = new List<ProblemMaster>();


            ViewBag.maxRows = "5";

            //TermID = "T091B030B119G262";
            //FrDate = "2023-03-20";
            //ToDate = "2023-03-20";
            //MessErrKeyWord = "";

            int pageNum = 1;
            try
            {

                ProdMasData = GetMasterSysErrorWord();
                if (ProdMasData.Count > 0)
                {
                    ViewBag.ProbMaster = "";
                    foreach (ProblemMaster obj in ProdMasData)
                    {
                        if (ViewBag.ProbMaster == "")
                            ViewBag.ProbMaster = obj.ProblemCode + "," + obj.ProblemName;
                        else
                            ViewBag.ProbMaster += "|" + obj.ProblemCode + "," + obj.ProblemName;
                    }
                }

                if (cmdButton == "Clear")
                    return RedirectToAction("EJAddTranProbTermAction");



                if (null == TermID && null == FrDate && null == ToDate && null == page)
                {
                    //FrDate = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
                    FrDate = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
                    ToDate = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
                    page = 1;
                }
                else
                {
                    // Return temp value back to it own variabl
                    FrDate = (FrDate ?? currFr);
                    ToDate = (ToDate ?? currTo);
                    FrTime = (FrTime ?? currFrTime);
                    ToTime = (ToTime ?? currToTime);
                    TermID = (TermID ?? currTID);
                    ddlProbMaster = (ddlProbMaster ?? currProbMaster);
                    MessErrKeyWord = (MessErrKeyWord ?? currMessErrKeyWord);
                }

                ViewBag.CurrentTID = (TermID ?? currTID);
                ViewBag.CurrentFr = (FrDate ?? currFr);
                ViewBag.CurrentTo = (ToDate ?? currTo);
                ViewBag.CurrentPageSize = (lstPageSize ?? currPageSize);
                ViewBag.CurrentProbMaster = ddlProbMaster == null ? currProbMaster : ddlProbMaster;
                ViewBag.CurrentMessErrKeyWord = MessErrKeyWord == null ? currMessErrKeyWord : MessErrKeyWord;

                long recCnt = 0;

                if (null == TermID)
                    param.TERMID = currTID == null ? "" : currTID;
                else
                    param.TERMID = TermID == null ? "" : TermID;

                if ((FrDate == null && currFr == null) && (FrDate == "" && currFr == ""))
                {
                    param.FRDATE = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
                }
                else
                {
                    if ((FrTime == "" && currFrTime == "") || (FrTime == null && currFrTime == null) ||
                        (FrTime == null && currFrTime == "") || (FrTime == "" && currFrTime == null))
                        param.FRDATE = FrDate + " 00:00:00";
                    else
                        param.FRDATE = FrDate + " " + FrTime;
                }

                if ((ToDate == null && currTo == null) && (ToDate == "" && currTo == ""))
                {
                    param.TODATE = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
                }
                else
                {
                    if ((ToTime == "" && currToTime == "") || (ToTime == null && currToTime == null) ||
                        (ToTime == null && currToTime == "") || (ToTime == "" && currToTime == null))
                        param.TODATE = ToDate + " 23:59:59";
                    else
                        param.TODATE = ToDate + " " + ToTime;
                }

                if (ddlProbMaster == null && currProbMaster == null)
                    param.PROBNAME = "All";
                else
                    param.PROBNAME = ddlProbMaster == null ? currProbMaster : ddlProbMaster;

                if (MessErrKeyWord == null && currMessErrKeyWord == null)
                    param.PROBKEYWORD = "";
                else
                    param.PROBKEYWORD = MessErrKeyWord == null ? currMessErrKeyWord : MessErrKeyWord;

                if (null != lstPageSize || null != currPageSize)
                {
                    param.PAGESIZE = String.IsNullOrEmpty(lstPageSize) == true ?
                        int.Parse(currPageSize) : int.Parse(lstPageSize);
                }
                else
                    param.PAGESIZE = 20;

                param.MONTHPERIOD = "";
                param.YEARPERIOD = "";
                param.TRXTYPE = "";

                if (ddlProbMaster != null && ddlProbMaster != "")
                {
                    recordset = GetErrorTermDeviceEJLog_Database(param, 1, 0);
                }

                if (MessErrKeyWord != null && MessErrKeyWord != "")
                {
                    recordset = GetErrorTermDeviceKWEJLog_Database(param, 1, 0);
                    ViewBag.CurrentProbMaster = "All";
                }


                //else
                //{ recordset = logicLogSeek.GetErrorTermDeviceEJLog(param, 1, 0); }

                if (null == recordset || recordset.Count <= 0)
                {
                    ViewBag.NoData = "true";

                }

                else
                {
                    recCnt = recordset.Count;
                    ejLog_dataList = recordset;
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
            { }
            return View(recordset.ToPagedList(pageNum, (int)param.PAGESIZE));
        }

        //public List<ej_trandeviceprob> GetErrorTermDeviceKWEJLog(ej_trandada_seek model, int startRowIndex, int maximumRows)
        //{
        //    List<ej_trandeviceprob> recordset = null;

        //    recordset = GetErrorTermDeviceKWEJLog_Database(model, GetPageIndex(startRowIndex, maximumRows), maximumRows);

        //    return recordset;
        //}

        private int GetPageIndex(int startRowIndex, int maximumRows)
        {
            if (maximumRows <= 0)
                return 0;
            else
                return (int)Math.Floor((double)startRowIndex / (double)maximumRows);
        }

        private List<ej_trandeviceprob> GetErrorTermDeviceEJLogCollectionFromReader(IDataReader reader)
        {
            int _seqNo = 1;
            List<ej_trandeviceprob> recordlst = new List<ej_trandeviceprob>();
            while (reader.Read())
            {
                recordlst.Add(GetErrorTermDeviceEJLogFromReader(reader, _seqNo));
                _seqNo++;
            }

            return recordlst;
        }

        //public  List<ej_trandeviceprob> GetErrorTermDeviceEJLog(ej_trandada_seek model, int startRowIndex, int maximumRows)
        //{
        //    List<ej_trandeviceprob> recordset = null;

        //    recordset = GetErrorTermDeviceEJLog_Database(model, GetPageIndex(startRowIndex, maximumRows), maximumRows);

        //    return recordset;
        //}

        public List<ej_trandeviceprob> GetErrorTermDeviceEJLog_Database(ej_trandada_seek model, int pageIndex, int pageSize)
        {
            try
            {
                using (MySqlConnection cn = new MySqlConnection("server=10.98.14.12;Port=3308;User Id=root;database=gsb_logview;password=P@ssw0rd;CharSet=utf8;"))
                {
                    MySqlCommand cmd = new MySqlCommand("GenDeviceProblemError", cn);

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("?pStratDate", model.FRDATE));
                    cmd.Parameters.Add(new MySqlParameter("?pEndDate", model.TODATE));
                    cmd.Parameters.Add(new MySqlParameter("?pTerminalID", model.TERMID));
                    cmd.Parameters.Add(new MySqlParameter("?pProbMaster", model.PROBNAME));

                    cn.Open();
                    return GetErrorTermDeviceEJLogCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                string err = ex.Message;
                return null;
            }
        }

        private ej_trandeviceprob GetErrorTermDeviceEJLogFromReader(IDataReader reader, int pSeqNo)
        {
            ej_trandeviceprob record = new ej_trandeviceprob();

            record.Seqno = pSeqNo;
            record.TerminalID = reader["terminalid"].ToString();
            record.BranchName = reader["branchname"].ToString();
            record.Location = reader["locationbranch"].ToString();
            record.ProbName = reader["probname"].ToString();
            record.Remark = reader["remark"].ToString();
            record.TransactionDate = Convert.ToDateTime(reader["trxdatetime"]);

            return record;
        }
        private List<ej_trandeviceprob> GetErrorTermDeviceKWEJLog_Database(ej_trandada_seek model, int pageIndex, int pageSize)
        {
            try
            {
                using (MySqlConnection cn = new MySqlConnection("server=10.98.14.12;Port=3308;User Id=root;database=gsb_logview;password=P@ssw0rd;CharSet=utf8;"))
                {
                    MySqlCommand cmd = new MySqlCommand("GenDeviceProblemErrorKW", cn);

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("?pStratDate", model.FRDATE));
                    cmd.Parameters.Add(new MySqlParameter("?pEndDate", model.TODATE));
                    cmd.Parameters.Add(new MySqlParameter("?pTerminalID", model.TERMID));
                    cmd.Parameters.Add(new MySqlParameter("?pProbKeyWord", model.PROBKEYWORD));

                    cn.Open();
                    return GetErrorTermDeviceEJLogCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                string err = ex.Message;
                return null;
            }
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

        private List<ProblemMaster> GetMasterSysErrorWord()
        {
            List<ProblemMaster> _result = new List<ProblemMaster>();
            DataTable _dt = new DataTable();
            DBService _objDB = new DBService();
            try
            {

                _dt = _objDB.GetAllMasterProblem();
                foreach (DataRow _dr in _dt.Rows)
                {
                    ProblemMaster obj = new ProblemMaster();
                    obj.ProblemCode = _dr["probcode"].ToString();
                    obj.ProblemName = _dr["probname"].ToString();
                    _result.Add(obj);
                }
            }
            catch (Exception ex)
            {

            }
            return _result;
        }



        [HttpPost]
        public ActionResult EJAddTermProb_ExportExc()
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
                if (ejLog_dataList == null || ejLog_dataList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory + @"\wwwroot";
                ExcelUtilities obj = new ExcelUtilities(param);


                // Session["PrefixRep"] = "EJAddTran";

                string folder_name = Path.Combine(strPath + @"\TermProbExcel", "tempfiles");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                obj.GenExcelFileDeviceTermPorb(ejLog_dataList);



                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;



                fname = "DeviceTermProbExcel_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");            

                strPathDesc = strPath + "\\TermProbExcel\\excelfile\\" + fname + ".xlsx";


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
            //try
            //{




            fname = "DeviceTermProbExcel_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

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

            tempPath = Path.GetFullPath(Environment.CurrentDirectory + "\\wwwroot\\TermProbExcel\\excelfile\\" + fname);


            Console.WriteLine("Boom : " + tempPath);

            if (rpttype.ToLower().EndsWith("s") == true)
                return File(tempPath + "xml", "application/vnd.openxmlformats-officedocument.spreadsheetml", fname);
            else if (rpttype.ToLower().EndsWith("f") == true)
                return File(tempPath + "xml", "application/pdf", fname);
            else  //(rpttype.ToLower().EndsWith("v") == true)
                return PhysicalFile(tempPath, "application/vnd.ms-excel", fname);

            //new FileContentResult(System.IO.File.ReadAllBytes(tempPath), "application/vnd.ms-excel");

            Console.WriteLine("3Fname : " + fname);

            //}
            //catch (Exception ex)
            //{
            //    ViewBag.ErrorMsg = "Download Method : " + ex.Message;
            //    return Json(new { success = false, fname });
            //}
        }










    }

}
