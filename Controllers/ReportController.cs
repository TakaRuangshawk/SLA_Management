using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SLA_Management.Data;
using SLA_Management.Data.ExcelUtilitie;

namespace SLA_Management.Controllers
{
    public class ReportController : Controller
    {
        private IConfiguration _myConfiguration;
        private static ConnectMySQL db_fv;
        static List<WhitelistReportModel> whitelistreport_dataList = new  List<WhitelistReportModel>();
        public ReportController(IConfiguration myConfiguration)
        {

            _myConfiguration = myConfiguration;
            db_fv = new ConnectMySQL(myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection"));
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult WhitelistReport()
        {
            ViewBag.maxRows = 50;
            ViewBag.SelectedMonth = DateTime.Now.AddMonths(-1).ToString("yyyy-MM");
            return View();
        }
        public IActionResult WhiteListFetchData(string row, string page, string search, string month)
        {
            int _page;
            string filterquery = string.Empty;
            string fromdate = string.Empty;
            string todate = string.Empty;
            if (page == null || search == "search")
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
            else if (search == "prev")
            {
                _page--;
            }
            int _row;
            if (row == null)
            {
                _row = 50;
            }
            else
            {
                _row = int.Parse(row);
            }

            month = month ?? DateTime.Now.ToString("yyyy-MM");
            DateTime _fromdate = DateTime.Parse(month + "-01");
            DateTime _todate = _fromdate.AddMonths(1).AddDays(-1);
            fromdate = _fromdate.ToString("yyyy-MM-dd");
            todate = _todate.ToString("yyyy-MM-dd");
            if (month != "")
            {
                filterquery += " and WARN_DATE between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' ";
            }
            else
            {
                filterquery += " and WARN_DATE between '" + DateTime.Now.ToString("yyyy-MM") + " 00:00:00' and '" + DateTime.Now.AddMonths(1).AddDays(-1).ToString("yyyy-MM") + " 23:59:59' ";
            }
            List<WhitelistReportModel> jsonData = new List<WhitelistReportModel>();

            using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
            {
                connection.Open();

                // Modify the SQL query to use the 'input' parameter for filtering
                string query = @"SELECT CAST(WARN_DATE AS DATE) AS eventdate, 
                CASE 
                    WHEN warn_type = 93 THEN 'Network access deny'
                    WHEN warn_type = 96 THEN 'Insert the USB disk illegally'
                    WHEN warn_type = 97 THEN 'Illegal to modify the registry'
                    WHEN warn_type = 98 THEN 'File illegally modified'
                    WHEN warn_type = 99 THEN 'Illegal to boot'
                    WHEN warn_type = 102 THEN 'Operating system configuration changes'
                    ELSE 'Unknown'
                END AS warn_type, 
                CASE 
                    WHEN SEVERITY_FLAG = 0 THEN 'Critical'
                    WHEN SEVERITY_FLAG = 1 THEN 'High'
                    WHEN SEVERITY_FLAG = 3 THEN 'Low'
                    ELSE 'Unknown'
                END AS severity_level, 
                COUNT(warn_type) AS total 
                FROM client_warning_record 
                WHERE WARN_DATE IS NOT NULL ";

                query = query.Replace("\r\n", "");

                query += filterquery + " group by CAST(WARN_DATE AS DATE), warn_type, SEVERITY_FLAG order by CAST(WARN_DATE AS DATE)  ";

                MySqlCommand command = new MySqlCommand(query, connection);

                int id_row = 0;
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        id_row += 1;
                        DateTime eventDate = (DateTime)reader["eventdate"];
                        jsonData.Add(new WhitelistReportModel
                        {
                            ID = (id_row).ToString(),
                            eventdate = eventDate.ToString("yyyy-MM-dd"),
                            warn_type = reader["warn_type"].ToString(),
                            severity_level = reader["severity_level"].ToString(),
                            total = reader["total"].ToString()
                        });
                    }
                }
            }
            whitelistreport_dataList = jsonData;
            int pages = (int)Math.Ceiling((double)jsonData.Count() / _row);
            List<WhitelistReportModel> filteredData = RangeFilter(jsonData, _page, _row);


            List<WhitelistPivotModel> pivotData = new List<WhitelistPivotModel>();
            using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
            {
                connection.Open();

                // Modify the SQL query to use the 'input' parameter for filtering
                string query = @" SELECT CASE 
                    WHEN warn_type = 93 THEN 'Network access deny'
                    WHEN warn_type = 96 THEN 'Insert the USB disk illegally'
                    WHEN warn_type = 97 THEN 'Illegal to modify the registry'
                    WHEN warn_type = 98 THEN 'File illegally modified'
                    WHEN warn_type = 99 THEN 'Illegal to boot'
                    WHEN warn_type = 102 THEN 'Operating system configuration changes'
                    ELSE 'Unknown'
                    END AS warn_type,
	                SUM(CASE WHEN SEVERITY_FLAG = 0 THEN 1 ELSE 0 END) AS critical,
                    SUM(CASE WHEN SEVERITY_FLAG = 1 THEN 1 ELSE 0 END) AS high,
                    SUM(CASE WHEN SEVERITY_FLAG = 3 THEN 1 ELSE 0 END) AS low FROM client_warning_record   WHERE WARN_DATE IS NOT NULL ";


                query += filterquery + " group by WARN_TYPE";

                MySqlCommand command = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pivotData.Add(new WhitelistPivotModel
                        {
                            warn_type = reader["warn_type"].ToString(),
                            critical = reader["critical"].ToString(),
                            high = reader["high"].ToString(),
                            low = reader["low"].ToString()
                        });
                    }
                }
            }
            var response = new DataResponse_WhitelistReportModel
            {
                JsonData = filteredData,
                PivotData = pivotData,
                Page = pages,
                currentPage = _page,
                TotalTerminal = jsonData.Count(),
            };
            return Json(response);
        }

        static List<WhitelistReportModel> RangeFilter<WhitelistReportModel>(List<WhitelistReportModel> inputList, int page, int row)
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

        public class WhitelistReportModel
        {
            public string ID { get; set; }
            public string eventdate { get; set; }
            public string warn_type { get; set; }
            public string severity_level { get; set; }
            public string total { get; set; }
        }
        public class WhitelistPivotModel
        {
            public string warn_type { get; set; }
            public string critical { get; set; }
            public string high { get; set; }
            public string low { get; set; }
        }
        public class DataResponse_WhitelistReportModel
        {
            public List<WhitelistReportModel> JsonData { get; set; }
            public List<WhitelistPivotModel> PivotData { get; set; }
            public int Page { get; set; }
            public int currentPage { get; set; }
            public int TotalTerminal { get; set; }
        }
        #region Excel WhitelistReport

        [HttpPost]
        public ActionResult WhitelistReport_ExportExc()
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

                if (whitelistreport_dataList == null || whitelistreport_dataList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;
                ExcelUtilities_WhitelistReport obj = new ExcelUtilities_WhitelistReport();


                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(whitelistreport_dataList);



                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;



                fname = "WhitelistReport_" + DateTime.Now.ToString("yyyyMMdd");

                strPathDesc = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname + ".xlsx";


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
        public ActionResult DownloadExportFile_WhitelistReport(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            string tsDate = "";
            string teDate = "";
            try
            {




                fname = "WhitelistReport_" + DateTime.Now.ToString("yyyyMMdd");

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

                tempPath = Path.GetFullPath(Environment.CurrentDirectory + _myConfiguration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname);




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
