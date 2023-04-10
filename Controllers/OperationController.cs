using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using SLAManagement.Data;
using SLA_Management.Models;
using PagedList;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

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
        public OperationController(IConfiguration myConfiguration)
        {
            _myConfiguration = myConfiguration;
            con = new ConnectSQL_Server(_myConfiguration["ConnectionStrings:DefaultConnection"]);
            sladailydowntime_table = "sla_reportdaily";
            slatracking_table = "sla_tracking";
            slamonthlydowntime_table = "sla_reportmonthly";
            startquery_reportdaily = "SELECT TOP 5000 ID,Report_Date, Open_Date, Appointment_Date, Closed_Repair_Date, Down_Time, AS_OpenDate, AS_AppointmentDate, AS_CloseRepairDate, AS_Downtime, Discount, Net_Downtime, AS_Discription, AS_CIT_Request, AS_Service_PM, Status, TERM_ID, Model, TERM_SEQ, Province, Location, Problem_Detail, Solving_Program, Service_Team, Contact_Name_Branch_CIT, Open_By, Remark FROM ";
            startquery_tracking = "SELECT TOP 5000 ID,APPNAME,UPDATE_DATE,STATUS,REMARK,USER_IP FROM " + slatracking_table;
            startquery_reportmonthly = "SELECT TOP 5000 t1.ID,t1.TERM_ID,t1.TERM_SEQ,t1.LOCATION,t1.PROVINCE,t1.INSTALL_LOT,t1.REPLENISHMENT_DATE,t1.STARTSERVICE_DATE,t1.TOTALSERVICEDAY,t1.SERVICE_GROUP,t1.SERVICE_DATE,t1.SERVICEDAY_CHARGE,t1.SERVICETIME_CHARGE_PERDAY,t1.SERVICETIME_PERMONTH_HOUR,t1.SERVICETIME_PERHOUR_MINUTE,t1.TOTALDOWNTIME_HOUR,t1.TOTALDOWNTIME_MINUTE,t1.ACTUAL_SERVICETIME_PERMONTH_HOUR,t1.ACTUAL_SERVICETIME_PERHOUR_MINUTE,t1.ACTUAL_PERCENTSLA,t1.RATECHARGE,t1.SERVICECHARGE,t1.NETCHARGE,t1.REMARK,t2.TERM_NAME FROM " + slamonthlydowntime_table;

        }
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
    }
}
