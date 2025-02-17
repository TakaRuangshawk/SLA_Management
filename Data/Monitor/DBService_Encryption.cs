
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
    public class DBService_Encryption : DBService
    {
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");
        public DBService_Encryption(IConfiguration myConfiguration) : base(myConfiguration)
        {
            ConnectString_MySQL = myConfiguration;

            FullNameConnection = ConnectString_MySQL.GetValue<string>("ConnectString_MySQL:FullNameConnection") ?? "";

            _objDb = new ConnectMySQL(FullNameConnection);
        }
        public DBService_Encryption(IConfiguration myConfiguration, string fullNameConnection) : base(myConfiguration)
        {
            ConnectString_MySQL = myConfiguration;
            FullNameConnection = fullNameConnection;
            _objDb = new ConnectMySQL(FullNameConnection);
        }              
        public List<Device_info_record> GetDeviceInfo()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM device_info order by TERM_SEQ;";
            DataTable dt = _objDb.GetDatatable(com);

            List<Device_info_record> result = ConvertDataTableToModel.ConvertDataTable<Device_info_record>(dt);

            return result;
        }

        public List<Version_info_record> GetVersionInfo()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM secureageversion_record order by Term_Seq;";
            DataTable dt = _objDb.GetDatatable(com);

            List<Version_info_record> result = ConvertDataTableToModel.ConvertDataTable<Version_info_record>(dt);

            return result;
        }
        public LatestUpdate_record GetLatestUpdate()
        {
            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT Update_Date, Update_By FROM secureageversion_record ORDER BY Update_Date DESC LIMIT 1;";
            DataTable dt = _objDb.GetDatatable(com);

            // Convert the result to a single record
            List<LatestUpdate_record> result = ConvertDataTableToModel.ConvertDataTable<LatestUpdate_record>(dt);
            return result.FirstOrDefault(); // Return only the first record or null if no data
        }
        public List<EncryptionModel> FetchAllData(string terminalId, string counterCode, string version, string policy)
        {
            List<EncryptionModel> _result = new List<EncryptionModel>();
            DataTable _dt = new DataTable();

            try
            {
                _dt = FetchAllData_MySQL((terminalId == null) ? "%" : terminalId, (counterCode == null || counterCode == "ALL") ? "%" : counterCode, (version == null || version == "ALL") ? "%" : version, (policy == null || policy == "ALL") ? "%" : policy);
                foreach (DataRow _dr in _dt.Rows)
                {
                    EncryptionModel obj = new EncryptionModel();
                    obj.Terminal_SEQ = _dr["TERM_SEQ"].ToString() ?? "";
                    obj.Terminal_ID = _dr["TERM_ID"].ToString() ?? "";
                    obj.Terminal_NAME = _dr["TERM_NAME"].ToString() ?? "";
                    obj.Counter_Code = _dr["COUNTER_CODE"].ToString() ?? "";
                    obj.Version = _dr["SecureAge_Version"].ToString() ?? "";
                    obj.Policy = _dr["Policy"].ToString() ?? "";
                    _result.Add(obj);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching all data.", ex);
            }
            return _result;
        }
        public DataTable FetchAllData_MySQL(string terminalId, string counterCode, string version, string policy)
        {
            DataTable _dt = new DataTable();
            string _sql = string.Empty;

            try
            {
                _sql = @"SELECT a.TERM_SEQ,b.TERM_ID,b.TERM_NAME,b.COUNTER_CODE,a.SecureAge_Version,a.Policy
                FROM secureageversion_record a
                INNER JOIN device_info b
                ON a.Term_Seq = b.TERM_SEQ 
                WHERE b.TERM_ID LIKE '" + terminalId + "' " +
                @"AND b.COUNTER_CODE LIKE '" + counterCode + "' " +
                @"AND a.SecureAge_Version LIKE '" + version + "' " +
                @"AND a.Policy LIKE '" + policy + "' " +
                @"ORDER BY a.TERM_SEQ ASC;";
                _dt = _objDb.GetDatatableNotParam(_sql);
                return _dt;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while getting data.", ex);
            }
        }

    }
}
