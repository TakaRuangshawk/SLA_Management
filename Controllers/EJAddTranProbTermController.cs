using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Mysqlx;
using PagedList;
using SLA_Management.Commons;
using SLA_Management.Data;
using SLA_Management.Data.ExcelUtilitie;
using SLA_Management.Data.TermProb;
using SLA_Management.Models.OperationModel;
using SLA_Management.Models.TermProbModel;
using SLAManagement.Data;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Globalization;


namespace SLA_Management.Controllers
{
    public class EJAddTranProbTermController : Controller
    {

        #region Local Variable
        private CultureInfo _cultureEnInfo = new CultureInfo("en-US");
        private static List<ej_trandeviceprob> ejLog_dataList = new List<ej_trandeviceprob>();
        private static ej_trandada_seek param = new ej_trandada_seek();
        private IConfiguration _myConfiguration;
        private DBService_TermProb dBService;
        private DBService_TermProb_CDM dBService_CDM;
        private DBService_TermProb_LRM dBService_LRM;


        #endregion

        #region Constructor

        public EJAddTranProbTermController(IConfiguration myConfiguration)
        {

            _myConfiguration = myConfiguration;
            dBService = new DBService_TermProb(_myConfiguration);

            dBService_CDM = new DBService_TermProb_CDM(_myConfiguration); 
            dBService_LRM = new DBService_TermProb_LRM(_myConfiguration);

        }

        #endregion

        #region Action page

