﻿using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SLA_Management.Data;
using SLA_Management.Models;
using SLA_Management.Models.OperationModel;
using SLA_Management.Data;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using SLA_Management.Models.Dashboard;

namespace SLA_Management.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _myConfiguration;
        private static List<feelviewstatus> recordset_homeshowstatus = new List<feelviewstatus>();
        private static List<comlogrecord> recordset_comlogrecord = new List<comlogrecord>();
        private static List<slatracking> recordset_slatracking = new List<slatracking>();
        private static List<secone> recordset_secone = new List<secone>();
        private static List<secone> recordset_secone_adm = new List<secone>();
        private DBService dBService;
        SqlCommand com = new SqlCommand();
        ConnectSQL_Server con;
        CultureInfo usaCulture = new CultureInfo("en-US");
        public HomeController(IConfiguration myConfiguration)
        {

            _myConfiguration = myConfiguration;
            dBService = new DBService(_myConfiguration);
            con = new ConnectSQL_Server(_myConfiguration["ConnectionStrings:DefaultConnection"]);
        }

        [HttpGet]
        public IActionResult GetDashboardData(string fromDate, string toDate)
        {
            List<DashboradViewModel> dashboardDataList = new List<DashboradViewModel>();
            List<PieChartModel> pieDataList = new List<PieChartModel>();
            string connectionString = _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_baac");
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var query = "SELECT D.TERM_SEQ,D.TERM_ID,D.TERM_NAME,D.COUNTER_CODE,COUNT(*) AS Total_Cases FROM reportcases R JOIN device_info D ON D.TERM_ID = R.Terminal_ID WHERE 1=1 " +
                    "AND R.Date_Inform BETWEEN @fromDate AND @toDate " +
                    "GROUP BY D.TERM_NAME,D.TERM_SEQ,D.TERM_ID, D.COUNTER_CODE ORDER BY COUNT(*) DESC LIMIT 5";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@fromDate", fromDate);
                    cmd.Parameters.AddWithValue("@toDate", toDate);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dashboardDataList.Add(new DashboradViewModel()
                            {
                                Serial_No = reader["TERM_SEQ"].ToString(),
                                Terminal_Id = reader["TERM_ID"].ToString(),
                                Terminal_Name = reader["TERM_NAME"].ToString(),
                                Counter_Code = reader["COUNTER_CODE"].ToString(),
                                Total_Cases = Convert.ToInt32(reader["Total_Cases"])
                            });
                        }
                    }
                }
            }

            //mysql
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Main_Problem,COUNT(Main_Problem) as Count FROM t_tsd_jobdetail WHERE 1=1 AND Main_Problem IS NOT NULL AND Open_Date BETWEEN @fromDate AND @toDate GROUP BY Main_Problem ORDER BY Main_Problem ASC";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@fromDate", fromDate + " 00:00:00");
                    cmd.Parameters.AddWithValue("@toDate", toDate + " 23:59:59");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pieDataList.Add(new PieChartModel()
                            {
                                name = reader["Main_Problem"].ToString(),
                                value = Convert.ToInt32(reader["Count"])
                            });
                        }
                    }
                }
            }
            return Json(new
            {
                success = true,
                data = dashboardDataList,
                pieDataList = pieDataList
            });
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
                    ViewBag.FVTotal = ((Convert.ToInt32(Data.onlineATM) + Convert.ToInt32(Data.onlineADM)) + (Convert.ToInt32(Data.offlineATM) + Convert.ToInt32(Data.offlineADM))).ToString();
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
            recordset_slatracking = GetSlatrackingFromSqlServer();
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
            recordset_secone = GetSECOneStatus();
            if(recordset_secone != null)
            {
                foreach (var data in recordset_secone)
                {
                    ViewBag.secone_online = data._online;
                    ViewBag.secone_offline = data._offline;
                }
            }
            else
            {
                ViewBag.secone_online = "-";
                ViewBag.secone_offline = "-";
            }
            recordset_secone_adm = GetSECOneADMStatus();
            if(recordset_secone_adm != null)
            {
                foreach (var data in recordset_secone_adm)
                {
                    ViewBag.secone_adm_online = data._online;
                    ViewBag.secone_adm_offline = data._offline;
                }
            }
            else
            {
                ViewBag.secone_adm_online = "-";
                ViewBag.secone_adm_offline = "-";
            }
            if(recordset_secone != null && recordset_secone_adm != null)
            {
                ViewBag.TotalSECONE_online = (Convert.ToInt32(ViewBag.secone_adm_online)+Convert.ToInt32(ViewBag.secone_online)).ToString();
                ViewBag.TotalSECONE_offine = (Convert.ToInt32(ViewBag.secone_adm_offline) + Convert.ToInt32(ViewBag.secone_offline)).ToString();
                ViewBag.TotalSECONE = (Convert.ToInt32(ViewBag.TotalSECONE_online) + Convert.ToInt32(ViewBag.TotalSECONE_offine)).ToString();
            }
            ViewBag.DateNow = DateTime.Now.AddDays(-1).ToString("dd - MM - yyyy",usaCulture);
            return View(recordset_slatracking);
        }
        public List<comlogrecord> GetComlogRecordFromSqlServer()
        {
            List<comlogrecord> dataList = new List<comlogrecord>();

            
            string sqlQuery = " SELECT COUNT(CASE WHEN ERROR IS NULL  AND TERM_ID LIKE '%G262%' THEN 1 END) AS ComlogADM,COUNT(CASE WHEN ERROR IS NULL  AND TERM_ID like '%G165%' THEN 1 END) AS ComlogATM ";
            sqlQuery += " FROM comlog_record where COMLOGDATE between '" + DateTime.Now.Date.AddDays(-1).ToString("yyyy-MM-dd",usaCulture) + " 00:00:00' AND '" + DateTime.Now.Date.AddDays(-1).ToString("yyyy-MM-dd", usaCulture) + " 23:59:59'";
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
        public List<slatracking> GetSlatrackingFromSqlServer() {
            List<slatracking> dataList = new List<slatracking>();
            string sqlQuery = " SELECT [APPNAME],[STATUS],[UPDATE_DATE]FROM (SELECT [APPNAME],[STATUS],[UPDATE_DATE],ROW_NUMBER() OVER (PARTITION BY [APPNAME] ORDER BY [UPDATE_DATE] DESC) AS rn ";
            sqlQuery += "  FROM sla_tracking where APPNAME in ('appChangeAndUnzip','InsertFileCOMLog','Translator','NDCT','Downtime','SLA Report')) ranked WHERE rn = 1 and YEAR(UPDATE_DATE) = YEAR(GETDATE()) ";
            sqlQuery += " ORDER BY CASE WHEN [APPNAME] = 'appChangeAndUnzip' THEN 1 WHEN [APPNAME] = 'InsertFileCOMLog' THEN 2 WHEN [APPNAME] = 'Translator' THEN 3 WHEN [APPNAME] = 'NDCT' THEN 4 WHEN [APPNAME] = 'Downtime' THEN 5 WHEN [APPNAME] = 'SLA Report' THEN 6 ELSE 6 END; ";
            try
            {
                using (SqlConnection connection = new SqlConnection(_myConfiguration.GetValue<string>("ConnectionStrings:DefaultConnection")))
                {

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            int n = 1;
                            while (reader.Read())
                            {
                                dataList.Add(GetSlatrackingFromReader(reader,n));
                                n++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }


            return dataList;
        }
        protected virtual slatracking GetSlatrackingFromReader(IDataReader reader,int n)
        {
            slatracking record = new slatracking();

            record.no = n.ToString();
            record.APPNAME = reader["APPNAME"].ToString();
            record.STATUS = reader["STATUS"].ToString();
            record.UPDATE_DATE = DateTime.Parse(reader["UPDATE_DATE"].ToString()).ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.DefaultThreadCurrentCulture);
            return record;
        }
        protected virtual comlogrecord GetComlogRecordFromReader(IDataReader reader)
        {
            comlogrecord record = new comlogrecord();

            record.comlogADM = reader["comlogADM"].ToString();
            record.comlogATM = reader["comlogATM"].ToString();
            return record;
        }
        public List<secone> GetSECOneStatus()
        {
            string _sql = string.Empty;

            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_SECOne:FullNameConnection")))
                {

                    _sql = "SELECT SUM(CASE WHEN AGENT_STATUS = 1 THEN 1 ELSE 0 END) AS _online, SUM(CASE WHEN AGENT_STATUS != 1 or AGENT_STATUS is null THEN 1 ELSE 0 END) AS _offline   ";
                    _sql += " FROM device_info  ";
                    cn.Open();

                    MySqlCommand cmd = new MySqlCommand(_sql, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                    return GetSECOneStatusCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                return null;
            }
        }
        protected virtual List<secone> GetSECOneStatusCollectionFromReader(IDataReader reader)
        {
            List<secone> recordlst = new List<secone>();
            while (reader.Read())
            {
                recordlst.Add(GetSECOneStatusFromReader(reader));
            }
            return recordlst;
        }
        protected virtual secone GetSECOneStatusFromReader(IDataReader reader)
        {
            secone record = new secone();

            record._online = reader["_online"].ToString();
            record._offline = reader["_offline"].ToString();
            return record;
        }
        public List<secone> GetSECOneADMStatus()
        {
            string _sql = string.Empty;

            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_SECOne_ADM:FullNameConnection")))
                {

                    _sql = "SELECT SUM(CASE WHEN AGENT_STATUS = 1    THEN 1 ELSE 0 END) AS _online, SUM(CASE WHEN AGENT_STATUS != 1 or AGENT_STATUS is null THEN 1 ELSE 0 END) AS _offline   ";
                    _sql += " FROM device_info  ";
                    cn.Open();

                    MySqlCommand cmd = new MySqlCommand(_sql, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                    return GetSECOneStatusADMCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                return null;
            }
        }
        protected virtual List<secone> GetSECOneStatusADMCollectionFromReader(IDataReader reader)
        {
            List<secone> recordlst = new List<secone>();
            while (reader.Read())
            {
                recordlst.Add(GetSECOneStatusADMFromReader(reader));
            }
            return recordlst;
        }
        protected virtual secone GetSECOneStatusADMFromReader(IDataReader reader)
        {
            secone record = new secone();

            record._online = reader["_online"].ToString();
            record._offline = reader["_offline"].ToString();
            return record;
        }
        public List<feelviewstatus> GetHomeStatus()
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
        protected virtual List<feelviewstatus> GetHomeStatusCollectionFromReader(IDataReader reader)
        {
            List<feelviewstatus> recordlst = new List<feelviewstatus>();
            while (reader.Read())
            {
                recordlst.Add(GetHomeStatusFromReader(reader));
            }
            return recordlst;
        }
        protected virtual feelviewstatus GetHomeStatusFromReader(IDataReader reader)
        {
            feelviewstatus record = new feelviewstatus();

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