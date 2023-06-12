using Microsoft.AspNetCore.Mvc;
using SLA_Management.Commons;
using SLA_Management.Models;
using System.Text;

namespace SLA_Management.Controllers
{
    public class ComlogNewController : Controller
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

        public ComlogNewController(Microsoft.AspNetCore.Hosting.IHostingEnvironment hostEnvironment, IConfiguration configuration)
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

                    insertFileCOMLog_temp = dataErrorLog.GetListFileComLog(filter.term_id + "", (DateTime)filter.forDateTime, (DateTime)filter.toDateTime);
                }
            }

            ViewBag.CurrentTermID = dataErrorLog.termIds.ToList();
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
                        string targetFileSave = Path.Combine(pathApp, "UploadFile", myuuidAsString + "_" + uplodeTerminal_ID + "_" + _FileName);
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
            /*else
            {
                if (param.forDateTime.HasValue && param.toDateTime.HasValue)
                {
                    if (param.forDateTime <= param.toDateTime)
                    {
                        obj = dataErrorLog.GetListFileComLog(param.term_id + "", (DateTime)param.forDateTime, (DateTime)param.toDateTime);
                    }
                }
            }*/

            //using System.Text;  
            StringBuilder str = new StringBuilder();
            str.Append("<table border=`" + "1px" + "`b>");
            str.Append("<tr>");
            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "No."));
            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "Term ID"));
            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "ComLog"));
            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "File Server"));
            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "Total record"));

            str.Append("</tr>");
            int count = 1;
            if (obj != null)
            {
                foreach (var val in obj)
                {
                    str.Append("<tr>");
                    str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + count + "</font></td>");
                    str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + val.Term_ID + "</font></td>");
                    str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + val.ComLog + "</font></td>");
                    str.Append("<td><font face=Arial Narrow size=" + "14px" + ">" + val.FileServer + "</font></td>");
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
            HttpContext.Response.Headers.Add("content-disposition", "attachment; filename=Information" + DateTime.Now.Year.ToString() + ".xlsx");
            this.Response.ContentType = "application/vnd.ms-excel";
            byte[] temp = System.Text.Encoding.UTF8.GetBytes(str.ToString());
            return File(temp, "application/vnd.ms-excel");
        }
    }
}
