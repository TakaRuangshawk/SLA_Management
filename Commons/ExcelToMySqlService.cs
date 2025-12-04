using CsvHelper;
using Models.ManagementModel;
using MySql.Data.MySqlClient;
using NPOI.HSSF.UserModel;
using OfficeOpenXml;
using SLA_Management.Models.TermProbModel;
using System;
using System.Data;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
namespace SLA_Management.Commons
{
    public class ExcelToMySqlService
    {
        private readonly string _connectionString;

        public ExcelToMySqlService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task ImportExcelDataAsync(Stream excelStream, string userName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(excelStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    for (int row = 2; row <= rowCount; row++) // แถวแรกเป็น header
                    {
                        // ====== Date_Inform (col 12) ======
                        var cellValue_dateInform = worksheet.Cells[row, 12].Value;
                        DateTime? dateInform = null;

                        if (cellValue_dateInform != null)
                        {
                            if (cellValue_dateInform is double)
                            {
                                dateInform = DateTime.FromOADate((double)cellValue_dateInform);
                            }
                            else
                            {
                                dateInform = DateTime.Parse(cellValue_dateInform.ToString());
                            }
                        }

                        // ====== Time_Inform (col 13) ======
                        var cellValue_timeInform = worksheet.Cells[row, 13].Value;
                        string timeInform = null;

                        if (cellValue_timeInform != null)
                        {
                            if (cellValue_timeInform is double)
                            {
                                // ถ้าเป็น Excel time/Date → แปลงเฉพาะเวลาเป็น HH:mm
                                var dt = DateTime.FromOADate((double)cellValue_timeInform);
                                timeInform = dt.ToString("HH:mm");
                            }
                            else
                            {
                                timeInform = cellValue_timeInform.ToString();
                            }
                        }

                        // ====== Date_Close_Pb (col 14) ======
                        var cellValue_dateClosePb = worksheet.Cells[row, 14].Value;
                        DateTime? dateClosePb = null;

                        if (cellValue_dateClosePb != null)
                        {
                            if (cellValue_dateClosePb is double)
                            {
                                dateClosePb = DateTime.FromOADate((double)cellValue_dateClosePb);
                            }
                            else
                            {
                                dateClosePb = DateTime.Parse(cellValue_dateClosePb.ToString());
                            }
                        }

                        // ====== Time_Close_Pb (col 15) ======
                        var cellValue_timeClosePb = worksheet.Cells[row, 15].Value;
                        string timeClosePb = null;

                        if (cellValue_timeClosePb != null)
                        {
                            if (cellValue_timeClosePb is double)
                            {
                                var dt = DateTime.FromOADate((double)cellValue_timeClosePb);
                                timeClosePb = dt.ToString("HH:mm");
                            }
                            else
                            {
                                timeClosePb = cellValue_timeClosePb.ToString();
                            }
                        }

                        // ====== Columns อื่น ๆ ======
                        var caseErrorNo = Convert.ToInt64(worksheet.Cells[row, 1].Value);
                        var terminalId = worksheet.Cells[row, 2].Value?.ToString();
                        var placeInstall = worksheet.Cells[row, 3].Value?.ToString();
                        var branchNamePb = worksheet.Cells[row, 4].Value?.ToString();
                        var issueName = worksheet.Cells[row, 5].Value?.ToString();
                        var repair1 = worksheet.Cells[row, 6].Value?.ToString();
                        var repair2 = worksheet.Cells[row, 7].Value?.ToString();
                        var repair3 = worksheet.Cells[row, 8].Value?.ToString();
                        var repair4 = worksheet.Cells[row, 9].Value?.ToString();
                        var repair5 = worksheet.Cells[row, 10].Value?.ToString();
                        var incidentNo = worksheet.Cells[row, 11].Value?.ToString();
                        var statusName = worksheet.Cells[row, 16].Value?.ToString();
                        var typeProject = worksheet.Cells[row, 17].Value?.ToString();
                        var updateDate = DateTime.Now;
                        var updateBy = userName;
                        var remark = "Imported from Excel";

                        // ⚠️ ตรงนี้สมมติว่าตารางมีคอลัมน์ Time_Inform, Time_Close_Pb แล้ว
                        var query = @"
                INSERT INTO ReportCases 
                (Case_Error_No, Terminal_ID, Place_Install, Branch_name_pb, Issue_Name, 
                 Repair1, Repair2, Repair3, Repair4, Repair5, Incident_No, 
                 Date_Inform, Time_Inform, Date_Close_Pb, Time_Close_Pb,
                 Status_Name, Type_Project, Update_Date, Update_By, Remark) 
                VALUES 
                (@CaseErrorNo, @TerminalId, @PlaceInstall, @BranchNamePb, @IssueName, 
                 @Repair1, @Repair2, @Repair3, @Repair4, @Repair5, @IncidentNo, 
                 @DateInform, @TimeInform, @DateClosePb, @TimeClosePb,
                 @StatusName, @TypeProject, @UpdateDate, @UpdateBy, @Remark)
                ON DUPLICATE KEY UPDATE 
                    Place_Install   = @PlaceInstall,
                    Branch_name_pb  = @BranchNamePb,
                    Issue_Name      = @IssueName,
                    Repair1         = @Repair1,
                    Repair2         = @Repair2,
                    Repair3         = @Repair3,
                    Repair4         = @Repair4,
                    Repair5         = @Repair5,
                    Incident_No     = @IncidentNo,
                    Date_Inform     = @DateInform,
                    Time_Inform     = @TimeInform,
                    Date_Close_Pb   = @DateClosePb,
                    Time_Close_Pb   = @TimeClosePb,
                    Status_Name     = @StatusName,
                    Type_Project    = @TypeProject,
                    Update_Date     = @UpdateDate,
                    Update_By       = @UpdateBy;";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@CaseErrorNo", caseErrorNo);
                            command.Parameters.AddWithValue("@TerminalId", terminalId);
                            command.Parameters.AddWithValue("@PlaceInstall", placeInstall);
                            command.Parameters.AddWithValue("@BranchNamePb", branchNamePb);
                            command.Parameters.AddWithValue("@IssueName", issueName);
                            command.Parameters.AddWithValue("@Repair1", repair1);
                            command.Parameters.AddWithValue("@Repair2", repair2);
                            command.Parameters.AddWithValue("@Repair3", repair3);
                            command.Parameters.AddWithValue("@Repair4", repair4);
                            command.Parameters.AddWithValue("@Repair5", repair5);
                            command.Parameters.AddWithValue("@IncidentNo", incidentNo);
                            command.Parameters.AddWithValue("@DateInform", (object?)dateInform ?? DBNull.Value);
                            command.Parameters.AddWithValue("@TimeInform", (object?)timeInform ?? DBNull.Value);
                            command.Parameters.AddWithValue("@DateClosePb", (object?)dateClosePb ?? DBNull.Value);
                            command.Parameters.AddWithValue("@TimeClosePb", (object?)timeClosePb ?? DBNull.Value);
                            command.Parameters.AddWithValue("@StatusName", statusName);
                            command.Parameters.AddWithValue("@TypeProject", typeProject);
                            command.Parameters.AddWithValue("@UpdateDate", updateDate);
                            command.Parameters.AddWithValue("@UpdateBy", updateBy);
                            command.Parameters.AddWithValue("@Remark", remark);

                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }


        public async Task ImportExcelProblemDataAsync(Stream excelStream, string userName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(excelStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                if (worksheet == null)
                    throw new Exception("📄 ไม่พบ worksheet แรกในไฟล์ Excel");

                var expectedHeaders = new[]
                {
             "terminalid", "probcode", "remark", "dtenderrcode13",
            "dterrcode13", "trxdatetime", "status", "createdate", "updatedate", "resolveprob"
        };

                for (int i = 0; i < expectedHeaders.Length; i++)
                {
                    var header = worksheet.Cells[1, i + 1].Value?.ToString()?.Trim().ToLower();
                    if (header != expectedHeaders[i])
                    {
                        throw new Exception($"❌ คอลัมน์ที่ {(i + 1)} ควรเป็น '{expectedHeaders[i]}' แต่พบว่าเป็น '{header ?? "null"}'");
                    }
                }

                var rowCount = worksheet.Dimension.Rows;

                if (rowCount <= 1)
                {
                    throw new Exception("⚠️ ไม่พบข้อมูลในไฟล์ Excel (น้อยกว่า 2 แถว)");
                }

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    for (int row = 2; row <= rowCount; row++) // Row 1 = Header
                    {
                        try
                        {
                            var terminalId = worksheet.Cells[row, 2].Value?.ToString();
                            var probcode = worksheet.Cells[row, 3].Value?.ToString();
                            var remark = worksheet.Cells[row, 4].Value?.ToString();

                            if (string.IsNullOrWhiteSpace(terminalId) || string.IsNullOrWhiteSpace(probcode))
                            {
                                throw new Exception($"Missing required fields at row {row} (terminalId/probcode)");
                            }

                            // dtenderrcode13
                            DateTime? dtEndErrCode13 = null;
                            var dtEndErrCode13Raw = worksheet.Cells[row, 5].Value;
                            if (dtEndErrCode13Raw is double d1) dtEndErrCode13 = DateTime.FromOADate(d1);
                            else if (DateTime.TryParse(dtEndErrCode13Raw?.ToString(), out var parsed1)) dtEndErrCode13 = parsed1;

                            // dterrcode13
                            long? dtErrCode13 = null;
                            var dtErrCode13Raw = worksheet.Cells[row, 6].Value;
                            if (long.TryParse(dtErrCode13Raw?.ToString(), out long parsedLong))
                                dtErrCode13 = parsedLong;

                            // trxdatetime
                            DateTime? trxdatetime = null;
                            var trxDatetimeRaw = worksheet.Cells[row, 7].Value;
                            if (trxDatetimeRaw is double d2) trxdatetime = DateTime.FromOADate(d2);
                            else if (DateTime.TryParse(trxDatetimeRaw?.ToString(), out var parsed2)) trxdatetime = parsed2;

                            // status
                            var status = worksheet.Cells[row, 8].Value?.ToString() ?? "ACT";

                            // createdate
                            DateTime? createdate = null;
                            var createdateRaw = worksheet.Cells[row, 9].Value;
                            if (createdateRaw is double d3) createdate = DateTime.FromOADate(d3);
                            else if (DateTime.TryParse(createdateRaw?.ToString(), out var parsed3)) createdate = parsed3;

                            // updatedate
                            DateTime? updatedate = null;
                            var updatedateRaw = worksheet.Cells[row, 10].Value;
                            if (updatedateRaw is double d4) updatedate = DateTime.FromOADate(d4);
                            else if (DateTime.TryParse(updatedateRaw?.ToString(), out var parsed4)) updatedate = parsed4;

                            var resolveProb = worksheet.Cells[row, 11].Value?.ToString();

                            var query = @"
INSERT INTO ejlog_devicetermprob
(terminalid, probcode, remark, trxdatetime, status, createdate, updatedate, resolveprob, dtenderrcode13, dterrcode13)
VALUES 
(@terminalid, @probcode, @remark, @trxdatetime, @status, @createdate, @updatedate, @resolveprob, @dtenderrcode13, @dterrcode13)
ON DUPLICATE KEY UPDATE
    remark = VALUES(remark),
    status = VALUES(status),
    updatedate = VALUES(updatedate),
    resolveprob = VALUES(resolveprob),
    dtenderrcode13 = VALUES(dtenderrcode13),
    dterrcode13 = VALUES(dterrcode13);";

                            using var command = new MySqlCommand(query, connection);
                            command.Parameters.AddWithValue("@terminalid", terminalId);
                            command.Parameters.AddWithValue("@probcode", probcode);
                            command.Parameters.AddWithValue("@remark", remark ?? "");
                            command.Parameters.AddWithValue("@trxdatetime", (object?)trxdatetime ?? DBNull.Value);
                            command.Parameters.AddWithValue("@status", status);
                            command.Parameters.AddWithValue("@createdate", (object?)createdate ?? DBNull.Value);
                            command.Parameters.AddWithValue("@updatedate", (object?)updatedate ?? DBNull.Value);
                            command.Parameters.AddWithValue("@resolveprob", userName ?? "");
                            command.Parameters.AddWithValue("@dtenderrcode13", (object?)dtEndErrCode13 ?? DBNull.Value);
                            command.Parameters.AddWithValue("@dterrcode13", (object?)dtErrCode13 ?? DBNull.Value);

                            int result = await command.ExecuteNonQueryAsync();

                            if (result <= 0)
                            {
                                throw new Exception($"Row {row}: Insert failed (0 affected rows).");
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"❌ Error at row {row}: {ex.Message}");
                        }
                    }
                }
            }
        }

