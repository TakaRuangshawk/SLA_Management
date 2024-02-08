using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SLA_Management.Data;
using SLA_Management.Models;
using SLA_Management.Models.HomeModel;
using SLA_Management.Models.OperationModel;
using SLAManagement.Data;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Globalization;
using System.Xml;

namespace SLA_Management.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _myConfiguration;
        private CultureInfo _cultureEnInfo = new CultureInfo("en-US");
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
        public IActionResult Index_bk()
        {
            recordset_homeshowstatus = GetHomeStatus();
            if (recordset_homeshowstatus != null)
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
            if (recordset_comlogrecord != null)
            {
                foreach (var Data in recordset_comlogrecord)
                {
                    if (Data.comlogADM != "" || Data.comlogATM != "")
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
            if (recordset_secone != null)
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
            if (recordset_secone_adm != null)
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
            if (recordset_secone != null && recordset_secone_adm != null)
            {
                ViewBag.TotalSECONE_online = (Convert.ToInt32(ViewBag.secone_adm_online) + Convert.ToInt32(ViewBag.secone_online)).ToString();
                ViewBag.TotalSECONE_offine = (Convert.ToInt32(ViewBag.secone_adm_offline) + Convert.ToInt32(ViewBag.secone_offline)).ToString();
                ViewBag.TotalSECONE = (Convert.ToInt32(ViewBag.TotalSECONE_online) + Convert.ToInt32(ViewBag.TotalSECONE_offine)).ToString();
            }
            ViewBag.DateNow = DateTime.Now.AddDays(-1).ToString("dd - MM - yyyy", usaCulture);
            return View(recordset_slatracking);
        }

        public IActionResult Index()
        {

            #region Set all feelview status
            Task[] tasks = new Task[4];

            List<feelviewstatus> feelviewstatusLRM = new List<feelviewstatus>();
            List<feelviewstatus> feelviewstatusRDM = new List<feelviewstatus>();
            List<feelviewstatus> feelviewstatus2IN1 = new List<feelviewstatus>();
            List<secone> feelviewstatusSECONE = new List<secone>();
        
            tasks[0] = Task.Run(() =>
            {
                feelviewstatusLRM = GetLRMStatus();
            });
            tasks[1] = Task.Run(() =>
            {
                feelviewstatusRDM = GetRDMStatus();
            });
            tasks[2] = Task.Run(() =>
            {
                feelviewstatus2IN1 = Get2IN1Status();
            });
            tasks[3] = Task.Run(() =>
            {
                feelviewstatusSECONE = GetSECOneStatus();
            });
         

            DateTime dateTime = DateTime.Now;

            List<EventDetail> myListLRM = new List<EventDetail>();
            List<EventDetail> myListRDM = new List<EventDetail>();
            List<EventDetail> myList2IN1 = new List<EventDetail>();

            int[] myListLRM_array = { 0, 0, 0, 0, 0 };
            int[] myListRDM_array = { 0, 0, 0, 0, 0 };
            int[] myList2IN1_array = { 0, 0, 0, 0, 0 };



            Task[] tasksDeviceError = new Task[3];
            tasksDeviceError[0] = Task.Run(() =>
            {
                myListLRM = GetLRMTopDeviceError(dateTime.ToString("yyyy-MM") + "-01 00:00:00", dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
            });
            tasksDeviceError[1] = Task.Run(() =>
            {
                myListRDM = GetRDMTopDeviceError(dateTime.ToString("yyyy-MM") + "-01 00:00:00", dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
            });
            tasksDeviceError[2] = Task.Run(() =>
            {
                myList2IN1 = Get2N1TopDeviceError(dateTime.ToString("yyyy-MM") + "-01 00:00:00", dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
            });


            Task.WaitAll(tasksDeviceError);

            #endregion

            #region GetLRMTopDeviceError
            // '2024-01-01 00:00:00' and '2024-01-27 00:00:00'

            
            List<TopErrorDevice> topErrorDevicesListLRM = new List<TopErrorDevice>();          

            if (myListLRM != null)
                myListLRM = myListLRM.OrderByDescending(detail => detail.total).ToList();
            else
                myListLRM = new List<EventDetail>(); 


             TopErrorDevice topErrorDevice;
            int count = 1;
            for (int i = 0; i < myListLRM.Count; i++)
            {
                if (count == 6) break;
                topErrorDevice = new TopErrorDevice(count.ToString(), myListLRM[i].MODULE_NAME, myListLRM[i].NAME_EN_US);
                topErrorDevicesListLRM.Add(topErrorDevice);
                myListLRM_array[i] = myListLRM[i].total;
                count++;
            }

            #endregion

            #region GetRDMTopDeviceError

            
            List<TopErrorDevice> topErrorDevicesListRDM = new List<TopErrorDevice>();

            if(myListRDM != null)
            myListRDM = myListRDM.OrderByDescending(detail => detail.total).ToList();
            else
                myListRDM = new List<EventDetail>();

            count = 1;
            for (int i = 0; i < myListRDM.Count; i++)
            {
                if(count == 6) break;
                topErrorDevice = new TopErrorDevice(count.ToString(), myListRDM[i].MODULE_NAME, myListRDM[i].NAME_EN_US);
                topErrorDevicesListRDM.Add(topErrorDevice);
                myListRDM_array[i] = myListRDM[i].total;
                count++;
            }


            #endregion

            #region Get2IN1TopDeviceError
            
            List<TopErrorDevice> topErrorDevicesList2IN1 = new List<TopErrorDevice>();

            if(myList2IN1 != null)
            myList2IN1 = myList2IN1.OrderByDescending(detail => detail.total).ToList();
            else
                myList2IN1 = new List<EventDetail>();

            count = 1;
            for (int i = 0; i < myList2IN1.Count; i++)
            {
                if (count == 6) break;
                topErrorDevice = new TopErrorDevice(count.ToString(), myList2IN1[i].MODULE_NAME, myList2IN1[i].NAME_EN_US);
                topErrorDevicesList2IN1.Add(topErrorDevice);
                myList2IN1_array[i] = myList2IN1[i].total;
                count++;
            }

            #endregion

            #region Set all value

            string total2IN1 = "0";
            string totalLRM = "0";
            string totalRDM = "0";
            string totalSECONE = "0";

            string online2IN1 = "0";
            string onlineLRM = "0";
            string onlineRDM = "0";
            string onlineSECONE = "0";

            string offline2IN1 = "0";
            string offlineLRM = "0";
            string offlineRDM = "0";
            string offlineSECONE = "0";

            Task.WaitAll(tasks);

            if (feelviewstatusLRM != null)
            {
                totalLRM = (Int32.Parse(feelviewstatusLRM[0].onlineATM) + Int32.Parse(feelviewstatusLRM[0].offlineATM)).ToString();
                onlineLRM = Int32.Parse(feelviewstatusLRM[0].onlineATM).ToString();
                offlineLRM = Int32.Parse(feelviewstatusLRM[0].offlineATM).ToString();
            }

            if (feelviewstatusRDM != null)
            {
                totalRDM = (Int32.Parse(feelviewstatusRDM[0].onlineATM) + Int32.Parse(feelviewstatusRDM[0].offlineATM)).ToString();
                onlineRDM = Int32.Parse(feelviewstatusRDM[0].onlineATM).ToString();
                offlineRDM = Int32.Parse(feelviewstatusRDM[0].offlineATM).ToString();
            }

            if (feelviewstatus2IN1 != null)
            {
                total2IN1 = (Int32.Parse(feelviewstatus2IN1[0].onlineATM) + Int32.Parse(feelviewstatus2IN1[0].offlineATM)).ToString();
                online2IN1 = Int32.Parse(feelviewstatus2IN1[0].onlineATM).ToString();
                offline2IN1 = Int32.Parse(feelviewstatus2IN1[0].offlineATM).ToString();
            }

            if (feelviewstatusSECONE != null)
            {
                totalSECONE = (Int32.Parse(feelviewstatusSECONE[0]._online) + Int32.Parse(feelviewstatusSECONE[0]._offline)).ToString();
                onlineSECONE = Int32.Parse(feelviewstatusSECONE[0]._online).ToString();
                offlineSECONE = Int32.Parse(feelviewstatusSECONE[0]._offline).ToString();
            }

            #endregion

            #region Input value to ViewBag

            ViewBag.FV2IN1Total = total2IN1;
            ViewBag.FVRDMTotal = totalRDM;
            ViewBag.FVLRMTotal = totalLRM;
            ViewBag.FVSECTotal = totalSECONE;

            ViewBag.online2IN1 = online2IN1;
            ViewBag.onlineRDM = onlineRDM;
            ViewBag.onlineLRM = onlineLRM;
            ViewBag.onlineSEC = onlineSECONE;

            ViewBag.offline2IN1 = offline2IN1;
            ViewBag.offlineRDM = offlineRDM;
            ViewBag.offlineLRM = offlineLRM;
            ViewBag.offlineSEC = offlineSECONE;


            //LRM device
            ViewBag.deviceLRMError = topErrorDevicesListLRM;


            //RDM device
            ViewBag.deviceRDMError = topErrorDevicesListRDM;


            //2IN1 device
            ViewBag.device2IN1Error = topErrorDevicesList2IN1;

            
            ViewBag.deviceLRMError1 = myListLRM_array[0];
            ViewBag.deviceLRMError2 = myListLRM_array[1];
            ViewBag.deviceLRMError3 = myListLRM_array[2];
            ViewBag.deviceLRMError4 = myListLRM_array[3];
            ViewBag.deviceLRMError5 = myListLRM_array[4];


            ViewBag.deviceRDMError1 = myListRDM_array[0];
            ViewBag.deviceRDMError2 = myListLRM_array[1];
            ViewBag.deviceRDMError3 = myListRDM_array[2];
            ViewBag.deviceRDMError4 = myListRDM_array[3];
            ViewBag.deviceRDMError5 = myListRDM_array[4];

            ViewBag.device2IN1Error1 = myList2IN1_array[0];
            ViewBag.device2IN1Error2 = myList2IN1_array[1];
            ViewBag.device2IN1Error3 = myList2IN1_array[2];
            ViewBag.device2IN1Error4 = myList2IN1_array[3];
            ViewBag.device2IN1Error5 = myList2IN1_array[4];


            ViewBag.FrMonthLRM = DateTime.Now.ToString("MMMM yyyy", _cultureEnInfo);
            ViewBag.FrMonthRDM = DateTime.Now.ToString("MMMM yyyy", _cultureEnInfo);
            ViewBag.FrMonth2IN1 = DateTime.Now.ToString("MMMM yyyy", _cultureEnInfo);

            #endregion

            return View();
        }

        #region LRM get Event
        public List<EventDetail> GetLRMTopDeviceError(string dateStart, string dateEnd)
        {
            string _sql = string.Empty;

            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL_FV_LRM:FullNameConnection")))
                {

                    _sql = @"SELECT b.MODULE_NAME, a.EVENT_ID, b.NAME_EN_US, count(*) as total
                            FROM ghbfeelview.`mt_caseflow_record_his` as a, ghbfeelview.device_inner_event as b
                            where START_TIME between ";
                    _sql += "'" + dateStart + "' and '" + dateEnd + "'";
                    _sql += @" and a.EVENT_ID = b.EVENT_ID
                            and b.EVENT_TYPE = 'error'
                            and (b.TERM_STATUS_ID != '4' and b.MODULE_NAME != 'SAFEDOOR')
                            and b.EVENT_ID != 'E1104' and b.EVENT_ID != 'E1044'
                            GROUP BY EVENT_ID 
                            ORDER BY total desc;";

                    cn.Open();

                    MySqlCommand cmd = new MySqlCommand(_sql, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();



                    return GetTopDeviceErrorGHBCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                return null;
            }
        }

        public List<feelviewstatus> GetLRMStatus()
        {
            string _sql = string.Empty;

            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL_FV_LRM:FullNameConnection")))
                {

                    _sql = "SELECT COUNT(CASE WHEN(DEVICE_STATUS_EVENT_ID != 'E1005' and DEVICE_STATUS_EVENT_ID != 'E1156' and DEVICE_STATUS_EVENT_ID != 'E1006' and DEVICE_STATUS_EVENT_ID != 'E1036') THEN 1 END) AS _onlineATM,  ";
                    _sql += " COUNT(CASE WHEN(DEVICE_STATUS_EVENT_ID = 'E1005' or DEVICE_STATUS_EVENT_ID = 'E1156' or DEVICE_STATUS_EVENT_ID = 'E1006' or DEVICE_STATUS_EVENT_ID = 'E1036') THEN 1 END) AS _offlineATM  ";
                    _sql += " FROM ghbfeelview.device_status_info  ";                   

                    cn.Open();

                    MySqlCommand cmd = new MySqlCommand(_sql, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();


                    return GetHomeStatusGHBCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                return null;
            }
        }

        #endregion

        #region RDM get Event
        public List<EventDetail> GetRDMTopDeviceError(string dateStart, string dateEnd)
        {
            string _sql = string.Empty;

            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL_FV_CDM:FullNameConnection")))
                {

                    _sql = @"SELECT b.MODULE_NAME, a.EVENT_ID, b.NAME_EN_US, count(*) as total
                            FROM gsb_pilot2020.`mt_caseflow_record_his` as a, gsb_pilot2020.device_inner_event as b
                            where START_TIME between ";
                    _sql += "'" + dateStart + "' and '" + dateEnd + "'";
                    _sql += @" and a.EVENT_ID = b.EVENT_ID
                            and b.EVENT_TYPE = 'error'
                            and (b.TERM_STATUS_ID != '4' and b.MODULE_NAME != 'SAFEDOOR')
                            and b.EVENT_ID != 'E1104' and b.EVENT_ID != 'E1044'
                            GROUP BY EVENT_ID 
                            ORDER BY total desc;";
            

                    cn.Open();

                    MySqlCommand cmd = new MySqlCommand(_sql, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();



                    return GetTopDeviceErrorGHBCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                return null;
            }
        }

        public List<feelviewstatus> GetRDMStatus()
        {
            string _sql = string.Empty;

            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL_FV_CDM:FullNameConnection")))
                {

                    _sql = "SELECT COUNT(CASE WHEN(DEVICE_STATUS_EVENT_ID != 'E1005' and DEVICE_STATUS_EVENT_ID != 'E1156' and DEVICE_STATUS_EVENT_ID != 'E1006' and DEVICE_STATUS_EVENT_ID != 'E1036') THEN 1 END) AS _onlineATM,  ";
                    _sql += " COUNT(CASE WHEN(DEVICE_STATUS_EVENT_ID = 'E1005' or DEVICE_STATUS_EVENT_ID = 'E1156' or DEVICE_STATUS_EVENT_ID = 'E1006' or DEVICE_STATUS_EVENT_ID = 'E1036') THEN 1 END) AS _offlineATM  ";
                    _sql += " FROM gsb_pilot2020.device_status_info  ";
                    //_sql += " left join device_info as b on a.TERM_ID = b.TERM_ID";

                    cn.Open();

                    MySqlCommand cmd = new MySqlCommand(_sql, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                    return GetHomeStatusGHBCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                return null;
            }
        }


        #endregion

        #region 2IN1 get Event 
        public List<EventDetail> Get2N1TopDeviceError(string dateStart, string dateEnd)
        {
            string _sql = string.Empty;

            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL_FV_2IN1:FullNameConnection")))
                {

                    _sql = @"SELECT b.MODULE_NAME, a.EVENT_ID, b.NAME_EN_US, count(*) as total
                            FROM fv_ghb2in1.`mt_caseflow_record_his` as a, fv_ghb2in1.device_inner_event as b
                            where START_TIME between ";
                    _sql += "'" + dateStart + "' and '" + dateEnd + "'";
                    _sql += @" and a.EVENT_ID = b.EVENT_ID
                            and b.EVENT_TYPE = 'error'
                            and (b.TERM_STATUS_ID != '4' and b.MODULE_NAME != 'SAFEDOOR')
                            and b.EVENT_ID != 'E1104' and b.EVENT_ID != 'E1044'
                            GROUP BY EVENT_ID 
                            ORDER BY total desc;";

                    //                    _sql = @"SELECT a.DEVICE_STATUS_EVENT_ID ,b.NAME_EN_US
                    //FROM ghbfeelview.device_status_info a , ghbfeelview.device_inner_event b 
                    //where ( a.DEVICE_STATUS_EVENT_ID = b.EVENT_ID ) and
                    //DEVICE_STATUS_EVENT_ID != 'E1002' and
                    //DEVICE_STATUS_EVENT_ID != 'E1003' and
                    //DEVICE_STATUS_EVENT_ID != 'E1041' and
                    //DEVICE_STATUS_EVENT_ID != 'E1130' and
                    //DEVICE_STATUS_EVENT_ID != 'E1225' and
                    //DEVICE_STATUS_EVENT_ID != 'E1129' and
                    //DEVICE_STATUS_EVENT_ID != 'E1128' and
                    //DEVICE_STATUS_EVENT_ID != 'E1047' ";
                    //_sql += " left join device_info as b on a.TERM_ID = b.TERM_ID";

                    cn.Open();

                    MySqlCommand cmd = new MySqlCommand(_sql, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();



                    return GetTopDeviceErrorGHBCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                return null;
            }
        }

        public List<feelviewstatus> Get2IN1Status()
        {
            string _sql = string.Empty;

            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL_FV_2IN1:FullNameConnection")))
                {

                    _sql = "SELECT COUNT(CASE WHEN(DEVICE_STATUS_EVENT_ID != 'E1005' and DEVICE_STATUS_EVENT_ID != 'E1156' and DEVICE_STATUS_EVENT_ID != 'E1006' and DEVICE_STATUS_EVENT_ID != 'E1036') THEN 1 END) AS _onlineATM,  ";
                    _sql += " COUNT(CASE WHEN(DEVICE_STATUS_EVENT_ID = 'E1005' or DEVICE_STATUS_EVENT_ID = 'E1156' or DEVICE_STATUS_EVENT_ID = 'E1006' or DEVICE_STATUS_EVENT_ID = 'E1036') THEN 1 END) AS _offlineATM  ";
                    _sql += " FROM fv_ghb2in1.device_status_info  ";
                    //_sql += " left join device_info as b on a.TERM_ID = b.TERM_ID";

                    cn.Open();

                    MySqlCommand cmd = new MySqlCommand(_sql, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();






                    return GetHomeStatusGHBCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                return null;
            }
        }

        #endregion

        #region SECONE get Event

        public List<secone> GetSECOneStatus()
        {
            string _sql = string.Empty;

            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_SECOne:FullNameConnection")))
                {

                    _sql = "SELECT SUM(CASE WHEN AGENT_STATUS = 1 THEN 1 ELSE 0 END) AS _online, SUM(CASE WHEN AGENT_STATUS != 1 or AGENT_STATUS is null THEN 1 ELSE 0 END) AS _offline   ";
                    _sql += " FROM seconeh5.device_info  ";
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

        #endregion

        #region Main Function
        protected virtual EventDetail GetTopDeviceErrorGHBFromReader(IDataReader reader)
        {
            EventDetail record = new EventDetail();


            record.MODULE_NAME = reader["MODULE_NAME"].ToString();

            record.EVENT_ID = reader["EVENT_ID"].ToString();

            record.NAME_EN_US = reader["NAME_EN_US"].ToString();

            record.total = Int32.Parse(reader["total"].ToString());

            return record;
        }

        protected virtual List<EventDetail> GetTopDeviceErrorGHBCollectionFromReader(IDataReader reader)
        {
            List<EventDetail> recordlst = new List<EventDetail>();
            while (reader.Read())
            {
                recordlst.Add(GetTopDeviceErrorGHBFromReader(reader));
            }
            return recordlst;
        }

        protected virtual feelviewstatus GetHomeStatusGHBFromReader(IDataReader reader)
        {
            feelviewstatus record = new feelviewstatus();


            record.onlineATM = reader["_onlineATM"].ToString();

            record.offlineATM = reader["_offlineATM"].ToString();
            return record;
        }

        protected virtual List<feelviewstatus> GetHomeStatusGHBCollectionFromReader(IDataReader reader)
        {
            List<feelviewstatus> recordlst = new List<feelviewstatus>();
            while (reader.Read())
            {
                recordlst.Add(GetHomeStatusGHBFromReader(reader));
            }
            return recordlst;
        }





        #endregion

        #region Old code
        public List<comlogrecord> GetComlogRecordFromSqlServer()
        {
            List<comlogrecord> dataList = new List<comlogrecord>();


            string sqlQuery = " SELECT COUNT(CASE WHEN ERROR IS NULL  AND TERM_ID LIKE '%G262%' THEN 1 END) AS ComlogADM,COUNT(CASE WHEN ERROR IS NULL  AND TERM_ID like '%G165%' THEN 1 END) AS ComlogATM ";
            sqlQuery += " FROM comlog_record where COMLOGDATE between '" + DateTime.Now.Date.AddDays(-1).ToString("yyyy-MM-dd", usaCulture) + " 00:00:00' AND '" + DateTime.Now.Date.AddDays(-1).ToString("yyyy-MM-dd", usaCulture) + " 23:59:59'";
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
            catch (Exception ex)
            {

            }


            return dataList;
        }
        public List<slatracking> GetSlatrackingFromSqlServer()
        {
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
                                dataList.Add(GetSlatrackingFromReader(reader, n));
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
        protected virtual slatracking GetSlatrackingFromReader(IDataReader reader, int n)
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
        public List<secone> GetSECOneADMStatus()
        {
            string _sql = string.Empty;

            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_SECOne_ADM:FullNameConnection")))
                {

                    _sql = "SELECT SUM(CASE WHEN AGENT_STATUS = 1 THEN 1 ELSE 0 END) AS _online, SUM(CASE WHEN AGENT_STATUS != 1 or AGENT_STATUS is null THEN 1 ELSE 0 END) AS _offline   ";
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

        #endregion
    }
}