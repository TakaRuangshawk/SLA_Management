﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using PagedList;
using SLA_Management.Commons;
using SLA_Management.Data;
using SLA_Management.Data.ExcelUtilitie;
using SLA_Management.Data.TermProb;
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
        private SqlCommand com = new SqlCommand();
        private ConnectSQL_Server con;
        private static List<ej_trandeviceprob> ejLog_dataList = new List<ej_trandeviceprob>();
        private static ej_trandada_seek param = new ej_trandada_seek();
        private IConfiguration _myConfiguration;
        private DBService_TermProb dBService;

        #endregion

        #region Constructor

        public EJAddTranProbTermController(IConfiguration myConfiguration)
        {

            _myConfiguration = myConfiguration;
            dBService = new DBService_TermProb(_myConfiguration);
        }

        #endregion

        #region Action page

        public IActionResult EJAddTranProbTermAction(string cmdButton, string TermID, string FrDate, string ToDate, string FrTime, string ToTime
        , string currTID, string currFr, string currTo, string currFrTime, string currToTime, string lstPageSize
        , string ddlProbMaster, string currProbMaster, string MessErrKeyWord, string currMessErrKeyWord
        , string currPageSize, int? page, string maxRows, string KeyWordList)
        {

            List<ej_trandeviceprob> recordset = new List<ej_trandeviceprob>();
            List<ProblemMaster> ProdMasData = new List<ProblemMaster>();
            List<string> terminalNames = new List<string>();
           

            string[] strErrorWordSeparate = _myConfiguration.GetValue<string>("KeyWordSeparate").ToUpper().Split(',');

            ejLog_dataList.Clear();

            DataTable terminalDBTable = dBService.GetClientFromDB();
            for (int i = 0; i < terminalDBTable.Rows.Count; i++)
            {
                terminalNames.Add(terminalDBTable.Rows[i]["terminalid"].ToString().Replace(".", ""));
            }




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
                    ViewBag.ConnectDB = "true";
                }
                else
                {
                    ViewBag.ConnectDB = "false";
                }


                if (ProdMasData.Count > 0)
                {
                    ViewBag.ProbMaster = "";
                    foreach (ProblemMaster obj in ProdMasData)
                    {
                        if (ViewBag.ProbMaster == "")
                        {
                            if (obj.ProblemName.Length > 65)
                            {
                                ViewBag.ProbMaster = obj.ProblemCode + "$" + obj.ProblemName;
                            }
                            else
                            {
                                ViewBag.ProbMaster = obj.ProblemCode + "$" + obj.ProblemName + " : " + obj.Memo;
                            }
                        }
                        else
                        {
                            if (obj.ProblemName.Length > 65)
                            {
                                ViewBag.ProbMaster += "|" + obj.ProblemCode + "$" + obj.ProblemName;
                            }
                            else
                            {
                                ViewBag.ProbMaster += "|" + obj.ProblemCode + "$" + obj.ProblemName + " : " + obj.Memo;
                            }
                        }

                    }
                }

                ViewBag.CurrentTID = terminalNames;
                ViewBag.TermID = TermID;
                ViewBag.CurrentFr = (FrDate ?? currFr);
                ViewBag.CurrentTo = (ToDate ?? currTo);
                ViewBag.CurrentPageSize = (lstPageSize ?? currPageSize);
                //ViewBag.CurrentProbMaster = ddlProbMaster == null ? currProbMaster : ddlProbMaster;
                ViewBag.CurrentProbMaster = KeyWordList;
                ViewBag.CurrentMessErrKeyWord = MessErrKeyWord == null ? currMessErrKeyWord : MessErrKeyWord;
                #endregion


                #region Set param

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
                    if (ddlProbMaster != null || TermID != null)
                    {
                        recordset = GetErrorTermDeviceEJLog_DatabaseAll(param, strErrorWordSeparate);
                    }
                }
                #endregion


                #region Set page
                long recCnt = 0;

                if (String.IsNullOrEmpty(maxRows))
                    ViewBag.maxRows = "5";
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

            }
            catch (Exception ex)
            {

            }
            return View(recordset.ToPagedList(pageNum, (int)param.PAGESIZE));
        }
        #endregion


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

                obj.GenExcelFileDeviceTermPorb(ejLog_dataList);



                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;



                fname = "DeviceTermProbExcel_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

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


        #endregion







    }

}
