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

namespace SLA_Management.Controllers
{
    public class ReportController : Controller
    {
        private IConfiguration _myConfiguration;
        private static ConnectMySQL db_all;
        private List<BalancingReportModel> balancingreport_dataList = new List<BalancingReportModel>();
        public ReportController(IConfiguration myConfiguration)
        {
            _myConfiguration = myConfiguration;
            db_all = new ConnectMySQL(myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection"));
            

        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult BalancingReport()
        {
            ViewBag.CurrentTID = GetDeviceInfoALL();
            return View();
        }
        private static List<Device_info> GetDeviceInfoALL()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM all_device_info order by TERM_SEQ;";
            DataTable testss = db_all.GetDatatable(com);

            List<Device_info> test = ConvertDataTableToModel.ConvertDataTable<Device_info>(testss);

            return test;
        }
        public IActionResult BalancingReportFetchData(string terminalno, string row, string page, string search, string todate, string fromdate)
        {
            int _page;
            int id_row = 0;
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

            List<BalancingReportModel> jsonData = new List<BalancingReportModel>();
            if (search == "search")
            {


                using (MySqlConnection connection = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_MySQL:FullNameConnection")))
                {
                    string transationdate_row = "";
                    connection.Open();

                    // Modify the SQL query to use the 'input' parameter for filtering
                    string query = @"SELECT t1.terminalid,adi.term_seq,adi.term_name,t1.min_datetime,t1.c1_inc,t1.c1_dec,t2.c1_out,t2.c1_end,t3.c2_inc,t3.c2_dec,t4.c2_out,t4.c2_end,t5.c3_inc,t5.c3_dec,t6.c3_out,t6.c3_end
FROM (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C1 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 INC', -1), ' ', 1) END AS c1_inc,CASE WHEN ej.remark LIKE '%C1 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 DEC', -1), ' ', 1) END AS c1_dec FROM ejlog_devicetermprob_ejreport ej
        WHERE probcode ='BALRP_01'
        GROUP BY ej.terminalid, min_date HAVING (c1_inc IS NOT NULL OR c1_dec IS NOT NULL)) t1
    JOIN (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C1 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 OUT', -1), ' ', 1) END AS c1_out,CASE WHEN ej.remark LIKE '%C1 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 END', -1), ' ', 1) END AS c1_end FROM ejlog_devicetermprob_ejreport ej
        WHERE probcode ='BALRP_02'
        GROUP BY
            ej.terminalid, min_date HAVING (c1_out IS NOT NULL OR c1_end IS NOT NULL)) t2 ON t1.terminalid = t2.terminalid AND t1.min_date = t2.min_date
    JOIN (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C2 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C2 INC', -1), ' ', 1) END AS c2_inc,CASE WHEN ej.remark LIKE '%C2 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C2 DEC', -1), ' ', 1) END AS c2_dec FROM ejlog_devicetermprob_ejreport ej
        WHERE probcode ='BALRP_03'
        GROUP BY ej.terminalid, min_date HAVING (c2_inc IS NOT NULL OR c2_dec IS NOT NULL)) t3 ON t1.terminalid = t3.terminalid AND t1.min_date = t3.min_date
    JOIN (SELECT ej.terminalid, DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C2 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C2 OUT', -1), ' ', 1) END AS c2_out,CASE WHEN ej.remark LIKE '%C2 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C2 END', -1), ' ', 1) END AS c2_end FROM ejlog_devicetermprob_ejreport ej
        WHERE probcode ='BALRP_04'
        GROUP BY ej.terminalid, min_date HAVING (c2_out IS NOT NULL OR c2_end IS NOT NULL)) t4 ON t1.terminalid = t4.terminalid AND t1.min_date = t4.min_date
    JOIN (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C3 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C3 INC', -1), ' ', 1) END AS c3_inc, CASE WHEN ej.remark LIKE '%C3 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C3 DEC', -1), ' ', 1) END AS c3_dec FROM ejlog_devicetermprob_ejreport ej
        WHERE probcode ='BALRP_05'
        GROUP BY ej.terminalid, min_date HAVING (c3_inc IS NOT NULL OR c3_dec IS NOT NULL)) t5 ON t1.terminalid = t5.terminalid AND t1.min_date = t5.min_date
    JOIN
    (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C3 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C3 OUT', -1), ' ', 1) END AS c3_out,CASE WHEN ej.remark LIKE '%C3 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C3 END', -1), ' ', 1) END AS c3_end FROM ejlog_devicetermprob_ejreport ej
        WHERE probcode ='BALRP_06'
        GROUP BY ej.terminalid, min_date HAVING(c3_out IS NOT NULL OR c3_end IS NOT NULL)) t6 ON t1.terminalid = t6.terminalid AND t1.min_date = t6.min_date
left join all_device_info adi on t1.terminalid = adi.term_id";

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
                            });
                        }
                    }
                    query = @"SELECT t1.terminalid,adi.term_seq,adi.term_name,t7.max_transaction_date as min_datetime,t7.c1_inc,t7.c1_dec,t8.c1_out,t8.c1_end,t9.c2_inc,t9.c2_dec,t10.c2_out,t10.c2_end,t11.c3_inc,t11.c3_dec,t12.c3_out,t12.c3_end
FROM (SELECT ej.terminalid,DATE(ej.trxdatetime) AS min_date,MIN(ej.trxdatetime) AS min_datetime,CASE WHEN ej.remark LIKE '%C1 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 INC', -1), ' ', 1) END AS c1_inc,CASE WHEN ej.remark LIKE '%C1 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX(ej.remark, 'C1 DEC', -1), ' ', 1) END AS c1_dec FROM ejlog_devicetermprob_ejreport ej 
        WHERE probcode ='BALRP_01'
        GROUP BY ej.terminalid, min_date HAVING (c1_inc IS NOT NULL OR c1_dec IS NOT NULL)) t1
JOIN (SELECT terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE '%C1 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_01' LIMIT 1), 'C1 INC', -1), ' ', 1) END AS c1_inc,CASE WHEN ej.remark LIKE '%C1 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_01' LIMIT 1), 'C1 DEC', -1), ' ', 1) END AS c1_dec FROM ejlog_devicetermprob_ejreport ej
    WHERE probcode ='BALRP_01'  
    GROUP BY terminalid,DATE(trxdatetime)) t7 ON t1.terminalid = t7.terminalid AND t1.min_date = t7.max_date
