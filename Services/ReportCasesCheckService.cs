using MySql.Data.MySqlClient;
using NPOI.SS.Formula.Functions;
using SLA_Management.Models.ManagementModel;
using System.Data;
using static SLA_Management.Controllers.ManagementController;

namespace SLA_Management.Services
{
    public class ReportCasesCheckService
    {
        private readonly IConfiguration _configuration;

        public ReportCasesCheckService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<ReportCaseStatusModel>> GetReportCasesAsync(DateTime? from, DateTime? to, string bankName)
        {
            var result = new List<ReportCaseStatusModel>();

            if (!from.HasValue || !to.HasValue)
                return result;

            string connectionDB = _configuration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + bankName);
            if (string.IsNullOrEmpty(connectionDB)) return result;

            try
            {
                using var conn = new MySqlConnection(connectionDB);
                await conn.OpenAsync();

                // 1. ดึง Date_Inform เฉพาะวันที่ อยู่ในช่วง from - to
                string query = @"
            SELECT DISTINCT DATE(Date_Inform) AS DateOnly
            FROM reportcases
            WHERE Date_Inform IS NOT NULL
              AND DATE(Date_Inform) BETWEEN @from AND @to;
        ";

                var foundDates = new HashSet<DateTime>();

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@from", from.Value.Date);
                    cmd.Parameters.AddWithValue("@to", to.Value.Date);

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        if (reader["DateOnly"] != DBNull.Value)
                            foundDates.Add(Convert.ToDateTime(reader["DateOnly"]).Date);
                    }
                }

                // 2. สร้าง list ของวันทุกวันในช่วง from → to
                var allDates = Enumerable.Range(0, (to.Value - from.Value).Days + 1)
                                         .Select(offset => from.Value.AddDays(offset).Date);

                // 3. ใส่ลงใน result ตามว่าพบหรือไม่พบ
                result = allDates.Select(date => new ReportCaseStatusModel
                {
                    DateInform = date,
                    Status = foundDates.Contains(date) ? "Found" : "NotFound"
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in GetReportCasesAsync: " + ex.Message);
            }

            return result;
        }

    }



}
