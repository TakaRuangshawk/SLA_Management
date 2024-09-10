using OfficeOpenXml;
using MySql.Data.MySqlClient;
using System;
using System.Data;
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
    }
}