        private DateTime? ParseDateTime(string input) =>
        string.IsNullOrWhiteSpace(input) ? null :
        DateTime.TryParse(input, out var dt) ? dt : null;

        private long? ParseLong(string input) =>
            string.IsNullOrWhiteSpace(input) ? null :
            long.TryParse(input, out var val) ? val : null;

        public async Task ImportCsvProblemDataAsync(Stream csvStream, string userName)
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            try
            {
                // Force parsing all records to trigger header & format validation early
                var records = csv.GetRecords<ProblemCsvModel>().ToList();

                if (records == null || records.Count == 0)
                {
                    throw new Exception("⚠️ ไม่พบข้อมูลในไฟล์ CSV หรือข้อมูลไม่ตรงกับโครงสร้างที่กำหนด");
                }

                foreach (var item in records)
                {
                    if (string.IsNullOrWhiteSpace(item.terminalid) || string.IsNullOrWhiteSpace(item.probcode))
                        continue;

                    string query = @"
INSERT INTO ejlog_devicetermprob
(terminalid, probcode, remark, trxdatetime, status, createdate, updatedate, resolveprob, dtenderrcode13, dterrcode13)
VALUES 
(@terminalid, @probcode, @remark, @trxdatetime, @status, @createdate, @updatedate, @resolveprob, @dtenderrcode13, @dterrcode13)
ON DUPLICATE KEY UPDATE
    remark = VALUES(remark),
    status = VALUES(status),
    updatedate = VALUES(updatedate),
    resolveprob = VALUES(resolveprob),
    dtenderrcode13 = VALUES(dtenderrcode13),
    dterrcode13 = VALUES(dterrcode13);";

                    using var command = new MySqlCommand(query, connection);

                    command.Parameters.AddWithValue("@terminalid", item.terminalid);
                    command.Parameters.AddWithValue("@probcode", item.probcode);
                    command.Parameters.AddWithValue("@remark", item.remark ?? "");
                    command.Parameters.AddWithValue("@trxdatetime", string.IsNullOrWhiteSpace(item.trxdatetime) ? DBNull.Value : (object?)ParseDateTime(item.trxdatetime));
                    command.Parameters.AddWithValue("@status", item.status ?? "ACT");
                    command.Parameters.AddWithValue("@createdate", string.IsNullOrWhiteSpace(item.createdate) ? DBNull.Value : (object?)ParseDateTime(item.createdate));
                    command.Parameters.AddWithValue("@updatedate", string.IsNullOrWhiteSpace(item.updatedate) ? DBNull.Value : (object?)ParseDateTime(item.updatedate));
                    command.Parameters.AddWithValue("@resolveprob", userName ?? "");
                    command.Parameters.AddWithValue("@dtenderrcode13", string.IsNullOrWhiteSpace(item.dtenderrcode13) ? DBNull.Value : (object?)ParseDateTime(item.dtenderrcode13));
                    command.Parameters.AddWithValue("@dterrcode13", string.IsNullOrWhiteSpace(item.dterrcode13) ? DBNull.Value : (object?)ParseLong(item.dterrcode13));

                    int result = await command.ExecuteNonQueryAsync();

                    if (result <= 0)
                    {
                        throw new Exception($"❌ Insert failed (0 affected rows) for terminalid={item.terminalid}");
                    }
                }
            }
            catch (HeaderValidationException hex)
            {
                throw new Exception("❌ CSV header format is invalid. โปรดตรวจสอบชื่อ column หรือลำดับให้ถูกต้อง", hex);
            }
            catch (ReaderException rex)
            {
                throw new Exception("❌ ข้อมูลในไฟล์ CSV มีรูปแบบไม่ถูกต้องหรือไม่สามารถอ่านได้", rex);
            }
            catch (Exception ex)
            {
                throw new Exception("❌ เกิดข้อผิดพลาดขณะนำเข้าไฟล์ CSV", ex);
            }
        }