        public IActionResult EJAddTranProbTermAction(string cmdButton, string TermID, string FrDate, string ToDate, string FrTime, string ToTime
        , string currTID, string currFr, string currTo, string currFrTime, string currToTime, string lstPageSize
        , string ddlProbMaster, string currProbMaster, string MessErrKeyWord, string currMessErrKeyWord
        , string currPageSize, int? page, string maxRows, string KeyWordList,string terminalType)
        {

            List<ej_trandeviceprob> recordset = new List<ej_trandeviceprob>();
            List<ProblemMaster> ProdMasData = new List<ProblemMaster>();
            List<ProblemMaster> ProdAllMasData = new List<ProblemMaster>();

            


            string[] strErrorWordSeparate = _myConfiguration.GetValue<string>("KeyWordSeparate").ToUpper().Split(',');

            ejLog_dataList.Clear();

            int pageNum = 1;
            try
            {


                if (cmdButton == "Clear")
                    return RedirectToAction("EJAddTranProbTermAction");


                #region Set viewBag and default data
                if (null == TermID && null == FrDate && null == ToDate && null == page)
                {

                    FrDate = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
                    ToDate = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
                    page = 1;
                }
                else
                {
                    // Return temp value back to it own variable
                    FrDate = (FrDate ?? currFr);
                    ToDate = (ToDate ?? currTo);
                    FrTime = (FrTime ?? currFrTime);
                    ToTime = (ToTime ?? currToTime);
                    TermID = (TermID ?? currTID);
                    ddlProbMaster = (ddlProbMaster ?? currProbMaster);
                    MessErrKeyWord = (MessErrKeyWord ?? currMessErrKeyWord);
                }

                if (DBService.CheckDatabase())
                {
                    ProdMasData = dBService.GetMasterSysErrorWord();
                    ProdAllMasData = dBService.GetAllMasterSysErrorWord();
                    List<string> list = ProdAllMasData.Select(p => p.ProbType).Distinct().ToList();

                    ViewBag.probTypeStr = new SelectList(list);  ;

                    ViewBag.ConnectDB = "true";
                }
                else
                {
                    ViewBag.ConnectDB = "false";
                }


                if (ProdMasData.Count > 0)
                {
                    ViewBag.ProbMaster = "";
                    string strTemp = string.Empty;
                    foreach (ProblemMaster obj in ProdMasData)
                    {
                        strTemp = "";
                        if (ViewBag.ProbMaster == "")
                        {
                            //if (obj.ProblemName.Length > 65)
                            //{
                            //    strTemp = obj.ProblemCode + "$" + obj.ProblemName;
                            //}
                            //else
                            //{
                            //    strTemp = obj.ProblemCode + "$" + obj.ProblemName + " : " + obj.Memo;
                            //}

                            strTemp = obj.ProblemCode + "$" + obj.Memo + " : " + obj.ProblemName;

                            //if (strTemp.Length > 65) strTemp = obj.ProblemCode + "$" + obj.ProblemName;

                        }
                        else
                        {
                            //if (obj.ProblemName.Length > 65)
                            //{
                            //    strTemp = "|" + obj.ProblemCode + "$" + obj.ProblemName;
                            //}
                            //else
                            //{
                            //    strTemp = "|" + obj.ProblemCode + "$" + obj.ProblemName + " : " + obj.Memo;


                            //}
                            strTemp = "|" + obj.ProblemCode + "$" + obj.Memo + " : " + obj.ProblemName;
                            //if (strTemp.Length > 65) strTemp = "|" + obj.ProblemCode + "$" + obj.ProblemName;


                        }

                        if (strTemp != null && strTemp != string.Empty)
                            ViewBag.ProbMaster += strTemp;

                    }
                }

                List<Device_info_record> term_2in1 = dBService.GetDeviceInfoFeelview();
                List<Device_info_record> term_cdm = dBService_CDM.GetDeviceInfoFeelview();
                List<Device_info_record> term_lrm = dBService_LRM.GetDeviceInfoFeelview();
                term_2in1.AddRange(term_cdm);
                term_2in1.AddRange(term_lrm);
                ViewBag.CurrentTID = term_2in1;
                ViewBag.TermID = TermID;
                ViewBag.CurrentFr = (FrDate ?? currFr);
                ViewBag.CurrentTo = (ToDate ?? currTo);
                ViewBag.CurrentPageSize = (lstPageSize ?? currPageSize);
                //ViewBag.CurrentProbMaster = ddlProbMaster == null ? currProbMaster : ddlProbMaster;
                ViewBag.CurrentProbMaster = KeyWordList;
                ViewBag.CurrentMessErrKeyWord = MessErrKeyWord == null ? currMessErrKeyWord : MessErrKeyWord;
                #endregion


                #region Set param
                bool chk_date = false;
                if (null == TermID)
                    param.TERMID = currTID == null ? "" : currTID;
                else
                    param.TERMID = TermID == null ? "" : TermID;

                if ((FrDate == null && currFr == null) && (FrDate == "" && currFr == ""))
                {
                    param.FRDATE = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
                    chk_date = false;
                }
                else
                {
                    if ((FrTime == "" && currFrTime == "") || (FrTime == null && currFrTime == null) ||
                        (FrTime == null && currFrTime == "") || (FrTime == "" && currFrTime == null))
                        param.FRDATE = FrDate + " 00:00:00";
                    else
                        param.FRDATE = FrDate + " " + FrTime;
                    chk_date = true;
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

                param.TERMINALTYPE = terminalType ?? "";
                param.MONTHPERIOD = "";
                param.YEARPERIOD = "";
                param.TRXTYPE = "";

                #endregion


                #region KeywordList
                //Console.WriteLine("TermID :" + TermID);
                if (KeyWordList != null)
                {
                    string[] KeyWordListTemp = KeyWordList.Split(",");
                    foreach (string KeyWord in KeyWordListTemp)
                    {
                        //Console.WriteLine(KeyWord);

                        param.PROBNAME = KeyWord;
                        if (ddlProbMaster != null || TermID != null)
                        {

                            recordset.AddRange(GetErrorTermDeviceEJLog_DatabaseAll(param, strErrorWordSeparate));

                        }

                    }
                }
                else
                {
                    if (chk_date)
                    {
                        recordset = GetErrorTermDeviceEJLog_DatabaseAll(param, strErrorWordSeparate);
                    }
                }
                #endregion


                #region Set page
                long recCnt = 0;

                if (String.IsNullOrEmpty(maxRows))
                    ViewBag.maxRows = "50";
                else
                    ViewBag.maxRows = maxRows;


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

                int amountrecordset = recordset.Count();

                if (amountrecordset > 5000)
                {
                    recordset.RemoveRange(5000, amountrecordset - 5000);
                }
                #endregion
                
             


                if (recordset.Count > 0)
                {
                    recordset =  recordset.OrderByDescending(x => x.TransactionDate).ToList();
                }

            }
            catch (Exception)
            {

            }
            return View(recordset.ToPagedList(pageNum, (int)param.PAGESIZE));
        }
        #endregion

        #region Private function
        private List<ej_trandeviceprob> GetErrorTermDeviceEJLog_DatabaseAll(ej_trandada_seek paramTemp, string[] strErrorWordSeparate)
        {
            List<ej_trandeviceprob> ej_Trandeviceprobs = new List<ej_trandeviceprob>();

            try
            {
                if (paramTemp.PROBNAME.Contains("SLA"))
                {
                    ej_Trandeviceprobs.AddRange(dBService.GetErrorTermDeviceEJLog_Database_sla(paramTemp));
                }
                else if (Array.IndexOf(strErrorWordSeparate, paramTemp.PROBNAME) > -1)
                {
                    ej_Trandeviceprobs.AddRange(dBService.GetErrorTermDeviceEJLog_Database_separate(paramTemp));
                }
                else
                {
                    ej_Trandeviceprobs.AddRange(dBService.GetErrorTermDeviceEJLog_Database(paramTemp));
                }
            }
            catch (Exception)
            {

            }


            return ej_Trandeviceprobs;

        }

        #endregion

        #region Excel

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

                string strPath = Environment.CurrentDirectory;
                ExcelUtilities_EJTermProb obj = new ExcelUtilities_EJTermProb(param);


                // Session["PrefixRep"] = "EJAddTran";

                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderInputTemplateTermProb_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                List<Device_info_record> term = dBService_LRM.GetDeviceInfoFeelview();
                 term.AddRange(dBService_CDM.GetDeviceInfoFeelview());
                 term.AddRange(dBService.GetDeviceInfoFeelview());

                foreach (var termAll in term)
                {
                   
                    var ejLog_dataListV2 = ejLog_dataList.FindAll(obj => obj.TerminalID == termAll.TERM_ID);
                    foreach(var ejLog_ in ejLog_dataListV2)
                    {
                        ejLog_.Location = termAll.TERM_LOCATION;
                    }
                                       
                }

                obj.GenExcelFileDeviceTermPorb(ejLog_dataList);



                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;



                fname = "DeviceTermProbExcel_" + DateTime.Now.ToString("yyyyMMdd");

                strPathDesc = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderTermProb_Excel") + fname + ".xlsx";


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




                fname = "DeviceTermProbExcel_" + DateTime.Now.ToString("yyyyMMdd");

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

                tempPath = Path.GetFullPath(Environment.CurrentDirectory + _myConfiguration.GetValue<string>("Collection_path:FolderTermProb_Excel") + fname);




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
        [HttpPost]
        public ActionResult InsertProbMaster(string username, string email, string probCodeStr, string probNameStr, string probTypeStr, string probTermStr,string displayflagStr ,string memo)
        {
            bool result = false;
            string error = "incomplete information";
            string _checkuser = "";
            List<checkuserfeelview> checkruser = GetCheckUserFeelview(username, email);
            foreach (var Data in checkruser)
            {
                if (Data.check == "yes")
                {
                    _checkuser = "yes";
                }
                else
                {
                    _checkuser = "no";
                }

            }

            if (_checkuser == "yes")
            {
                try
                {
                    if (probCodeStr != null && probNameStr != null && probTypeStr != null && probTermStr != null)
                        result = dBService.InsertDataToProbMaster(probCodeStr, probNameStr, probTypeStr, probTermStr,memo,username, displayflagStr);

                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }


                if (result == false)
                    if (dBService.ErrorMessage != null) error = dBService.ErrorMessage;

                return Json(new { result = result, error = error });
            }
            else
            {
                return Json(new { result = result, error = "Your username or Your e-mail is incorrect. " });
            }



        }
        #region check username and email from feelview
        public List<checkuserfeelview> GetCheckUserFeelview(string user, string email)
        {
            string _sql = string.Empty;

            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {

                    _sql = "SELECT CASE WHEN COUNT(*) > 0 THEN 'yes' ELSE 'no' END AS _check FROM fv_system_users WHERE   ";
                    _sql += " ACCOUNT = '" + user + "' AND EMAIL = '" + email + "'; ";
                    cn.Open();

                    MySqlCommand cmd = new MySqlCommand(_sql, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                    return GetCheckUserFeelviewCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                return null;
            }
        }
        protected virtual List<checkuserfeelview> GetCheckUserFeelviewCollectionFromReader(IDataReader reader)
        {
            List<checkuserfeelview> recordlst = new List<checkuserfeelview>();
            while (reader.Read())
            {
                recordlst.Add(GetCheckUserFeelviewFromReader(reader));
            }
            return recordlst;
        }
        protected virtual checkuserfeelview GetCheckUserFeelviewFromReader(IDataReader reader)
        {
            checkuserfeelview record = new checkuserfeelview();

            record.check = reader["_check"].ToString();
            return record;
        }
        public class checkuserfeelview
        {
            public string check { get; set; }
        }
        #endregion

        #endregion

    }

}
