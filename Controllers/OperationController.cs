﻿using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using SLAManagement.Data;
using SLA_Management.Models;
using PagedList;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using SLA_Management.Models.TermProbModel;
using MySql.Data.MySqlClient;
using System.Data.Common;
using SLA_Management.Data.TermProbDB;
using SLA_Management.Data.TermProbDB.ExcelUtilitie;
using MySqlX.XDevAPI.Common;

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
        private int pageNum = 1;
        private List<terminalAndSeq> terminalIDAndSeqList = new List<terminalAndSeq>();
        private List<string> terminalIDList = new List<string>();
        private List<string> terminalSEQList = new List<string>();
        private DBService dBService;
        #endregion
        public OperationController(IConfiguration myConfiguration)
        {
            
            _myConfiguration = myConfiguration;
            dBService = new DBService(_myConfiguration);
            con = new ConnectSQL_Server(_myConfiguration["ConnectionStrings:DefaultConnection"]);
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
            FatchDataMainDowntime_m(TerminalID, TerminalSEQ, Month,Year,Orderby,Sortby);
            int pageSize = con.GetCountTable("SELECT COUNT(*) FROM " + slamonthlydowntime_table + "_" + Year + Month);
            if (pageSize == 0)
            {
                pageSize = 1;
            }
            return View(messageRequestList_m.ToPagedList(1, pageSize));
        }
        private void FatchDataMainDowntime_m(string TerminalID, string TerminalSEQ, string Month, string Year,string Orderby, string Sortby)
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
            if (TerminalSEQ != "" || TerminalID !="")
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
            if(Orderby != "TERM_SEQ")
            {
                Orderby += ",TERM_SEQ";
            }
            com.CommandText += " order by " + Orderby + " " +Sortby;
            Console.WriteLine("com.CommandText :  " + com.CommandText.ToString());
            messageRequestList_m = SLAReportMonthly.mapToList(con.GetDatatable(com)).ToList();
        }
        public IActionResult SlaReportDaily(string TerminalID, string Month, string Year, string chk_date,string date, string maxRows)
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
            if(Month != null)
            {
                Month = DateTime.ParseExact(Month, "MMMM", CultureInfo.CurrentCulture).Month.ToString("D2");
            }
            if(chk_date == "true")
            {
                if (date != null)
                {
                    date = date.Replace("-", "");
                    
                    Year = date.Substring(0,4);
                    Month = date.Substring(4, 2);
                    Day = date.Substring(6, 2);
                    Console.WriteLine("test date data : " + date + "Year : " + Year + "Month : " + Month + "Day : " + Day);

                }
            }
            FatchDataMainDowntime(TerminalID, Day ,Month, Year);
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
            int pageSize = con.GetCountTable("SELECT COUNT(*) FROM "+ sladailydowntime_table);
            if (pageSize == 0)
            {
                pageSize = 1;
            }
            //FatchDataMainDowntime(TerminalID, Problem_Detail, frDate, toDate, status);
            return View(messageRequestList.ToPagedList(1, pageSize));
        }
        private void FatchDataMainDowntime(string TerminalID, string Day,string Month, string Year)
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
            if(Month == null) Month = "";
            if (Year == null) Year = "";
            if(Month != "" && Year != "")
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
                if(TerminalID != "")
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
            DataSet ds =  new DataSet();
            sda1.Fill(ds);
            ViewBag.statusname = ds.Tables[0];
            List<SelectListItem> getstatusname = new List<SelectListItem>();
            foreach(System.Data.DataRow dr in ViewBag.statusname.Rows)
            {
                getstatusname.Add(new SelectListItem { Text = @dr["MODULE_DESC"].ToString(), Value = @dr["MODULE_DESC"].ToString() });
            }
            ViewBag.Status = getstatusname;
            con_binding.Close();
            #endregion
            if (maxRows == null)
            {
                maxRows = "20";
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
        public IActionResult Regulator(string cmdButton, string TermID, string FrDate, string ToDate,
        string currTID, string currFr, string currTo, string lstPageSize, string currPageSize, 
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

        #region CheckEJLastUpdate
        public IActionResult CheckEJLastUpdate(string cmdButton, string TermID, string Hours,string TermSEQ,string TerminalType,string status,
        string currTID, string currHours, string currTSEQ,string lstPageSize, string currPageSize,
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
            param_checkej.TerminalType= ViewBag.TerminalType;
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
                if (DBService.CheckDatabase())
                {
                    terminalIDAndSeqList = GetTerminalAndSeqFromDB();

                    terminalIDList = GetListTerminalID(terminalIDAndSeqList);
                    terminalSEQList = GetListTerminalSEQ(terminalIDAndSeqList);

                    if (terminalIDList != null && terminalIDList.Count > 0)
                    {
                        ViewBag.CurrentTID = terminalIDList;

                    }
                    if (terminalSEQList != null && terminalSEQList.Count > 0)
                    {
                        ViewBag.CurrentTSEQ = terminalSEQList;

                    }
                    ViewBag.ConnectDB = "true";
                }
                else
                {
                    ViewBag.ConnectDB = "false";
                }
                if (null == TermID && null == Hours  && null == page)
                {
                    page = 1;
                }
                else
                {
                    // Return temp value back to it own variable
                    Hours = (Hours ?? currHours);
                    TermID = (TermID ?? currTID);
                    TermSEQ = (TermSEQ ?? currTSEQ);
                }
                ViewBag.CurrentTerminalno = TermID;
                //ViewBag.CurrentHours = currHours;
                ViewBag.CurrentPageSize = (lstPageSize ?? currPageSize);
                if (null == TermID)
                    param_checkej.TerminalNo = currTID == null ? "" : currTID;
                else
                    param_checkej.TerminalNo = TermID == null ? "" : TermID;
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
                param_checkej.TerminalNo = TermID ?? "";
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
            string formattedDate = currentDate.ToString("yyyyMMdd");
            string hours = "";
            if(model.Hours != "")
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
                    _sql += " IF(TIMESTAMPDIFF(HOUR,c.LASTTRAN_TIME,a.UPDATE_DATE ) > 1, 'warning', '') AS status FROM gsb_adm_fv.ejournal_upload_log as a ";
                    _sql += " left join gsb_adm_fv.device_info as b on a.TERM_ID = b.TERM_ID ";
                    _sql += " left join gsb_adm_fv.device_status_info as c on a.TERM_ID = c.TERM_ID ";
                    _sql += " WHERE a.UPDATE_DATE >= DATE(NOW()) AND TIMESTAMPDIFF(HOUR, a.UPDATE_DATE, NOW()) > "+hours+" AND a.FILE_NAME = 'EJ"+ formattedDate + ".txt'";

                    if (model.TerminalNo.ToString() != "")
                    {
                        _sqlWhere += " and a.TERM_ID = '" + model.TerminalNo + "'";
                    }
                    if(model.SerialNo.ToString() != "")
                    {
                        _sqlWhere += " and b.TERM_SEQ = '" + model.SerialNo + "'";
                    }
                    if (model.status.ToString() == "warning")
                    {
                        _sqlWhere += " and TIMESTAMPDIFF(HOUR,c.LASTTRAN_TIME,a.UPDATE_DATE ) > 1";
                    }
                    if(model.TerminalType.ToString() != "")
                    {
                        _sqlWhere += " and a.TERM_ID like '%" + model.TerminalType +"'";
                    }   
                    _sql +=  _sqlWhere;
                    _sql += " order by a.UPDATE_DATE asc";

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

            record.term_id = reader["TERM_ID"].ToString();
            record.term_seq = reader["TERM_SEQ"].ToString();
            record.term_name = reader["TERM_NAME"].ToString();
            //record.update_date = reader["UPDATE_DATE"].ToString();
            record.update_date = DateTime.Parse(reader["UPDATE_DATE"].ToString()).ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            record.lastran_date = DateTime.Parse(reader["LASTTRAN_TIME"].ToString()).ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            record.status = reader["status"].ToString();
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
    }
}
