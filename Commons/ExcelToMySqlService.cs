using OfficeOpenXml;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Models.ManagementModel;
using NPOI.HSSF.UserModel;
namespace SLA_Management.Commons
{
    public class ExcelToMySqlService
    {
        private readonly string _connectionString;

        public ExcelToMySqlService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task ImportExcelDataAsync(Stream excelStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(excelStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    for (int row = 2; row <= rowCount; row++) // Assuming the first row is the header
                    {
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
                        var cellValue_dateClosePb = worksheet.Cells[row, 13].Value;
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
                        var statusName = worksheet.Cells[row, 14].Value?.ToString();
                        var typeProject = worksheet.Cells[row, 15].Value?.ToString();
                        var updateDate = DateTime.Now;
                        var updateBy = "System";
                        var remark = "Imported from Excel";

                        var query = @"
                        INSERT INTO ReportCases 
                        (Case_Error_No, Terminal_ID, Place_Install, Branch_name_pb, Issue_Name, Repair1, Repair2, Repair3, Repair4, Repair5, Incident_No, Date_Inform, Date_Close_Pb, Status_Name, Type_Project, Update_Date, Update_By, Remark) 
                        VALUES 
                        (@CaseErrorNo, @TerminalId, @PlaceInstall, @BranchNamePb, @IssueName, @Repair1, @Repair2, @Repair3, @Repair4, @Repair5, @IncidentNo, @DateInform, @DateClosePb, @StatusName, @TypeProject, @UpdateDate, @UpdateBy, @Remark)
                        ON DUPLICATE KEY UPDATE 
                            Place_Install = @PlaceInstall,
                            Branch_name_pb = @BranchNamePb,
                            Issue_Name = @IssueName,
                            Repair1 = @Repair1,
                            Repair2 = @Repair2,
                            Repair3 = @Repair3,
                            Repair4 = @Repair4,
                            Repair5 = @Repair5,
                            Incident_No = @IncidentNo,
                            Date_Inform = @DateInform,
                            Date_Close_Pb = @DateClosePb,
                            Status_Name = @StatusName,
                            Type_Project = @TypeProject,
                            Update_Date = @UpdateDate,
                            Update_By = @UpdateBy;";

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
                            command.Parameters.AddWithValue("@DateInform", dateInform);
                            command.Parameters.AddWithValue("@DateClosePb", dateClosePb);
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

        #region EncryptionMonitor
        public async Task ImportEncryptionExcelDataAsync(Stream excelStream, string fileName)
        {
            var fileExtension = Path.GetExtension(fileName)?.ToLower();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                switch (fileExtension)
                {
                    case ".xlsx":
                        await ProcessXlsxFileAsync(excelStream, connection);
                        break;

                    case ".xls":
                        await ProcessXlsFileAsync(excelStream, connection);
                        break;

                    case ".csv":
                        await ProcessCsvFileAsync(excelStream, connection);
                        break;

                    default:
                        throw new NotSupportedException("Unsupported file format. Please upload .xlsx, .xls, or .csv");
                }
            }
        }
        private async Task ProcessXlsxFileAsync(Stream excelStream, MySqlConnection connection)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(excelStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                {
                    await ProcessRowAsync(worksheet.Cells[row, 1].Value?.ToString(), worksheet.Cells[row, 2].Value?.ToString(), connection);
                }
            }
        }

        private async Task ProcessXlsFileAsync(Stream excelStream, MySqlConnection connection)
        {
            var workbook = new HSSFWorkbook(excelStream);
            var sheet = workbook.GetSheetAt(0);
            for (int row = 2; row <= sheet.LastRowNum; row++)
            {
                var currentRow = sheet.GetRow(row);
                if (currentRow != null)
                {
                    await ProcessRowAsync(currentRow.GetCell(0)?.ToString(), currentRow.GetCell(1)?.ToString(), connection);
                }
            }
        }

        private async Task ProcessCsvFileAsync(Stream excelStream, MySqlConnection connection)
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

                    await ProcessRowAsync(values[0], values[1], connection);
                }
            }
        }

        private async Task ProcessRowAsync(string terminalSeq, string version, MySqlConnection connection)
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
                await InsertOrUpdateRecord(connection, terminalSeq, parsedVersion, policy);
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
        private async Task InsertOrUpdateRecord(MySqlConnection connection, string terminalSeq, string version, string policy)
        {
            var query = @"INSERT INTO secureageversion_record 
                     (Term_Seq, SecureAge_Version, Policy, Update_Date, Update_By, Remark) 
                     VALUES 
                     (@TermSeq, @SecureAgeVersion, @Policy, NOW(), 'System', 'Imported From File')
                     ON DUPLICATE KEY UPDATE 
                     SecureAge_Version = @SecureAgeVersion,
                     Policy = @Policy,
                     Update_Date = NOW(),
                     Update_By = 'System';";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TermSeq", terminalSeq);
                command.Parameters.AddWithValue("@SecureAgeVersion", version);
                command.Parameters.AddWithValue("@Policy", policy);

                await command.ExecuteNonQueryAsync();
            }
        }
        #endregion

        public async Task ImportExcelDataAsync_CardRetain(Stream excelStream)
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
                                    (Location, TerminalID, TerminalName, CardNo, Date, Reason, Vendor, ErrorCode, InBankFlag, CardStatus, Telephone, UpdateDate) 
                                    VALUES 
                                    (@Location, @TerminalID, @TerminalName, @CardNo, @Date, @Reason, @Vendor, @ErrorCode, @InBankFlag, @CardStatus, @Telephone, @UpdateDate);";

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