JOIN (select terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE 'C1 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_02'  LIMIT 1), 'C1 OUT', -1), ' ', 1) END AS c1_out,CASE WHEN ej.remark LIKE '%C1 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_02' LIMIT 1), 'C1 END', -1), ' ', 1) END AS c1_end FROM ejlog_devicetermprob_ejreport ej
    WHERE probcode ='BALRP_02'  
    GROUP BY terminalid,DATE(trxdatetime)) t8 ON t1.terminalid = t8.terminalid AND t1.min_date = t8.max_date
JOIN (select terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE '%C2 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_03' LIMIT 1), 'C2 INC', -1), ' ', 1) END AS c2_inc,CASE WHEN ej.remark LIKE '%C2 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_03' LIMIT 1), 'C2 DEC', -1), ' ', 1) END AS c2_dec FROM ejlog_devicetermprob_ejreport ej
    WHERE probcode ='BALRP_03'  
    GROUP BY terminalid,DATE(trxdatetime)) t9 ON t1.terminalid = t9.terminalid AND t1.min_date = t9.max_date
JOIN (SELECT terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE '%C2 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_04' LIMIT 1), 'C2 OUT', -1), ' ', 1) END AS c2_out,CASE WHEN ej.remark LIKE '%C2 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_04' LIMIT 1), 'C2 END', -1), ' ', 1) END AS c2_end FROM ejlog_devicetermprob_ejreport ej
    WHERE probcode ='BALRP_04'  
    GROUP BY terminalid,DATE(trxdatetime)) t10 ON t1.terminalid = t10.terminalid AND t1.min_date = t10.max_date
JOIN (SELECT terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE '%C3 INC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_05' LIMIT 1), 'C3 INC', -1), ' ', 1) END AS c3_inc,CASE WHEN ej.remark LIKE '%C3 DEC%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_05' LIMIT 1), 'C3 DEC', -1), ' ', 1) END AS c3_dec FROM ejlog_devicetermprob_ejreport ej
    WHERE probcode ='BALRP_05'  
    GROUP BY terminalid,DATE(trxdatetime)) t11 ON t1.terminalid = t11.terminalid AND t1.min_date = t11.max_date
JOIN (SELECT terminalid,DATE(ej.trxdatetime) AS max_date,MAX(trxdatetime) AS max_transaction_date,CASE WHEN ej.remark LIKE 'C3 OUT%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_06' LIMIT 1), 'C3 OUT', -1), ' ', 1) END AS c3_out,CASE WHEN ej.remark LIKE '%C3 END%' THEN SUBSTRING_INDEX(SUBSTRING_INDEX((SELECT remark FROM ejlog_devicetermprob_ejreport subej WHERE subej.terminalid = ej.terminalid AND subej.trxdatetime = MAX(ej.trxdatetime) and probcode ='BALRP_06' LIMIT 1), 'C3 END', -1), ' ', 1) END AS c3_end FROM ejlog_devicetermprob_ejreport ej
    WHERE probcode ='BALRP_06'  
    GROUP BY terminalid, DATE(trxdatetime)) t12 ON t1.terminalid = t12.terminalid AND t1.min_date = t12.max_date
left join all_device_info adi on t1.terminalid = adi.term_id";
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
                            });
                        }
                    }
                }
            }
            else
            {
                jsonData = balancingreport_dataList;
            }
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

        }
        public class DataResponse_BalancingReport
        {
            public List<BalancingReportModel> JsonData { get; set; }
            public int Page { get; set; }
            public int currentPage { get; set; }
            public int TotalTerminal { get; set; }
        }
    }
}
