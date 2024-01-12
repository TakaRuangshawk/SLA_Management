using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SLA_Management.Commons;
using SLA_Management.Models.OperationModel;
using System.Data;
using System;
using SLA_Management.Data;
using SLAManagement.Data;
using System.Drawing;
using static SLA_Management.Controllers.OperationController;
using SLA_Management.Data.ExcelUtilitie;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using static Mysqlx.Datatypes.Scalar.Types;

namespace SLA_Management.Controllers
{
    public class ReportController : Controller
    {
        private IConfiguration _myConfiguration;
        private static ConnectMySQL db_all;
        private List<BalancingReportModel> balancingreport_dataList = new List<BalancingReportModel>();
        private List<HardwareReportWebModel> hardwarereport_dataList = new List<HardwareReportWebModel>();
        public ReportController(IConfiguration myConfiguration)
        {
            _myConfiguration = myConfiguration;
            db_all = new ConnectMySQL(myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection"));
            

        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult HardwareReport()
        {
            ViewBag.CurrentTID = GetDeviceInfoALL();
            return View();
        }
        public IActionResult HardwareReportFetchData(string terminalno, string row, string page, string search, string todate, string fromdate)
        {
            int _page;
            int id_row = 0;
            string filterquery = string.Empty;
            string connectionstring = string.Empty;
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
                _row = 20;
            }
            else
            {
                _row = int.Parse(row);
            }
            terminalno = terminalno ?? "";
            fromdate = fromdate ?? DateTime.Now.ToString("yyyy-MM-dd");
            todate = todate ?? DateTime.Now.ToString("yyyy-MM-dd");
            List<HardwareReportQueryModel> datas = new List<HardwareReportQueryModel>();
            if (terminalno.Length > 0)
            {
                char firstLetter = terminalno[0];

                switch (firstLetter)
                {
                    case 'A':
                        connectionstring = _myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection");
                        break;

                    case 'R':
                        connectionstring = _myConfiguration.GetValue<string>("ConnectString_MySQL_FV_CDM:FullNameConnection");
                        break;
                    case 'L':
                        connectionstring = _myConfiguration.GetValue<string>("ConnectString_MySQL_FV_LRM:FullNameConnection");
                        break;

                        
                    default:
                        connectionstring = _myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection");
                        break;
                }
            }
            
            using (MySqlConnection connection = new MySqlConnection(connectionstring))
            {
                string transationdate_row = "";
                connection.Open();

                if (terminalno != "")
                {
                    filterquery += " and TERM_ID = '" + terminalno + "' ";
                }
                string query = @"SELECT TERM_ID,EVENT_ID,END_TIME FROM mt_caseflow_record_his ";

                query += " WHERE END_TIME BETWEEN '"+ fromdate +" 00:00:00' AND '"+ todate +" 23:59:59' " + filterquery;
  

                MySqlCommand command = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        datas.Add(new HardwareReportQueryModel
                        {

                           TERM_ID = reader["TERM_ID"].ToString(),
                           EVENT_ID = reader["EVENT_ID"].ToString(),
                           END_TIME = reader["END_TIME"].ToString(),

                        });
                    }
                }
            }
            List<HardwareReportWebModel> problemList = new List<HardwareReportWebModel>
            {
                new HardwareReportWebModel { problem_name = "Terminal Maintenance Mode", problem_count = datas.Count(x => x.EVENT_ID == "E1002") },
                new HardwareReportWebModel { problem_name = "Terminal OffLineMode", problem_count = datas.Count(x => x.EVENT_ID == "E1005") },
                new HardwareReportWebModel { problem_name = "Terminal StopService", problem_count = datas.Count(x => x.EVENT_ID == "E1006") },
                new HardwareReportWebModel { problem_name = "Cash Dispenser Error", problem_count = datas.Count(x => x.EVENT_ID == "E1010") },
                new HardwareReportWebModel { problem_name = "Cash Acceptor Error", problem_count = datas.Count(x => x.EVENT_ID == "E1012") },
                new HardwareReportWebModel { problem_name = "Cash Acceptor Note Jam", problem_count = datas.Count(x => x.EVENT_ID == "E1020") },
                new HardwareReportWebModel { problem_name = "Card Reader Error", problem_count = datas.Count(x => x.EVENT_ID == "E1036") },
                new HardwareReportWebModel { problem_name = "Thai ID Card Reader Error", problem_count = datas.Count(x => x.EVENT_ID == "E1038") },
                new HardwareReportWebModel { problem_name = "EPP Keypad Error", problem_count = datas.Count(x => x.EVENT_ID == "E1046") },
                new HardwareReportWebModel { problem_name = "Cassettes Error", problem_count = datas.Count(x => x.EVENT_ID == "E1127") },
                new HardwareReportWebModel { problem_name = "CardRetractBin Full", problem_count = datas.Count(x => x.EVENT_ID == "E1136") },
                new HardwareReportWebModel { problem_name = "Withdrawal Hardware Fault", problem_count = datas.Count(x => x.EVENT_ID == "E1149") },
                new HardwareReportWebModel { problem_name = "Withdrawal Cassette Issue", problem_count = datas.Count(x => x.EVENT_ID == "E1150") },
                new HardwareReportWebModel { problem_name = "Withdrawal No Cassette", problem_count = datas.Count(x => x.EVENT_ID == "E1151") },
                new HardwareReportWebModel { problem_name = "Withdrawal Retract Reject Full", problem_count = datas.Count(x => x.EVENT_ID == "E1152") },
                new HardwareReportWebModel { problem_name = "Withdrawal No CashReplenishment", problem_count = datas.Count(x => x.EVENT_ID == "E1153") },
                new HardwareReportWebModel { problem_name = "Communication Interrupt", problem_count = datas.Count(x => x.EVENT_ID == "E1156") },
                new HardwareReportWebModel { problem_name = "Vibration Alarm", problem_count = datas.Count(x => x.EVENT_ID == "E1179") },
                new HardwareReportWebModel { problem_name = "Box Alarm", problem_count = datas.Count(x => x.EVENT_ID == "E1182") },
                new HardwareReportWebModel { problem_name = "AntiJump Alarm", problem_count = datas.Count(x => x.EVENT_ID == "E1183") },
                new HardwareReportWebModel { problem_name = "Heat Alarm", problem_count = datas.Count(x => x.EVENT_ID == "E1185") },
                new HardwareReportWebModel { problem_name = "FaceCamera Error", problem_count = datas.Count(x => x.EVENT_ID == "E1217") },
                new HardwareReportWebModel { problem_name = "ShutterCamera Error", problem_count = datas.Count(x => x.EVENT_ID == "E1220") },
                new HardwareReportWebModel { problem_name = "CardReader Error Card Jammed", problem_count = datas.Count(x => x.EVENT_ID == "E1283") },
                new HardwareReportWebModel { problem_name = "Receipt Printer Error", problem_count = datas.Count(x => x.EVENT_ID == "E1375") },
                new HardwareReportWebModel { problem_name = "RPRXPaper Low", problem_count = datas.Count(x => x.EVENT_ID == "E2197") },
                new HardwareReportWebModel { problem_name = "RPRXPaper Out", problem_count = datas.Count(x => x.EVENT_ID == "E2198") },
                new HardwareReportWebModel { problem_name = "RPRXPaper NotSupport", problem_count = datas.Count(x => x.EVENT_ID == "E2199") },
                new HardwareReportWebModel { problem_name = "RPRXPaper Unknown", problem_count = datas.Count(x => x.EVENT_ID == "E2200") },
                new HardwareReportWebModel { problem_name = "RPRXPaper Jammed", problem_count = datas.Count(x => x.EVENT_ID == "E2201") },
                new HardwareReportWebModel { problem_name = "Cash Dispensing Shutter Jammed", problem_count = datas.Count(x => x.EVENT_ID == "E2204") },
                new HardwareReportWebModel { problem_name = "Cash Dispensing Shutter Unknown", problem_count = datas.Count(x => x.EVENT_ID == "E2205") },
                new HardwareReportWebModel { problem_name = "Cash Dispensing Shutter Not Support", problem_count = datas.Count(x => x.EVENT_ID == "E2206") },
                new HardwareReportWebModel { problem_name = "Cash Acceptor Shutter Jammed", problem_count = datas.Count(x => x.EVENT_ID == "E2209") },
                new HardwareReportWebModel { problem_name = "Cash Acceptor Shutter Unknown", problem_count = datas.Count(x => x.EVENT_ID == "E2210") },
                new HardwareReportWebModel { problem_name = "Cash Acceptor Shutter Not Support", problem_count = datas.Count(x => x.EVENT_ID == "E2211") },
                new HardwareReportWebModel { problem_name = "Other Error", problem_count = datas.Count(x => x.EVENT_ID == "E9999") }

            };
            int totalProblemCount = problemList.Sum(item => item.problem_count);
            hardwarereport_dataList = problemList;
            var response = new DataResponse_HardwareReport
            {
                JsonData = hardwarereport_dataList,
                Page = 1,
                currentPage = _page,
                TotalTerminal = totalProblemCount,
            };
            return Json(response);
        }
        public IActionResult BalancingReport()
        {
            ViewBag.CurrentTID = GetDeviceInfoALL();
            return View();
        }
       
