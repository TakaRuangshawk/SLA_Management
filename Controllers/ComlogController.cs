using Azure;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;
using PagedList;
using SLA_Management.Commons;
using SLA_Management.Models;
using SLA_Management.Models.COMLogModel;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace SLA_Management.Controllers
{
    public class ComlogController : Controller
    {

        private static string ip  { get; set; }
        private static int port  { get; set; }
        private static string username { get; set; }
        private static string password { get; set; }
        private static string partLinuxUploadFile { get; set; }
        private static string sqlServer { get; set; }
        private static CheckFileInFileServer dataErrorLog { get; set; }
        public static List<InsertListFileComLog> insertFileCOMLog_temp { get; set; }

      

        private Microsoft.AspNetCore.Hosting.IHostingEnvironment hostEnvironment { get; set; }
        private IConfiguration _myConfiguration { get; set; }

        public ComlogController(Microsoft.AspNetCore.Hosting.IHostingEnvironment hostEnvironment, IConfiguration configuration)
        {
            this.hostEnvironment = hostEnvironment;
            _myConfiguration = configuration;
            sqlServer = _myConfiguration.GetValue<string>("ConnectionStrings:DefaultConnection");
            ip = _myConfiguration.GetValue<string>("FileServer:IP");
            port = _myConfiguration.GetValue<int>("FileServer:Port");
            username = _myConfiguration.GetValue<string>("FileServer:Username");
            password = _myConfiguration.GetValue<string>("FileServer:Password");
            partLinuxUploadFile = _myConfiguration.GetValue<string>("FileServer:partLinuxUploadFileComlog");
            dataErrorLog = new CheckFileInFileServer(ip, port, username, password, partLinuxUploadFile, sqlServer);
        }

        [HttpGet]
        public IActionResult Index(P_Search filter , int? maxRows)
        {
            
            int pageSize = 20;
            insertFileCOMLog_temp = new List<InsertListFileComLog>();
            pageSize = maxRows.HasValue ? maxRows.Value : 20;
            
            if (filter.forDateTime.HasValue && filter.toDateTime.HasValue)
            {
                if (filter.forDateTime <= filter.toDateTime)
                {
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
                        string targetDirectory = Path.Combine(pathApp, "UploadFile");

                        if (!Directory.Exists(targetDirectory))
                        {
                            Directory.CreateDirectory(targetDirectory);
                        }


                        string targetFileSave = Path.Combine(targetDirectory, myuuidAsString + "_" + uplodeTerminal_ID + "_" + _FileName);
                        using (var stream = System.IO.File.Create(targetFileSave))
                        {
                            item.CopyTo(stream);
                            filesUpload.Add(new UploadFileFrom(targetFileSave, partLinuxUploadFile + uplodeTerminal_ID + "/" + _FileName));
                        }
                    }
                }
            }
            SendFileToServer sendFileToServer = new SendFileToServer(ip, username, password, port);
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
                obj =insertFileCOMLog_temp;
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
            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "No.") );
            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "Term ID"));
            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "ComLog"));
            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "File Server"));
            str.Append(string.Format("<td><b><font face=Arial Narrow size=3>{0}</font></b></td>", "Total record"));
            
            str.Append("</tr>");
            int count = 1;
            if (obj != null )
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
