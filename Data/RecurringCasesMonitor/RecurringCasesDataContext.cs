#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SLA_Management.Commons;
using SLA_Management.Models.OperationModel;
using SLA_Management.Models.RecurringCasesMonitor;
using System.Drawing;
using System.Globalization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static SLA_Management.Controllers.MaintenanceController;


namespace SLA_Management.Data.RecurringCasesMonitor
{
    public class RecurringCasesDataContext 
    {
        private readonly string _connectionString;
        public RecurringCasesDataContext(string connection) 
        {
            _connectionString = connection;
        }

        public List<Device_info_record> GetDeviceInfoFeelview()
        {
            List<Device_info_record> device_Info_Records = new List<Device_info_record>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.OpenAsync();
                string query = "SELECT * FROM device_info order by TERM_SEQ;";
                using (MySqlCommand cmd = new MySqlCommand(query,connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            device_Info_Records.Add(new Device_info_record()
                            {
                                TERM_ID = reader["TERM_ID"].ToString(),
                                COUNTER_CODE = reader["COUNTER_CODE"].ToString(),
                                TYPE_ID = reader["TYPE_ID"].ToString(),
                                TERM_SEQ = reader["TERM_SEQ"].ToString(),
                                TERM_NAME = reader["TERM_NAME"].ToString(),
                            });
                        }
                    }
                }
            }
            
            return device_Info_Records;
        }
        public List<RecurringCaseDetail> GetRecurringCaseDetailsList(string termID,string frDate,string toDate)
        {
            string informDate;
            List<RecurringCaseDetail> recurringCaseDetails = new List<RecurringCaseDetail>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.OpenAsync();
                string query = "SELECT Date_Inform,Case_Error_No,Issue_Name,Repair1,Repair2,Incident_No FROM reportcases WHERE Terminal_ID=@termId AND Date_Inform BETWEEN @frDate AND @toDate";
                using(var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@termId",termID);
                    cmd.Parameters.AddWithValue("@frDate",frDate);
                    cmd.Parameters.AddWithValue("@toDate",toDate);
                    using (var reader = cmd.ExecuteReader()) 
                    {
                        while (reader.Read()) 
                        {
                            if (reader["Date_Inform"] != DBNull.Value)
                            {
                                DateTime xValue = Convert.ToDateTime(reader["Date_Inform"]);
                                informDate = xValue.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                informDate = "-";
                            }
                            var detail = new RecurringCaseDetail
                            {
                                Date_Inform = informDate,
                                Case_Error_No = reader["Case_Error_No"].ToString(),
                                Issue_Name = reader["Issue_Name"].ToString(),
                                Repair1 = reader["Repair1"].ToString(),
                                Repair2 = reader["Repair2"].ToString(),      
                                Incident_No = reader["Incident_No"].ToString(),
                                
                            };
                            recurringCaseDetails.Add(detail);
                        }
                    }
                }
            }
            return recurringCaseDetails;
        }
        public List<RecurringCase> GetRecurringTerminalList(string termID, string frDate, string toDate, string terminalType,string orderBy)
        {
            List<RecurringCase> recurringCases = new List<RecurringCase>();            
            if (frDate != null && toDate != null) 
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.OpenAsync();
                    string procName = "GetTotalRecurringCases";
                    using (MySqlCommand cmd = new MySqlCommand(procName, connection))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@terminal", termID);
                        cmd.Parameters.AddWithValue("@from_date", frDate);
                        cmd.Parameters.AddWithValue("@to_date", toDate);
                        cmd.Parameters.AddWithValue("@counter_code", terminalType == "ALL" ? null : terminalType);
                        cmd.Parameters.AddWithValue("@order_by", orderBy);

                        using (var reader = cmd.ExecuteReader())
                        {
                            int recordCount = 1;
                            while (reader.Read())
                            {
                                var recurCase = new RecurringCase
                                {
                                    No = recordCount,
                                    Serial_No = reader["TERM_SEQ"].ToString(),
                                    Terminal_Id = reader["TERM_ID"].ToString(),
                                    Terminal_Name = reader["TERM_NAME"].ToString(),
                                    Counter_Code = reader["COUNTER_CODE"].ToString(),
                                    Total_Recurring_Cases = Convert.ToInt32(reader["Total_Cases"])
                                };
                                recurringCases.Add(recurCase);
                                recordCount++;

                            }
                        }
                    }
                }
                return recurringCases;
            }
            else
            {
                return recurringCases;
            }
        }        
    }
}
