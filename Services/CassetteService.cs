using MySql.Data.MySqlClient;
using SLA_Management.Commons;
using SLA_Management.Data;
using SLA_Management.Models.CassetteStatus;
using SLA_Management.Models.TermProbModel;
using System.Data;

namespace SLA_Management.Services
{
    public class CassetteService
    {
        private readonly IConfiguration _configuration;

        public CassetteService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string? GetConnectionStringByBank(string bankName)
        {
            var key = $"ConnectString_NonOutsource:FullNameConnection_{bankName.ToLower()}";
            return _configuration.GetValue<string>(key);
        }

        public List<string> GetLotTypes(string bankName)
        {
            var connStr = GetConnectionStringByBank(bankName);
            var db_mysql = new ConnectMySQL(connStr);

            var com = new MySqlCommand
            {
                CommandText = @"SELECT DISTINCT COUNTER_CODE 
                                FROM device_info 
                                WHERE COUNTER_CODE IS NOT NULL AND COUNTER_CODE <> '' 
                                ORDER BY COUNTER_CODE;"
            };

            var dt = db_mysql.GetDatatable(com);
            var lotTypes = new List<string>();

            foreach (DataRow row in dt.Rows)
            {
                string value = row["COUNTER_CODE"]?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    lotTypes.Add(value);
                }
            }

            return lotTypes;
        }

        public (string DateText, string UpdatedBy) GetLatestUpdate(string bankName)
        {
            try
            {
                var connStr = GetConnectionStringByBank(bankName);
                if (string.IsNullOrEmpty(connStr)) return ("-", "-");

                var db_mysql = new ConnectMySQL(connStr);
                var cmd = new MySqlCommand("SELECT Upload_Date, Upload_By FROM import_file_data ORDER BY Upload_Date DESC LIMIT 1;");
                var dt = db_mysql.GetDatatable(cmd);

                var latest = ConvertDataTableToModel.ConvertDataTable<ImportFileData>(dt).FirstOrDefault();
                return latest != null
                    ? (latest.Upload_Date.ToString("dd/MM/yyyy HH:mm"), latest.Upload_By)
                    : ("-", "-");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetLatestUpdate Error: {ex.Message}");
                return ("-", "-");
            }
        }

        public async Task<List<report_display>> GetCassetteStatusAsync(string terminalID, string lotType, DateTime? fromdate, string bankName)
        {
            var reportCassette = new List<report_display>();
            string connectionDB = GetConnectionStringByBank(bankName);
            if (string.IsNullOrEmpty(connectionDB)) return reportCassette;

            var dbPrefix = bankName.ToLower() + "_logview";

            try
            {
                using var conn = new MySqlConnection(connectionDB);
                await conn.OpenAsync();

                string query = $@"
SELECT 
    rtc.TermId AS terminalNo,
    COALESCE(di.TERM_SEQ, '-') AS serialNo,
    COALESCE(di.TERM_NAME, '-') AS terminalName,
    COALESCE(di.COUNTER_CODE, '-') AS lotType,
    COALESCE(MAX(CASE WHEN rtc.Cassette_Id = '1000A' AND rtc.Cassette_Status = '9' THEN CONCAT('Fail(', rtc.Cassette_Remain, ')') END), '-') AS cassette1000A,
    COALESCE(MAX(CASE WHEN rtc.Cassette_Id = '1000B' AND rtc.Cassette_Status = '9' THEN CONCAT('Fail(', rtc.Cassette_Remain, ')') END), '-') AS cassette1000B,
    COALESCE(MAX(CASE WHEN rtc.Cassette_Id = '500' AND rtc.Cassette_Status = '9' THEN CONCAT('Fail(', rtc.Cassette_Remain, ')') END), '-') AS cassette500,
    COALESCE(MAX(CASE WHEN rtc.Cassette_Id = '100' AND rtc.Cassette_Status = '9' THEN CONCAT('Fail(', rtc.Cassette_Remain, ')') END), '-') AS cassette100,
    MAX(cef.Data_Date) AS Data_DateTime
FROM {dbPrefix}.report_terminal_cassette rtc
LEFT JOIN {dbPrefix}.device_info di ON rtc.TermId = di.TERM_ID
LEFT JOIN {dbPrefix}.import_file_data cef ON rtc.Cassette_Event_File_Id = cef.Id
GROUP BY rtc.TermId, di.COUNTER_CODE
ORDER BY rtc.TermId;";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    reportCassette.Add(new report_display
                    {
                        terminalNo = reader["terminalNo"]?.ToString() ?? "-",
                        serialNo = reader["serialNo"]?.ToString() ?? "-",
                        terminalName = reader["terminalName"]?.ToString() ?? "-",
                        lotType = reader["lotType"]?.ToString() ?? "-",
                        cassette1000A = reader["cassette1000A"]?.ToString() ?? "-",
                        cassette1000B = reader["cassette1000B"]?.ToString() ?? "-",
                        cassette500 = reader["cassette500"]?.ToString() ?? "-",
                        cassette100 = reader["cassette100"]?.ToString() ?? "-",
                        Data_DateTime = reader["Data_DateTime"] != DBNull.Value ? reader.GetDateTime("Data_DateTime") : null
                    });
                }

                // Filtering
                if (!string.IsNullOrEmpty(terminalID))
                    reportCassette = reportCassette
                        .Where(x => x.terminalNo?.Contains(terminalID, StringComparison.OrdinalIgnoreCase) == true).ToList();

                if (!string.IsNullOrEmpty(lotType))
                    reportCassette = reportCassette
                        .Where(x => x.lotType?.Equals(lotType, StringComparison.OrdinalIgnoreCase) == true).ToList();

                if (fromdate.HasValue)
                    reportCassette = reportCassette
                        .Where(x => x.Data_DateTime.HasValue && x.Data_DateTime.Value.Date >= fromdate.Value.Date).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in GetCassetteStatusAsync: " + ex.Message);

            }


            return reportCassette;
        }

        public static int ExtractNumber(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            var match = System.Text.RegularExpressions.Regex.Match(text, @"\((\d+)\)");
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }


    }

}
