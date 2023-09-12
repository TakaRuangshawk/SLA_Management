using MySql.Data.MySqlClient;
using SLA_Management.Commons;
using SLA_Management.Models.OperationModel;
using SLA_Management.Models.TermProbModel;
using System.Data;

namespace SLA_Management.Data.TermProb
{
    public class DBService_TermProb_CDM
    {
        protected static IConfiguration ConnectString_MySQL;

        protected string _strErrDB = string.Empty;

        protected static ConnectMySQL _objDb;

        protected static string FullNameConnection = string.Empty;
        #region Constructor
        public DBService_TermProb_CDM(IConfiguration myConfiguration)
        {
            ConnectString_MySQL = myConfiguration;

            string _FullNameConnection = ConnectString_MySQL.GetValue<string>("ConnectString_MySQL_FV_CDM:FullNameConnection");

            _objDb = new ConnectMySQL(_FullNameConnection);
        }
        #endregion
        public List<Device_info_record> GetDeviceInfoFeelview()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM device_info order by TERM_SEQ;";
            DataTable tableTemp = _objDb.GetDatatable(com);

            List<Device_info_record> deviceInfoRecordsList = ConvertDataTableToModel.ConvertDataTable<Device_info_record>(tableTemp);

            return deviceInfoRecordsList;
        }
    }
}
