using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PagedList;
using SLA_Management.Data.AuditReport;
using SLA_Management.Data.ExcelUtilitie;
using SLA_Management.Data.TermProb;
using SLA_Management.Models.OperationModel;
using SLA_Management.Models.ReportModel;
using SLA_Management.Models.TermProbModel;
using System.Globalization;

namespace SLA_Management.Controllers
{
    public class AuditingReportController : Controller
    {

        #region Local Variable
        private CultureInfo _cultureEnInfo = new CultureInfo("en-US");
        private static List<fv_system_users> fv_system_users_dataList = new List<fv_system_users>();
        private static ej_trandada_seek param = new ej_trandada_seek();
        private IConfiguration _myConfiguration;
        private DBService_AuditReportcs dBService;


        #endregion

        #region Constructor

        public AuditingReportController(IConfiguration myConfiguration)
        {

            _myConfiguration = myConfiguration;
            dBService = new DBService_AuditReportcs(_myConfiguration);

        }

        #endregion


        public IActionResult AuditingReportAction(string cmdButton,string status,string sortBy,string systemType, string lstPageSize, string currPageSize, int? page, string maxRows)
        {


            List<fv_system_users> recordset = new List<fv_system_users>();
            List<fv_system_users> audit_Info_Records = new List<fv_system_users>();
            int pageNum = 1;
            try
            {
                if (cmdButton == "Clear")
                    return RedirectToAction("AuditingReportAction");

                if (sortBy == null) sortBy = "-";

                switch (systemType)
                {
                    case "SECOne":
                        audit_Info_Records = dBService.GetAuditReportcsSECOne(sortBy);
                        break;
                    case "Feelview":
                        audit_Info_Records = dBService.GetAuditReportcsFeelview(sortBy);
                        break;
                    default: 
                        audit_Info_Records = new List<fv_system_users>();
                        break;
                }

                
                int countDay = 0;
                foreach (var record in audit_Info_Records)
                {
                    TimeSpan difference = DateTime.Now - record.LastLoginDateTime;

                    countDay = difference.Days;

                    if (countDay > 90)
                    {
                        record.Status = "Warning";
                    }
                    else if (countDay <= 90)
                    {
                        record.Status = "-";
                    }
                }

                if(status != null && status != "All")
                {
                    audit_Info_Records = audit_Info_Records.Where(obj => obj.Status == status).ToList();
                }

                recordset = audit_Info_Records;

                fv_system_users_dataList = recordset;


                ViewBag.CurrentPageSize = (lstPageSize ?? currPageSize);


                if (null != lstPageSize || null != currPageSize)
                {
                    param.PAGESIZE = String.IsNullOrEmpty(lstPageSize) == true ?
                        int.Parse(currPageSize) : int.Parse(lstPageSize);
                }
                else
                    param.PAGESIZE = 20;

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



        #region Excel

        [HttpPost]
        public ActionResult AuditReport_ExportExc()
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

                if (fv_system_users_dataList == null || fv_system_users_dataList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;
                ExcelUtilities_AuditReport obj = new ExcelUtilities_AuditReport();


                // Session["PrefixRep"] = "EJAddTran";

                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderInputTemplateAuditReport_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                obj.GenExcelFileUserDetailReport(fv_system_users_dataList);



                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;

                

                fname = "AuditReportExcel_" + fv_system_users_dataList[0].System + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

                strPathDesc = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderAuditReport_Excel") + fname + ".xlsx";


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




                fname = "AuditReportExcel_" + fv_system_users_dataList[0].System + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

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

                tempPath = Path.GetFullPath(Environment.CurrentDirectory + _myConfiguration.GetValue<string>("Collection_path:FolderAuditReport_Excel") + fname);




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