        public IActionResult BalancingReportFetchData(string terminalno, string row, string page, string search, string todate, string fromdate)
        {
            int _page;
            int id_row = 0;
            string filterquery = string.Empty;
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
                _row = 20;
            }
            else
            {
                _row = int.Parse(row);
            }
            terminalno = terminalno ?? "";
            fromdate = fromdate ?? DateTime.Now.ToString("yyyy-MM-dd");
            todate = todate ?? DateTime.Now.ToString("yyyy-MM-dd");
            List<BalancingReportModel> jsonData = new List<BalancingReportModel>();
            using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
            {
                string transationdate_row = "";
                connection.Open();

                if (terminalno != "")
                {
                    filterquery += " and terminalid = '" + terminalno + "' ";
                }
                string query = @"  SELECT t1.terminalid,adi.term_seq,adi.term_name,t1.min_datetime,
                FORMAT(CAST(SUBSTRING(t1.c1_inc, 3) AS UNSIGNED), 0) AS c1_dep ,
                FORMAT(CAST(SUBSTRING(t1.c1_dec, 3) AS UNSIGNED), 0) AS c1_dec ,
                FORMAT(CAST(SUBSTRING(t2.c1_out, 3) AS UNSIGNED), 0) AS c1_out ,
                FORMAT(CAST(SUBSTRING(t2.c1_end, 3) AS UNSIGNED), 0) AS c1_end ,
                FORMAT(CAST(SUBSTRING(t3.c2_inc, 3) AS UNSIGNED), 0) AS c2_dep ,
                FORMAT(CAST(SUBSTRING(t3.c2_dec, 3) AS UNSIGNED), 0) AS c2_dec ,
                FORMAT(CAST(SUBSTRING(t4.c2_out, 3) AS UNSIGNED), 0) AS c2_out ,
                FORMAT(CAST(SUBSTRING(t4.c2_end, 3) AS UNSIGNED), 0) AS c2_end ,
                FORMAT(CAST(SUBSTRING(t5.c3_inc, 3) AS UNSIGNED), 0) AS c3_dep ,
                FORMAT(CAST(SUBSTRING(t5.c3_dec, 3) AS UNSIGNED), 0) AS c3_dec ,
                FORMAT(CAST(SUBSTRING(t6.c3_out, 3) AS UNSIGNED), 0) AS c3_out ,
                FORMAT(CAST(SUBSTRING(t6.c3_end, 3) AS UNSIGNED), 0) AS c3_end,
                '-' AS c1_inc,
                '-' AS c2_inc,
                '-' AS c3_inc	
                FROM (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C1 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 INC', -1), ' ', 1) END AS c1_inc,
                CASE WHEN ej.remark LIKE '%C1 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 DEC', -1), ' ', 1) END AS c1_dec FROM ejlog_devicetermprob_ejreport ej  ";

