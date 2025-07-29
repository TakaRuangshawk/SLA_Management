using MySql.Data.MySqlClient;

using Serilog;
using SLA_Management.Commons;
using SLA_Management.Models.CassetteStatus;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Services
{
    public class ReportCassetteBoxService
    {
        private string _connectionString { get; set; }

        public ReportCassetteBoxService(string connectionString)
        {
            _connectionString = connectionString;

        }

        public bool AddImportFileData(ImportFileData cassetteEventFile)
        {
            bool result = false;
            MySqlConnection conn = new MySqlConnection(_connectionString);
            string sql = @"INSERT INTO `import_file_data`(`Id`,`Name_File`,`Upload_By`,`Upload_Date`,`Data_Date`,`Import_Data_Project`) VALUES (@Id,@Name_File,@Upload_By,@Upload_Date,@Data_Date,@Import_Data_Project);";
            try
            {
                conn.Open();
                MySqlCommand com = new MySqlCommand(sql);
                com.Parameters.AddWithValue("@Id", cassetteEventFile.Id);
                com.Parameters.AddWithValue("@Name_File", cassetteEventFile.Name_File);
                com.Parameters.AddWithValue("@Upload_By", cassetteEventFile.Upload_By);
                com.Parameters.AddWithValue("@Upload_Date", cassetteEventFile.Upload_Date);
                com.Parameters.AddWithValue("@Data_Date", cassetteEventFile.Data_Date);
                com.Parameters.AddWithValue("@Import_Data_Project", cassetteEventFile.Import_Data_Project);


                com.Connection = conn;

                com.ExecuteNonQuery();
                result = true;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "AddImportFileData Error : ");

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

            }
            return result;

        }
        public bool AddReportCassette(ReportCassetteDB reportCassetteDB)
        {
            bool result = false;
            MySqlConnection conn = new MySqlConnection(_connectionString);
            string sql = @"INSERT INTO `report_cassette` (`Id`,`Cassette_Id`,`Cassette_Status_Count`,`Cassette_Status`,`Cassette_Event_File_Id`) VALUES (@Id,@Cassette_Id,@Cassette_Status_Count,@Cassette_Status,@Cassette_Event_File_Id);";
            try
            {
                conn.Open();
                MySqlCommand com = new MySqlCommand(sql);
                com.Parameters.AddWithValue("@Id", reportCassetteDB.Id);
                com.Parameters.AddWithValue("@Cassette_Id", reportCassetteDB.Cassette_Id);
                com.Parameters.AddWithValue("@Cassette_Status_Count", reportCassetteDB.Cassette_Status_Count);
                com.Parameters.AddWithValue("@Cassette_Status", reportCassetteDB.Cassette_Status);
                com.Parameters.AddWithValue("@Cassette_Event_File_Id", reportCassetteDB.Cassette_Event_File_Id);
                com.Connection = conn;

                com.ExecuteNonQuery();
                result = true;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "AddReportCassette Error : ");

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

            }
            return result;

        }
        public bool AddReportTerminalCassette(ReportTerminalCassetteDB reportTerminalCassetteDB)
        {
            bool result = false;
            MySqlConnection conn = new MySqlConnection(_connectionString);
            string sql = @"INSERT INTO `report_terminal_cassette` (`Id`,`TermId`,`Cassette_Id`,`Cassette_Status`,`Cassette_Remain`,`Cassette_Event_File_Id`) VALUES (@Id,@TermId,@Cassette_Id,@Cassette_Status,@Cassette_Remain,@Cassette_Event_File_Id);";
            try
            {
                conn.Open();
                MySqlCommand com = new MySqlCommand(sql);
                com.Parameters.AddWithValue("@Id", reportTerminalCassetteDB.Id);
                com.Parameters.AddWithValue("@TermId", reportTerminalCassetteDB.TermId);
                com.Parameters.AddWithValue("@Cassette_Id", reportTerminalCassetteDB.Cassette_Id);
                com.Parameters.AddWithValue("@Cassette_Status", reportTerminalCassetteDB.Cassette_Status);
                com.Parameters.AddWithValue("@Cassette_Remain", reportTerminalCassetteDB.Cassette_Remain);
                com.Parameters.AddWithValue("@Cassette_Event_File_Id", reportTerminalCassetteDB.Cassette_Event_File_Id);
                com.Connection = conn;

                com.ExecuteNonQuery();
                result = true;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "AddReportTerminalCassette Error : ");

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

            }
            return result;

        }


        public void DeleteImportFileData(string id)
        {

            MySqlConnection conn = new MySqlConnection(_connectionString);
            string sql = @"DELETE FROM `import_file_data`
                            WHERE Id = @Id;
                           DELETE FROM `report_cassette`
                            WHERE Cassette_Event_File_Id = @Id;
                           DELETE FROM `report_terminal_cassette`
                            WHERE Cassette_Event_File_Id = @Id;";
            try
            {
                conn.Open();
                MySqlCommand com = new MySqlCommand(sql);
                com.Parameters.AddWithValue("@Id", id);

                com.Connection = conn;

                com.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                Log.Error(ex, "DeleteImportFileData Error : ");

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

            }


        }

        public List<ImportFileData> GetImportFileDataByDate(DateTime data, string projectName)
        {
            List<ImportFileData> result = new List<ImportFileData>();
            DateTime start = new DateTime(data.Year, data.Month, data.Day, 0, 0, 0);
            DateTime end = start.AddDays(1);
            MySqlConnection conn = new MySqlConnection(_connectionString);
            string sql = @"SELECT 
                              JSON_OBJECT(	'Id', Id,
				                            'Name_File', Name_File,
                                            'Upload_By', Upload_By,
                                            'Upload_Date', DATE_FORMAT(Upload_Date, '%Y-%m-%dT%H:%i:%sZ') ,
                                            'Data_Date',DATE_FORMAT(Data_Date, '%Y-%m-%dT%H:%i:%sZ') ) AS json_result
                            FROM import_file_data  
                            where Import_Data_Project = @Import_Data_Project  and  Data_Date between @start and @end;";
            try
            {
                conn.Open();
                MySqlCommand com = new MySqlCommand(sql);
                com.Parameters.AddWithValue("@start", start);
                com.Parameters.AddWithValue("@end", end);
                com.Parameters.AddWithValue("@Import_Data_Project", projectName);
                com.Connection = conn;

                using (var reader = com.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string jsonText = reader.GetString("json_result");
                        if (jsonText.EndsWith("|"))
                        {
                            jsonText = jsonText.Substring(0, jsonText.Length - 1);
                        }
                        ImportFileData item = JsonSerializer.Deserialize<ImportFileData>(jsonText);
                        result.Add(item);
                    }
                }



            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetImportFileDataByDate Error : ");

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

            }
            return result;

        }


        public void MapReportCassette(string[] eventMoniterConfig, string[] cassetteMoniterConfig, List<ReportCassette> reportCassetteDatas, List<ReportTerminalCassette> reportTerminalCassetteDatas, string fileName, List<TerminalCassette> data)
        {
            var allCassetteStatus = data.Where(i => i.cassetteBoxStatuses != null)
                .SelectMany(d => d.cassetteBoxStatuses)
                .Where(e => eventMoniterConfig.Contains(e.bs))
                    .Select(s => s.bs)
                    .Distinct()
                    .ToList();

            var allCassetteId = data.Where(i => i.cassetteBoxStatuses != null)
                .SelectMany(d => d.cassetteBoxStatuses)
                .Where(e => cassetteMoniterConfig.Contains(e.bid))
                    .Select(s => s.bid)
                    .Distinct()
                    .ToList();

            var dataReport = data.Where(i => i.cassetteBoxStatuses.Where(e => allCassetteStatus.Contains(e.bs)).Count() != 0)
                .SelectMany(entry => entry.cassetteBoxStatuses
                .Select(item => new ReportTerminalCassette()
                {
                    termId = entry.terminalId,
                    cassetteId = item.bid,
                    cassetteStatus = item.bs,
                    cassetteRemain = int.TryParse(item.bln, out int number) ? number : 0,
                    fileData = fileName
                }))
                .Where(i => allCassetteStatus.Contains(i.cassetteStatus))
                .ToList();

            List<ReportCassette> reportCassette = new List<ReportCassette>();

            foreach (var item in allCassetteId)
            {
                foreach (var status in allCassetteStatus)
                {

                    reportCassette.Add(new ReportCassette(item, dataReport.Where(i => i.cassetteId == item && i.cassetteStatus == status).Count(), fileName, status));
                }

            }

            reportCassetteDatas.AddRange(reportCassette);
            reportTerminalCassetteDatas.AddRange(dataReport);
        }


        public class InsertResult
        {
            public bool Inserted { get; set; }
            public int CassetteCount { get; set; }
            public int TerminalCassetteCount { get; set; }

            public int CassetteInsertSucceed { get; set; }
            public int TerminalCassetteSucceed { get; set; }

            public int CassetteInsertError { get; set; }
            public int TerminalCassetteError { get; set; }


        }
        public class ReadFile
        {
            public static TerminalCassetteData GetStatusCaseboxV1(string pathFile, DateTime queryDate)
            {
                List<TerminalCassette> data = new List<TerminalCassette>();
                using (FileStream fs = new FileStream(pathFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: false))

                    ReadDataV1(fs, data);
                TerminalCassetteData terminalCassetteData = new TerminalCassetteData();
                terminalCassetteData.queryDate = queryDate;
                terminalCassetteData.data = data;
                return terminalCassetteData;
            }
            public static TerminalCassetteData GetStatusCaseboxV1(Stream fileStream, DateTime queryDate)
            {
                List<TerminalCassette> data = new List<TerminalCassette>();
                ReadDataV1(fileStream, data);


                TerminalCassetteData terminalCassetteData = new TerminalCassetteData();
                terminalCassetteData.queryDate = queryDate;
                terminalCassetteData.data = data;
                return terminalCassetteData;
            }
            private static void ReadDataV1(Stream fileStream, List<TerminalCassette> data)
            {
                using (StreamReader sr = new StreamReader(fileStream, Encoding.UTF8))
                {
                    string line;
                    int index = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        ++index;
                        if (index == 1)
                        {
                            continue;
                        }
                        if (string.IsNullOrEmpty(line))
                        {

                            continue; // ข้ามบรรทัดว่าง
                        }
                        try
                        {



                            data.Add(ConvertData.ConvertToTerminalCassette(line));
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Data Error {line} : {ex}");
                        }

                    }
                    sr.Close();
                    fileStream.Close();
                }
            }
            private static void ReadDataV2(Stream fileStream, List<TerminalCassette> data)
            {
                using (StreamReader sr = new StreamReader(fileStream, Encoding.UTF8))
                {
                    string line;
                    int index = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        ++index;
                        if (index == 1)
                        {
                            continue;
                        }
                        if (string.IsNullOrEmpty(line))
                        {

                            continue; // ข้ามบรรทัดว่าง
                        }
                        if (line.EndsWith("|"))
                        {
                            line = line.Substring(0, line.Length - 1);
                        }

                        try
                        {
                            data.Add(ConvertData.ConvertToTerminalCassetteV2(line));
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"ReadDataV2 Error {line} : {ex}");
                        }

                    }
                    sr.Close();
                    fileStream.Close();
                }
            }
            public static TerminalCassetteData GetStatusCaseboxV2(Stream fileStream)
            {
                List<TerminalCassette> data = new List<TerminalCassette>();
                ReadDataV2(fileStream, data);

                TerminalCassetteData terminalCassetteData = new TerminalCassetteData();
                terminalCassetteData.queryDate = data.FirstOrDefault() != null ? data.First().queryDate : DateTime.Now;
                terminalCassetteData.data = data;
                return terminalCassetteData;
            }

            public static TerminalCassetteData GetStatusCaseboxV2(string pathFile)
            {
                List<TerminalCassette> data = new List<TerminalCassette>();
                using (FileStream fs = new FileStream(pathFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: false))

                    ReadDataV2(fs, data);
                TerminalCassetteData terminalCassetteData = new TerminalCassetteData();
                terminalCassetteData.queryDate = data.FirstOrDefault() != null ? data.First().queryDate : DateTime.Now;
                terminalCassetteData.data = data;
                return terminalCassetteData;
            }
        }

        public class InsertReport
        {
            private static string[] eventMoniterConfig = { "9" };
            private static string[] cassetteMoniterConfig = { "00100", "00500", "1000A", "1000B" };
            private static string projectNameConfig = "ReadStatusCasebox";

            public static InsertResult Insert(List<Stream> files, string connectionStringReportConfig, string username)
            {
                var result = new InsertResult
                {
                    Inserted = false,
                    CassetteCount = 0,
                    TerminalCassetteCount = 0,

                    CassetteInsertSucceed = 0,
                    TerminalCassetteSucceed = 0,

                    CassetteInsertError = 0,
                    TerminalCassetteError = 0


                };
                TerminalCassetteData terminalCassetteData = null;
                foreach (var file in files)
                {


                    TerminalCassetteData readData = ReportCassetteBoxService.ReadFile.GetStatusCaseboxV2(file); ;

                    if (terminalCassetteData == null)
                    {
                        terminalCassetteData = readData;
                    }
                    else if (readData.queryDate.ToString("yyyyMMdd").Equals(terminalCassetteData.queryDate.ToString("yyyyMMdd")))
                    {
                        terminalCassetteData.data.AddRange(readData.data);
                    }


                }
                if (terminalCassetteData == null || terminalCassetteData.data.Count == 0)
                    return result;

                List<ReportCassette> reportCassetteDatas = new List<ReportCassette>();
                List<ReportTerminalCassette> reportTerminalCassetteDatas = new List<ReportTerminalCassette>();


                var dateTextFileReport = terminalCassetteData.queryDate;

                var textFileReport = $"{Guid.NewGuid().ToString()}_{dateTextFileReport.ToString("yyyyMMdd")}.txt";

                ReportCassetteBoxService reportCassetteBoxService = new ReportCassetteBoxService(connectionStringReportConfig);
                reportCassetteBoxService.MapReportCassette(eventMoniterConfig, cassetteMoniterConfig, reportCassetteDatas, reportTerminalCassetteDatas, textFileReport, terminalCassetteData.data);
                var cassetteEventFiles = reportCassetteBoxService.GetImportFileDataByDate(terminalCassetteData.queryDate, projectNameConfig);
                if (cassetteEventFiles.Count != 0)
                {
                    foreach (var cassetteEventFileDate in cassetteEventFiles)
                    {
                        reportCassetteBoxService.DeleteImportFileData(cassetteEventFileDate.Id);
                    }

                }
                var cassetteEventFile = new ImportFileData()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name_File = textFileReport,
                    Upload_By = username,
                    Upload_Date = DateTime.Now,
                    Data_Date = terminalCassetteData.queryDate,
                    Import_Data_Project = projectNameConfig
                };
                if (reportCassetteBoxService.AddImportFileData(cassetteEventFile))
                {
                    result.Inserted = true;
                    result.CassetteCount = reportCassetteDatas.Count;
                    result.TerminalCassetteCount = reportTerminalCassetteDatas.Count;

                    foreach (var reportCassette in reportCassetteDatas)
                    {
                        var insertStatus = reportCassetteBoxService.AddReportCassette(new ReportCassetteDB()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Cassette_Id = reportCassette.cassetteId,
                            Cassette_Status_Count = reportCassette.cassetteStatusCount,
                            Cassette_Status = reportCassette.cassetteStatus,
                            Cassette_Event_File_Id = cassetteEventFile.Id,
                        });

                        if (insertStatus)
                        {
                            result.CassetteInsertSucceed++;
                        }
                        else
                        {
                            result.CassetteInsertError++;
                        }

                    }

                    foreach (var reportTerminalCassette in reportTerminalCassetteDatas)
                    {
                        var insertStatus = reportCassetteBoxService.AddReportTerminalCassette(new ReportTerminalCassetteDB()
                        {
                            Id = Guid.NewGuid().ToString(),
                            TermId = reportTerminalCassette.termId,
                            Cassette_Id = reportTerminalCassette.cassetteId,
                            Cassette_Status = reportTerminalCassette.cassetteStatus,
                            Cassette_Remain = reportTerminalCassette.cassetteRemain,
                            Cassette_Event_File_Id = cassetteEventFile.Id

                        });


                        if (insertStatus)
                        {
                            result.TerminalCassetteSucceed++;
                        }
                        else
                        {
                            result.TerminalCassetteError++;
                        }
                    }

                }

                return result;
            }
        }
    }
}