        #region EncryptionMonitor
        public async Task ImportEncryptionExcelDataAsync(Stream excelStream, string fileName, string userName)
        {
            var fileExtension = Path.GetExtension(fileName)?.ToLower();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                switch (fileExtension)
                {
                    case ".xlsx":
                        await ProcessXlsxFileAsync(excelStream, connection, userName);
                        break;

                    case ".xls":
                        await ProcessXlsFileAsync(excelStream, connection, userName);
                        break;

                    case ".csv":
                        await ProcessCsvFileAsync(excelStream, connection, userName);
                        break;

                    default:
                        throw new NotSupportedException("Unsupported file format. Please upload .xlsx, .xls, or .csv");
                }
            }
        }
        private async Task ProcessXlsxFileAsync(Stream excelStream, MySqlConnection connection, string userName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(excelStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                {
                    await ProcessRowAsync(worksheet.Cells[row, 1].Value?.ToString(), worksheet.Cells[row, 2].Value?.ToString(), connection, userName);
                }
            }
        }

        private async Task ProcessXlsFileAsync(Stream excelStream, MySqlConnection connection, string userName)
        {
            var workbook = new HSSFWorkbook(excelStream);
            var sheet = workbook.GetSheetAt(0);
            for (int row = 2; row <= sheet.LastRowNum; row++)
            {
                var currentRow = sheet.GetRow(row);
                if (currentRow != null)
                {
                    await ProcessRowAsync(currentRow.GetCell(0)?.ToString(), currentRow.GetCell(1)?.ToString(), connection, userName);
                }
            }
        }

        private async Task ProcessCsvFileAsync(Stream excelStream, MySqlConnection connection, string userName)
        {
            using (var reader = new StreamReader(excelStream))
            {
                bool isFirstRow = true;
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (isFirstRow) { isFirstRow = false; continue; }
                    var values = line.Split(',');
                    if (values.Length < 3) continue;

                    await ProcessRowAsync(values[0], values[1], connection, userName);
                }
            }
        }

        private async Task ProcessRowAsync(string terminalSeq, string version, MySqlConnection connection, string userName)
        {
            if (!string.IsNullOrEmpty(version))
            {
                var parsedData = ParseVersion(version);

                // Skip the record if no underscore was found
                if (parsedData == null)
                {
                    return;
                }

                var (parsedVersion, policy) = parsedData.Value;

                terminalSeq = terminalSeq?.Trim().Trim('"');
                await InsertOrUpdateRecord(connection, terminalSeq, parsedVersion, policy, userName);
            }
        }

        private (string version, string policy)? ParseVersion(string version)
        {
            if (!version.Contains("_"))
            {
                return null; // Skip processing if no underscore is found
            }

            string[] parts = version.Split('_');
            if (parts.Length == 3)
            {
                return (parts[1].Trim(), parts[2].Trim().Trim('"'));
            }

            return (version.Trim(), string.Empty);
        }
        private async Task InsertOrUpdateRecord(MySqlConnection connection, string terminalSeq, string version, string policy, string userName)
        {
            var query = @"INSERT INTO secureageversion_record 
                     (Term_Seq, SecureAge_Version, Policy, Update_Date, Update_By, Remark) 
                     VALUES 
                     (@TermSeq, @SecureAgeVersion, @Policy, NOW(), @UserName, 'Imported From File')
                     ON DUPLICATE KEY UPDATE 
                     SecureAge_Version = @SecureAgeVersion,
                     Policy = @Policy,
                     Update_Date = NOW(),
                     Update_By = @UserName;";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TermSeq", terminalSeq);
                command.Parameters.AddWithValue("@SecureAgeVersion", version);
                command.Parameters.AddWithValue("@Policy", policy);
                command.Parameters.AddWithValue("@UserName", userName);

                await command.ExecuteNonQueryAsync();
            }
        }
        #endregion

        public async Task ImportExcelDataAsync_CardRetain(Stream excelStream, string userName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(excelStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        try
                        {
                            for (int row = 4; row <= rowCount; row++)
                            {

                                var cardRetain = new CardRetain
                                {
                                    Location = worksheet.Cells[row, 1].Value?.ToString(),
                                    TerminalID = worksheet.Cells[row, 2].Value?.ToString(),
                                    TerminalName = worksheet.Cells[row, 3].Value?.ToString(),
                                    CardNo = worksheet.Cells[row, 4].Value?.ToString(),
                                    Date = worksheet.Cells[row, 5].Value.ToString(),
                                    Reason = worksheet.Cells[row, 6].Value?.ToString(),
                                    Vendor = worksheet.Cells[row, 7].Value?.ToString(),
                                    ErrorCode = worksheet.Cells[row, 8].Value?.ToString(),
                                    InBankFlag = worksheet.Cells[row, 9].Value?.ToString(),
                                    CardStatus = worksheet.Cells[row, 10].Value?.ToString(),
                                    Telephone = worksheet.Cells[row, 11].Value?.ToString(),
                                    UpdateDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    UpdateBy = userName,
                                };


                                var queryCheck = @"
                            SELECT COUNT(*) 
                            FROM cardretain 
                            WHERE TerminalID = @TerminalID 
                            AND Date = @Date 
                            AND Location = @Location";

                                using (var commandCheck = new MySqlCommand(queryCheck, connection, (MySqlTransaction)transaction))
                                {
                                    commandCheck.Parameters.AddWithValue("@TerminalID", cardRetain.TerminalID);
                                    commandCheck.Parameters.AddWithValue("@Date", cardRetain.Date);
                                    commandCheck.Parameters.AddWithValue("@Location", cardRetain.Location);

                                    var existingRecordCount = Convert.ToInt32(await commandCheck.ExecuteScalarAsync());

                                    if (existingRecordCount == 0)
                                    {

                                        var queryInsert = @"
                                    INSERT INTO cardretain 
                                    (Location, TerminalID, TerminalName, CardNo, Date, Reason, Vendor, ErrorCode, InBankFlag, CardStatus, Telephone, UpdateDate, UpdateBy) 
                                    VALUES 
                                    (@Location, @TerminalID, @TerminalName, @CardNo, @Date, @Reason, @Vendor, @ErrorCode, @InBankFlag, @CardStatus, @Telephone, @UpdateDate, @UpdateBy);";

                                        using (var commandInsert = new MySqlCommand(queryInsert, connection, (MySqlTransaction)transaction))
                                        {

                                            commandInsert.Parameters.AddWithValue("@Location", cardRetain.Location);
                                            commandInsert.Parameters.AddWithValue("@TerminalID", cardRetain.TerminalID);
                                            commandInsert.Parameters.AddWithValue("@TerminalName", cardRetain.TerminalName);
                                            commandInsert.Parameters.AddWithValue("@CardNo", cardRetain.CardNo);
                                            commandInsert.Parameters.AddWithValue("@Date", cardRetain.Date);
                                            commandInsert.Parameters.AddWithValue("@Reason", cardRetain.Reason);
                                            commandInsert.Parameters.AddWithValue("@Vendor", cardRetain.Vendor);
                                            commandInsert.Parameters.AddWithValue("@ErrorCode", cardRetain.ErrorCode);
                                            commandInsert.Parameters.AddWithValue("@InBankFlag", cardRetain.InBankFlag);
                                            commandInsert.Parameters.AddWithValue("@CardStatus", cardRetain.CardStatus);
                                            commandInsert.Parameters.AddWithValue("@Telephone", cardRetain.Telephone);
                                            commandInsert.Parameters.AddWithValue("@UpdateDate", cardRetain.UpdateDate);
                                            commandInsert.Parameters.AddWithValue("@UpdateBy", cardRetain.UpdateBy);

                                            await commandInsert.ExecuteNonQueryAsync();
                                        }
                                    }
                                    else
                                    {
                                        // Data already exists, so skip this record or log the duplicate
                                        // Optionally: Log the duplicate or handle accordingly
                                    }
                                }
                            }

                            await transaction.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();

                            throw;
                        }
                    }
                }
            }
        }



    }
}
