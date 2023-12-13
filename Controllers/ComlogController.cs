using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using SLA_Management.Commons;
using SLA_Management.Models;
using SLA_Management.Models.COMLogModel;
using System.Text;

namespace SLA_Management.Controllers
{
    public class ComlogController : Controller
    {

        private static string ipFileserver { get; set; }
        private static int portFileserver { get; set; }
        private static string usernameFileserver { get; set; }
        private static string passwordFileserver { get; set; }
        private static string partLinuxUploadFileserver { get; set; }
        private static string SlaSqlServer { get; set; }
        private static CheckFileInFileServerNew dataErrorLog  { get; set; }
        public static List<InsertListFileComLog> insertFileCOMLog_temp { get; set; }



        private Microsoft.AspNetCore.Hosting.IHostingEnvironment hostEnvironment { get; set; }
        private IConfiguration _myConfiguration { get; set; }

        public ComlogController(Microsoft.AspNetCore.Hosting.IHostingEnvironment hostEnvironment, IConfiguration configuration)
        {
            this.hostEnvironment = hostEnvironment;
            _myConfiguration = configuration;
            SlaSqlServer = _myConfiguration.GetValue<string>("ConnectionStrings:DefaultConnection");
            ipFileserver = _myConfiguration.GetValue<string>("FileServer:IP");
            portFileserver = _myConfiguration.GetValue<int>("FileServer:Port");
            usernameFileserver = _myConfiguration.GetValue<string>("FileServer:Username");
            passwordFileserver = _myConfiguration.GetValue<string>("FileServer:Password");
            partLinuxUploadFileserver = _myConfiguration.GetValue<string>("FileServer:partLinuxUploadFileComlog");
            dataErrorLog = new CheckFileInFileServerNew(ipFileserver, portFileserver, usernameFileserver, passwordFileserver, partLinuxUploadFileserver, SlaSqlServer);
        }

        [HttpGet]
        public IActionResult Index(P_Search filter, int? maxRows)
        {

            int pageSize = 20;
            insertFileCOMLog_temp = new List<InsertListFileComLog>();
            pageSize = maxRows.HasValue ? maxRows.Value : 20;

            if (filter.forDateTime.HasValue && filter.toDateTime.HasValue)
            {
                if (filter.forDateTime <= filter.toDateTime)
                {
                    if ((DateTime)filter.toDateTime > DateTime.Now)
                    {
                        
                        filter.toDateTime = SetTime(DateTime.Now, 23, 59, 59);
                    }

                    insertFileCOMLog_temp = dataErrorLog.GetListFileComLog(filter.term_id, (DateTime)filter.forDateTime, (DateTime)filter.toDateTime);
                }
            }

            ViewBag.CurrentTermID = dataErrorLog.termIds;
            ViewBag.pageSize = pageSize;
            ViewBag.ExportDataFile = filter;
            ViewBag.TERM_ID = filter.term_id;
            ViewBag.CurrentFr = filter.forDateTime.HasValue ? filter.forDateTime : null;
            ViewBag.CurrentTo = filter.toDateTime.HasValue ? filter.toDateTime : null;
            //return View(insertFileCOMLog_temp.ToPagedList(pageIndex, pageSize));
            return View(insertFileCOMLog_temp);
        }

        private DateTime SetTime(DateTime date,int hour, int minute, int second)
        {
            
            return new DateTime(date.Year, date.Month,date.Day, hour, minute, second);
        }

        [HttpPost]
        public IActionResult UploadFile(string uplodeTerminal_ID, List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);

