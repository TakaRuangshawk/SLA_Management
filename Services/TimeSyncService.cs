using MySql.Data.MySqlClient;
using Serilog;
using SLA_Management.Models.CassetteStatus;
using SLA_Management.Models.TimeSync;
using SLA_Management.Services;
using System.Data;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Services
{
    public class TimeSyncService 
    {
        private readonly IConfiguration _configuration;
        private readonly string projectNameConfig = "TimeSyncReport";

        public TimeSyncService(IConfiguration configuration)
        {
            _configuration = configuration;

        }
        public string? GetConnectionStringByBank(string bankName)
        {
            var key = $"ConnectString_NonOutsource:FullNameConnection_{bankName.ToLower()}";
            return _configuration.GetValue<string>(key);
        }
        public ImportFileData GetLatestUpdate(string bankName)
        {
            try
            {
                var connStr = GetConnectionStringByBank(bankName);
                if (string.IsNullOrEmpty(connStr)) return null;

                ImportFileService importFileService = new ImportFileService(connStr);


                var importFileLatest = importFileService.GetImportFileDataLatest(projectNameConfig);


                return importFileLatest;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetLatestUpdate Error: {ex.Message}");
                return null;
            }
        }
        public async Task<List<ReportTimeSync>> GetTimeSyncReportAsync(string terminalID, DateTime? fromdate, string statusType,  string bankName)
        {
            var importFileLatest = GetLatestUpdate(bankName);
            var reportTimeSync = new List<ReportTimeSync>();
            string connectionDB = GetConnectionStringByBank(bankName);
            List<string> querys = new List<string>();
            if (string.IsNullOrEmpty(connectionDB)) return reportTimeSync;

            if (fromdate.HasValue)
            {
                DateTime dateValue = fromdate.Value;
                DateTime start = new DateTime(dateValue.Year, dateValue.Month, dateValue.Day, 0, 0, 0);
                DateTime end = start.AddDays(1);
                string startStr = $"{start.Year}-{start.Month}-{start.Day} 00:00:00";
                string endStr = $"{end.Year}-{end.Month}-{end.Day} 00:00:00";
                querys.Add($"TimeSync_Date between '{startStr}' AND '{endStr}'");
            }
            string sq1 = "";
            if (querys.Count != 0)
            {
                string joinedFruits = String.Join("and ", querys);
                sq1 += $"where {joinedFruits}";
            }
            




            string sq2 = "";
            if (!string.IsNullOrEmpty(terminalID))
            {
                sq2 = $"where dev.TERM_ID = '{terminalID}'";
                
            }
            
            try
            {
                using var conn = new MySqlConnection(connectionDB);
                await conn.OpenAsync();

                string query = $@"select dev.DEVICE_ID , dev.TERM_ID as Terminal_ID , dev.TERM_IP as IP ,dev.TERM_NAME as Terminal_Name, dev.TERM_SEQ as Serial_No ,re.TimeSync_Date as Latest_Sync_Time
                                    from device_info dev 
                                    LEFT JOIN (SELECT Device_Id, MAX(TimeSync_Date) AS TimeSync_Date
                                      FROM report_timesync
                                      {sq1}
                                      GROUP BY Device_Id
                                    ) re
                                    ON dev.DEVICE_ID = re.Device_Id
                                    {sq2};";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                int count = 1;
                while (await reader.ReadAsync())
                {
                    int ordLatest = reader.GetOrdinal("Latest_Sync_Time"); // <= ใช้ ordinal
                    bool hasLatest = !reader.IsDBNull(ordLatest);
                    string latestStr = hasLatest ? DateTime.SpecifyKind(reader.GetDateTime(ordLatest), DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm:ss") : "-";
                    reportTimeSync.Add(new ReportTimeSync()
                    {
                        No = count,
                        Terminal_ID = reader["Terminal_ID"]?.ToString() ?? "-",
                        IP = reader["IP"]?.ToString() ?? "-",
                        Terminal_Name = reader["Terminal_Name"]?.ToString() ?? "-",
                        Serial_No = reader["Serial_No"]?.ToString() ?? "-",
                        TimeSync_Status = hasLatest ? "Yes" : "No",
                        Latest_Sync_Time = latestStr
                    });
                    count++;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in GetTimeSyncReportAsync: " + ex.Message);

            }
            
            reportTimeSync = reportTimeSync
            .GroupBy(x => x.Terminal_ID)
            .Select(g =>
            {
                var preferred = g.Where(x => x.TimeSync_Status == "Yes")
                                 .OrderByDescending(x => x.Latest_Sync_Time)
                                 .FirstOrDefault();

                return preferred ?? g.OrderByDescending(x => x.Latest_Sync_Time).First();
            })
            .ToList();

            if (!string.IsNullOrEmpty(statusType))
            {
                reportTimeSync = reportTimeSync.Where(i => i.TimeSync_Status == statusType).ToList();
            }



            return reportTimeSync;
        }



    }
    public class ReportTimeSyncLog : ImportFileService
    {
        private string _connectionString { get; set; }
        
        public ReportTimeSyncLog(string connectionString) : base(connectionString)
        {
            _connectionString = connectionString;
        }


        public bool AddReportTimeSyncLog(TimeSyncLog timeSyncLog)
        {
            bool result = false;
            MySqlConnection conn = new MySqlConnection(_connectionString);
            string sql = @"INSERT INTO `report_timesync`
                            (`Id`,
                            `Device_Id`,
                            `TimeSync_Date`,
                            `TimeSync_Log_File_Id`)
                            VALUES
                            (@Id,
                            @Device_Id,
                            @TimeSync_Date,
                            @TimeSync_Log_File_Id);";
            try
            {
                conn.Open();
                MySqlCommand com = new MySqlCommand(sql);
                com.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                com.Parameters.AddWithValue("@Device_Id", timeSyncLog.Device_Id);
                com.Parameters.AddWithValue("@TimeSync_Date", timeSyncLog.TimeSync_Date);
                com.Parameters.AddWithValue("@TimeSync_Log_File_Id", timeSyncLog.TimeSync_Log_File_Id);

                com.Connection = conn;

                com.ExecuteNonQuery();
                result = true;

            }
            catch (Exception ex)
            {
                //Log.Error(ex, "AddImportFileData Error : ");

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


        public void DeleteReportTimeSyncLogById(string id)
        {

            MySqlConnection conn = new MySqlConnection(_connectionString);
            string sql = @"DELETE FROM `report_timesync` WHERE TimeSync_Log_File_Id = @Id;";
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
                Log.Error(ex, "DeleteReportTimeSyncLogById Error : ");

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

            }


        }

        public List<DeviceInfo> GetDeviceInfos()
        {
            List<DeviceInfo> result = new List<DeviceInfo>();
            
            MySqlConnection conn = new MySqlConnection(_connectionString);
            string sql = @"SELECT JSON_OBJECT(
    'DEVICE_ID', `DEVICE_ID`,
    'TERM_ID', `TERM_ID`,
    'DEPT_ID', `DEPT_ID`,
    'TYPE_ID', `TYPE_ID`,
    'BRAND_ID', `BRAND_ID`,
    'MODEL_ID', `MODEL_ID`,
    'TERM_SEQ', `TERM_SEQ`,
    'COUNTER_CODE', `COUNTER_CODE`,
    'TERM_IP', `TERM_IP`,
    'STATUS', `STATUS`,
    'TERM_NAME', `TERM_NAME`,
    'TERM_ADDR', `TERM_ADDR`,
    'TERM_LOCATION', `TERM_LOCATION`,
    'TERM_ZONE', `TERM_ZONE`,
    'CONTROL_BY', `CONTROL_BY`,
    'REPLENISH_BY', `REPLENISH_BY`,
    'POST', `POST`,
    'INSTALL_DATE', `INSTALL_DATE`,
    'ACTIVE_DATE', `ACTIVE_DATE`,
    'SERVICE_TYPE', `SERVICE_TYPE`,
    'INSTALL_TYPE', `INSTALL_TYPE`,
    'LAYOUT_TYPE', `LAYOUT_TYPE`,
    'MAN_ID', `MAN_ID`,
    'SERVICEMAN_ID', `SERVICEMAN_ID`,
    'COMPANY_ID', `COMPANY_ID`,
    'COMPANY_NAME', `COMPANY_NAME`,
    'SERVICE_BEGINDATE', `SERVICE_BEGINDATE`,
    'SERVICE_ENDDATE', `SERVICE_ENDDATE`,
    'SERVICE_YEARS', `SERVICE_YEARS`,
    'IS_CCTV', `IS_CCTV`,
    'IS_UPS', `IS_UPS`,
    'IS_INTERNATIONAL', `IS_INTERNATIONAL`,
    'BUSINESS_BEGINTIME', `BUSINESS_BEGINTIME`,
    'BUSINESS_ENDTIME', `BUSINESS_ENDTIME`,
    'IS_VIP', `IS_VIP`,
    'AREA_ID', `AREA_ID`,
    'AREA_ADDR', `AREA_ADDR`,
    'FUNCTION_TYPE', `FUNCTION_TYPE`,
    'LONGITUDE', `LONGITUDE`,
    'LATITUDE', `LATITUDE`,
    'PROVINCE', `PROVINCE`,
    'LOT_TYPE', `LOT_TYPE`,
    'AUDITING', `AUDITING`,
    'CURRENT_IP', `CURRENT_IP`,
    'VERSION_ATMC', `VERSION_ATMC`,
    'VERSION_SP', `VERSION_SP`,
    'VERSION_AGENT', `VERSION_AGENT`,
    'VERSION_MB', `VERSION_MB`,
    'FLAG_XFS', `FLAG_XFS`,
    'FLAG_EJ', `FLAG_EJ`,
    'FLAG_FSN', `FLAG_FSN`,
    'EJ_FILES', `EJ_FILES`,
    'FSN_PATH', `FSN_PATH`,
    'TASK_PARA', `TASK_PARA`,
    'VERSION_AD', `VERSION_AD`,
    'MODIFY_USERID', `MODIFY_USERID`,
    'MODIFY_DATE', DATE_FORMAT(CONVERT_TZ(`MODIFY_DATE`, @@session.time_zone, '+00:00'), '%Y-%m-%dT%H:%i:%sZ'),
    'ADD_USERID', `ADD_USERID`,
    'ADD_DATE', DATE_FORMAT(CONVERT_TZ(`ADD_DATE`, @@session.time_zone, '+00:00'), '%Y-%m-%dT%H:%i:%sZ'),
    'ASSET_NO', `ASSET_NO`,
    'CASH_BOX_NUM', `CASH_BOX_NUM`,
    'SERVICE_SMS_TYPE', `SERVICE_SMS_TYPE`,
    'EJ_OPEN_DATE', `EJ_OPEN_DATE`,
    'ATMC_UPDATE_TIME', DATE_FORMAT(CONVERT_TZ(`ATMC_UPDATE_TIME`, @@session.time_zone, '+00:00'), '%Y-%m-%dT%H:%i:%sZ'),
    'SP_UPDATE_TIME', DATE_FORMAT(CONVERT_TZ(`SP_UPDATE_TIME`, @@session.time_zone, '+00:00'), '%Y-%m-%dT%H:%i:%sZ'),
    'AGENT_UPDATE_TIME', DATE_FORMAT(CONVERT_TZ(`AGENT_UPDATE_TIME`, @@session.time_zone, '+00:00'), '%Y-%m-%dT%H:%i:%sZ'),
    'VERSION_NV', `VERSION_NV`,
    'NV_UPDATE_TIME', DATE_FORMAT(CONVERT_TZ(`NV_UPDATE_TIME`, @@session.time_zone, '+00:00'), '%Y-%m-%dT%H:%i:%sZ'),
    'VERSION_MAIN', `VERSION_MAIN`,
    'MAIN_UPDATE_TIME', DATE_FORMAT(CONVERT_TZ(`MAIN_UPDATE_TIME`, @@session.time_zone, '+00:00'), '%Y-%m-%dT%H:%i:%sZ')
  )AS json_result
FROM `device_info`;";
            try
            {
                conn.Open();
                MySqlCommand com = new MySqlCommand(sql);
                
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
                        DeviceInfo item = JsonSerializer.Deserialize<DeviceInfo>(jsonText);
                        result.Add(item);
                    }
                }



            }
            catch (Exception ex)
            {
                //Log.Error(ex, "GetImportFileDataByDate Error : ");

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
        public class InsertResult
        {
            public bool Inserted { get; set; }
            public int ReportTimeSyncCount { get; set; }
            public int ReportTimeSyncSucceed { get; set; }
            public int ReportTimeSyncError { get; set; }
            


        }

        public class ReadFile
        {


            private static void ReadData(Stream fileStream, List<TimeSyncLogEntry> data)
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
                            
                            if (AccessLogParser.TryParse(line,out var log))
                            {
                                data.Add(log);
                            }
                            
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"ReadData Error {line} : {ex}");
                        }

                    }
                    sr.Close();
                    fileStream.Close();
                }
            }
            public static List<TimeSyncLog> GetTimeSyncReports(Stream fileStream,List<DeviceInfo> devices)
            {
                List<TimeSyncLogEntry> logs = new List<TimeSyncLogEntry>();


                ReadData(fileStream, logs);



                List<TimeSyncLog> data = new List<TimeSyncLog>();




                foreach (var item in logs)
                {
                    var device = devices.FirstOrDefault(i => i.TERM_IP == item.Ip.ToString());

                    if (device != null)
                    {
                        data.Add(new TimeSyncLog()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Device_Id = device.DEVICE_ID,
                            TimeSync_Date = item.Timestamp,
                            TimeSync_Log_File_Id = ""
                        });
                    }
                    
                }

                return data;

            }

        }
        public class InsertReport
        {
            private static string projectNameConfig = "TimeSyncReport";

            public static InsertResult Insert(List<Stream> files, string connectionStringReportConfig, string username)
            {


                var result = new InsertResult
                {
                    Inserted = false,
                    ReportTimeSyncCount = 0,
                    ReportTimeSyncSucceed = 0,

                    ReportTimeSyncError = 0,
                    


                };
                var nameFile = new List<string>();
                List<TimeSyncLog> timeSyncLogs = new List<TimeSyncLog>();
                ReportTimeSyncLog timeSyncLogReport = new ReportTimeSyncLog(connectionStringReportConfig);
                List<DeviceInfo> deviceInfos = timeSyncLogReport.GetDeviceInfos();
                foreach (var file in files)
                {


                    List<TimeSyncLog> readData = ReportTimeSyncLog.ReadFile.GetTimeSyncReports(file,deviceInfos); ;

                    timeSyncLogs.AddRange(readData);
                    
                    if (file is FileStream fileStream)
                    {
                        nameFile.Add(Path.GetFileName(fileStream.Name));
                    }
                    else
                    {
                        nameFile.Add($"{Guid.NewGuid().ToString()}.txt");

                    }


                }
                if (timeSyncLogs == null || timeSyncLogs.Count == 0)
                    return result;

                
                
                var textFileReport = string.Join(", ", nameFile);


                var dateData = timeSyncLogs.MaxBy(d => d.TimeSync_Date)?.TimeSync_Date??DateTime.Now;



                bool statusInsert = false;
                var importFileData = new ImportFileData()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name_File = textFileReport,
                    Upload_By = username,
                    Upload_Date = DateTime.Now,
                    Data_Date = dateData,
                    Import_Data_Project = projectNameConfig
                };


                

                var cassetteEventFile = timeSyncLogReport.GetFirstImportFileDataByDate(dateData, projectNameConfig);
                if (cassetteEventFile != null)
                {
                    importFileData.Id = cassetteEventFile.Id;
                    timeSyncLogReport.DeleteReportTimeSyncLogById(importFileData.Id);
                    statusInsert = timeSyncLogReport.UpdateImportFileData(importFileData);
                }
                else
                {
                    statusInsert = timeSyncLogReport.AddImportFileData(importFileData);
                }

                if (statusInsert)
                {
                    result.Inserted = true;
                    result.ReportTimeSyncCount = timeSyncLogs.Count;

                    //foreach (var timeSyncLog in timeSyncLogs)
                    //{
                    //    timeSyncLog.TimeSync_Log_File_Id = importFileData.Id;
                    //    var insertStatus = timeSyncLogReport.AddReportTimeSyncLog(timeSyncLog);

                    //    if (insertStatus)
                    //    {
                    //        result.ReportTimeSyncSucceed++;
                    //    }
                    //    else
                    //    {
                    //        result.ReportTimeSyncError++;
                    //    }

                    //}

                    int success = 0, error = 0;
                    Parallel.ForEach(timeSyncLogs, new ParallelOptions { MaxDegreeOfParallelism = 20 }, timeSyncLog =>
                    {
                        timeSyncLog.TimeSync_Log_File_Id = importFileData.Id;
                        var insertStatus = timeSyncLogReport.AddReportTimeSyncLog(timeSyncLog);

                        if (insertStatus)
                        {
                            Interlocked.Increment(ref success);
                            //result.ReportTimeSyncSucceed++;
                        }
                        else
                        {
                            Interlocked.Increment(ref error);
                            //result.ReportTimeSyncError++;
                        }
                    });
                    result.ReportTimeSyncSucceed = success;
                    result.ReportTimeSyncError = error;

                }

                return result;
            }
        }


        public static class AccessLogParser
        {
            // ตัวอย่างรูปแบบ: 
            // 10.182.14.65 - - [08/Sep/2025:00:03:15 +0700] "HEAD / HTTP/1.1" 404 -
            private static readonly Regex LineRx = new(
                @"^(?<ip>\S+)\s+\S+\s+\S+\s+\[(?<ts>[^\]]+)\]\s+""(?<method>\S+)\s+(?<path>\S+)\s+(?<proto>[^""]+)""\s+(?<status>\d{3})\s+(?<size>\S+)",
                RegexOptions.Compiled
            );

            public static bool TryParse(string line, out TimeSyncLogEntry entry)
            {
                entry = null;
                if (string.IsNullOrWhiteSpace(line)) return false;

                var m = LineRx.Match(line);
                if (!m.Success) return false;

                // IP
                if (!IPAddress.TryParse(m.Groups["ip"].Value, out var ip))
                    return false;

                // Timestamp (เช่น 08/Sep/2025:00:03:15 +0700 -> แทรก ":" ให้เป็น +07:00)
                var tsRaw = m.Groups["ts"].Value;
                var tsNorm = NormalizeApacheTimestamp(tsRaw);
                if (!DateTime.TryParseExact(
                        tsNorm,
                        "dd/MMM/yyyy:HH:mm:ss zzz",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var timestamp))
                {
                    return false;
                }

                // Request line
                var method = m.Groups["method"].Value;
                var path = m.Groups["path"].Value;
                var proto = m.Groups["proto"].Value;

                // Status
                if (!int.TryParse(m.Groups["status"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var status))
                    return false;

                // Size ("-" => null)
                long? bytes = null;
                var sizeStr = m.Groups["size"].Value;
                if (sizeStr != "-" && long.TryParse(sizeStr, NumberStyles.None, CultureInfo.InvariantCulture, out var sizeVal))
                {
                    bytes = sizeVal;
                }

                entry = new TimeSyncLogEntry
                {
                    Ip = ip,
                    Timestamp = timestamp,
                    Method = method,
                    Path = path,
                    Protocol = proto,
                    Status = status,
                    Bytes = bytes
                };
                return true;
            }

            /*public static IEnumerable<TimeSyncLogEntry> ParseMany(IEnumerable<string> lines)
            {
                foreach (var line in lines)
                {
                    if (TryParse(line, out var e))
                        yield return e;
                }
            }*/

            private static string NormalizeApacheTimestamp(string ts)
            {
                // "08/Sep/2025:00:03:15 +0700" -> "08/Sep/2025:00:03:15 +07:00"
                var lastSpace = ts.LastIndexOf(' ');
                if (lastSpace < 0) return ts;

                var offset = ts[(lastSpace + 1)..]; // ส่วน +0700
                if (Regex.IsMatch(offset, @"^[+-]\d{4}$"))
                {
                    // แทรก ":" หลังสองหลักแรกของนาทีโซนเวลา
                    offset = offset.Insert(3, ":");  // +07:00
                    return ts[..(lastSpace + 1)] + offset;
                }
                return ts;
            }
        }
    }

    

}
