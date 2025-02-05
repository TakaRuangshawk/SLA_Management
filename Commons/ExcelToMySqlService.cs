using OfficeOpenXml;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Models.ManagementModel;
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
        public async Task ImportEncryptionExcelDataAsync(Stream excelStream)
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
                        var terminalSeq = worksheet.Cells[row, 1].Value?.ToString();
                        var version = worksheet.Cells[row, 2].Value?.ToString();
                        var policy = worksheet.Cells[row, 3].Value?.ToString();
                        var updateDate = DateTime.Now;
                        var updateBy = "System";
                        var remark = "Imported From Excel";

                        var query = @"INSERT INTO secureageversion_record 
                                    (Term_Seq, SecureAge_Version, Policy, Update_Date, Update_By, Remark) 
                                    VALUES 
                                    (@TermSeq, @SecureAgeVersion, @Policy, @UpdateDate, @UpdateBy, @Remark)
                                    ON DUPLICATE KEY UPDATE 
                                    SecureAge_Version = @SecureAgeVersion,
                                    Policy = @Policy,
                                    Update_Date = @UpdateDate,
                                    Update_By = @UpdateBy;";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@TermSeq", terminalSeq);
                            command.Parameters.AddWithValue("@SecureAgeVersion", version);
                            command.Parameters.AddWithValue("@Policy", policy);
                            command.Parameters.AddWithValue("@UpdateDate", updateDate);
                            command.Parameters.AddWithValue("@UpdateBy", updateBy);
                            command.Parameters.AddWithValue("@Remark", remark);

                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
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
                            for (int row = 4; row <= rowCount; row++) // Skip header row
                            {
                                // Map Excel row to CardRetain object
                                var cardRetain = new CardRetain
                                {
                                    Location = worksheet.Cells[row, 1].Value?.ToString(),
                                    TerminalID = worksheet.Cells[row, 2].Value?.ToString(),
                                    TerminalName = worksheet.Cells[row, 3].Value?.ToString(),
                                    CardNo = worksheet.Cells[row, 4].Value?.ToString(),
                                    Date = worksheet.Cells[row, 5].Value?.ToString(),
                                    Reason = worksheet.Cells[row, 6].Value?.ToString(),
                                    Vendor = worksheet.Cells[row, 7].Value?.ToString(),
                                    ErrorCode = worksheet.Cells[row, 8].Value?.ToString(),
                                    InBankFlag = worksheet.Cells[row, 9].Value?.ToString(),
                                    CardStatus = worksheet.Cells[row, 10].Value?.ToString(),
                                    Telephone = worksheet.Cells[row, 11].Value?.ToString(),
                                    UpdateDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), // Current timestamp
                                };

                                // Database Insert Query
                                var query = @"
                        INSERT INTO cardretain 
                        (Location, TerminalID, TerminalName, CardNo, Date, Reason, Vendor, ErrorCode, InBankFlag, CardStatus, Telephone, UpdateDate) 
                        VALUES 
                        (@Location, @TerminalID, @TerminalName, @CardNo, @Date, @Reason, @Vendor, @ErrorCode, @InBankFlag, @CardStatus, @Telephone, @UpdateDate)
                        ON DUPLICATE KEY UPDATE 
                            Location = @Location,
                            TerminalID = @TerminalID,
                            TerminalName = @TerminalName,
                            CardNo = @CardNo,
                            Date = @Date,
                            Reason = @Reason,
                            Vendor = @Vendor,
                            ErrorCode = @ErrorCode,
                            InBankFlag = @InBankFlag,
                            CardStatus = @CardStatus,
                            Telephone = @Telephone,
                            UpdateDate = @UpdateDate;";

                                using (var command = new MySqlCommand(query, connection, (MySqlTransaction)transaction))
                                {
                                    // Adding parameters to the query
                                    command.Parameters.AddWithValue("@Location", cardRetain.Location);
                                    command.Parameters.AddWithValue("@TerminalID", cardRetain.TerminalID);
                                    command.Parameters.AddWithValue("@TerminalName", cardRetain.TerminalName);
                                    command.Parameters.AddWithValue("@CardNo", cardRetain.CardNo);
                                    command.Parameters.AddWithValue("@Date", cardRetain.Date);
                                    command.Parameters.AddWithValue("@Reason", cardRetain.Reason);
                                    command.Parameters.AddWithValue("@Vendor", cardRetain.Vendor);
                                    command.Parameters.AddWithValue("@ErrorCode", cardRetain.ErrorCode);
                                    command.Parameters.AddWithValue("@InBankFlag", cardRetain.InBankFlag);
                                    command.Parameters.AddWithValue("@CardStatus", cardRetain.CardStatus);
                                    command.Parameters.AddWithValue("@Telephone", cardRetain.Telephone);
                                    command.Parameters.AddWithValue("@UpdateDate", cardRetain.UpdateDate);

                                    await command.ExecuteNonQueryAsync();
                                }
                            }

                            await transaction.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            // Log exception here (ex.Message)
                            throw;
                        }
                    }
                }
            }
        }




    }
}
