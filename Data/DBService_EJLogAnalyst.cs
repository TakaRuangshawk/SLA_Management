

using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using SLA_Management.Commons;
using SLA_Management.Models;
using SLA_Management.Models.LogMonitorModel;
using SLA_Management.Models.OperationModel;
using System.Data;
using System.Globalization;
using System.Transactions;


namespace SLA_Management.Data
{
    public class DBService_EJLoganalyst : DBService
    {
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");
        public DBService_EJLoganalyst(IConfiguration myConfiguration) : base(myConfiguration)
        {
            ConnectString_MySQL = myConfiguration;

            FullNameConnection = ConnectString_MySQL.GetValue<string>("ConnectString_MySQL:FullNameConnection");

            _objDb = new ConnectMySQL(FullNameConnection);

        }
        public DBService_EJLoganalyst(IConfiguration myConfiguration, string fullNameConnection) : base(myConfiguration)
        {
            ConnectString_MySQL = myConfiguration;

            FullNameConnection = fullNameConnection;
            //ConnectString_MySQL.GetValue<string>("ConnectString_MySQL:FullNameConnection");

            _objDb = new ConnectMySQL(FullNameConnection);
        }
        public List<Device_info_record> GetDeviceInfo()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM fv_device_info order by TERM_SEQ;";
            DataTable tableTemp = _objDb.GetDatatable(com);

            List<Device_info_record> deviceInfoRecordsList = ConvertDataTableToModel.ConvertDataTable<Device_info_record>(tableTemp);

            return deviceInfoRecordsList;
        
        }

        public List<Transaction_status_record> GetTransactionStatus()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT Transaction_Status AS TransactionStatus FROM EJLogAnalyst;";
            DataTable tableTemp = _objDb.GetDatatable(com);

            List<Transaction_status_record> transactionStatusList = ConvertDataTableToModel.ConvertDataTable<Transaction_status_record>(tableTemp);

