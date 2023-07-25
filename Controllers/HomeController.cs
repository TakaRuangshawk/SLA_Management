using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SLA_Management.Data.TermProbDB;
using SLA_Management.Models;
using SLA_Management.Models.TermProbModel;
using SLAManagement.Data;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

namespace SLA_Management.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _myConfiguration;
        private static List<homeshowstatus> recordset_homeshowstatus = new List<homeshowstatus>();
        private static List<comlogrecord> recordset_comlogrecord = new List<comlogrecord>();
        private DBService dBService;
        SqlCommand com = new SqlCommand();
        ConnectSQL_Server con;

        public HomeController(IConfiguration myConfiguration)
        {

            _myConfiguration = myConfiguration;
            dBService = new DBService(_myConfiguration);
            con = new ConnectSQL_Server(_myConfiguration["ConnectionStrings:DefaultConnection"]);
        }
        public IActionResult Index()
        {
            recordset_homeshowstatus = GetHomeStatus();
            if(recordset_homeshowstatus != null)
            {
                foreach (var Data in recordset_homeshowstatus)
                {
                    ViewBag.onlineATM = Data.onlineATM;
                    ViewBag.onlineADM = Data.onlineADM;
                    ViewBag.offlineATM = Data.offlineATM;
                    ViewBag.offlineADM = Data.offlineADM;
                    ViewBag.onlineTotal = (Convert.ToInt32(Data.onlineATM) + Convert.ToInt32(Data.onlineADM)).ToString();
                    ViewBag.offlineTotal = (Convert.ToInt32(Data.offlineATM) + Convert.ToInt32(Data.offlineADM)).ToString();
                }
            }
            else
            {
                ViewBag.onlineATM = "-";
                ViewBag.onlineADM = "-";
                ViewBag.offlineATM = "-";
                ViewBag.offlineADM = "-";
            }
            recordset_comlogrecord = GetComlogRecordFromSqlServer();
            if(recordset_comlogrecord != null)
            {
                foreach (var Data in recordset_comlogrecord)
                {
                    if(Data.comlogADM != "" || Data.comlogATM != "")
                    {
                        ViewBag.comlogADM = Data.comlogADM;
                        ViewBag.comlogATM = Data.comlogATM;
                        ViewBag.comlogTotal = (Convert.ToInt32(Data.comlogADM) + Convert.ToInt32(Data.comlogATM)).ToString();
                    }
                    else
                    {
                        ViewBag.comlogATM = "-";
                        ViewBag.comlogADM = "-";
                        ViewBag.comlogTotal = "-";
                    }
                    
                }
            }
            else
            {
                ViewBag.comlogATM = "-";
                ViewBag.comlogADM = "-";
            }
            ViewBag.DateNow = DateTime.Now.ToString("dd - MM - yyyy");
            return View();
        }
        public List<comlogrecord> GetComlogRecordFromSqlServer()
        {
            List<comlogrecord> dataList = new List<comlogrecord>();

            
            string sqlQuery = " SELECT COUNT(CASE WHEN ERROR IS NULL  AND TERM_ID LIKE '%G262%' THEN 1 END) AS ComlogADM,COUNT(CASE WHEN ERROR IS NULL  AND TERM_ID like '%G165%' THEN 1 END) AS ComlogATM ";
            sqlQuery += " FROM comlog_record where COMLOGDATE between '" + DateTime.Now.Date.AddDays(-1).ToString("yyyy-MM-dd") + " 00:00:00' AND '" + DateTime.Now.Date.AddDays(-1).ToString("yyyy-MM-dd") + " 23:59:59'";
            try
            {
                using (SqlConnection connection = new SqlConnection(_myConfiguration.GetValue<string>("ConnectionStrings:DefaultConnection")))
                {

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dataList.Add(GetComlogRecordFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }
            

            return dataList;
        }
        protected virtual comlogrecord GetComlogRecordFromReader(IDataReader reader)
        {
            comlogrecord record = new comlogrecord();

            record.comlogADM = reader["comlogADM"].ToString();
            record.comlogATM = reader["comlogATM"].ToString();
            return record;
        }
        public List<homeshowstatus> GetHomeStatus()
        {
            string _sql = string.Empty;

            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection")))
                {

                    _sql = "SELECT COUNT(CASE WHEN(SUBSTRING(TERM_ID, '14', 4) = '165'and DEVICE_STATUS_EVENT_ID != 'E1005' and DEVICE_STATUS_EVENT_ID != 'E1156' and DEVICE_STATUS_EVENT_ID != 'E1006' and DEVICE_STATUS_EVENT_ID != 'E1036') THEN 1 END) AS _onlineATM,  ";
                    _sql += " COUNT(CASE WHEN(SUBSTRING(TERM_ID, '14', 4) = '165'and (DEVICE_STATUS_EVENT_ID = 'E1005' or DEVICE_STATUS_EVENT_ID = 'E1156' or DEVICE_STATUS_EVENT_ID = 'E1006' or DEVICE_STATUS_EVENT_ID = 'E1036')) THEN 1 END) AS _offlineATM,  ";
                    _sql += " COUNT(CASE WHEN(SUBSTRING(TERM_ID, '14', 4) = '262'and DEVICE_STATUS_EVENT_ID != 'E1005' and DEVICE_STATUS_EVENT_ID != 'E1156' and DEVICE_STATUS_EVENT_ID != 'E1006' and DEVICE_STATUS_EVENT_ID != 'E1036') THEN 1 END) AS _onlineADM,  ";
                    _sql += " COUNT(CASE WHEN(SUBSTRING(TERM_ID, '14', 4) = '262'and (DEVICE_STATUS_EVENT_ID = 'E1005' or DEVICE_STATUS_EVENT_ID = 'E1156' or DEVICE_STATUS_EVENT_ID = 'E1006' or DEVICE_STATUS_EVENT_ID = 'E1036')) THEN 1 END) AS _offlineADM";
                    _sql += " FROM gsb_adm_fv.device_status_info  ";
                    //_sql += " left join device_info as b on a.TERM_ID = b.TERM_ID";

                    cn.Open();

                    MySqlCommand cmd = new MySqlCommand(_sql, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                    return GetHomeStatusCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                return null;
            }
        }
        protected virtual List<homeshowstatus> GetHomeStatusCollectionFromReader(IDataReader reader)
        {
            List<homeshowstatus> recordlst = new List<homeshowstatus>();
            while (reader.Read())
            {
                recordlst.Add(GetHomeStatusFromReader(reader));
            }
            return recordlst;
        }
        protected virtual homeshowstatus GetHomeStatusFromReader(IDataReader reader)
        {
            homeshowstatus record = new homeshowstatus();

            record.onlineADM = reader["_onlineADM"].ToString();
            record.onlineATM = reader["_onlineATM"].ToString();
            record.offlineADM = reader["_offlineADM"].ToString();
            record.offlineATM = reader["_offlineATM"].ToString();
            return record;
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
    }
}