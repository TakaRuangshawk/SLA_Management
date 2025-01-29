using MySql.Data.MySqlClient;
using SLA_Management.Commons;
using SLA_Management.Models;
using SLA_Management.Models.OperationModel;
using SLA_Management.Models.TermProbModel;
using System.Data;
using System.Globalization;

namespace SLA_Management.Data.HealthCheck
{
    public class DBService_HealthCheck : DBService
    {
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        #region Constructor

        public DBService_HealthCheck(IConfiguration myConfiguration) : base(myConfiguration)
        {
            ConnectString_MySQL = myConfiguration;

            FullNameConnection = ConnectString_MySQL.GetValue<string>("ConnectString_MySQL:FullNameConnection");

            _objDb = new ConnectMySQL(FullNameConnection);
        }
        public DBService_HealthCheck(IConfiguration myConfiguration, string fullNameConnection) : base(myConfiguration)
        {
            ConnectString_MySQL = myConfiguration;

            FullNameConnection = fullNameConnection;
            //ConnectString_MySQL.GetValue<string>("ConnectString_MySQL:FullNameConnection");

            _objDb = new ConnectMySQL(FullNameConnection);
        }
        #endregion

        public List<HealthCheckModel> GetAllTerminalHaveErrorSLA445(string fromDateTime, string toDateTime)
        {
            List<HealthCheckModel> _result = new List<HealthCheckModel>();
            DataTable _dt = new DataTable();

            try
            {

                _dt = GetAllTerminalHaveErrorSLA445_MySQL(fromDateTime, toDateTime);
                foreach (DataRow _dr in _dt.Rows)
                {
                    HealthCheckModel obj = new HealthCheckModel();
                    obj.Terminal_ID = _dr["TERM_ID"].ToString();
                    obj.Problem_ID = _dr["probcode"].ToString();
                    obj.Status = "false";
                    if (_dr["latest_trxdatetime"] != DBNull.Value)
                    {
                        DateTime tempDate;
                        // Try parsing to DateTime to handle invalid date formats gracefully.
                        if (DateTime.TryParse(_dr["latest_trxdatetime"].ToString(), out tempDate))
                        {
                            obj.Transaction_DateTime = tempDate;

                            if ((DateTime.Now - tempDate).TotalDays > 5)
                            {
                                obj.Status = "true";
                            }

                        }
                        else
                        {
                            obj.Status = "N/A";
                            obj.Transaction_DateTime = null; // If parsing fails, set to null or a default value.
                        }
                    }
                    else
                    {
                        obj.Status = "N/A";
                        obj.Transaction_DateTime = null; // If DBNull, set to null.
                    }
                    obj.Terminal_Type = _dr["COUNTER_CODE"].ToString();
                    obj.Terminal_Name = _dr["Terminal_Name"].ToString();
                    obj.Serial_No = _dr["Serial_No"].ToString();




                    _result.Add(obj);
                }
            }
            catch (Exception ex)
            {

            }
            return _result;
        }


        public List<Device_info_record> GetDeviceInfoFeelview()
        {

            MySqlCommand com = new MySqlCommand();
            com.CommandText = "SELECT * FROM device_info order by TERM_SEQ;";
            DataTable tableTemp = _objDb.GetDatatable(com);

            List<Device_info_record> deviceInfoRecordsList = ConvertDataTableToModel.ConvertDataTable<Device_info_record>(tableTemp);

            return deviceInfoRecordsList;
        }


        public DataTable GetAllTerminalHaveErrorSLA445_MySQL(string fromDateTime, string toDateTime)
        {
            DataTable _dt = new DataTable();
            string _sql = string.Empty;

            if (fromDateTime == null || toDateTime == null)
            {
                fromDateTime = DateTime.Now.ToString("yyyy-MM-dd HHmmss", _cultureEnInfo);
                toDateTime = DateTime.Now.ToString("yyyy-MM-dd HHmmss", _cultureEnInfo);

            }

            try
            {
                //_sql = @"SELECT b.TERM_ID, a.probcode, MAX(a.trxdatetime) AS latest_trxdatetime, b.COUNTER_CODE as COUNTER_CODE , b.TERM_SEQ as Serial_No , b.TERM_NAME as Terminal_Name
                //        FROM device_info b
                //        LEFT JOIN ejlog_devicetermprob a
                //        ON a.terminalid = b.TERM_ID 
                //        AND a.probcode = 'SLA_445'
                //        AND a.trxdatetime BETWEEN '" + fromDateTime + "' AND '" + toDateTime + "' " +
                //        @"GROUP BY  b.TERM_ID , b.COUNTER_CODE
                //        HAVING 
                //         MAX(a.trxdatetime) IS NOT NULL
                //        ORDER BY 
                //        CASE 
                //            WHEN a.probcode IS NOT NULL THEN 0
                //        ELSE 1
                //            END,
                //        b.TERM_ID , b.COUNTER_CODE;";


                _sql = @"SELECT 
    b.TERM_ID, 
    a.probcode, 
    MAX(a.trxdatetime) AS latest_trxdatetime, 
    COALESCE(b.COUNTER_CODE, '-') AS COUNTER_CODE, 
    COALESCE(b.TERM_SEQ, '-') AS Serial_No, 
    COALESCE(b.TERM_NAME, '-') AS Terminal_Name
FROM device_info b
LEFT JOIN ejlog_devicetermprob a
    ON a.terminalid = b.TERM_ID 
    AND a.probcode = 'SLA_445'
WHERE b.Status IN ('use', 'roustop')
GROUP BY 
    b.TERM_ID
ORDER BY 
    b.TERM_ID;
;
";
                _dt = _objDb.GetDatatableNotParam(_sql);
                return _dt;
            }
            catch (Exception ex)
            { throw ex; }
        }


    }
}
