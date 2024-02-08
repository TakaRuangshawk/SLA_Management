using MySql.Data.MySqlClient;
using SLA_Management.Commons;
using SLA_Management.Models.OperationModel;
using SLA_Management.Models.ReportModel;
using System.Data;

namespace SLA_Management.Data.AuditReport
{
    public class DBService_AuditReportcs : DBService
    {
        static string FullNameConnection_SECOne = string.Empty;

        protected static ConnectMySQL _objDb_SECOne;
        public DBService_AuditReportcs(IConfiguration myConfiguration) : base(myConfiguration)
        {
            ConnectString_MySQL = myConfiguration;

            FullNameConnection = ConnectString_MySQL.GetValue<string>("ConnectString_MySQL:FullNameConnection");

            FullNameConnection_SECOne = ConnectString_MySQL.GetValue<string>("ConnectString_SECOne:FullNameConnection");

            _objDb = new ConnectMySQL(FullNameConnection);

            _objDb_SECOne = new ConnectMySQL(FullNameConnection_SECOne);
        }

        //"SELECT * FROM system_users order by UPDATE_DATE DESC;"
        public List<fv_system_users> GetAuditReportcsFeelview()
        {

            MySqlCommand com = new MySqlCommand();

            com.CommandText = "SELECT * FROM fv_system_users order by UPDATE_DATE;";
            DataTable tableTemp = _objDb.GetDatatable(com);

            List<fv_system_users> auditReportcsRecordsList = (from DataRow dr in tableTemp.Rows
                                                           select new fv_system_users()
                                                           {
                                                               System = "Feelview",
                                                               AccountName = dr["ACCOUNT"].ToString(),
                                                               UserName = dr["NAME"].ToString(),
                                                               LastLoginDateTime = DateTime.TryParse(dr["LAST_LOGINTIME"].ToString(), out DateTime lastLoginTime) ? lastLoginTime : DateTime.MinValue,
                                                               Status = dr["FLAG"].ToString(),
                                                           }).ToList();


            return auditReportcsRecordsList;
        }

        public List<fv_system_users> GetAuditReportcsSECOne()
        {

            MySqlCommand com = new MySqlCommand();

            com.CommandText = "SELECT * FROM system_users order by UPDATE_DATE;";
            DataTable tableTemp = _objDb_SECOne.GetDatatable(com);

            List<fv_system_users> auditReportcsRecordsList = (from DataRow dr in tableTemp.Rows
                                                              select new fv_system_users()
                                                              {
                                                                  System = "SECOne",
                                                                  AccountName = dr["ACCOUNT"].ToString(),
                                                                  UserName = dr["NAME"].ToString(),
                                                                  LastLoginDateTime = DateTime.TryParse(dr["LAST_LOGINTIME"].ToString(), out DateTime lastLoginTime) ? lastLoginTime : DateTime.MinValue,
                                                                  Status = dr["FLAG"].ToString(),
                                                              }).ToList();


            return auditReportcsRecordsList;
        }




    }
}