            return transactionStatusList;

        }

        public LatestUpdate_record GetLatestUpdate()
        {
            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT Update_Date, Update_By FROM ejloganalyst ORDER BY Update_Date DESC LIMIT 1;";
            DataTable dt = _objDb.GetDatatable(com);

            // Convert the result to a single record
            List<LatestUpdate_record> result = ConvertDataTableToModel.ConvertDataTable<LatestUpdate_record>(dt);
            return result.FirstOrDefault(); // Return only the first record or null if no data
        }

        public List<EJLogAnalystModel> FetchAllData(string terminalId,string transactionStatus, string FrDate, string ToDate)
        {
            List<EJLogAnalystModel> _result = new List<EJLogAnalystModel>();
            DataTable _dt = new DataTable();

            try
            {
                _dt = FetchAllData_MySQL(string.IsNullOrEmpty(terminalId) ? "%" : terminalId, transactionStatus, FrDate, ToDate);



                foreach (DataRow _dr in _dt.Rows)
                {
                    EJLogAnalystModel obj = new EJLogAnalystModel();
                    

                    obj.Id = _dr["Id"] == DBNull.Value ? 0 : Convert.ToInt64(_dr["Id"]);
                    obj.TerminalId = _dr["Terminal_Id"]?.ToString();

                    obj.TransactionDateTime = _dr["Transaction_Date"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(_dr["Transaction_Date"]);

                    obj.TransactionType = _dr["Transaction_Type"]?.ToString();

                    obj.TransactionStatus = _dr["Transaction_Status"]?.ToString() ?? "FAIL";

                    obj.FullTransaction = _dr["FullTransaction"]?.ToString() ?? "";

                    obj.Amount = _dr["Amount"] == DBNull.Value
                        ? null
                        : Convert.ToDecimal(_dr["Amount"]);

                    obj.CardNumber = _dr["Card_Number"]?.ToString();
                    obj.SequenceNo = _dr["Sequence_No"]?.ToString();
                    obj.EJFileName = _dr["EJ_File_Name"]?.ToString();
                    obj.TermSeq = _dr["TERM_SEQ"]?.ToString();
                    obj.TypeId = _dr["TYPE_ID"]?.ToString();
                    obj.TermName = _dr["TERM_NAME"]?.ToString();

                    _result.Add(obj);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching EJ log data.", ex);
            }

            return _result;
        }

        public DataTable FetchAllData_MySQL(string terminalId, string transactionStatus, string FrDate, string ToDate)
        {
            try
            {
                string sql = @"
                    SELECT 
                        ej.*,
                        d.TERM_SEQ,
                        d.TYPE_ID,
                        d.TERM_NAME
                    FROM EJLogAnalyst ej
                    LEFT JOIN fv_device_info d
                        ON ej.Terminal_Id = d.TERM_ID
                    WHERE ej.Terminal_Id LIKE @terminalId
                    ";

                using var cmd = new MySqlCommand();

                cmd.Parameters.AddWithValue("@terminalId", "%" + terminalId + "%");

                if (!string.IsNullOrWhiteSpace(transactionStatus) &&
                    !transactionStatus.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                {
                    sql += " AND ej.Transaction_Status = @transactionStatus ";
                    cmd.Parameters.AddWithValue("@transactionStatus", transactionStatus);
                }

                if (!string.IsNullOrWhiteSpace(FrDate) && !string.IsNullOrWhiteSpace(ToDate))
                {
                    sql += " AND DATE(ej.Transaction_Date) BETWEEN STR_TO_DATE(@FrDate, '%Y-%m-%d') AND STR_TO_DATE(@ToDate, '%Y-%m-%d') ";
                    cmd.Parameters.AddWithValue("@FrDate", FrDate);
                    cmd.Parameters.AddWithValue("@ToDate", ToDate);
                }


                sql += " ORDER BY ej.Transaction_Date DESC ";

                cmd.CommandText = sql;  

                return _objDb.GetDatatable(cmd);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while getting EJ data.", ex);
            }
        }
        public interface IEJPatternService
        {
            List<string> GetPatterns();
            void InvalidateCache();
            List<KeyValuePair<string, string>> GetTransactionTypes();
            void InvalidateTransactionTypeCache();
        }
        
        public class EJPatternService : IEJPatternService
        {
            private List<string> _patterns;
            private List<KeyValuePair<string, string>> _transactionTypes; 

            private readonly string _connStr;
            private readonly object _lock = new object();

            public EJPatternService(IConfiguration config)
            {
                _connStr = config.GetValue<string>("ConnectString_MySQL:FullNameConnection");
            }
            //GSBEJErrorPattern
            public List<string> GetPatterns()
            {
                if (_patterns != null) return _patterns;

                lock (_lock)
                {
                    if (_patterns != null) return _patterns;

                    var result = new List<string>();
                    using var conn = new MySqlConnection(_connStr);
                    conn.Open();
                    using var cmd = new MySqlCommand(
                        "SELECT Pattern FROM GSBEJErrorPattern WHERE IsActive = 1", conn);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                        result.Add(reader.GetString(0));

                    _patterns = result;
                }

                return _patterns;
            }

            public void InvalidateCache() => _patterns = null;
            // GetTransactionTypes
            public List<KeyValuePair<string, string>> GetTransactionTypes()
            {
                if (_transactionTypes != null) return _transactionTypes;
                lock (_lock)
                {
                    if (_transactionTypes != null) return _transactionTypes;
                    var result = new List<KeyValuePair<string, string>>();
                    using var conn = new MySqlConnection(_connStr);
                    conn.Open();
                    using var cmd = new MySqlCommand(
                        @"SELECT TransCode, TransTypeName 
                  FROM GSBEJTransactionType 
                  WHERE IsActive = 1 
                  ORDER BY Id ASC", conn);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                        result.Add(new KeyValuePair<string, string>(
                            reader.GetString(0),
                            reader.GetString(1)));
                    _transactionTypes = result;
                }
                return _transactionTypes;
            }
             public void InvalidateTransactionTypeCache() => _transactionTypes = null;
        }     
    }
}
