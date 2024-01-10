using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using SLAManagement.Data;
using PagedList;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using SLA_Management.Models.TermProbModel;
using MySql.Data.MySqlClient;
using System.Data.Common;
using MySqlX.XDevAPI.Common;
using SLA_Management.Commons;
using SLA_Management.Data.ExcelUtilitie;
using SLA_Management.Data;
using SLA_Management.Models.OperationModel;
using SLA_Management.Models.ReportModel;
using System.Security.AccessControl;

namespace SLA_Management.Controllers
{
    public class OperationController : Controller
    {

        CultureInfo _cultureEnInfo = new CultureInfo("en-US");
        SqlCommand com = new SqlCommand();
        ConnectSQL_Server con;
        List<SLAReportDaily> messageRequestList = new List<SLAReportDaily>();
        List<SLAReportMonthly> messageRequestList_m = new List<SLAReportMonthly>();
        List<TrackingDetail> trackingDetails = new List<TrackingDetail>();
        List<SelectListItem> ddlMonths = new List<SelectListItem>();
        List<SelectListItem> ddlYears = new List<SelectListItem>();
        private IConfiguration _myConfiguration;
        private string sladailydowntime_table;
        private string slatracking_table;
        private string slamonthlydowntime_table;
        private string startquery_reportdaily;
        private string startquery_tracking;
        private string startquery_reportmonthly;
        private List<GatewayTransaction> recordset = new List<GatewayTransaction>();
        #region regulator parameter
        private static regulator_seek param = new regulator_seek();
        private static ej_trandada_seek param_ej = new ej_trandada_seek();
        private static ejchecksize_seek param_checkej = new ejchecksize_seek();
        private List<Regulator> recordset_regulator = new List<Regulator>();
        private long recCnt = 0;
        private static List<Regulator> regulator_dataList = new List<Regulator>();
        private static List<ej_terminalperoffline> recordset_checkejsize = new List<ej_terminalperoffline>();
        private static List<ej_terminalperoffline> checkejsize_dataList = new List<ej_terminalperoffline>();
        private static List<ejloglastupdate> recordset_ejloglastupdate = new List<ejloglastupdate>();
        private static List<ejloglastupdate> ejloglastupdate_datalist = new List<ejloglastupdate>();
        private static List<TicketManagement> recordset_ticketManagement = new List<TicketManagement>();
        private static List<TicketManagement> ticket_dataList = new List<TicketManagement>();
        private int pageNum = 1;
        private List<terminalAndSeq> terminalIDAndSeqList = new List<terminalAndSeq>();
        private List<string> terminalIDList = new List<string>();
        private List<string> terminalSEQList = new List<string>();
        private DBService dBService;
        CultureInfo usaCulture = new CultureInfo("en-US");
        private static ConnectSQL_Server db;
        private static ConnectMySQL db_fv;
        private static string sqlConnection;
        private static string sqlServer { get; set; }
        public static string SqlConnection { get => sqlConnection; set => sqlConnection = value; }
        #endregion
        public OperationController(IConfiguration myConfiguration)
        {

            _myConfiguration = myConfiguration;
            dBService = new DBService(_myConfiguration);
            con = new ConnectSQL_Server(_myConfiguration["ConnectionStrings:DefaultConnection"]);
            sqlServer = _myConfiguration.GetValue<string>("ConnectionStrings:DefaultConnection");
            db = new ConnectSQL_Server(sqlServer);
            db_fv = new ConnectMySQL(myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection"));
            sladailydowntime_table = "sla_reportdaily";
            slatracking_table = "sla_tracking";
            slamonthlydowntime_table = "sla_reportmonthly";
            startquery_reportdaily = "SELECT TOP 5000 ID,Report_Date, Open_Date, Appointment_Date, Closed_Repair_Date, Down_Time, AS_OpenDate, AS_AppointmentDate, AS_CloseRepairDate, AS_Downtime, Discount, Net_Downtime, AS_Discription, AS_CIT_Request, AS_Service_PM, Status, TERM_ID, Model, TERM_SEQ, Province, Location, Problem_Detail, Solving_Program, Service_Team, Contact_Name_Branch_CIT, Open_By, Remark FROM ";
            startquery_tracking = "SELECT TOP 5000 ID,APPNAME,UPDATE_DATE,STATUS,REMARK,USER_IP FROM " + slatracking_table;
            startquery_reportmonthly = "SELECT TOP 5000 t1.ID,t1.TERM_ID,t1.TERM_SEQ,t1.LOCATION,t1.PROVINCE,t1.INSTALL_LOT,t1.REPLENISHMENT_DATE,t1.STARTSERVICE_DATE,t1.TOTALSERVICEDAY,t1.SERVICE_GROUP,t1.SERVICE_DATE,t1.SERVICEDAY_CHARGE,t1.SERVICETIME_CHARGE_PERDAY,t1.SERVICETIME_PERMONTH_HOUR,t1.SERVICETIME_PERHOUR_MINUTE,t1.TOTALDOWNTIME_HOUR,t1.TOTALDOWNTIME_MINUTE,t1.ACTUAL_SERVICETIME_PERMONTH_HOUR,t1.ACTUAL_SERVICETIME_PERHOUR_MINUTE,t1.ACTUAL_PERCENTSLA,t1.RATECHARGE,t1.SERVICECHARGE,t1.NETCHARGE,t1.REMARK,t2.TERM_NAME FROM " + slamonthlydowntime_table;

        }
        #region sla
        public IActionResult SlaReportMonthly(string TerminalID, string TerminalSEQ, string Month, string Year, string Orderby, string Sortby, string maxRows)
        {
            string Day = "";
            if (maxRows == null || maxRows == "") maxRows = "100";
            ViewBag.maxRows = maxRows;
            if (TerminalID == null) TerminalID = "";
            ViewBag.idCard = TerminalID;
            if (TerminalSEQ == null) TerminalSEQ = "";
            ViewBag.seqCard = TerminalSEQ;
            if (Month != null)
            {
                Month = DateTime.ParseExact(Month, "MMMM", CultureInfo.CurrentCulture).Month.ToString("D2");
            }
            Console.WriteLine(Year + " | " + Month);
            FatchDataMainDowntime_m(TerminalID, TerminalSEQ, Month, Year, Orderby, Sortby);
            int pageSize = con.GetCountTable("SELECT COUNT(*) FROM " + slamonthlydowntime_table + "_" + Year + Month);
            if (pageSize == 0)
            {
                pageSize = 1;
            }
            return View(messageRequestList_m.ToPagedList(1, pageSize));
        }
        private void FatchDataMainDowntime_m(string TerminalID, string TerminalSEQ, string Month, string Year, string Orderby, string Sortby)
        {
            if (messageRequestList.Count > 0)
            {
                messageRequestList.Clear();
            }
            if (Month == null) Month = "";
            if (Year == null) Year = "";
            if (Orderby == null) Orderby = "TERM_SEQ";
            if (Sortby == null || Sortby == "") Sortby = "asc";
            if (Month != "" && Year != "")
            {
                com.CommandText = startquery_reportmonthly + "_" + Year + Month + " as t1 left join device_info_record as t2 on t1.TERM_ID =t2.TERM_ID ";
            }
            if (TerminalSEQ != "" || TerminalID != "")
            {
                com.CommandText += "Where ";

            }
            if (TerminalSEQ != "")
            {
                com.CommandText += " t1.TERM_SEQ = '" + TerminalSEQ + "' ";
            }
            if (TerminalID != "")
            {

                if (TerminalSEQ != "")
                {
                    com.CommandText += "AND t1.TERM_ID = '" + TerminalID + "' ";
                }
                else
                {
                    com.CommandText += " t1.TERM_ID = '" + TerminalID + "' ";
                }
            }
            if (Orderby != "TERM_SEQ")
            {
                Orderby += ",TERM_SEQ";
            }
            com.CommandText += " order by " + Orderby + " " + Sortby;
            Console.WriteLine("com.CommandText :  " + com.CommandText.ToString());
            messageRequestList_m = SLAReportMonthly.mapToList(con.GetDatatable(com)).ToList();
        }
        public IActionResult SlaReportDaily(string TerminalID, string Month, string Year, string chk_date, string date, string maxRows)
        {
            string Day = "";
            if (maxRows == null || maxRows == "") maxRows = "200";
            ViewBag.maxRows = maxRows;
            if (TerminalID == null) TerminalID = "";
            if (date != null)
            {
                chk_date = "true";
            }
            else
            {
                chk_date = "false";
            }
            if (date == null) date = DateTime.Now.ToString("yyyyMMdd");
            ViewBag.idCard = TerminalID;
            if (Month != null)
            {
                Month = DateTime.ParseExact(Month, "MMMM", CultureInfo.CurrentCulture).Month.ToString("D2");
            }
            if (chk_date == "true")
            {
                if (date != null)
                {
                    date = date.Replace("-", "");

                    Year = date.Substring(0, 4);
                    Month = date.Substring(4, 2);
                    Day = date.Substring(6, 2);
                    Console.WriteLine("test date data : " + date + "Year : " + Year + "Month : " + Month + "Day : " + Day);

                }
            }
            FatchDataMainDowntime(TerminalID, Day, Month, Year);
            int pageSize = 0;
            if (Day != "")
            {
                pageSize = con.GetCountTable("SELECT COUNT(*) FROM " + sladailydowntime_table + "_" + Year + Month + " Where Open_Date between '" + Year + "-" + Month + "-" + Day + " 00:00:00' and '" + Year + "-" + Month + "-" + Day + " 23:59:59'");
            }
            else
            {
                pageSize = con.GetCountTable("SELECT COUNT(*) FROM " + sladailydowntime_table + "_" + Year + Month);
            }

            if (pageSize == 0)
            {
                pageSize = 1;
            }
            return View(messageRequestList.ToPagedList(1, pageSize));
        }
        //old version
        public IActionResult SlaReportDaily2(string TerminalID, string Problem_Detail, string frDate, string toDate, string status, string maxRows, int? Year)
        {
            if (maxRows == null || maxRows == "") maxRows = "20";
            if (TerminalID == null) TerminalID = "";
            if (Problem_Detail == null) Problem_Detail = "";
            if (status == null) status = "";
            if (frDate != null && toDate != null)
            {
                DateTime fr = DateTime.Parse(frDate);
                DateTime to = DateTime.Parse(toDate).AddDays(1);
                ViewBag.frDate = fr.ToString("dd/mm/yyyy");
                ViewBag.toDate = to.ToString("dd/mm/yyyy");
            }
            ViewBag.maxRows = maxRows;
            ViewBag.status = status;
            ViewBag.idCard = TerminalID;
            ViewBag.Problem_Detail = Problem_Detail;
            int pageSize = con.GetCountTable("SELECT COUNT(*) FROM " + sladailydowntime_table);
            if (pageSize == 0)
            {
                pageSize = 1;
            }
            //FatchDataMainDowntime(TerminalID, Problem_Detail, frDate, toDate, status);
            return View(messageRequestList.ToPagedList(1, pageSize));
        }
        private void FatchDataMainDowntime(string TerminalID, string Day, string Month, string Year)
        {
            if (messageRequestList.Count > 0)
            {
                messageRequestList.Clear();
            }
            #region for date
            //if (frDate != null)
            //{
            //    frDate = frDate + " 00:00:00";
            //    if (toDate == "" || toDate == null)
            //    {

            //        DateTime toDateNull = DateTime.Parse(frDate);
            //        DateTime tempDate = toDateNull.AddMonths(toDateNull.Month);
            //        DateTime tempDate2 = new DateTime(toDateNull.Year, toDateNull.Month, toDateNull.Day);
            //        // DateTime lastDayOfMonth = tempDate2.AddDays(-1);

            //        toDate = tempDate2.ToString("yyyy-MM-dd");

            //    }
            //}

            //if (toDate != null)
            //{
            //    toDate = toDate + " 23:59:59";
            //    if (frDate == "" || frDate == null)
            //    {
            //        DateTime frDateNull = DateTime.Parse(toDate);
            //        DateTime tempDate = frDateNull.AddMonths(frDateNull.Month);
            //        DateTime tempDate2 = new DateTime(frDateNull.Year, frDateNull.Month, 1);


            //        frDate = tempDate2.ToString("yyyy-MM-dd");

            //    }
            //}
            #endregion 
            if (Day == null) Day = "";
            if (Month == null) Month = "";
            if (Year == null) Year = "";
            if (Month != "" && Year != "")
            {
                com.CommandText = startquery_reportdaily + sladailydowntime_table + "_" + Year + Month;
            }
            //com.CommandText = startquery_reportdaily;
            if (TerminalID != "")
            {
                com.CommandText += " WHERE  TERM_ID = '" + TerminalID + "' ";

                #region
                //if (frDate != null && toDate != null)
                //{
                //    if (TerminalID != "" || Problem_Detail != "" || status != "")
                //    {
                //        com.CommandText += " AND Open_Date BETWEEN '" + frDate + "' AND '" + toDate + "' ";
                //    }
                //    else
                //    {
                //        com.CommandText += " Open_Date BETWEEN '" + frDate + "' AND '" + toDate + "' ";

                //    }
                //}
                #endregion

            }
            if (Day != "")
            {
                if (TerminalID != "")
                {
                    com.CommandText += " AND Report_Date between '" + Year + "-" + Month + "-" + Day + " ' and '" + Year + "-" + Month + "-" + Day + " '";
                }
                else
                {
                    com.CommandText += " Where Report_Date between '" + Year + "-" + Month + "-" + Day + " ' and '" + Year + "-" + Month + "-" + Day + " '";
                }

            }
            com.CommandText += " order by TERM_ID,Open_Date ASC";
            Console.WriteLine("com.CommandText :  " + com.CommandText.ToString());
            messageRequestList = SLAReportDaily.mapToList(con.GetDatatable(com)).ToList();
        }

        public IActionResult SlaTracking(string id, string appName, string remark, string userIP, string frDate, string toDate, string status, string maxRows)
        {
            #region binding
            SqlConnection con_binding = new SqlConnection("Data Source = 10.98.14.13; Initial Catalog = SLADB; Persist Security Info = True; User ID = sa; Password = P@ssw0rd;");
            string sql_binding_status = "select * FROM base24_master_module  WHERE MODULE_ID like 'STATUS%'";
            SqlCommand sqlcomm = new SqlCommand(sql_binding_status, con_binding);
            con_binding.Open();
            SqlDataAdapter sda1 = new SqlDataAdapter(sqlcomm);
            DataSet ds = new DataSet();
            sda1.Fill(ds);
            ViewBag.statusname = ds.Tables[0];
            List<SelectListItem> getstatusname = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in ViewBag.statusname.Rows)
            {
                getstatusname.Add(new SelectListItem { Text = @dr["MODULE_DESC"].ToString(), Value = @dr["MODULE_DESC"].ToString() });
            }
            ViewBag.Status = getstatusname;
            con_binding.Close();
            #endregion
            if (maxRows == null)
            {
                maxRows = "50";
            }
            ViewBag.maxRows = maxRows;

            //set defalut data
            if (id == null) id = "";
            ViewBag.id = id;

            if (appName == null) appName = "";
            ViewBag.appName = appName;

            if (userIP == null) userIP = "";
            ViewBag.userIP = userIP;

            if (remark == null) remark = "";
            ViewBag.remark = remark;

            if (status == null) status = "";
            ViewBag.status = status;


            if (frDate != null && toDate != null)
            {
                DateTime fr = DateTime.Parse(frDate);
                DateTime to = DateTime.Parse(toDate).AddDays(1);
                ViewBag.frDate = fr.ToString("dd/mm/yyyy");
                ViewBag.toDate = to.ToString("dd/mm/yyyy");
            }
            else
            {
                frDate = DateTime.Now.ToString("yyyy-MM-dd");
                toDate = DateTime.Now.ToString("yyyy-MM-dd");
                ViewBag.frDate = frDate;
                ViewBag.toDate = toDate;
            }

            FatchDataMainTracking(id, appName, remark, userIP, frDate, toDate, status);

            int pageSize = trackingDetails.Count;
            if (pageSize == 0)
            {
                pageSize = 1;
            }


            return View(trackingDetails.ToPagedList(1, pageSize));

        }
        private void FatchDataMainTracking(string id, string appName, string remark, string userIP, string frDate, string toDate, string status)
        {
            if (trackingDetails.Count > 0)
            {
                trackingDetails.Clear();
            }
            if (frDate != null)
            {
                frDate = frDate + " 00:00:00";
                if (toDate == "" || toDate == null)
                {

                    DateTime toDateNull = DateTime.Parse(frDate);
                    DateTime tempDate = toDateNull.AddMonths(toDateNull.Month);
                    DateTime tempDate2 = new DateTime(toDateNull.Year, toDateNull.Month, toDateNull.Day);

                    toDate = tempDate2.ToString("yyyy-MM-dd");

                }
            }

            if (toDate != null)
            {
                toDate = toDate + " 23:59:59";
                if (frDate == "" || frDate == null)
                {
                    DateTime frDateNull = DateTime.Parse(toDate);
                    DateTime tempDate = frDateNull.AddMonths(frDateNull.Month);
                    DateTime tempDate2 = new DateTime(frDateNull.Year, frDateNull.Month, 1);


                    frDate = tempDate2.ToString("yyyy-MM-dd");

                }
            }



            com.CommandText = startquery_tracking;


            if (appName != "" || remark != "" || userIP != "" || status != "" || (frDate != null && toDate != null))
            {
                com.CommandText += " WHERE ";
                if (appName != "") com.CommandText += " APPNAME = '" + appName + " ' ";

                if (remark != "")
                {
                    if (appName != "")
                    {
                        com.CommandText += " AND USER_IP = '" + userIP + "' ";
                    }
                    else
                    {
                        com.CommandText += " USER_IP = '" + userIP + "' ";
                    }

                }
                if (userIP != "")
                {
                    if (appName != "" || remark != "")
                    {
                        com.CommandText += "AND REMARK = '" + remark + "' ";
                    }
                    else
                    {
                        com.CommandText += " REMARK = '" + remark + "' ";
                    }
                }
                if (status != "")
                {
                    if (appName != "" || remark != "" || userIP != "")
                    {
                        com.CommandText += " AND STATUS = '" + status + "' ";
                    }
                    else
                    {
                        com.CommandText += " STATUS = '" + status + "' ";
                    }

                }
                if (frDate != null && toDate != null)
                {
                    if (appName != "" || remark != "" || userIP != "" || status != "")
                    {
                        com.CommandText += " AND UPDATE_DATE BETWEEN '" + frDate + "' AND '" + toDate + "' ";
                    }
                    else
                    {
                        com.CommandText += " UPDATE_DATE BETWEEN '" + frDate + "' AND '" + toDate + "' ";

                    }
                }


            }
            com.CommandText += " order by UPDATE_DATE DESC";
            Console.WriteLine("com.CommandText :  " + com.CommandText.ToString());

            trackingDetails = TrackingDetail.mapToList(con.GetDatatable(com)).ToList();

        }
        #endregion
        #region Regulator
        public IActionResult Regulator(string cmdButton, string TermID, string TermSEQ, string FrDate, string ToDate,
        string currTID, string currTSEQ, string currFr, string currTo, string lstPageSize, string currPageSize,
        int? page, string maxRows)
        {
            if (cmdButton == "Clear")
                return RedirectToAction("Regulator");
            if (String.IsNullOrEmpty(maxRows))
                ViewBag.maxRows = "50";
            else
                ViewBag.maxRows = maxRows;
            try
            {
                if (DBService.CheckDatabase())
                {
                    ViewBag.CurrentTID = GetDeviceInfoFeelview();
                    ViewBag.TermID = TermID;
                    ViewBag.ConnectDB = "true";
                }
                else
                {
                    ViewBag.ConnectDB = "false";
                }
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
                    TermID = (TermID ?? currTID);
                    TermSEQ = (TermSEQ ?? currTSEQ);
                }
                ViewBag.CurrentTerminalno = TermID;
                ViewBag.CurrentFr = (FrDate ?? currFr);
                ViewBag.CurrentTo = (ToDate ?? currTo);
                ViewBag.CurrentPageSize = (lstPageSize ?? currPageSize);
                if (null == TermID)
                    param.TerminalNo = currTID == null ? "" : currTID;
                else
                    param.TerminalNo = TermID == null ? "" : TermID;
                if (null == TermSEQ)
                    param_checkej.SerialNo = currTSEQ == null ? "" : currTSEQ;
                else
                    param_checkej.SerialNo = TermSEQ == null ? "" : TermSEQ;

                if ((FrDate == null && currFr == null) && (FrDate == "" && currFr == ""))
                {
                    param.FRDATE = DateTime.Now.ToString("yyyy-MM-dd", usaCulture) + " 00:00:00";
                }
                else
                {
                    param.FRDATE = FrDate + " 00:00:00";
                }

                if ((ToDate == null && currTo == null) && (ToDate == "" && currTo == ""))
                {
                    //param.TODATE = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
                    param.TODATE = FrDate + " 23:59:59";
                }
                else
                {
                    param.TODATE = ToDate + " 23:59:59";
                }
                if (null != lstPageSize || null != currPageSize)
                {
                    param.PAGESIZE = String.IsNullOrEmpty(lstPageSize) == true ?
                        int.Parse(currPageSize) : int.Parse(lstPageSize);
                }
                else
                {
                    param.PAGESIZE = 50;
                }
                param.TerminalNo = TermID ?? "";
                param.SerialNo = TermSEQ ?? "";
                recordset_regulator = GetRegulator_Database(param);
                if (null == recordset_regulator || recordset_regulator.Count <= 0)
                {
                    ViewBag.NoData = "true";

                }

                else
                {
                    recCnt = recordset_regulator.Count;
                    regulator_dataList = recordset_regulator;
                    param.PAGESIZE = recordset_regulator.Count;
                }


                if (recCnt > 0)
                {
                    ViewBag.Records = String.Format("{0:#,##0}", recCnt.ToString());
                }
                else
                    ViewBag.Records = "0";

                pageNum = (page ?? 1);
            }
            catch (Exception ex)
            {

            }
            return View(recordset_regulator.ToPagedList(pageNum, (int)param.PAGESIZE));
        }
        #region Constructor Regulator
        private List<Regulator> GetRegulator_Database(regulator_seek model)
        {
            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    MySqlCommand cmd = new MySqlCommand("oper_regulator", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("?terminal", model.TerminalNo));
                    cmd.Parameters.Add(new MySqlParameter("?serialno", model.SerialNo));
                    cmd.Parameters.Add(new MySqlParameter("?frdate", model.FRDATE));
                    cmd.Parameters.Add(new MySqlParameter("?todate", model.TODATE));
                    cn.Open();
                    return GetRegulatorCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                string err = ex.Message;
                return null;
            }
        }
        private List<Regulator> GetRegulatorCollectionFromReader(IDataReader reader)
        {
            string _seqNo = "";
            string terminalIDTemp = "";
            List<Regulator> resultList = new List<Regulator>();
            while (reader.Read())
            {
                resultList.Add(GetGatewayFromReader(reader));
            }
            return resultList;
        }
        private Regulator GetGatewayFromReader(IDataReader reader)
        {
            Regulator record = new Regulator();
            record.TERMID = reader["TERMID"].ToString();
            record.TERM_SEQ = reader["TERM_SEQ"].ToString();
            record.TERM_NAME = reader["TERM_NAME"].ToString();
            record.DEP100 = $"{reader["DEP100"]:n0}";
            record.DEP500 = $"{reader["DEP500"]:n0}";
            record.DEP1000 = $"{reader["DEP1000"]:n0}";
            record.WDL100 = $"{reader["WDL100"]:n0}";
            record.WDL500 = $"{reader["WDL500"]:n0}";
            record.WDL1000 = $"{reader["WDL1000"]:n0}";
            record.DIFF100 = $"{reader["DIFF100"]:n0}";
            record.DIFF500 = $"{reader["DIFF500"]:n0}";
            record.DIFF1000 = $"{reader["DIFF1000"]:n0}";
            return record;
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
        private List<string> GetListTerminalID(List<terminalAndSeq> terminalIDAndSeqList)
        {
            List<string> result = new List<string>();

            foreach (var terminalID in terminalIDAndSeqList)
            {
                result.Add(terminalID.terminalid.ToString());
            }

            return result;
        }
        private List<string> GetListTerminalSEQ(List<terminalAndSeq> terminalIDAndSeqList)
        {
            List<string> result = new List<string>();

            foreach (var terminalSEQ in terminalIDAndSeqList)
            {
                result.Add(terminalSEQ.TERM_SEQ.ToString());
            }

            return result;
        }
        private List<terminalAndSeq> GetTerminalAndSeqFromDB()
        {
            DBService _objDB = new DBService(_myConfiguration);
            DataTable _dt = new DataTable();
            List<terminalAndSeq> _result = new List<terminalAndSeq>();
            try
            {
                _dt = _objDB.GetClientData();
                foreach (DataRow _dr in _dt.Rows)
                {
                    terminalAndSeq obj = new terminalAndSeq();
                    obj.TERM_SEQ = _dr["TERM_SEQ"].ToString();
                    obj.terminalid = _dr["terminalid"].ToString();

                    _result.Add(obj);
                }



            }
            catch (Exception ex)
            { }
            return _result;
        }
        #endregion
        #endregion

        #region CheckEJSize

        public IActionResult CheckEJSize(string cmdButton, string TermID, string FrDate, string ToDate,
        string currTID, string currFr, string currTo, string lstPageSize, string currPageSize,
        int? page, string maxRows)
        {
            if (cmdButton == "Clear")
                return RedirectToAction("CheckEJSize");
            if (String.IsNullOrEmpty(maxRows))
                ViewBag.maxRows = "50";
            else
                ViewBag.maxRows = maxRows;
            try
            {
                if (DBService.CheckDatabase())
                {
                    terminalIDAndSeqList = GetTerminalAndSeqFromDB();

                    terminalIDList = GetListTerminalID(terminalIDAndSeqList);

                    if (terminalIDList != null && terminalIDList.Count > 0)
                    {
                        ViewBag.CurrentTID = terminalIDList;

                    }
                    ViewBag.ConnectDB = "true";
                }
                else
                {
                    ViewBag.ConnectDB = "false";
                }
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
                    TermID = (TermID ?? currTID);
                }
                ViewBag.CurrentTerminalno = TermID;
                ViewBag.CurrentFr = (FrDate ?? currFr);
                ViewBag.CurrentTo = (ToDate ?? currTo);
                ViewBag.CurrentPageSize = (lstPageSize ?? currPageSize);
                if (null == TermID)
                    param.TerminalNo = currTID == null ? "" : currTID;
                else
                    param.TerminalNo = TermID == null ? "" : TermID;

                if ((FrDate == null && currFr == null) && (FrDate == "" && currFr == ""))
                {
                    param.FRDATE = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
                }
                else
                {
                    param.FRDATE = FrDate + " 00:00:00";
                }

                if ((ToDate == null && currTo == null) && (ToDate == "" && currTo == ""))
                {
                    //param.TODATE = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
                    param.TODATE = FrDate + " 23:59:59";
                }
                else
                {
                    param.TODATE = ToDate + " 23:59:59";
                }
                if (null != lstPageSize || null != currPageSize)
                {
                    param.PAGESIZE = String.IsNullOrEmpty(lstPageSize) == true ?
                        int.Parse(currPageSize) : int.Parse(lstPageSize);
                }
                else
                {
                    param.PAGESIZE = 50;
                }
                param.TerminalNo = TermID ?? "";
                recordset_checkejsize = GetOffLineTermEJLog(param);
                if (null == recordset_checkejsize || recordset_checkejsize.Count <= 0)
                {
                    ViewBag.NoData = "true";

                }

                else
                {
                    recCnt = recordset_checkejsize.Count;
                    checkejsize_dataList = recordset_checkejsize;
                    param.PAGESIZE = recordset_checkejsize.Count;
                }


                if (recCnt > 0)
                {
                    ViewBag.Records = String.Format("{0:#,##0}", recCnt.ToString());
                }
                else
                    ViewBag.Records = "0";

                pageNum = (page ?? 1);
            }
            catch (Exception ex)
            {

            }
            return View(recordset_checkejsize.ToPagedList(pageNum, (int)param.PAGESIZE));
        }
        private string _sql = string.Empty;
        private string _sqlWhere = string.Empty;
        public List<ej_terminalperoffline> GetOffLineTermEJLog(regulator_seek model)
        {
            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {

                    _sql = "Select tp.*, vter.termname, vter.Location as BranchLoc, vter.Branch, dv.TERM_IP from ";
                    _sql += "ejlog_synclogfiletext tp inner join v_terminal_info vter on vter.terminalid = tp.terminalid ";
                    _sql += "left outer join fv_device_info dv on dv.TERM_ID = tp.terminalid Where 1=1 ";

                    _sqlWhere = " tp.lasttimedownload between '" + model.FRDATE + "' and '" + model.TODATE + "'";

                    if (model.TerminalNo.ToString() != "")
                        _sqlWhere += " and tp.terminalid = '" + model.TerminalNo + "'";

                    _sql += "and " + _sqlWhere;

                    _sql += "order by tp.lasttimedownload desc";

                    cn.Open();

                    MySqlCommand cmd = new MySqlCommand(_sql, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                    return GetOffLineTermEJLogCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                string err = ex.Message;
                return null;
            }
        }
        //public static List<ej_terminalperoffline> GetOffLineTermEJLog(ej_trandada_seek model, int startRowIndex, int maximumRows)
        //{
        //    List<ej_terminalperoffline> recordset = null;

        //    recordset = EJLogSiteProvider.LogSeek.GetOffLineTermEJLog(model, GetPageIndex(startRowIndex, maximumRows), maximumRows);

        //    return recordset;
        //}
        protected virtual List<ej_terminalperoffline> GetOffLineTermEJLogCollectionFromReader(IDataReader reader)
        {
            int _seqNo = 1;
            List<ej_terminalperoffline> recordlst = new List<ej_terminalperoffline>();
            while (reader.Read())
            {
                recordlst.Add(GetOffLineTermEJLogFromReader(reader, _seqNo));
                _seqNo++;
            }
            return recordlst;
        }
        protected virtual ej_terminalperoffline GetOffLineTermEJLogFromReader(IDataReader reader, int pSeqNo)
        {
            ej_terminalperoffline record = new ej_terminalperoffline();

            record.seqno = pSeqNo;
            record.terminalid = reader["terminalid"].ToString();
            record.BranchName = reader["termname"].ToString();
            record.location = reader["BranchLoc"].ToString();
            record.ipaddress = reader["TERM_IP"].ToString();
            record.lasttimeupload = reader["lasttimedownload"].ToString();
            record.trandate = Convert.ToDateTime(reader["trandateej"]);
            record.downloadsize = Convert.ToInt64(reader["lastsizedownload"]);

            return record;
        }
        #endregion
        #region Ticket
        private DateTime SetTime(DateTime date, int hour, int minute, int second)
        {

            return new DateTime(date.Year, date.Month, date.Day, hour, minute, second);
        }
        private static List<Device_info_record> GetDeviceInfoRecord()
        {

            SqlCommand com = new SqlCommand();
            com.CommandText = "SELECT * FROM [device_info_record] where STATUS = 1  order by TERM_SEQ;";
            DataTable testss = db.GetDatatable(com);

            List<Device_info_record> test = ConvertDataTableToModel.ConvertDataTable<Device_info_record>(testss);

            return test;
        }
        private static List<Device_info_record> GetDeviceInfoFeelview()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM device_info order by TERM_SEQ;";
            DataTable testss = db_fv.GetDatatable(com);

            List<Device_info_record> test = ConvertDataTableToModel.ConvertDataTable<Device_info_record>(testss);

            return test;
        }
        private static List<TicketJob> GetJobNumber(string frommonth, string tomonth)
        {

            SqlCommand com = new SqlCommand();
            com.CommandText = "SELECT Job_No FROM t_tsd_JobDetail where Open_Date between '" + frommonth + " 00:00:00' and '" + tomonth + " 23:59:59'"; ;
            DataTable testss = db.GetDatatable(com);

            List<TicketJob> test = ConvertDataTableToModel.ConvertDataTable<TicketJob>(testss);

            return test;
        }
        [HttpGet]
        public IActionResult TicketManagement(string termid, string ticket, DateTime? todate, DateTime? fromdate, string mainproblem, string terminaltype, string jobno, int? maxRows, string cmdButton)
        {

            int pageSize = 20;
            recordset_ticketManagement = new List<TicketManagement>();
            pageSize = maxRows.HasValue ? maxRows.Value : 100;
            termid = termid ?? "";
            mainproblem = mainproblem ?? "";
            terminaltype = terminaltype ?? "";
            jobno = jobno ?? "";


            if (fromdate.HasValue && todate.HasValue)
            {
                if (fromdate <= todate)
                {
                    if ((DateTime)todate > DateTime.Now)
                    {

                        todate = SetTime(DateTime.Now, 23, 59, 59);
                    }
                    DateTime _fromdate = (DateTime)fromdate;
                    DateTime _todate = (DateTime)todate;
                    recordset_ticketManagement = GetTicketManagementFromSqlServer(_fromdate.ToString("yyyy-MM-dd"), _todate.ToString("yyyy-MM-dd"), termid, mainproblem, jobno, terminaltype);
                    ticket_dataList = recordset_ticketManagement;
                }
            }

            DateTime job_fromdate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
            DateTime job_todate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            ViewBag.CurrentJobNo = GetJobNumber(job_fromdate.ToString("yyyy-MM-dd"), job_todate.ToString("yyyy-MM-dd"));
            ViewBag.JobNo = jobno;
            ViewBag.countrow = recordset_ticketManagement.Count;
            ViewBag.maxRows = pageSize;
            ViewBag.CurrentTermID = GetDeviceInfoRecord();
            ViewBag.pageSize = pageSize;
            ViewBag.TERM_ID = termid;
            ViewBag.terminaltype = terminaltype;
            ViewBag.mainproblem = mainproblem;
            if (fromdate != null)
            {
                ViewBag.CurrentFr = fromdate;
            }
            else
            {
                ViewBag.CurrentFr = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1).ToString("yyyy-MM-dd");
            }
            if (todate != null)
            {
                ViewBag.CurrentTo = todate;
            }
            else
            {
                ViewBag.CurrentTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)).ToString("yyyy-MM-dd");
            }
            if (cmdButton == "Clear")
            {
                ViewBag.countrow = 0;
                return View();
            }
            return View(recordset_ticketManagement);
        }
        public List<TicketManagement> GetTicketManagementFromSqlServer(string fromdate, string todate, string termid, string mainproblem, string jobno, string terminaltype)
        {
            List<TicketManagement> dataList = new List<TicketManagement>();
            DateTime fromdateTimeValue = DateTime.ParseExact(fromdate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            string fordb_fromdate = fromdateTimeValue.ToString("yyyyMM");
            DateTime todateTimeValue = DateTime.ParseExact(todate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            string fordb_todate = todateTimeValue.ToString("yyyyMM");
            int diff = 0;
            int fromyear = int.Parse(fordb_fromdate.Substring(0, 4));
            int frommonth = int.Parse(fordb_fromdate.Substring(4, 2));
            DateTime firstDayOfMonth = new DateTime(fromyear, frommonth, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            static List<string> GenerateMonthList(string fromDate, string toDate)
            {
                List<string> monthList = new List<string>();
                DateTime fromDateDT = DateTime.ParseExact(fromDate, "yyyyMM", null);
                DateTime toDateDT = DateTime.ParseExact(toDate, "yyyyMM", null);
                while (fromDateDT <= toDateDT)
                {
                    monthList.Add(fromDateDT.ToString("yyyyMM"));
                    fromDateDT = fromDateDT.AddMonths(1);
                }
                return monthList;
            }
            List<string> monthList = GenerateMonthList(fordb_fromdate, fordb_todate);
            //string sqlQuery = " SELECT a.Open_Date,a.Appointment_Date,a.Closed_Repair_Date,a.Down_Time,a.Actual_Open_Date,a.Actual_Appointment_Date, ";
            //sqlQuery += " a.Actual_Closed_Repair_Date,a.Actual_Down_Time,a.Status,a.TERM_ID,b.TERM_SEQ,b.TERM_NAME,b.MODEL_NAME,b.PROVINCE,a.Problem_Detail,a.Solving_Program, ";
            //sqlQuery += " a.Service_Team,a.Contact_Name_Branch_CIT,a.Open_By,a.Remark,a.Job_No,a.Aservice_Status,a.Service_Type,a.Open_Name,a.Assign_By, ";
            //sqlQuery += " a.Zone_Area,a.Main_Problem,a.Sub_Problem,a.Main_Solution,a.Sub_Solution,a.Part_of_use,a.TechSupport,a.CIT_Request,a.Terminal_Status ";
            //sqlQuery += " FROM t_tsd_JobDetail_" + fordb_fromdate + " a left join device_info_record b on a.TERM_ID = b.TERM_ID WHERE ";
            //if(fordb_fromdate != fordb_todate)
            //{
            //    if (fromdate != "")
            //    {
            //        sqlQuery += " a.Open_Date between '" + DateTime.Parse(fromdate).ToString("yyyy-MM-dd") + " 00:00:00' and '" + lastDayOfMonth.ToString("yyyy-MM-dd") + " 23:59:59' ";
            //    }
            //    else
            //    {
            //        sqlQuery += " a.Open_Date between '" + firstDayOfMonth.ToString("yyyy-MM-dd") + " 00:00:00' and '" + lastDayOfMonth.ToString("yyyy-MM-dd") + " 23:59:59' ";
            //    }
            //}
            //else
            //{
            //    sqlQuery += " a.Open_Date between '" + DateTime.Parse(fromdate).ToString("yyyy-MM-dd") + " 00:00:00' and '" + DateTime.Parse(todate).ToString("yyyy-MM-dd") + " 23:59:59' ";
            //}
            string sqlQuery = " SELECT a.Open_Date,a.Appointment_Date,a.Closed_Repair_Date,a.Down_Time,a.Actual_Open_Date,a.Actual_Appointment_Date, ";
            sqlQuery += " a.Actual_Closed_Repair_Date,a.Actual_Down_Time,a.Status,a.TERM_ID,b.TERM_SEQ,b.TERM_NAME,b.MODEL_NAME,b.PROVINCE,a.Problem_Detail,a.Solving_Program, ";
            sqlQuery += " a.Service_Team,a.Contact_Name_Branch_CIT,a.Open_By,a.Remark,a.Job_No,a.Aservice_Status,a.Service_Type,a.Open_Name,a.Assign_By, ";
            sqlQuery += " a.Zone_Area,a.Main_Problem,a.Sub_Problem,a.Main_Solution,a.Sub_Solution,a.Part_of_use,a.TechSupport,a.CIT_Request,a.Terminal_Status ";
            sqlQuery += " FROM t_tsd_JobDetail a left join device_info_record b on a.TERM_ID = b.TERM_ID WHERE ";
            sqlQuery += " a.Open_Date between '" + DateTime.Parse(fromdate).ToString("yyyy-MM-dd") + " 00:00:00' and '" + DateTime.Parse(todate).ToString("yyyy-MM-dd") + " 23:59:59' ";

            if (termid != "")
            {
                sqlQuery += " and a.TERM_ID = '" + termid + "' ";
            }
            if (jobno != "")
            {
                sqlQuery += " and a.Job_No ='" + jobno + "' ";
            }
            if (mainproblem != "")
            {
                sqlQuery += " and a.Main_Problem like '%" + mainproblem + "%' ";
            }
            if (terminaltype != "")
            {
                sqlQuery += " and b.TERM_ID like '%" + terminaltype + "%' ";
            }
            sqlQuery += " order by a.Open_Date asc";
            //if (fordb_fromdate != fordb_todate)
            //{

            //    foreach (string month in monthList)
            //    {
            //        if (diff > 0)
            //        {
            //            sqlQuery += " UNION ALL SELECT a.Open_Date,a.Appointment_Date,a.Closed_Repair_Date,a.Down_Time,a.Actual_Open_Date,a.Actual_Appointment_Date, ";
            //            sqlQuery += " a.Actual_Closed_Repair_Date,a.Actual_Down_Time,a.Status,a.TERM_ID,b.TERM_SEQ,b.TERM_NAME,b.MODEL_NAME,b.PROVINCE,a.Problem_Detail,a.Solving_Program, ";
            //            sqlQuery += " a.Service_Team,a.Contact_Name_Branch_CIT,a.Open_By,a.Remark,a.Job_No,a.Aservice_Status,a.Service_Type,a.Open_Name,a.Assign_By, ";
            //            sqlQuery += " a.Zone_Area,a.Main_Problem,a.Sub_Problem,a.Main_Solution,a.Sub_Solution,a.Part_of_use,a.TechSupport,a.CIT_Request,a.Terminal_Status ";
            //            sqlQuery += " FROM t_tsd_JobDetail_" + month + " a left join device_info_record b on a.TERM_ID = b.TERM_ID WHERE ";
            //            if (month != fordb_todate)
            //            {
            //                int checkyear = int.Parse(month.Substring(0, 4));
            //                int checkmonth = int.Parse(month.Substring(4, 2));
            //                DateTime _firstDayOfMonth = new DateTime(checkyear, checkmonth, 1);
            //                DateTime _lastDayOfMonth = _firstDayOfMonth.AddMonths(1).AddDays(-1);
            //                sqlQuery += " a.Open_Date between '" + _firstDayOfMonth.ToString("yyyy-MM-dd") + " 00:00:00' and '" + _lastDayOfMonth.ToString("yyyy-MM-dd") + " 23:59:59' ";
            //            }
            //            else
            //            {
            //                int checkyear = int.Parse(month.Substring(0, 4));
            //                int  checkmonth = int.Parse(month.Substring(4, 2));
            //                DateTime _firstDayOfMonth = new DateTime(checkyear, checkmonth, 1);
            //                DateTime _lastDayOfMonth = _firstDayOfMonth.AddMonths(1).AddDays(-1);
            //                sqlQuery += " a.Open_Date between '" + _firstDayOfMonth.ToString("yyyy-MM-dd") + " 00:00:00' and '" + DateTime.Parse(todate).ToString("yyyy-MM-dd") + " 23:59:59' ";
            //            }
            //            if (termid != "")
            //            {
            //                sqlQuery += " and a.TERM_ID = '" + termid + "' ";
            //            }
            //            if (jobno != "")
            //            {
            //                sqlQuery += " and a.Job_No ='" + jobno + "' ";
            //            }
            //            if (mainproblem != "")
            //            {
            //                sqlQuery += " and a.Main_Problem like '%" + mainproblem + "%' ";
            //            }
            //            if (terminaltype != "")
            //            {
            //                sqlQuery += " and b.TERM_ID like '%" + terminaltype + "%' ";
            //            }

            //        }

            //        diff++;
            //    }

            //}
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
                                dataList.Add(GetTicketManagementFromReader(reader));
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
        protected virtual TicketManagement GetTicketManagementFromReader(IDataReader reader)
        {
            TicketManagement record = new TicketManagement();
            if (reader["Open_Date"] != DBNull.Value)
            {
                DateTime xValue = Convert.ToDateTime(reader["Open_Date"]);
                record.Open_Date = xValue.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                record.Open_Date = "-";
            }
            if (reader["Appointment_Date"] != DBNull.Value)
            {
                DateTime xValue = Convert.ToDateTime(reader["Appointment_Date"]);
                record.Appointment_Date = xValue.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                record.Appointment_Date = "-";
            }
            if (reader["Closed_Repair_Date"] != DBNull.Value)
            {
                DateTime xValue = Convert.ToDateTime(reader["Closed_Repair_Date"]);
                record.Closed_Repair_Date = xValue.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                record.Closed_Repair_Date = "-";
            }
            record.Down_Time = reader["Down_Time"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Down_Time"].ToString()) ? "-" : reader["Down_Time"].ToString();
            if (reader["Actual_Open_Date"] != DBNull.Value)
            {
                DateTime xValue = Convert.ToDateTime(reader["Actual_Open_Date"]);
                record.Actual_Open_Date = xValue.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                record.Actual_Open_Date = "-";
            }
            if (reader["Actual_Appointment_Date"] != DBNull.Value)
            {
                DateTime xValue = Convert.ToDateTime(reader["Actual_Appointment_Date"]);
                record.Actual_Appointment_Date = xValue.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                record.Actual_Appointment_Date = "-";
            }
            if (reader["Actual_Closed_Repair_Date"] != DBNull.Value)
            {
                DateTime xValue = Convert.ToDateTime(reader["Actual_Closed_Repair_Date"]);
                record.Actual_Appointment_Date = xValue.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                record.Actual_Closed_Repair_Date = "-";
            }
            record.Actual_Down_Time = reader["Actual_Down_Time"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Actual_Down_Time"].ToString()) ? "-" : reader["Actual_Down_Time"].ToString();
            record.Status = reader["Status"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Status"].ToString()) ? "-" : reader["Status"].ToString();
            record.TERM_ID = reader["TERM_ID"] is DBNull ? "-" : string.IsNullOrEmpty(reader["TERM_ID"].ToString()) ? "-" : reader["TERM_ID"].ToString();
            record.TERM_SEQ = reader["TERM_SEQ"] is DBNull ? "-" : string.IsNullOrEmpty(reader["TERM_SEQ"].ToString()) ? "-" : reader["TERM_SEQ"].ToString();
            record.MODEL_NAME = reader["MODEL_NAME"] is DBNull ? "-" : string.IsNullOrEmpty(reader["MODEL_NAME"].ToString()) ? "-" : reader["MODEL_NAME"].ToString();
            record.PROVINCE = reader["PROVINCE"] is DBNull ? "-" : string.IsNullOrEmpty(reader["PROVINCE"].ToString()) ? "-" : reader["PROVINCE"].ToString();
            record.TERM_NAME = reader["TERM_NAME"] is DBNull ? "-" : string.IsNullOrEmpty(reader["TERM_NAME"].ToString()) ? "-" : reader["TERM_NAME"].ToString();
            record.Problem_Detail = reader["Problem_Detail"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Problem_Detail"].ToString()) ? "-" : reader["Problem_Detail"].ToString().Replace("\n", "").Replace("\r", "").Replace(",", "|").Replace("/", "|");
            record.Solving_Program = reader["Solving_Program"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Solving_Program"].ToString()) ? "-" : reader["Solving_Program"].ToString().Replace("\n", "").Replace("\r", "").Replace(",", "|").Replace("/", "|");
            record.Service_Team = reader["Service_Team"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Service_Team"].ToString()) ? "-" : reader["Service_Team"].ToString();
            record.Contact_Name_Branch_CIT = reader["Contact_Name_Branch_CIT"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Contact_Name_Branch_CIT"].ToString()) ? "-" : reader["Contact_Name_Branch_CIT"].ToString().Replace("\n", "").Replace("\r", "").Replace(",", "|").Replace("/", "|");
            record.Open_By = reader["Open_By"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Open_By"].ToString()) ? "-" : reader["Open_By"].ToString().Replace("\n", "").Replace("\r", "").Replace(",", "|").Replace("/", "|");
            record.Remark = reader["Remark"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Remark"].ToString()) ? "-" : reader["Remark"].ToString();
            record.Job_No = reader["Job_No"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Job_No"].ToString()) ? "-" : reader["Job_No"].ToString();
            record.Aservice_Status = reader["Aservice_Status"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Aservice_Status"].ToString()) ? "-" : reader["Aservice_Status"].ToString();
            record.Service_Type = reader["Service_Type"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Service_Type"].ToString()) ? "-" : reader["Service_Type"].ToString();
            record.Open_Name = reader["Open_Name"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Open_Name"].ToString()) ? "-" : reader["Open_Name"].ToString();
            record.Assign_By = reader["Assign_By"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Assign_By"].ToString()) ? "-" : reader["Assign_By"].ToString();
            record.Zone_Area = reader["Zone_Area"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Zone_Area"].ToString()) ? "-" : reader["Zone_Area"].ToString();
            record.Main_Problem = reader["Main_Problem"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Main_Problem"].ToString()) ? "-" : reader["Main_Problem"].ToString();
            record.Sub_Problem = reader["Sub_Problem"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Sub_Problem"].ToString()) ? "-" : reader["Sub_Problem"].ToString();
            record.Main_Solution = reader["Main_Solution"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Main_Solution"].ToString()) ? "-" : reader["Main_Solution"].ToString();
            record.Sub_Solution = reader["Sub_Solution"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Sub_Solution"].ToString()) ? "-" : reader["Sub_Solution"].ToString();
            record.Part_of_use = reader["Part_of_use"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Part_of_use"].ToString()) ? "-" : reader["Part_of_use"].ToString();
            record.TechSupport = reader["TechSupport"] is DBNull ? "-" : string.IsNullOrEmpty(reader["TechSupport"].ToString()) ? "-" : reader["TechSupport"].ToString();
            record.CIT_Request = reader["CIT_Request"] is DBNull ? "-" : string.IsNullOrEmpty(reader["CIT_Request"].ToString()) ? "-" : reader["CIT_Request"].ToString();
            record.Terminal_Status = reader["Terminal_Status"] is DBNull ? "-" : string.IsNullOrEmpty(reader["Terminal_Status"].ToString()) ? "-" : reader["Terminal_Status"].ToString();
            return record;
        }
        #endregion
        #region CheckEJLastUpdate
        public IActionResult CheckEJLastUpdate(string cmdButton, string termid, string Hours, string TermSEQ, string TerminalType, string status,
        string currTID, string currHours, string currTSEQ, string lstPageSize, string currPageSize,
        int? page, string maxRows)
        {
            if (cmdButton == "Clear")
                return RedirectToAction("CheckEJLastUpdate");
            if (String.IsNullOrEmpty(maxRows))
                ViewBag.maxRows = "50";
            else
                ViewBag.maxRows = maxRows;
            if (String.IsNullOrEmpty(TerminalType))
            {
                ViewBag.TerminalType = "";
            }
            else
            {
                ViewBag.TerminalType = TerminalType;
            }
            if (String.IsNullOrEmpty(status))
            {
                ViewBag.status = "";
            }
            else
            {
                ViewBag.status = status;
            }
            param_checkej.status = ViewBag.status;
            param_checkej.TerminalType = ViewBag.TerminalType;
            //add hours
            List<String> items = new List<String>();
            DateTime currentTime = DateTime.Now;

            for (int i = 0; i <= currentTime.Hour; i++)
            {

                items.Add(i.ToString());
            }

            ViewBag.CurrentHours = items;
            try
            {
                ViewBag.CurrentTID = GetDeviceInfoFeelview();
                ViewBag.TERM_ID = termid;
                if (null == termid && null == Hours && null == page)
                {
                    page = 1;
                }
                else
                {
                    // Return temp value back to it own variable
                    Hours = (Hours ?? currHours);
                    termid = (termid ?? currTID);
                    TermSEQ = (TermSEQ ?? currTSEQ);
                }
                ViewBag.CurrentTerminalno = termid;
                //ViewBag.CurrentHours = currHours;
                ViewBag.CurrentPageSize = (lstPageSize ?? currPageSize);
                if (null == termid)
                    param_checkej.TerminalNo = currTID == null ? "" : currTID;
                else
                    param_checkej.TerminalNo = termid == null ? "" : termid;
                if (null == TermSEQ)
                    param_checkej.SerialNo = currTSEQ == null ? "" : currTSEQ;
                else
                    param_checkej.SerialNo = TermSEQ == null ? "" : TermSEQ;

                if ((Hours == null && currHours == null))
                {

                    param_checkej.Hours = currHours == null ? "" : currHours;
                }
                else
                {
                    param_checkej.Hours = Hours == null ? "" : Hours;
                }

                if (null != lstPageSize || null != currPageSize)
                {
                    param_checkej.PAGESIZE = String.IsNullOrEmpty(lstPageSize) == true ?
                        int.Parse(currPageSize) : int.Parse(lstPageSize);
                }
                else
                {
                    param_checkej.PAGESIZE = 50;
                }
                param_checkej.TerminalNo = termid ?? "";
                param_checkej.SerialNo = TermSEQ ?? "";
                recordset_ejloglastupdate = GetEJLastUpdate(param_checkej);
                if (null == recordset_ejloglastupdate || recordset_ejloglastupdate.Count <= 0)
                {
                    ViewBag.NoData = "true";

                }

                else
                {
                    recCnt = recordset_ejloglastupdate.Count;
                    ejloglastupdate_datalist = recordset_ejloglastupdate;
                    param_checkej.PAGESIZE = recordset_ejloglastupdate.Count;

                }


                if (recCnt > 0)
                {
                    ViewBag.Records = String.Format("{0:#,##0}", recCnt.ToString());
                }
                else
                    ViewBag.Records = "0";

                pageNum = (page ?? 1);
            }
            catch (Exception ex)
            {

            }
            return View(recordset_ejloglastupdate.ToPagedList(pageNum, (int)param_checkej.PAGESIZE));
        }
        public List<ejloglastupdate> GetEJLastUpdate(ejchecksize_seek model)
        {

            DateTime currentDate = DateTime.Now;
            string formattedDate = currentDate.ToString("yyyyMMdd", usaCulture);
            string hours = "";
            if (model.Hours != "")
            {
                hours = model.Hours;
            }
            else
            {
                hours = "1";
            }
            try
            {
                using (MySqlConnection cn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection")))
                {

                    _sql = "SELECT a.TERM_ID,b.TERM_NAME,b.TERM_SEQ,a.UPDATE_DATE,c.LASTTRAN_TIME, ";
                    _sql += " CASE WHEN(c.DEVICE_STATUS_EVENT_ID != 'E1005' and c.DEVICE_STATUS_EVENT_ID != 'E1156' and c.DEVICE_STATUS_EVENT_ID != 'E1006' and c.DEVICE_STATUS_EVENT_ID != 'E1036') THEN 'online' ELSE 'offline' END as terminalstatus,   ";
                    _sql += " CASE WHEN c.LASTTRAN_TIME > DATE_ADD(a.UPDATE_DATE, INTERVAL 1 HOUR) THEN 'warning'ELSE ''END AS status";
                    _sql += " FROM gsb_adm_fv.ejournal_upload_log as a";
                    _sql += " left join gsb_adm_fv.device_info as b on a.TERM_ID = b.TERM_ID ";
                    _sql += " left join gsb_adm_fv.device_status_info as c on a.TERM_ID = c.TERM_ID ";
                    _sql += " WHERE c.LASTTRAN_TIME is not null and a.UPDATE_DATE >= DATE(NOW()) AND TIMESTAMPDIFF(HOUR, a.UPDATE_DATE, NOW()) > " + hours + " AND a.FILE_NAME = 'EJ" + formattedDate + ".txt'";

                    if (model.TerminalNo.ToString() != "")
                    {
                        _sqlWhere += " and a.TERM_ID = '" + model.TerminalNo + "'";
                    }
                    if (model.SerialNo.ToString() != "")
                    {
                        _sqlWhere += " and b.TERM_SEQ = '" + model.SerialNo + "'";
                    }
                    if (model.status.ToString() == "warning")
                    {
                        _sqlWhere += " and (TIMESTAMPDIFF(HOUR,a.UPDATE_DATE,c.LASTTRAN_TIME ) > 1 OR DATEDIFF(DATE(a.UPDATE_DATE), DATE(c.LASTTRAN_TIME)) > 0)";
                    }
                    if (model.TerminalType.ToString() != "")
                    {
                        _sqlWhere += " and a.TERM_ID like '%" + model.TerminalType + "'";
                    }
                    _sql += _sqlWhere;
                    _sql += " group by a.TERM_ID order by c.LASTTRAN_TIME > DATE_ADD(a.UPDATE_DATE, INTERVAL 1 HOUR) desc ,b.TERM_SEQ asc";

                    cn.Open();

                    MySqlCommand cmd = new MySqlCommand(_sql, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                    return GetLastUpdateTermEJLogCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                string err = ex.Message;
                return null;
            }
        }
        protected virtual List<ejloglastupdate> GetLastUpdateTermEJLogCollectionFromReader(IDataReader reader)
        {
            int _seqNo = 1;
            List<ejloglastupdate> recordlst = new List<ejloglastupdate>();
            while (reader.Read())
            {
                recordlst.Add(GetLastUpdateTermEJLogFromReader(reader));
                _seqNo++;
            }
            return recordlst;
        }
        protected virtual ejloglastupdate GetLastUpdateTermEJLogFromReader(IDataReader reader)
        {
            ejloglastupdate record = new ejloglastupdate();
            if (reader["LASTTRAN_TIME"] != null)
            {
                record.lastran_date = DateTime.Parse(reader["LASTTRAN_TIME"].ToString()).ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            }
            else
            {
                record.lastran_date = DateTime.Parse(reader["UPDATE_DATE"].ToString()).ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }
            record.term_id = reader["TERM_ID"].ToString();
            record.term_seq = reader["TERM_SEQ"].ToString();
            record.term_name = reader["TERM_NAME"].ToString();
            //record.update_date = reader["UPDATE_DATE"].ToString();
            record.update_date = DateTime.Parse(reader["UPDATE_DATE"].ToString()).ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            record.lastran_date = DateTime.Parse(reader["LASTTRAN_TIME"].ToString()).ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            record.status = reader["status"].ToString();
            record.terminalstatus = reader["terminalstatus"].ToString();
            return record;
        }
        #endregion
        #region Excel

        [HttpPost]
        public ActionResult Regulator_ExportExc()
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

                if (regulator_dataList == null || regulator_dataList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;
                ExcelUtilities_Regulator obj = new ExcelUtilities_Regulator(param);


                // Session["PrefixRep"] = "EJAddTran";

                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(regulator_dataList);



                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;



                fname = "Regulator_" + DateTime.Now.ToString("yyyyMMdd");

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
        public ActionResult DownloadExportFile(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            string tsDate = "";
            string teDate = "";
            try
            {




                fname = "Regulator_" + DateTime.Now.ToString("yyyyMMdd");

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
        #region Excel checklastupdate

        [HttpPost]
        public ActionResult CheckLastUpdate_ExportExc()
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

                if (ejloglastupdate_datalist == null || ejloglastupdate_datalist.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;
                ExcelUtilities_CheckLastUpdate obj = new ExcelUtilities_CheckLastUpdate(param_checkej);


                // Session["PrefixRep"] = "EJAddTran";

                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(ejloglastupdate_datalist);



                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;



                fname = "CheckLastUpdate_" + DateTime.Now.ToString("yyyyMMdd");

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
        public ActionResult DownloadExportFile_CheckLastUpdate(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            string tsDate = "";
            string teDate = "";
            try
            {




                fname = "CheckLastUpdate_" + DateTime.Now.ToString("yyyyMMdd");

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
        #region Excel Ticket

        [HttpPost]
        public ActionResult Ticket_ExportExc()
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

                if (ticket_dataList == null || ticket_dataList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;
                ExcelUtilities_Regulator obj = new ExcelUtilities_Regulator(param);


                // Session["PrefixRep"] = "EJAddTran";

                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(ticket_dataList);



                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;



                fname = "Ticket_" + DateTime.Now.ToString("yyyyMMdd");

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
        public ActionResult Ticket_DownloadExportFile(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            string tsDate = "";
            string teDate = "";
            try
            {




                fname = "Ticket_" + DateTime.Now.ToString("yyyyMMdd");

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