                query += " WHERE probcode ='BALRP_01' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY ej.terminalid, min_date HAVING (c1_inc IS NOT NULL OR c1_dec IS NOT NULL)) t1";
                query += " JOIN (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C1 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 OUT', -1), ' ', 1) END AS c1_out,CASE WHEN ej.remark LIKE '%C1 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 END', -1), ' ', 1) END AS c1_end FROM ejlog_devicetermprob_ejreport ej ";
                query += " WHERE probcode ='BALRP_02' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY ej.terminalid, min_date HAVING (c1_out IS NOT NULL OR c1_end IS NOT NULL)) t2 ON t1.terminalid = t2.terminalid AND t1.min_date = t2.min_date";
                query += " JOIN (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C2 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C2 INC', -1), ' ', 1) END AS c2_inc,CASE WHEN ej.remark LIKE '%C2 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C2 DEC', -1), ' ', 1) END AS c2_dec FROM ejlog_devicetermprob_ejreport ej ";
                query += " WHERE probcode ='BALRP_03' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY ej.terminalid, min_date HAVING (c2_inc IS NOT NULL OR c2_dec IS NOT NULL)) t3 ON t1.terminalid = t3.terminalid AND t1.min_date = t3.min_date";
                query += " JOIN (SELECT ej.terminalid, DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C2 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C2 OUT', -1), ' ', 1) END AS c2_out,CASE WHEN ej.remark LIKE '%C2 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C2 END', -1), ' ', 1) END AS c2_end FROM ejlog_devicetermprob_ejreport ej ";
                query += " WHERE probcode ='BALRP_04' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY ej.terminalid, min_date HAVING (c2_out IS NOT NULL OR c2_end IS NOT NULL)) t4 ON t1.terminalid = t4.terminalid AND t1.min_date = t4.min_date";
                query += " JOIN (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C3 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C3 INC', -1), ' ', 1) END AS c3_inc, CASE WHEN ej.remark LIKE '%C3 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C3 DEC', -1), ' ', 1) END AS c3_dec FROM ejlog_devicetermprob_ejreport ej ";
                query += " WHERE probcode ='BALRP_05' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY ej.terminalid, min_date HAVING (c3_inc IS NOT NULL OR c3_dec IS NOT NULL)) t5 ON t1.terminalid = t5.terminalid AND t1.min_date = t5.min_date";
                query += " JOIN (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C3 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C3 OUT', -1), ' ', 1) END AS c3_out,CASE WHEN ej.remark LIKE '%C3 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C3 END', -1), ' ', 1) END AS c3_end FROM ejlog_devicetermprob_ejreport ej ";
                query += " WHERE probcode ='BALRP_06' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY ej.terminalid, min_date HAVING (c3_out IS NOT NULL OR c3_end IS NOT NULL)) t6 ON t1.terminalid = t6.terminalid AND t1.min_date = t6.min_date";
                query += " left join all_device_info adi on t1.terminalid = adi.term_id ";

                MySqlCommand command = new MySqlCommand(query, connection);
               