            string pathApp = hostEnvironment.WebRootPath;
            List<UploadFileFrom> filesUpload = new List<UploadFileFrom>();
            if (files != null && uplodeTerminal_ID != "")
            {
                foreach (var item in files)
                {
                    string _FileName = Path.GetFileName(item.FileName);

                    if (item.FileName.Length > 3 && item.FileName.Substring(0, 3) == "COM")
                    {
                        var filePath = Path.GetTempFileName();
                        Guid myuuid = Guid.NewGuid();
                        string myuuidAsString = myuuid.ToString();
                        string pathDowload = Path.Combine(pathApp, "UploadFile");
                        string targetFileSave = Path.Combine(pathDowload, myuuidAsString + "_" + uplodeTerminal_ID + "_" + _FileName);
                        if (!Directory.Exists(pathDowload))
                        {
                            Directory.CreateDirectory(pathDowload);
                        }
                        using (var stream = System.IO.File.Create(targetFileSave))
                        {
                            item.CopyTo(stream);
                            filesUpload.Add(new UploadFileFrom(targetFileSave, partLinuxUploadFileserver + uplodeTerminal_ID + "/" + _FileName));
                        }
                    }
                }
            }
            SendFileToServer sendFileToServer = new SendFileToServer(ipFileserver, usernameFileserver, passwordFileserver, portFileserver);
            sendFileToServer.SendAll(filesUpload.ToArray());
            foreach (var file in filesUpload)
            {
                if (System.IO.File.Exists(file.targetFile))
                {
                    System.IO.File.Delete(file.targetFile);
                }
            }
            return Json(new { count = files.Count, size = size });
        }

        [HttpPost]
        public IActionResult ExportRecordtoExcel()
        {
            //add test data
            List<InsertListFileComLog> obj = new List<InsertListFileComLog>();

            if (insertFileCOMLog_temp != null)
            {
                obj = insertFileCOMLog_temp;
            }
              
            var str =ModelToBit(obj);

            DateTime dateNow = DateTime.Now;
            string fileName = "ComlogManagement" + dateNow.Year.ToString("0000") + dateNow.Month.ToString("00") + dateNow.Day.ToString("00");





            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                // Write header row
                worksheet.Cells[1, 1].Value = "No.";
                worksheet.Cells[1, 2].Value = "Terminal ID";
                worksheet.Cells[1, 3].Value = "Serial No.";
                worksheet.Cells[1, 4].Value = "Terminal Name";
                worksheet.Cells[1, 5].Value = "Com Log";

                worksheet.Cells[1, 6].Value = "File Server";
                worksheet.Cells[1, 7].Value = "Status File";
                worksheet.Cells[1, 8].Value = "Rows Record";

                // Write data rows
                var rowIndex = 2;
                
                foreach (var item in insertFileCOMLog_temp)
                {
                    worksheet.Cells[rowIndex, 1].Value = rowIndex - 1;
                    worksheet.Cells[rowIndex, 2].Value = item.Term_ID;
                    worksheet.Cells[rowIndex, 3].Value = item.SerialNo;
                    worksheet.Cells[rowIndex, 4].Value = item.TerminalName;

                    worksheet.Cells[rowIndex, 5].Value = item.ComLog;
                    worksheet.Cells[rowIndex, 6].Value = item.FileServer;
                    worksheet.Cells[rowIndex, 7].Value = item.StatusFile;
                    worksheet.Cells[rowIndex, 8].Value = item.TOTAL_RECORD;
                    
                    rowIndex++;
                }

                // Auto fit columns for better readability
                worksheet.Cells.AutoFitColumns();

                // Convert the Excel package to a byte array
                var fileContents = package.GetAsByteArray();

                // Set the content type and file name for the response
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName+".xlsx");
            }
            




        }

        private static StringBuilder ModelToBit(List<InsertListFileComLog> obj)
        {
            StringBuilder str = new StringBuilder();
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "No."));
            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "Terminal ID"));
            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "Serial No."));
            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "Terminal Name"));
            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "Com Log"));

            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "File Server"));
            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "Status File"));
            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "Rows Record"));

            str.Append("</tr>");
            int count = 1;
            if (obj != null)
            {
                foreach (var val in obj)
                {
                    str.Append("<tr>");
                    str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + count + "</font></td>");
                    str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + val.Term_ID + "</font></td>");
                    str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + val.SerialNo + "</font></td>");
                    str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + val.TerminalName + "</font></td>");
                    str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + val.ComLog + "</font></td>");
                    str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + val.FileServer + "</font></td>");
                    str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + val.StatusFile + "</font></td>");
                    str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + val.TOTAL_RECORD + "</font></td>");
                    str.Append("</tr>");
                    count++;
                }
            }
            else
            {
                str.Append("<tr> No Data ");
                str.Append("</tr>");
            }
            str.Append("</table>");
            return str;
        }
    }
}
