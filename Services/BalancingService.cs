using MySql.Data.MySqlClient;
using SLA_Management.Models.TermProbModel;
using static SLA_Management.Controllers.ReportController;

namespace SLA_Management.Services
{

    public class BalancingService
    {
        private readonly IConfiguration _configuration;

        public BalancingService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<BalancingReportModel>> GetBalancingReportAsync(string terminalno, DateTime? fromDate, DateTime? toDate, string bankName)
        {
            List<BalancingReportModel> reports = new();

            try
            {
                string connectionString = _configuration.GetConnectionString($"Connection_{bankName.ToLower()}");

                using var conn = new MySqlConnection(connectionString);
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
        SELECT 
            term_id,
            term_name,
            term_seq,
            transationdate,
            c1_inc, c2_inc, c3_inc,
            c1_dep, c2_dep, c3_dep,
            c1_out, c2_out, c3_out,
            c1_end, c2_end, c3_end
        FROM report_display
        WHERE (@terminalno = '' OR term_id = @terminalno)
          AND (@fromDate IS NULL OR transationdate >= @fromDate)
          AND (@toDate IS NULL OR transationdate <= @toDate)
        ORDER BY transationdate DESC";

                cmd.Parameters.AddWithValue("@terminalno", terminalno ?? "");
                cmd.Parameters.AddWithValue("@fromDate", fromDate.HasValue ? fromDate : DBNull.Value);
                cmd.Parameters.AddWithValue("@toDate", toDate.HasValue ? toDate : DBNull.Value);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    reports.Add(new BalancingReportModel
                    {
                        term_id = reader["term_id"]?.ToString(),
                        term_name = reader["term_name"]?.ToString(),
                        term_seq = reader["term_seq"]?.ToString(),
                        transationdate = reader["transationdate"]?.ToString(),

                        c1_inc = reader["c1_inc"]?.ToString(),
                        c2_inc = reader["c2_inc"]?.ToString(),
                        c3_inc = reader["c3_inc"]?.ToString(),

                        c1_dep = reader["c1_dep"]?.ToString(),
                        c2_dep = reader["c2_dep"]?.ToString(),
                        c3_dep = reader["c3_dep"]?.ToString(),

                        c1_out = reader["c1_out"]?.ToString(),
                        c2_out = reader["c2_out"]?.ToString(),
                        c3_out = reader["c3_out"]?.ToString(),

                        c1_end = reader["c1_end"]?.ToString(),
                        c2_end = reader["c2_end"]?.ToString(),
                        c3_end = reader["c3_end"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                // 🔴 สามารถเปลี่ยนเป็น logger จริงได้ตามระบบ เช่น _logger.LogError(ex, "...");
                Console.Error.WriteLine($"❌ Error in GetBalancingReportAsync: {ex.Message}");
            }

            return reports;
        }

    }
}
