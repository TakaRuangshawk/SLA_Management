using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using Serilog;
using SLA_Management.Commons;
using SLA_Management.Data;
using SLA_Management.Models.OperationModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SLA_Management.Controllers
{
    public class SudoMonitorController : Controller
    {
        private IConfiguration _myConfiguration { get; set; }
        private static string sqlReport { get; set; }

        public static List<SudoVersionRecord> sudoVersions { get; set; }

        public ConnectMySQL connectMySQL { get; set; }

        public SudoMonitorController(IConfiguration configuration)
        {
            _myConfiguration = configuration;

            // ใช้ connection string ตามที่ให้มา (hard-code)
            sqlReport = "Server=10.98.14.28;Port=3306;Database=gsb_adm_fv;Uid=root;Pwd=P@ssw0rd;Charset=utf8mb4;";

            if (sudoVersions == null)
            {
                sudoVersions = new List<SudoVersionRecord>();
            }

            connectMySQL = new ConnectMySQL(sqlReport);
        }

        [HttpGet]
        public IActionResult SudoVersionMenu()
        {
            // dropdown Terminal No เฉพาะ Z = 1
            ViewBag.CurrentTID = GetFVDeviceInfoForSudo();
            return View();
        }

        #region Add Terminal (Update Z flag)

        [HttpPost]
        public IActionResult UpdateSudoTerminalSelection([FromBody] SudoTerminalSelectionRequest request)
        {
            if (request == null || request.selectedTerminalIds == null)
            {
                return Json(new
                {
                    success = false,
                    updatedCount = 0,
                    updatedIds = new List<string>(),
                    message = "No data."
                });
            }

            try
            {
                var selected = request.selectedTerminalIds.Distinct().ToList();

                using (var conn = new MySqlConnection(sqlReport))
                {
                    conn.Open();

                    using (var tx = conn.BeginTransaction())
                    {
                        // 1) set Z = 1 สำหรับ TERM_ID ที่เลือก
                        if (selected.Count > 0)
                        {
                            var inParams = new List<string>();
                            MySqlCommand cmdSet1 = new MySqlCommand();
                            cmdSet1.Connection = conn;
                            cmdSet1.Transaction = tx;

                            for (int i = 0; i < selected.Count; i++)
                            {
                                string p = "@p" + i;
                                inParams.Add(p);
                                cmdSet1.Parameters.AddWithValue(p, selected[i]);
                            }

                            cmdSet1.CommandText = $@"
UPDATE device_info
SET VERSION_MB = CASE
    WHEN VERSION_MB IS NULL OR VERSION_MB = '' THEN CONCAT('1_0_', DATE_FORMAT(NOW(), '%Y%m%d'))
    ELSE CONCAT(
        '1_',
        SUBSTRING_INDEX(SUBSTRING_INDEX(VERSION_MB, '_', -2), '_', 1), '_',
        SUBSTRING_INDEX(VERSION_MB, '_', -1)
    )
END
WHERE (STATUS = 'use' OR STATUS = 'roustop')
  AND TERM_ID IN ({string.Join(",", inParams)});
";
                            cmdSet1.ExecuteNonQuery();
                        }

                        // 2) set Z = 0 สำหรับตู้ที่เหลือ (use/roustop แต่ไม่อยู่ใน selected)
                        MySqlCommand cmdSet0 = new MySqlCommand();
                        cmdSet0.Connection = conn;
                        cmdSet0.Transaction = tx;

                        string notInClause = "";
                        if (selected.Count > 0)
                        {
                            var notInParams = new List<string>();
                            for (int i = 0; i < selected.Count; i++)
                            {
                                string p = "@n" + i;
                                notInParams.Add(p);
                                cmdSet0.Parameters.AddWithValue(p, selected[i]);
                            }
                            notInClause = $"AND TERM_ID NOT IN ({string.Join(",", notInParams)})";
                        }

                        cmdSet0.CommandText = $@"
UPDATE device_info
SET VERSION_MB = CASE
    WHEN VERSION_MB IS NULL OR VERSION_MB = '' THEN CONCAT('0_0_', DATE_FORMAT(NOW(), '%Y%m%d'))
    ELSE CONCAT(
        '0_',
        SUBSTRING_INDEX(SUBSTRING_INDEX(VERSION_MB, '_', -2), '_', 1), '_',
        SUBSTRING_INDEX(VERSION_MB, '_', -1)
    )
END
WHERE (STATUS = 'use' OR STATUS = 'roustop')
{notInClause};
";

                        cmdSet0.ExecuteNonQuery();

                        tx.Commit();
                    }
                }

                return Json(new
                {
                    success = true,
                    updatedCount = request.selectedTerminalIds.Count,
                    updatedIds = request.selectedTerminalIds,
                    message = "Update Sudo terminals completed."
                });
            }
            catch (Exception ex)
            {
                Log.Error($"UpdateSudoTerminalSelection Error : {ex}");
                return Json(new
                {
                    success = false,
                    updatedCount = 0,
                    updatedIds = new List<string>(),
                    message = "Error : " + ex.Message
                });
            }
        }

        #endregion

        #region Dropdown / Terminal list

        /// <summary>
        /// dropdown สำหรับ Sudo Version: เอาเฉพาะตู้ที่ Z = 1
        /// </summary>
        public List<Device_info_record> GetFVDeviceInfoForSudo()
        {
            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand();
                mySqlCommand.CommandText = @"
SELECT TERM_ID, TERM_SEQ, TERM_NAME, TYPE_ID
FROM device_info
WHERE (STATUS = 'use' OR STATUS = 'roustop')
  AND VERSION_MB IS NOT NULL
  AND VERSION_MB <> ''
  AND SUBSTRING_INDEX(VERSION_MB, '_', 1) = '1';
";

                return ConvertDataTableToModel
                    .ConvertDataTable<Device_info_record>(connectMySQL.GetDatatable(mySqlCommand));
            }
            catch (Exception ex)
            {
                Log.Error($"GetFVDeviceInfoForSudo Error : {ex}");
                return new List<Device_info_record>();
            }
        }

        /// <summary>
        /// อ่าน device_info สำหรับ dropdown Terminal No (ทุก Z)
        /// </summary>
        public List<Device_info_record> GetFVDeviceInfo()
        {
            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand();
                mySqlCommand.CommandText =
                    "SELECT TERM_ID,TERM_SEQ,TERM_NAME,TYPE_ID " +
                    "FROM device_info " +
                    "WHERE STATUS = 'use' OR STATUS = 'roustop';";

                return ConvertDataTableToModel
                    .ConvertDataTable<Device_info_record>(connectMySQL.GetDatatable(mySqlCommand));
            }
            catch (Exception ex)
            {
                Log.Error($"GetFVDeviceInfo Error : {ex}");
                return new List<Device_info_record>();
            }
        }

        /// <summary>
        /// ดึง list ตู้ทั้งหมด + z_flag สำหรับ modal Add Terminal
        /// </summary>
        [HttpGet]
        public IActionResult GetSudoTerminalList()
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = @"
SELECT TERM_SEQ, TERM_ID, TERM_NAME, VERSION_MB
FROM device_info
WHERE STATUS = 'use' OR STATUS = 'roustop';
";

                var dt = connectMySQL.GetDatatable(cmd);
                var list = new List<SudoTerminalListItem>();

                foreach (DataRow row in dt.Rows)
                {
                    string termSeq = row["TERM_SEQ"]?.ToString() ?? "";
                    string termId = row["TERM_ID"]?.ToString() ?? "";
                    string termName = row["TERM_NAME"]?.ToString() ?? "";
                    string versionMb = row["VERSION_MB"]?.ToString() ?? "";

                    int z = GetZFlagFromVersionMb(versionMb);

                    list.Add(new SudoTerminalListItem
                    {
                        term_seq = termSeq,
                        term_id = termId,
                        term_name = termName,
                        version_mb = versionMb,
                        z_flag = z
                    });
                }

                return Json(list);
            }
            catch (Exception ex)
            {
                Log.Error($"GetSudoTerminalList Error : {ex}");
                return Json(new List<SudoTerminalListItem>());
            }
        }

        #endregion

        #region Main grid (Search / Paging / Export)

        /// <summary>
        /// ใช้กับปุ่ม Search หน้า Sudo Version
        /// </summary>
        [HttpGet]
        public IActionResult GetSudoVersion(SudoSearchData searchData)
        {
            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand();

                string sql = @"
SELECT
    TERM_SEQ,
    TERM_ID,
    TERM_NAME,
    VERSION_MB
FROM device_info

";

                List<string> where = new List<string>();

                if (!string.IsNullOrEmpty(searchData.terminal_no))
                {
                    where.Add("TERM_ID = @terminal_no");
                    mySqlCommand.Parameters.AddWithValue("@terminal_no", searchData.terminal_no);
                }

                if (where.Count > 0)
                {
                    sql += " AND " + string.Join(" AND ", where);
                }

                sql += ";";

                mySqlCommand.CommandText = sql;

                var dt = connectMySQL.GetDatatable(mySqlCommand);

                // แปลง status filter จาก dropdown (True/False หรือ 1/0)
                int? statusFlag = null;
                if (!string.IsNullOrEmpty(searchData.status))
                {
                    if (searchData.status.Equals("True", StringComparison.OrdinalIgnoreCase))
                        statusFlag = 1;
                    else if (searchData.status.Equals("False", StringComparison.OrdinalIgnoreCase))
                        statusFlag = 0;
                    else if (searchData.status == "1")
                        statusFlag = 1;
                    else if (searchData.status == "0")
                        statusFlag = 0;
                }

                sudoVersions = new List<SudoVersionRecord>();

                foreach (DataRow row in dt.Rows)
                {
                    string termSeq = row["TERM_SEQ"]?.ToString() ?? "";
                    string termId = row["TERM_ID"]?.ToString() ?? "";
                    string termName = row["TERM_NAME"]?.ToString() ?? "";
                    string versionMb = row["VERSION_MB"]?.ToString() ?? "";

                    // แปลง VERSION_MB → mustCheck(Z), flag(X), lastUpdated
                    ParseVersionMb(versionMb, out bool mustCheck, out int flag, out DateTime? lastUpdated);

                    // แสดงเฉพาะ Z = 1 เท่านั้น
                    if (!mustCheck)
                    {
                        continue;
                    }

                    // ถ้ามี filter status → filter ที่นี่
                    if (statusFlag.HasValue && flag != statusFlag.Value)
                    {
                        continue;
                    }

                    string statusText = (flag == 1 ? "OK" : "Warning") + $" ({flag})";

                    sudoVersions.Add(new SudoVersionRecord
                    {
                        serial_no = termSeq,
                        terminal_no = termId,
                        terminal_name = termName,
                        last_updated = lastUpdated,
                        status = statusText
                    });
                }

                int maxRows = searchData.maxRows > 0 ? searchData.maxRows : 50;

                return Json(new
                {
                    devices = RangeFilter<SudoVersionRecord>(sudoVersions, 1, maxRows),
                    sumtotal = sudoVersions.Count
                });
            }
            catch (Exception ex)
            {
                Log.Error($"GetSudoVersion Error : {ex}");
                int maxRows = searchData.maxRows > 0 ? searchData.maxRows : 50;
                return Json(new
                {
                    devices = RangeFilter<SudoVersionRecord>(sudoVersions, 1, maxRows),
                    sumtotal = sudoVersions.Count
                });
            }
        }

        /// <summary>
        /// ใช้ตอนเปลี่ยนหน้า (paging)
        /// </summary>
        [HttpGet]
        public List<SudoVersionRecord> GetSudoPage(int page, int max_row)
        {
            try
            {
                return RangeFilter<SudoVersionRecord>(sudoVersions, page, max_row);
            }
            catch (Exception ex)
            {
                Log.Error($"SudoMonitorController.GetSudoPage Error : {ex}");
                return new List<SudoVersionRecord>();
            }
        }

        /// <summary>
        /// Export Excel สำหรับ Sudo Version
        /// </summary>
        [HttpPost]
        public IActionResult ExportSudoToExcel()
        {
            List<SudoVersionRecord> obj = new List<SudoVersionRecord>();

            if (sudoVersions != null)
            {
                obj = sudoVersions;
            }

            DateTime dateNow = DateTime.Now;
            string fileName = "SudoVersion_" +
                              dateNow.Year.ToString("0000") +
                              dateNow.Month.ToString("00") +
                              dateNow.Day.ToString("00");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                // Header
                worksheet.Cells[1, 1].Value = "Row";
                worksheet.Cells[1, 2].Value = "Serial No";
                worksheet.Cells[1, 3].Value = "Terminal No";
                worksheet.Cells[1, 4].Value = "Terminal Name";
                worksheet.Cells[1, 5].Value = "Last Updated";
                worksheet.Cells[1, 6].Value = "Status";

                // Data
                var rowIndex = 2;

                foreach (var item in obj)
                {
                    worksheet.Cells[rowIndex, 1].Value = rowIndex - 1;
                    worksheet.Cells[rowIndex, 2].Value = item.serial_no;
                    worksheet.Cells[rowIndex, 3].Value = item.terminal_no;
                    worksheet.Cells[rowIndex, 4].Value = item.terminal_name;
                    worksheet.Cells[rowIndex, 5].Value = item.last_updated?.ToString("yyyy-MM-dd HH:mm:ss");
                    worksheet.Cells[rowIndex, 6].Value = item.status;

                    rowIndex++;
                }

                worksheet.Cells.AutoFitColumns();

                var fileContents = package.GetAsByteArray();

                return File(
                    fileContents,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName + ".xlsx"
                );
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Helper ตัดหน้า list สำหรับ paging
        /// </summary>
        public List<T> RangeFilter<T>(List<T> inputList, int page, int row)
        {
            int start_row;
            int end_row;

            if (page == 1)
            {
                start_row = 0;
            }
            else
            {
                start_row = (page - 1) * row;
            }

            end_row = start_row + row - 1;

            if (inputList.Count < end_row)
            {
                end_row = inputList.Count - 1;
            }

            return inputList.Skip(start_row).Take(row).ToList();
        }

        /// <summary>
        /// แปลง VERSION_MB => mustCheck(Z), flag(X=0/1), lastUpdated
        /// รองรับหลาย format:
        ///   1) Z_X_YYYYMMDD   (ใหม่)
        ///   2) Z_X            (ใหม่แบบสั้น)
        ///   3) X_YYYYMMDD     (เก่า → ถือว่า Z = X)
        ///   4) X              (เก่า → ถือว่า Z = X, ไม่มีวันที่)
        /// หมายเหตุ: ตอนนี้ lastUpdated จะอ่านจาก YYYYMMDD ทุกกรณีที่มีค่า
        /// ไม่สนว่า X เป็น 0 หรือ 1 แล้ว (ใช้เป็น "วันที่เช็คล่าสุด")
        /// </summary>
        private static void ParseVersionMb(
            string versionMb,
            out bool mustCheck,
            out int flag,
            out DateTime? lastUpdated)
        {
            mustCheck = false;
            flag = 0;
            lastUpdated = null;

            if (string.IsNullOrWhiteSpace(versionMb))
            {
                return;
            }

            string raw = versionMb.Trim();
            var parts = raw
                .Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToArray();

            if (parts.Length == 0)
            {
                return;
            }

            string z = "0";
            string x = "0";
            string dateStr = "";

            if (parts.Length == 1)
            {
                // "0" หรือ "1" เฉย ๆ → treat เป็นแบบเก่า X
                x = parts[0];
                z = x;
            }
            else if (parts.Length == 2)
            {
                // อาจเป็น X_YYYYMMDD (เก่า) หรือ Z_X (ใหม่)
                bool secondLooksLikeDate =
                    parts[1].Length >= 8 &&
                    parts[1].All(char.IsDigit);

                if (secondLooksLikeDate)
                {
                    // X_YYYYMMDD → X เป็นทั้ง Z และ X
                    x = parts[0];
                    z = x;
                    dateStr = parts[1];
                }
                else
                {
                    // Z_X
                    z = parts[0];
                    x = parts[1];
                }
            }
            else
            {
                // >= 3 → Z_X_YYYYMMDD
                z = parts[0];
                x = parts[1];
                dateStr = parts[2];
            }

            mustCheck = (z == "1");
            flag = (x == "1") ? 1 : 0;

            // **ใหม่**: ถ้ามี YYYYMMDD ให้ parse เป็น lastUpdated ไม่ว่าจะ X=0 หรือ 1
            if (!string.IsNullOrEmpty(dateStr) &&
                dateStr.Length >= 8 &&
                dateStr.All(char.IsDigit))
            {
                if (DateTime.TryParseExact(
                    dateStr.Substring(0, 8),
                    "yyyyMMdd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime dt))
                {
                    lastUpdated = dt;
                }
            }
        }


        /// <summary>
        /// ดึงค่า Z flag (mustCheck) จาก VERSION_MB
        /// </summary>
        private static int GetZFlagFromVersionMb(string versionMb)
        {
            ParseVersionMb(versionMb, out bool mustCheck, out int _, out DateTime? _);
            return mustCheck ? 1 : 0;
        }

        #endregion
    }
}
