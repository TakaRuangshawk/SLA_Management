﻿
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using SLA_Management.Commons;
using SLA_Management.Models;
using SLA_Management.Models.Monitor;
using SLA_Management.Models.OperationModel;
using System.Data;
using System.Globalization;

namespace SLA_Management.Data.Monitor
{
    public class DBService_LogAnalysis : DBService
    {
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");
        public DBService_LogAnalysis(IConfiguration myConfiguration) : base(myConfiguration)
        {
            ConnectString_MySQL = myConfiguration;

            FullNameConnection = ConnectString_MySQL.GetValue<string>("ConnectString_MySQL:FullNameConnection") ?? "";

            _objDb = new ConnectMySQL(FullNameConnection);
        }
        public DBService_LogAnalysis(IConfiguration myConfiguration, string fullNameConnection) : base(myConfiguration)
        {
            ConnectString_MySQL = myConfiguration;
            FullNameConnection = fullNameConnection;
            _objDb = new ConnectMySQL(FullNameConnection);
        }
        public List<LogAnalysisModel> FetchAllData(string TermID, string fromDateTime, string toDateTime,string categoryType, string counterCode)
        {
            List<LogAnalysisModel> _result = new List<LogAnalysisModel>();
            DataTable _dt = new DataTable();

            try
            {
                _dt = FetchAllData_MySQL((TermID == null) ? "%" : TermID, fromDateTime, toDateTime, (categoryType == null || categoryType == "ALL") ? "%" : categoryType, (counterCode == null || counterCode == "ALL") ? "%" : counterCode);
                foreach (DataRow _dr in _dt.Rows)
                {
                    LogAnalysisModel obj = new LogAnalysisModel();
                    obj.Incident_No = _dr["incident_no"].ToString() ?? "";
                    obj.Terminal_SEQ = _dr["TERM_SEQ"].ToString() ?? "";
                    obj.Terminal_ID = _dr["TERM_ID"].ToString() ?? "";
                    obj.Terminal_NAME = _dr["TERM_NAME"].ToString() ?? "";
                    obj.Incident_Date = Convert.ToDateTime(_dr["incident_date"]).ToString("yyyy-MM-dd");                    
                    obj.Category = _dr["incident_name"].ToString() ?? "";
                    obj.SubCategory = _dr["analyst_01"].ToString() ?? "";
                    obj.Analyst_Info = _dr["analyst_02"].ToString() ?? "";
                    obj.Inform_By = _dr["inform_by"].ToString() ?? "";
                    obj.Counter_Code = _dr["COUNTER_CODE"].ToString() ?? "";
                    _result.Add(obj);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching all data.", ex);
            }
            return _result;
        }
        public DataTable FetchAllData_MySQL(string TermID, string fromDateTime, string toDateTime, string categoryType, string counterCode)
        {
            DataTable _dt = new DataTable();
            string _sql = string.Empty;
            var incidentName = string.Empty;

            if (fromDateTime == null || toDateTime == null)
            {
                fromDateTime = DateTime.Now.ToString("yyyy-MM-dd HHmmss", _cultureEnInfo);
                toDateTime = DateTime.Now.ToString("yyyy-MM-dd HHmmss", _cultureEnInfo);
            }

            try
            {
                _sql = @"SELECT b.TERM_SEQ,a.incident_no,a.TERM_ID,b.TERM_NAME,a.incident_date, 
                a.incident_name,a.analyst_01,a.analyst_02,a.inform_by,b.COUNTER_CODE
                FROM sisbu_analysis a
                Inner JOIN device_info b
                ON a.TERM_ID = b.TERM_ID 
                WHERE a.TERM_ID LIKE '" + TermID + "' " +
                @"AND a.incident_date BETWEEN '" + fromDateTime + "' AND '" + toDateTime + "' " +
                @"AND a.incident_name LIKE '" + categoryType + "' " +
                @"AND b.COUNTER_CODE LIKE '" + counterCode + "' " +
                @"ORDER BY a.incident_date ASC;";
                _dt = _objDb.GetDatatableNotParam(_sql);
                return _dt;
            }
            catch (Exception ex)
            { 
                throw new ApplicationException("An error occurred while getting data.", ex);
            }
        }       
        public List<Device_info_record> GetDeviceInfo()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM device_info order by TERM_SEQ;";
            DataTable dt = _objDb.GetDatatable(com);

            List<Device_info_record> result = ConvertDataTableToModel.ConvertDataTable<Device_info_record>(dt);

            return result;
        }
        public List<Sisbu_analysis_record> GetCategory()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM sisbu_analysis order by TERM_ID;";
            DataTable dt = _objDb.GetDatatable(com);

            List<Sisbu_analysis_record> result = ConvertDataTableToModel.ConvertDataTable<Sisbu_analysis_record>(dt);

            return result;
        }       
        public List<TotalCase_record> FetchTotalCaseData(string TermID, string fromDateTime, string toDateTime, string categoryType, string counterCode)
        {

            DataTable _dt = new DataTable();
            MySqlCommand _sql = new MySqlCommand();
            var incidentName = string.Empty;
            if (TermID == null)
            {
                TermID = "%";
            }
            if (fromDateTime == null || toDateTime == null)
            {
                fromDateTime = DateTime.Now.ToString("yyyy-MM-dd HHmmss", _cultureEnInfo);
                toDateTime = DateTime.Now.ToString("yyyy-MM-dd HHmmss", _cultureEnInfo);
            }
            if (categoryType == null || categoryType == "ALL")
            {
                categoryType = "%";  
            }
            if (counterCode == null || counterCode == "ALL")
            {
                counterCode = "%";
            }
            try
            {
                string query = @"SELECT DISTINCT a.incident_name, a.analyst_01, CONVERT(COUNT(a.incident_name), CHAR) AS TotalCount
                         FROM sisbu_analysis a
                         INNER JOIN device_info b ON a.TERM_ID = b.TERM_ID
                         WHERE a.TERM_ID LIKE @termID
                         AND a.incident_date BETWEEN @fromDate AND @toDate
                         AND a.incident_name LIKE @categoryType
                         AND b.COUNTER_CODE LIKE @counterCode
                         GROUP BY a.incident_name, a.analyst_01;";

                _sql.CommandText = query;
                _sql.Parameters.AddWithValue("@termID", TermID);
                _sql.Parameters.AddWithValue("@fromDate", fromDateTime);
                _sql.Parameters.AddWithValue("@toDate", toDateTime);
                _sql.Parameters.AddWithValue("@categoryType", categoryType);
                _sql.Parameters.AddWithValue("@counterCode", counterCode);

                _dt = _objDb.GetDatatable(_sql);
            }
            catch (Exception ex)
            { 
                throw new ApplicationException("An error occurred while fetching total case data.", ex);
            }

            List<TotalCase_record> result = ConvertDataTableToModel.ConvertDataTable<TotalCase_record>(_dt);

            return result;
        }        
    }
}
