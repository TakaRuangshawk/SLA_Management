using Microsoft.AspNetCore.Mvc;
using SLA_Management.Commons;
using SLA_Management.Models;

namespace SLA_Management.Controllers
{
    public class ComlogController : Controller
    {
        public static List<InsertListFileComLog> insertFileCOMLog_temp { get; set; }
        public IActionResult Index()
        {
            string IP = "10.98.10.31";
            int port = 22;
            string username = "root";
            string password = "P@ssw0rd";
            string partLinuxUploadFile = "/opt/fileserverGSB/COMLog/";
            string sqlServer = "Data Source=10.98.14.13;Initial Catalog=SLADB;Persist Security Info=True;User ID=sa;Password=P@ssw0rd;";
            CheckFileInFileServer dataErrorLog = new CheckFileInFileServer(IP, port, username, password, partLinuxUploadFile, sqlServer);
            insertFileCOMLog_temp = dataErrorLog.GetListFileComLog("",DateTime.Now.AddDays(-1), DateTime.Now);
            ViewBag.CurrentTermID = dataErrorLog.termIds.ToList();
            return View();
        }
    }
}