                using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            id_row += 1;
                            if (reader["min_datetime"] != DBNull.Value && DateTime.TryParse(reader["min_datetime"].ToString(), out DateTime trxDateTime))
                            {
                                transationdate_row = trxDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                transationdate_row = "-";
                            }
                            jsonData.Add(new BalancingReportModel
                            {

                                no = (id_row).ToString(),
                                term_seq = reader["term_seq"].ToString(),
                                term_id = reader["terminalid"].ToString(),
                                term_name = reader["term_name"].ToString(),
                                transationdate = transationdate_row,
                                c1_inc = reader["c1_inc"].ToString(),
                                c1_dec = reader["c1_dec"].ToString(),
                                c1_out = reader["c1_out"].ToString(),
                                c1_end = reader["c1_end"].ToString(),
                                c2_inc = reader["c2_inc"].ToString(),
                                c2_dec = reader["c2_dec"].ToString(),
                                c2_out = reader["c2_out"].ToString(),
                                c2_end = reader["c2_end"].ToString(),
                                c3_inc = reader["c3_inc"].ToString(),
                                c3_dec = reader["c3_dec"].ToString(),
                                c3_out = reader["c3_out"].ToString(),
                                c3_end = reader["c3_end"].ToString(),
                                c1_dep = reader["c1_dep"].ToString(),
                                c2_dep = reader["c2_dep"].ToString(),
                                c3_dep = reader["c3_dep"].ToString()
                            });
                        }
                    }

                query = @"SELECT 
                 t1.terminalid,
                 adi.term_seq,
                 adi.term_name,
                 t7.max_transaction_date AS min_datetime,
                 FORMAT(CAST(SUBSTRING(t7.c1_inc, 3) AS UNSIGNED), 0) AS c1_inc,
                 FORMAT(CAST(SUBSTRING(t7.c1_dec, 3) AS UNSIGNED), 0) AS c1_dec,
                 FORMAT(CAST(SUBSTRING(t8.c1_out, 3) AS UNSIGNED), 0) AS c1_out,
                 FORMAT(CAST(SUBSTRING(t8.c1_end, 3) AS UNSIGNED), 0) AS c1_end,
                 (FORMAT(CAST(SUBSTRING(t8.c1_end, 3) AS UNSIGNED), 0) -
                 FORMAT(CAST(SUBSTRING(t7.c1_inc, 3) AS UNSIGNED), 0) +
                 FORMAT(CAST(SUBSTRING(t8.c1_out, 3) AS UNSIGNED), 0)) AS c1_dep,
                 FORMAT(CAST(SUBSTRING(t9.c2_inc, 3) AS UNSIGNED), 0) AS c2_inc,
                 FORMAT(CAST(SUBSTRING(t9.c2_dec, 3) AS UNSIGNED), 0) AS c2_dec,
                 FORMAT(CAST(SUBSTRING(t10.c2_out, 3) AS UNSIGNED), 0) AS c2_out,
                 FORMAT(CAST(SUBSTRING(t10.c2_end, 3) AS UNSIGNED), 0) AS c2_end,
                 (FORMAT(CAST(SUBSTRING(t10.c2_end, 3) AS UNSIGNED), 0) -
                 FORMAT(CAST(SUBSTRING(t9.c2_inc, 3) AS UNSIGNED), 0) +
                 FORMAT(CAST(SUBSTRING(t10.c2_out, 3) AS UNSIGNED), 0)) AS c2_dep,
                 FORMAT(CAST(SUBSTRING(t11.c3_inc, 3) AS UNSIGNED), 0) AS c3_inc,
                 FORMAT(CAST(SUBSTRING(t11.c3_dec, 3) AS UNSIGNED), 0) AS c3_dec,
                 FORMAT(CAST(SUBSTRING(t12.c3_out, 3) AS UNSIGNED), 0) AS c3_out,
                 FORMAT(CAST(SUBSTRING(t12.c3_end, 3) AS UNSIGNED), 0) AS c3_end,
                 (FORMAT(CAST(SUBSTRING(t12.c3_end, 3) AS UNSIGNED), 0) -
                 FORMAT(CAST(SUBSTRING(t11.c3_inc, 3) AS UNSIGNED), 0) +
                 FORMAT(CAST(SUBSTRING(t12.c3_out, 3) AS UNSIGNED), 0)) AS c3_dep
                        FROM (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C1 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 INC', -1), ' ', 1) END AS c1_inc,CASE WHEN ej.remark LIKE '%C1 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 DEC', -1), ' ', 1) END AS c1_dec FROM ejlog_devicetermprob_ejreport ej ";
                query += " WHERE probcode ='BALRP_01' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY ej.terminalid, min_date HAVING (c1_inc IS NOT NULL OR c1_dec IS NOT NULL)) t1 ";
                query += " JOIN (SELECT terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE '%C1 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_01' LIMIT 1), 'C1 INC', -1), ' ', 1) END AS c1_inc,CASE WHEN ej.remark LIKE '%C1 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_01' LIMIT 1), 'C1 DEC', -1), ' ', 1) END AS c1_dec FROM ejlog_devicetermprob_ejreport ej ";
                query += " WHERE probcode ='BALRP_01' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY terminalid,DATE(trxdatetime)) t7 ON t1.terminalid = t7.terminalid AND t1.min_date = t7.max_date ";
                query += " JOIN (SELECT terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE '%C1 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_02' LIMIT 1), 'C1 OUT', -1), ' ', 1) END AS c1_out,CASE WHEN ej.remark LIKE '%C1 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_02' LIMIT 1), 'C1 END', -1), ' ', 1) END AS c1_end FROM ejlog_devicetermprob_ejreport ej ";
                query += " WHERE probcode ='BALRP_02' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY terminalid,DATE(trxdatetime)) t8 ON t1.terminalid = t8.terminalid AND t1.min_date = t8.max_date ";
                query += " JOIN (SELECT terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE '%C2 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_03' LIMIT 1), 'C2 INC', -1), ' ', 1) END AS c2_inc,CASE WHEN ej.remark LIKE '%C2 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_03' LIMIT 1), 'C2 DEC', -1), ' ', 1) END AS c2_dec FROM ejlog_devicetermprob_ejreport ej ";
                query += " WHERE probcode ='BALRP_03' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY terminalid,DATE(trxdatetime)) t9 ON t1.terminalid = t9.terminalid AND t1.min_date = t9.max_date ";
                query += " JOIN (SELECT terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE '%C2 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_04' LIMIT 1), 'C2 OUT', -1), ' ', 1) END AS c2_out,CASE WHEN ej.remark LIKE '%C2 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_04' LIMIT 1), 'C2 END', -1), ' ', 1) END AS c2_end FROM ejlog_devicetermprob_ejreport ej ";
                query += " WHERE probcode ='BALRP_04' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY terminalid,DATE(trxdatetime)) t10 ON t1.terminalid = t10.terminalid AND t1.min_date = t10.max_date ";
                query += " JOIN (SELECT terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE '%C3 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_05' LIMIT 1), 'C3 INC', -1), ' ', 1) END AS c3_inc,CASE WHEN ej.remark LIKE '%C3 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_05' LIMIT 1), 'C3 DEC', -1), ' ', 1) END AS c3_dec FROM ejlog_devicetermprob_ejreport ej ";
                query += " WHERE probcode ='BALRP_05' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY terminalid,DATE(trxdatetime)) t11 ON t1.terminalid = t11.terminalid AND t1.min_date = t11.max_date ";
                query += " JOIN (SELECT terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE '%C3 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_06' LIMIT 1), 'C3 OUT', -1), ' ', 1) END AS c3_out,CASE WHEN ej.remark LIKE '%C3 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_06' LIMIT 1), 'C3 END', -1), ' ', 1) END AS c3_end FROM ejlog_devicetermprob_ejreport ej ";
                query += " WHERE probcode ='BALRP_06' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY terminalid,DATE(trxdatetime)) t12 ON t1.terminalid = t12.terminalid AND t1.min_date = t12.max_date ";
                query += " left join all_device_info adi on t1.terminalid = adi.term_id ";
                command = new MySqlCommand(query, connection);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        id_row += 1;
                        if (reader["min_datetime"] != DBNull.Value && DateTime.TryParse(reader["min_datetime"].ToString(), out DateTime trxDateTime))
                        {
                            transationdate_row = trxDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else
                        {
                            transationdate_row = "-";
                        }
                        jsonData.Add(new BalancingReportModel
                        {

                            no = (id_row).ToString(),
                            term_seq = reader["term_seq"].ToString(),
                            term_id = reader["terminalid"].ToString(),
                            term_name = reader["term_name"].ToString(),
                            transationdate = transationdate_row,
                            c1_inc = reader["c1_inc"].ToString(),
                            c1_dec = reader["c1_dec"].ToString(),
                            c1_out = reader["c1_out"].ToString(),
                            c1_end = reader["c1_end"].ToString(),
                            c2_inc = reader["c2_inc"].ToString(),
                            c2_dec = reader["c2_dec"].ToString(),
                            c2_out = reader["c2_out"].ToString(),
                            c2_end = reader["c2_end"].ToString(),
                            c3_inc = reader["c3_inc"].ToString(),
                            c3_dec = reader["c3_dec"].ToString(),
                            c3_out = reader["c3_out"].ToString(),
                            c3_end = reader["c3_end"].ToString(),
                            c1_dep = "0",
                            c2_dep = "0",
                            c3_dep = "0"
                        });
                    }
                }

            }
            

            jsonData = jsonData
            .OrderBy(x => x.term_id)  // Sort by terminalid
            .ThenBy(x => x.transationdate)  // Then sort by transactiondate
            .ToList();
            balancingreport_dataList = jsonData;
            int pages = (int)Math.Ceiling((double)jsonData.Count() / _row);
            List<BalancingReportModel> filteredData = RangeFilter_br(jsonData, _page, _row);
            var response = new DataResponse_BalancingReport
            {
                JsonData = filteredData,
                Page = pages,
                currentPage = _page,
                TotalTerminal = jsonData.Count(),
            };
            return Json(response);
        }
        static List<BalancingReportModel> RangeFilter_br<BalancingReportModel>(List<BalancingReportModel> inputList, int page, int row)
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

        public class BalancingReportModel
        {
            public string no { get; set; }
            public string term_id { get; set; }
            public string term_seq { get; set; }
            public string term_name { get; set; }
            public string transationdate { get; set; }
            public string c1_inc { get; set; }
            public string c1_dec { get; set; }
            public string c1_out { get; set; }
            public string c1_end { get; set; }
            public string c2_inc { get; set; }
            public string c2_dec { get; set; }
            public string c2_out { get; set; }
            public string c2_end { get; set; }
            public string c3_inc { get; set; }
            public string c3_dec { get; set; }
            public string c3_out { get; set; }
            public string c3_end { get; set; }
            public string c1_dep { get; set; }
            public string c2_dep { get; set; }
            public string c3_dep { get; set; }

        }
        public class DataResponse_BalancingReport
        {
            public List<BalancingReportModel> JsonData { get; set; }
            public int Page { get; set; }
            public int currentPage { get; set; }
            public int TotalTerminal { get; set; }
        }
        public class HardwareReportQueryModel
        {
            public string ID { get; set; }
            public string TERM_ID { get; set; }
            public string EVENT_ID { get; set; }
            public string END_TIME { get; set; }
        }
        public class HardwareReportWebModel
        {
            public string problem_name { get; set; }
            public int problem_count { get; set; }
        }
        public class DataResponse_HardwareReport
        {
            public List<HardwareReportWebModel> JsonData { get; set; }
            public int Page { get; set; }
            public int currentPage { get; set; }
            public int TotalTerminal { get; set; }
        }
        #region anyfunction
        private static List<Device_info> GetDeviceInfoALL()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM all_device_info order by TERM_SEQ;";
            DataTable testss = db_all.GetDatatable(com);

            List<Device_info> test = ConvertDataTableToModel.ConvertDataTable<Device_info>(testss);

            return test;
        }
        #endregion
        #region Excel BalancingReport

        [HttpGet]
        public ActionResult BalancingReport_ExportExc(string terminalno, string todate, string fromdate)
        {
            string filterquery = string.Empty;
            string fname = string.Empty;
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;

            try
            {
                terminalno = terminalno ?? "";
                fromdate = fromdate ?? DateTime.Now.ToString("yyyy-MM-dd");
                todate = todate ?? DateTime.Now.ToString("yyyy-MM-dd");
                List<BalancingReportModel> jsonData = new List<BalancingReportModel>();
                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    string transationdate_row = "";
                    connection.Open();

                    if (terminalno != "")
                    {
                        filterquery += " and terminalid = '" + terminalno + "' ";
                    }
                    string query = @" SELECT t1.terminalid,adi.term_seq,adi.term_name,t1.min_datetime,FORMAT(CAST(SUBSTRING(t1.c1_inc, 3) AS UNSIGNED), 0) AS c1_inc ,FORMAT(CAST(SUBSTRING(t1.c1_dec, 3) AS UNSIGNED), 0) AS c1_dec ,FORMAT(CAST(SUBSTRING(t2.c1_out, 3) AS UNSIGNED), 0) AS c1_out ,FORMAT(CAST(SUBSTRING(t2.c1_end, 3) AS UNSIGNED), 0) AS c1_end ,
                    FORMAT(CAST(SUBSTRING(t3.c2_inc, 3) AS UNSIGNED), 0) AS c2_inc ,FORMAT(CAST(SUBSTRING(t3.c2_dec, 3) AS UNSIGNED), 0) AS c2_dec ,FORMAT(CAST(SUBSTRING(t4.c2_out, 3) AS UNSIGNED), 0) AS c2_out ,FORMAT(CAST(SUBSTRING(t4.c2_end, 3) AS UNSIGNED), 0) AS c2_end ,
                    FORMAT(CAST(SUBSTRING(t5.c3_inc, 3) AS UNSIGNED), 0) AS c3_inc ,FORMAT(CAST(SUBSTRING(t5.c3_dec, 3) AS UNSIGNED), 0) AS c3_dec ,FORMAT(CAST(SUBSTRING(t6.c3_out, 3) AS UNSIGNED), 0) AS c3_out ,FORMAT(CAST(SUBSTRING(t6.c3_end, 3) AS UNSIGNED), 0) AS c3_end  ,
                     FORMAT(COALESCE(CAST(SUBSTRING(t1.c1_inc, 3) AS UNSIGNED) - (SELECT SUBSTRING(SUBSTRING_INDEX(SUBSTRING_INDEX(subej.remark, 'C1 INC', -1), ' ', 1),3) FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = t1.terminalid AND subej.trxdatetime = (SELECT MAX(trxdatetime) FROM ejlog_devicetermprob_ejreport WHERE terminalid = t1.terminalid AND trxdatetime < t1.min_datetime AND probcode ='BALRP_01')AND probcode ='BALRP_01' LIMIT 1
	                 ), 0 
                     ), 0
                     ) AS c1_dep,
                     FORMAT(COALESCE(CAST(SUBSTRING(t3.c2_inc, 3) AS UNSIGNED) - (SELECT SUBSTRING(SUBSTRING_INDEX(SUBSTRING_INDEX(subej.remark, 'C2 INC', -1), ' ', 1),3) FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = t1.terminalid AND subej.trxdatetime = (SELECT MAX(trxdatetime) FROM ejlog_devicetermprob_ejreport WHERE terminalid = t1.terminalid AND trxdatetime < t1.min_datetime AND probcode ='BALRP_03')AND probcode ='BALRP_03' LIMIT 1
	                 ), 0 
                     ), 0
                     ) AS c2_dep,
                     FORMAT(COALESCE(CAST(SUBSTRING(t5.c3_inc, 3) AS UNSIGNED) - (SELECT SUBSTRING(SUBSTRING_INDEX(SUBSTRING_INDEX(subej.remark, 'C3 INC', -1), ' ', 1),3) FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = t1.terminalid AND subej.trxdatetime = (SELECT MAX(trxdatetime) FROM ejlog_devicetermprob_ejreport WHERE terminalid = t1.terminalid AND trxdatetime < t1.min_datetime AND probcode ='BALRP_05')AND probcode ='BALRP_05' LIMIT 1
	                 ), 0 
                     ), 0
                     ) AS c3_dep

                    FROM (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C1 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 INC', -1), ' ', 1) END AS c1_inc,
                    CASE WHEN ej.remark LIKE '%C1 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 DEC', -1), ' ', 1) END AS c1_dec
                                    
                    FROM ejlog_devicetermprob_ejreport ej  ";

                    query += " WHERE probcode ='BALRP_01' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY ej.terminalid, min_date HAVING (c1_inc IS NOT NULL OR c1_dec IS NOT NULL)) t1";
                    query += " JOIN (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C1 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 OUT', -1), ' ', 1) END AS c1_out,CASE WHEN ej.remark LIKE '%C1 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 END', -1), ' ', 1) END AS c1_end FROM ejlog_devicetermprob_ejreport ej ";
                    query += " WHERE probcode ='BALRP_02' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY ej.terminalid, min_date HAVING (c1_out IS NOT NULL OR c1_end IS NOT NULL)) t2 ON t1.terminalid = t2.terminalid AND t1.min_date = t2.min_date";
                    query += " JOIN (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C2 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C2 INC', -1), ' ', 1) END AS c2_inc,CASE WHEN ej.remark LIKE '%C2 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C2 DEC', -1), ' ', 1) END AS c2_dec FROM ejlog_devicetermprob_ejreport ej ";
                    query += " WHERE probcode ='BALRP_03' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY ej.terminalid, min_date HAVING (c2_inc IS NOT NULL OR c2_dec IS NOT NULL)) t3 ON t1.terminalid = t3.terminalid AND t1.min_date = t3.min_date";
                    query += " JOIN (SELECT ej.terminalid, DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C2 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C2 OUT', -1), ' ', 1) END AS c2_out,CASE WHEN ej.remark LIKE '%C2 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C2 END', -1), ' ', 1) END AS c2_end FROM ejlog_devicetermprob_ejreport ej ";
                    query += " WHERE probcode ='BALRP_04' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY ej.terminalid, min_date HAVING (c2_out IS NOT NULL OR c2_end IS NOT NULL)) t4 ON t1.terminalid = t4.terminalid AND t1.min_date = t4.min_date";
                    query += " JOIN (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C3 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C3 INC', -1), ' ', 1) END AS c3_inc, CASE WHEN ej.remark LIKE '%C3 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C3 DEC', -1), ' ', 1) END AS c3_dec FROM ejlog_devicetermprob_ejreport ej ";
                    query += " WHERE probcode ='BALRP_05' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY ej.terminalid, min_date HAVING (c3_inc IS NOT NULL OR c3_dec IS NOT NULL)) t5 ON t1.terminalid = t5.terminalid AND t1.min_date = t5.min_date";
                    query += " JOIN (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C3 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C3 OUT', -1), ' ', 1) END AS c3_out,CASE WHEN ej.remark LIKE '%C3 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C3 END', -1), ' ', 1) END AS c3_end FROM ejlog_devicetermprob_ejreport ej ";
                    query += " WHERE probcode ='BALRP_06' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY ej.terminalid, min_date HAVING (c3_out IS NOT NULL OR c3_end IS NOT NULL)) t6 ON t1.terminalid = t6.terminalid AND t1.min_date = t6.min_date";
                    query += " left join all_device_info adi on t1.terminalid = adi.term_id ";

                    MySqlCommand command = new MySqlCommand(query, connection);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
    
                            if (reader["min_datetime"] != DBNull.Value && DateTime.TryParse(reader["min_datetime"].ToString(), out DateTime trxDateTime))
                            {
                                transationdate_row = trxDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                transationdate_row = "-";
                            }
                            jsonData.Add(new BalancingReportModel
                            {


                                term_seq = reader["term_seq"].ToString(),
                                term_id = reader["terminalid"].ToString(),
                                term_name = reader["term_name"].ToString(),
                                transationdate = transationdate_row,
                                c1_inc = reader["c1_inc"].ToString(),
                                c1_dec = reader["c1_dec"].ToString(),
                                c1_out = reader["c1_out"].ToString(),
                                c1_end = reader["c1_end"].ToString(),
                                c2_inc = reader["c2_inc"].ToString(),
                                c2_dec = reader["c2_dec"].ToString(),
                                c2_out = reader["c2_out"].ToString(),
                                c2_end = reader["c2_end"].ToString(),
                                c3_inc = reader["c3_inc"].ToString(),
                                c3_dec = reader["c3_dec"].ToString(),
                                c3_out = reader["c3_out"].ToString(),
                                c3_end = reader["c3_end"].ToString(),
                                c1_dep = reader["c1_dep"].ToString(),
                                c2_dep = reader["c2_dep"].ToString(),
                                c3_dep = reader["c3_dep"].ToString()

                            });
                        }
                    }

                    query = @"SELECT t1.terminalid,adi.term_seq,adi.term_name,t7.max_transaction_date as min_datetime,
                        FORMAT(CAST(SUBSTRING(t7.c1_inc, 3) AS UNSIGNED), 0) AS c1_inc,FORMAT(CAST(SUBSTRING(t7.c1_dec, 3) AS UNSIGNED), 0) AS c1_dec,FORMAT(CAST(SUBSTRING(t8.c1_out, 3) AS UNSIGNED), 0) AS c1_out,FORMAT(CAST(SUBSTRING(t8.c1_end, 3) AS UNSIGNED), 0) AS c1_end,
                        FORMAT(CAST(SUBSTRING(t9.c2_inc, 3) AS UNSIGNED), 0) AS c2_inc,FORMAT(CAST(SUBSTRING(t9.c2_dec, 3) AS UNSIGNED), 0) AS c2_dec,FORMAT(CAST(SUBSTRING(t10.c2_out, 3) AS UNSIGNED), 0) AS c2_out,FORMAT(CAST(SUBSTRING(t10.c2_end, 3) AS UNSIGNED), 0) AS c2_end,
                        FORMAT(CAST(SUBSTRING(t11.c3_inc, 3) AS UNSIGNED), 0) AS c3_inc,FORMAT(CAST(SUBSTRING(t11.c3_dec, 3) AS UNSIGNED), 0) AS c3_dec,FORMAT(CAST(SUBSTRING(t12.c3_out, 3) AS UNSIGNED), 0) AS c3_out,FORMAT(CAST(SUBSTRING(t12.c3_end, 3) AS UNSIGNED), 0) AS c3_end
                        FROM (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C1 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 INC', -1), ' ', 1) END AS c1_inc,CASE WHEN ej.remark LIKE '%C1 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 DEC', -1), ' ', 1) END AS c1_dec FROM ejlog_devicetermprob_ejreport ej ";
                    query += " WHERE probcode ='BALRP_01' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY ej.terminalid, min_date HAVING (c1_inc IS NOT NULL OR c1_dec IS NOT NULL)) t1 ";
                    query += " JOIN (SELECT terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE '%C1 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_01' LIMIT 1), 'C1 INC', -1), ' ', 1) END AS c1_inc,CASE WHEN ej.remark LIKE '%C1 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_01' LIMIT 1), 'C1 DEC', -1), ' ', 1) END AS c1_dec FROM ejlog_devicetermprob_ejreport ej ";
                    query += " WHERE probcode ='BALRP_01' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY terminalid,DATE(trxdatetime)) t7 ON t1.terminalid = t7.terminalid AND t1.min_date = t7.max_date ";
                    query += " JOIN (SELECT terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE '%C1 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_02' LIMIT 1), 'C1 OUT', -1), ' ', 1) END AS c1_out,CASE WHEN ej.remark LIKE '%C1 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_02' LIMIT 1), 'C1 END', -1), ' ', 1) END AS c1_end FROM ejlog_devicetermprob_ejreport ej ";
                    query += " WHERE probcode ='BALRP_02' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY terminalid,DATE(trxdatetime)) t8 ON t1.terminalid = t8.terminalid AND t1.min_date = t8.max_date ";
                    query += " JOIN (SELECT terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE '%C2 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_03' LIMIT 1), 'C2 INC', -1), ' ', 1) END AS c2_inc,CASE WHEN ej.remark LIKE '%C2 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_03' LIMIT 1), 'C2 DEC', -1), ' ', 1) END AS c2_dec FROM ejlog_devicetermprob_ejreport ej ";
                    query += " WHERE probcode ='BALRP_03' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY terminalid,DATE(trxdatetime)) t9 ON t1.terminalid = t9.terminalid AND t1.min_date = t9.max_date ";
                    query += " JOIN (SELECT terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE '%C2 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_04' LIMIT 1), 'C2 OUT', -1), ' ', 1) END AS c2_out,CASE WHEN ej.remark LIKE '%C2 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_04' LIMIT 1), 'C2 END', -1), ' ', 1) END AS c2_end FROM ejlog_devicetermprob_ejreport ej ";
                    query += " WHERE probcode ='BALRP_04' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY terminalid,DATE(trxdatetime)) t10 ON t1.terminalid = t10.terminalid AND t1.min_date = t10.max_date ";
                    query += " JOIN (SELECT terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE '%C3 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_05' LIMIT 1), 'C3 INC', -1), ' ', 1) END AS c3_inc,CASE WHEN ej.remark LIKE '%C3 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_05' LIMIT 1), 'C3 DEC', -1), ' ', 1) END AS c3_dec FROM ejlog_devicetermprob_ejreport ej ";
                    query += " WHERE probcode ='BALRP_05' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY terminalid,DATE(trxdatetime)) t11 ON t1.terminalid = t11.terminalid AND t1.min_date = t11.max_date ";
                    query += " JOIN (SELECT terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE '%C3 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_06' LIMIT 1), 'C3 OUT', -1), ' ', 1) END AS c3_out,CASE WHEN ej.remark LIKE '%C3 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_06' LIMIT 1), 'C3 END', -1), ' ', 1) END AS c3_end FROM ejlog_devicetermprob_ejreport ej ";
                    query += " WHERE probcode ='BALRP_06' and ej.trxdatetime between '" + fromdate + " 00:00:00' and '" + todate + " 23:59:59' " + filterquery + " GROUP BY terminalid,DATE(trxdatetime)) t12 ON t1.terminalid = t12.terminalid AND t1.min_date = t12.max_date ";
                    query += " left join all_device_info adi on t1.terminalid = adi.term_id ";
                    command = new MySqlCommand(query, connection);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                           
                            if (reader["min_datetime"] != DBNull.Value && DateTime.TryParse(reader["min_datetime"].ToString(), out DateTime trxDateTime))
                            {
                                transationdate_row = trxDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                transationdate_row = "-";
                            }
                            jsonData.Add(new BalancingReportModel
                            {

                                term_seq = reader["term_seq"].ToString(),
                                term_id = reader["terminalid"].ToString(),
                                term_name = reader["term_name"].ToString(),
                                transationdate = transationdate_row,
                                c1_inc = reader["c1_inc"].ToString(),
                                c1_dec = reader["c1_dec"].ToString(),
                                c1_out = reader["c1_out"].ToString(),
                                c1_end = reader["c1_end"].ToString(),
                                c2_inc = reader["c2_inc"].ToString(),
                                c2_dec = reader["c2_dec"].ToString(),
                                c2_out = reader["c2_out"].ToString(),
                                c2_end = reader["c2_end"].ToString(),
                                c3_inc = reader["c3_inc"].ToString(),
                                c3_dec = reader["c3_dec"].ToString(),
                                c3_out = reader["c3_out"].ToString(),
                                c3_end = reader["c3_end"].ToString(),
                                c1_dep = "0",
                                c2_dep = "0",
                                c3_dep = "0"

                            });
                        }
                    }

                }


                jsonData = jsonData
                .OrderBy(x => x.term_id)  // Sort by terminalid
                .ThenBy(x => x.transationdate)  // Then sort by transactiondate
                .ToList();
                balancingreport_dataList = jsonData;
                if (balancingreport_dataList == null || balancingreport_dataList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;


                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }
                ExcelUtilities_BalancingReport obj = new ExcelUtilities_BalancingReport();
                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(balancingreport_dataList);



                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;



                fname = "BalancingReport_" + DateTime.Now.ToString("yyyyMMdd");

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
        public ActionResult BalancingReport_DownloadExportFile(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            string tsDate = "";
            string teDate = "";
            try
            {




                fname = "BalancingReport_" + DateTime.Now.ToString("yyyyMMdd");

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
        #region Excel HardwareRport

        [HttpGet]
        public ActionResult HardwareReport_ExportExc(string terminalno, string todate, string fromdate)
        {
            string fname = "";
            string tsDate = "";
            string teDate = "";
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;
            string connectionstring = string.Empty;
            string filterquery = string.Empty;
            try
            {
                terminalno = terminalno ?? "";
                fromdate = fromdate ?? DateTime.Now.ToString("yyyy-MM-dd");
                todate = todate ?? DateTime.Now.ToString("yyyy-MM-dd");
                List<HardwareReportQueryModel> datas = new List<HardwareReportQueryModel>();
                if (terminalno.Length > 0)
                {
                    char firstLetter = terminalno[0];

                    switch (firstLetter)
                    {
                        case 'A':
                            connectionstring = _myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection");
                            break;

                        case 'R':
                            connectionstring = _myConfiguration.GetValue<string>("ConnectString_MySQL_FV_CDM:FullNameConnection");
                            break;
                        case 'L':
                            connectionstring = _myConfiguration.GetValue<string>("ConnectString_MySQL_FV_LRM:FullNameConnection");
                            break;


                        default:
                            connectionstring = _myConfiguration.GetValue<string>("ConnectString_FVMySQL:FullNameConnection");
                            break;
                    }
                }

                using (MySqlConnection connection = new MySqlConnection(connectionstring))
                {
                    string transationdate_row = "";
                    connection.Open();

                    if (terminalno != "")
                    {
                        filterquery += " and TERM_ID = '" + terminalno + "' ";
                    }
                    string query = @"SELECT TERM_ID,EVENT_ID,END_TIME FROM mt_caseflow_record_his ";

                    query += " WHERE END_TIME BETWEEN '" + fromdate + " 00:00:00' AND '" + todate + " 23:59:59' " + filterquery;


                    MySqlCommand command = new MySqlCommand(query, connection);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            datas.Add(new HardwareReportQueryModel
                            {

                                TERM_ID = reader["TERM_ID"].ToString(),
                                EVENT_ID = reader["EVENT_ID"].ToString(),
                                END_TIME = reader["END_TIME"].ToString(),

                            });
                        }
                    }
                }
                List<HardwareReportWebModel> problemList = new List<HardwareReportWebModel>
                {
                new HardwareReportWebModel { problem_name = "Terminal Maintenance Mode", problem_count = datas.Count(x => x.EVENT_ID == "E1002") },
                new HardwareReportWebModel { problem_name = "Terminal OffLineMode", problem_count = datas.Count(x => x.EVENT_ID == "E1005") },
                new HardwareReportWebModel { problem_name = "Terminal StopService", problem_count = datas.Count(x => x.EVENT_ID == "E1006") },
                new HardwareReportWebModel { problem_name = "Cash Dispenser Error", problem_count = datas.Count(x => x.EVENT_ID == "E1010") },
                new HardwareReportWebModel { problem_name = "Cash Acceptor Error", problem_count = datas.Count(x => x.EVENT_ID == "E1012") },
                new HardwareReportWebModel { problem_name = "Cash Acceptor Note Jam", problem_count = datas.Count(x => x.EVENT_ID == "E1020") },
                new HardwareReportWebModel { problem_name = "Card Reader Error", problem_count = datas.Count(x => x.EVENT_ID == "E1036") },
                new HardwareReportWebModel { problem_name = "Thai ID Card Reader Error", problem_count = datas.Count(x => x.EVENT_ID == "E1038") },
                new HardwareReportWebModel { problem_name = "EPP Keypad Error", problem_count = datas.Count(x => x.EVENT_ID == "E1046") },
                new HardwareReportWebModel { problem_name = "Cassettes Error", problem_count = datas.Count(x => x.EVENT_ID == "E1127") },
                new HardwareReportWebModel { problem_name = "CardRetractBin Full", problem_count = datas.Count(x => x.EVENT_ID == "E1136") },
                new HardwareReportWebModel { problem_name = "Withdrawal Hardware Fault", problem_count = datas.Count(x => x.EVENT_ID == "E1149") },
                new HardwareReportWebModel { problem_name = "Withdrawal Cassette Issue", problem_count = datas.Count(x => x.EVENT_ID == "E1150") },
                new HardwareReportWebModel { problem_name = "Withdrawal No Cassette", problem_count = datas.Count(x => x.EVENT_ID == "E1151") },
                new HardwareReportWebModel { problem_name = "Withdrawal Retract Reject Full", problem_count = datas.Count(x => x.EVENT_ID == "E1152") },
                new HardwareReportWebModel { problem_name = "Withdrawal No CashReplenishment", problem_count = datas.Count(x => x.EVENT_ID == "E1153") },
                new HardwareReportWebModel { problem_name = "Communication Interrupt", problem_count = datas.Count(x => x.EVENT_ID == "E1156") },
                new HardwareReportWebModel { problem_name = "Vibration Alarm", problem_count = datas.Count(x => x.EVENT_ID == "E1179") },
                new HardwareReportWebModel { problem_name = "Box Alarm", problem_count = datas.Count(x => x.EVENT_ID == "E1182") },
                new HardwareReportWebModel { problem_name = "AntiJump Alarm", problem_count = datas.Count(x => x.EVENT_ID == "E1183") },
                new HardwareReportWebModel { problem_name = "Heat Alarm", problem_count = datas.Count(x => x.EVENT_ID == "E1185") },
                new HardwareReportWebModel { problem_name = "FaceCamera Error", problem_count = datas.Count(x => x.EVENT_ID == "E1217") },
                new HardwareReportWebModel { problem_name = "ShutterCamera Error", problem_count = datas.Count(x => x.EVENT_ID == "E1220") },
                new HardwareReportWebModel { problem_name = "CardReader Error Card Jammed", problem_count = datas.Count(x => x.EVENT_ID == "E1283") },
                new HardwareReportWebModel { problem_name = "Receipt Printer Error", problem_count = datas.Count(x => x.EVENT_ID == "E1375") },
                new HardwareReportWebModel { problem_name = "RPRXPaper Low", problem_count = datas.Count(x => x.EVENT_ID == "E2197") },
                new HardwareReportWebModel { problem_name = "RPRXPaper Out", problem_count = datas.Count(x => x.EVENT_ID == "E2198") },
                new HardwareReportWebModel { problem_name = "RPRXPaper NotSupport", problem_count = datas.Count(x => x.EVENT_ID == "E2199") },
                new HardwareReportWebModel { problem_name = "RPRXPaper Unknown", problem_count = datas.Count(x => x.EVENT_ID == "E2200") },
                new HardwareReportWebModel { problem_name = "RPRXPaper Jammed", problem_count = datas.Count(x => x.EVENT_ID == "E2201") },
                new HardwareReportWebModel { problem_name = "Cash Dispensing Shutter Jammed", problem_count = datas.Count(x => x.EVENT_ID == "E2204") },
                new HardwareReportWebModel { problem_name = "Cash Dispensing Shutter Unknown", problem_count = datas.Count(x => x.EVENT_ID == "E2205") },
                new HardwareReportWebModel { problem_name = "Cash Dispensing Shutter Not Support", problem_count = datas.Count(x => x.EVENT_ID == "E2206") },
                new HardwareReportWebModel { problem_name = "Cash Acceptor Shutter Jammed", problem_count = datas.Count(x => x.EVENT_ID == "E2209") },
                new HardwareReportWebModel { problem_name = "Cash Acceptor Shutter Unknown", problem_count = datas.Count(x => x.EVENT_ID == "E2210") },
                new HardwareReportWebModel { problem_name = "Cash Acceptor Shutter Not Support", problem_count = datas.Count(x => x.EVENT_ID == "E2211") },
                new HardwareReportWebModel { problem_name = "Other Error", problem_count = datas.Count(x => x.EVENT_ID == "E9999") }
                };
                int totalProblemCount = problemList.Sum(item => item.problem_count);
                hardwarereport_dataList = problemList;
                if (hardwarereport_dataList == null || hardwarereport_dataList.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });

                string strPath = Environment.CurrentDirectory;


                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }
                ExcelUtilities_HardwareReport obj = new ExcelUtilities_HardwareReport();
                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(hardwarereport_dataList, totalProblemCount, fromdate+ " ถึง " + todate,terminalno);
                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;
                fname = "HardwareReport_" + DateTime.Now.ToString("yyyyMMdd");
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
        public ActionResult HardwareReport_DownloadExportFile(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            string tsDate = "";
            string teDate = "";
            try
            {




                fname = "HardwareReport_" + DateTime.Now.ToString("yyyyMMdd");

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
