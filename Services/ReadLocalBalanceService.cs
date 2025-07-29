using MySql.Data.MySqlClient;
using SLA_Management.Models.BalanceModel;
using SLA_Management.Models.CassetteStatus;
using System.Data;
using System.Text;
using System.Text.Json;
using static Services.ReportCassetteBoxService;

namespace SLA_Management.Services
{

    public class ReadLocalBalanceService
    {
        private string _connectionString { get; set; }

        public ReadLocalBalanceService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool AddLocalBalance(LocalBalance localBalance)
        {

            MySqlConnection conn = new MySqlConnection(_connectionString);
            string sql = @"INSERT IGNORE INTO `local_balance`
                                    (`BALANCE_ID`,
                                    `TERM_ID`,
                                    `SERIAL_NUMBER`,
                                    `BALANCING_DATE`,
                                    `INITIAL_TYPE1000AQTY`,
                                    `INITIAL_TYPE1000BQTY`,
                                    `INITIAL_TYPE1000CQTY`,
                                    `INITIAL_TYPE1000DQTY`,
                                    `INITIAL_TYPE1000QTY`,
                                    `INITIAL_TYPE1000AMOUNT`,
                                    `INITIAL_TYPE500AQTY`,
                                    `INITIAL_TYPE500BQTY`,
                                    `INITIAL_TYPE500CQTY`,
                                    `INITIAL_TYPE500DQTY`,
                                    `INITIAL_TYPE500QTY`,
                                    `INITIAL_TYPE500AMOUNT`,
                                    `INITIAL_TYPE100AQTY`,
                                    `INITIAL_TYPE100BQTY`,
                                    `INITIAL_TYPE100CQTY`,
                                    `INITIAL_TYPE100DQTY`,
                                    `INITIAL_TYPE100QTY`,
                                    `INITIAL_TYPE100AMOUNT`,
                                    `DEPOSIT_TYPE1000QTY`,
                                    `DEPOSIT_TYPE1000AMOUNT`,
                                    `DEPOSIT_TYPE500QTY`,
                                    `DEPOSIT_TYPE500AMOUNT`,
                                    `DEPOSIT_TYPE100QTY`,
                                    `DEPOSIT_TYPE100AMOUNT`,
                                    `WITHDRAW_TYPE1000QTY`,
                                    `WITHDRAW_TYPE1000AMOUNT`,
                                    `WITHDRAW_TYPE500QTY`,
                                    `WITHDRAW_TYPE500AMOUNT`,
                                    `WITHDRAW_TYPE100QTY`,
                                    `WITHDRAW_TYPE100AMOUNT`,
                                    `BALANCE_TYPE1000QTY`,
                                    `BALANCE_TYPE1000AMOUNT`,
                                    `BALANCE_TYPE500QTY`,
                                    `BALANCE_TYPE500AMOUNT`,
                                    `BALANCE_TYPE100QTY`,
                                    `BALANCE_TYPE100AMOUNT`,
                                    `RETRACT_TYPE1000QTY`,
                                    `RETRACT_TYPE1000AMOUNT`,
                                    `RETRACT_TYPE500QTY`,
                                    `RETRACT_TYPE500AMOUNT`,
                                    `RETRACT_TYPE100QTY`,
                                    `RETRACT_TYPE100AMOUNT`,
                                    `RETRACT_TYPE_UNKNOWN_AMOUNT`,
                                    `REJECT_TYPE1000QTY`,
                                    `REJECT_TYPE1000AMOUNT`,
                                    `REJECT_TYPE500QTY`,
                                    `REJECT_TYPE500AMOUNT`,
                                    `REJECT_TYPE100QTY`,
                                    `REJECT_TYPE100AMOUNT`)
                                    VALUES
                                    (@BALANCE_ID,
                                    @TERM_ID,
                                    @SERIAL_NUMBER,
                                    @BALANCING_DATE,
                                    @INITIAL_TYPE1000AQTY,
                                    @INITIAL_TYPE1000BQTY,
                                    @INITIAL_TYPE1000CQTY,
                                    @INITIAL_TYPE1000DQTY,
                                    @INITIAL_TYPE1000QTY,
                                    @INITIAL_TYPE1000AMOUNT,
                                    @INITIAL_TYPE500AQTY,
                                    @INITIAL_TYPE500BQTY,
                                    @INITIAL_TYPE500CQTY,
                                    @INITIAL_TYPE500DQTY,
                                    @INITIAL_TYPE500QTY,
                                    @INITIAL_TYPE500AMOUNT,
                                    @INITIAL_TYPE100AQTY,
                                    @INITIAL_TYPE100BQTY,
                                    @INITIAL_TYPE100CQTY,
                                    @INITIAL_TYPE100DQTY,
                                    @INITIAL_TYPE100QTY,
                                    @INITIAL_TYPE100AMOUNT,
                                    @DEPOSIT_TYPE1000QTY,
                                    @DEPOSIT_TYPE1000AMOUNT,
                                    @DEPOSIT_TYPE500QTY,
                                    @DEPOSIT_TYPE500AMOUNT,
                                    @DEPOSIT_TYPE100QTY,
                                    @DEPOSIT_TYPE100AMOUNT,
                                    @WITHDRAW_TYPE1000QTY,
                                    @WITHDRAW_TYPE1000AMOUNT,
                                    @WITHDRAW_TYPE500QTY,
                                    @WITHDRAW_TYPE500AMOUNT,
                                    @WITHDRAW_TYPE100QTY,
                                    @WITHDRAW_TYPE100AMOUNT,
                                    @BALANCE_TYPE1000QTY,
                                    @BALANCE_TYPE1000AMOUNT,
                                    @BALANCE_TYPE500QTY,
                                    @BALANCE_TYPE500AMOUNT,
                                    @BALANCE_TYPE100QTY,
                                    @BALANCE_TYPE100AMOUNT,
                                    @RETRACT_TYPE1000QTY,
                                    @RETRACT_TYPE1000AMOUNT,
                                    @RETRACT_TYPE500QTY,
                                    @RETRACT_TYPE500AMOUNT,
                                    @RETRACT_TYPE100QTY,
                                    @RETRACT_TYPE100AMOUNT,
                                    @RETRACT_TYPE_UNKNOWN_AMOUNT,
                                    @REJECT_TYPE1000QTY,
                                    @REJECT_TYPE1000AMOUNT,
                                    @REJECT_TYPE500QTY,
                                    @REJECT_TYPE500AMOUNT,
                                    @REJECT_TYPE100QTY,
                                    @REJECT_TYPE100AMOUNT);
                                    ";

            try
            {
                conn.Open();
                MySqlCommand com = new MySqlCommand(sql);





                com.Parameters.AddWithValue("@BALANCE_ID", localBalance.BALANCE_ID);
                com.Parameters.AddWithValue("@TERM_ID", localBalance.TERM_ID);
                com.Parameters.AddWithValue("@SERIAL_NUMBER", localBalance.SERIAL_NUMBER);
                com.Parameters.AddWithValue("@BALANCING_DATE", localBalance.BALANCING_DATE);
                com.Parameters.AddWithValue("@INITIAL_TYPE1000AQTY", localBalance.INITIAL_TYPE1000AQTY);
                com.Parameters.AddWithValue("@INITIAL_TYPE1000BQTY", localBalance.INITIAL_TYPE1000BQTY);
                com.Parameters.AddWithValue("@INITIAL_TYPE1000CQTY", localBalance.INITIAL_TYPE1000CQTY);
                com.Parameters.AddWithValue("@INITIAL_TYPE1000DQTY", localBalance.INITIAL_TYPE1000DQTY);
                com.Parameters.AddWithValue("@INITIAL_TYPE1000QTY", localBalance.INITIAL_TYPE1000QTY);
                com.Parameters.AddWithValue("@INITIAL_TYPE1000AMOUNT", localBalance.INITIAL_TYPE1000AMOUNT);
                com.Parameters.AddWithValue("@INITIAL_TYPE500AQTY", localBalance.INITIAL_TYPE500AQTY);
                com.Parameters.AddWithValue("@INITIAL_TYPE500BQTY", localBalance.INITIAL_TYPE500BQTY);
                com.Parameters.AddWithValue("@INITIAL_TYPE500CQTY", localBalance.INITIAL_TYPE500CQTY);
                com.Parameters.AddWithValue("@INITIAL_TYPE500DQTY", localBalance.INITIAL_TYPE500DQTY);
                com.Parameters.AddWithValue("@INITIAL_TYPE500QTY", localBalance.INITIAL_TYPE500QTY);
                com.Parameters.AddWithValue("@INITIAL_TYPE500AMOUNT", localBalance.INITIAL_TYPE500AMOUNT);
                com.Parameters.AddWithValue("@INITIAL_TYPE100AQTY", localBalance.INITIAL_TYPE100AQTY);
                com.Parameters.AddWithValue("@INITIAL_TYPE100BQTY", localBalance.INITIAL_TYPE100BQTY);
                com.Parameters.AddWithValue("@INITIAL_TYPE100CQTY", localBalance.INITIAL_TYPE100CQTY);
                com.Parameters.AddWithValue("@INITIAL_TYPE100DQTY", localBalance.INITIAL_TYPE100DQTY);
                com.Parameters.AddWithValue("@INITIAL_TYPE100QTY", localBalance.INITIAL_TYPE100QTY);
                com.Parameters.AddWithValue("@INITIAL_TYPE100AMOUNT", localBalance.INITIAL_TYPE100AMOUNT);
                com.Parameters.AddWithValue("@DEPOSIT_TYPE1000QTY", localBalance.DEPOSIT_TYPE1000QTY);
                com.Parameters.AddWithValue("@DEPOSIT_TYPE1000AMOUNT", localBalance.DEPOSIT_TYPE1000AMOUNT);
                com.Parameters.AddWithValue("@DEPOSIT_TYPE500QTY", localBalance.DEPOSIT_TYPE500QTY);
                com.Parameters.AddWithValue("@DEPOSIT_TYPE500AMOUNT", localBalance.DEPOSIT_TYPE500AMOUNT);
                com.Parameters.AddWithValue("@DEPOSIT_TYPE100QTY", localBalance.DEPOSIT_TYPE100QTY);
                com.Parameters.AddWithValue("@DEPOSIT_TYPE100AMOUNT", localBalance.DEPOSIT_TYPE100AMOUNT);
                com.Parameters.AddWithValue("@WITHDRAW_TYPE1000QTY", localBalance.WITHDRAW_TYPE1000QTY);
                com.Parameters.AddWithValue("@WITHDRAW_TYPE1000AMOUNT", localBalance.WITHDRAW_TYPE1000AMOUNT);
                com.Parameters.AddWithValue("@WITHDRAW_TYPE500QTY", localBalance.WITHDRAW_TYPE500QTY);
                com.Parameters.AddWithValue("@WITHDRAW_TYPE500AMOUNT", localBalance.WITHDRAW_TYPE500AMOUNT);
                com.Parameters.AddWithValue("@WITHDRAW_TYPE100QTY", localBalance.WITHDRAW_TYPE100QTY);
                com.Parameters.AddWithValue("@WITHDRAW_TYPE100AMOUNT", localBalance.WITHDRAW_TYPE100AMOUNT);
                com.Parameters.AddWithValue("@BALANCE_TYPE1000QTY", localBalance.BALANCE_TYPE1000QTY);
                com.Parameters.AddWithValue("@BALANCE_TYPE1000AMOUNT", localBalance.BALANCE_TYPE1000AMOUNT);
                com.Parameters.AddWithValue("@BALANCE_TYPE500QTY", localBalance.BALANCE_TYPE500QTY);
                com.Parameters.AddWithValue("@BALANCE_TYPE500AMOUNT", localBalance.BALANCE_TYPE500AMOUNT);
                com.Parameters.AddWithValue("@BALANCE_TYPE100QTY", localBalance.BALANCE_TYPE100QTY);
                com.Parameters.AddWithValue("@BALANCE_TYPE100AMOUNT", localBalance.BALANCE_TYPE100AMOUNT);
                com.Parameters.AddWithValue("@RETRACT_TYPE1000QTY", localBalance.RETRACT_TYPE1000QTY);
                com.Parameters.AddWithValue("@RETRACT_TYPE1000AMOUNT", localBalance.RETRACT_TYPE1000AMOUNT);
                com.Parameters.AddWithValue("@RETRACT_TYPE500QTY", localBalance.RETRACT_TYPE500QTY);
                com.Parameters.AddWithValue("@RETRACT_TYPE500AMOUNT", localBalance.RETRACT_TYPE500AMOUNT);
                com.Parameters.AddWithValue("@RETRACT_TYPE100QTY", localBalance.RETRACT_TYPE100QTY);
                com.Parameters.AddWithValue("@RETRACT_TYPE100AMOUNT", localBalance.RETRACT_TYPE100AMOUNT);
                com.Parameters.AddWithValue("@RETRACT_TYPE_UNKNOWN_AMOUNT", localBalance.RETRACT_TYPE_UNKNOWN_AMOUNT);
                com.Parameters.AddWithValue("@REJECT_TYPE1000QTY", localBalance.REJECT_TYPE1000QTY);
                com.Parameters.AddWithValue("@REJECT_TYPE1000AMOUNT", localBalance.REJECT_TYPE1000AMOUNT);
                com.Parameters.AddWithValue("@REJECT_TYPE500QTY", localBalance.REJECT_TYPE500QTY);
                com.Parameters.AddWithValue("@REJECT_TYPE500AMOUNT", localBalance.REJECT_TYPE500AMOUNT);
                com.Parameters.AddWithValue("@REJECT_TYPE100QTY", localBalance.REJECT_TYPE100QTY);
                com.Parameters.AddWithValue("@REJECT_TYPE100AMOUNT", localBalance.REJECT_TYPE100AMOUNT);




                com.Connection = conn;

                com.ExecuteNonQuery();

                int rowsAffected = com.ExecuteNonQuery();
                return rowsAffected > 0;

            }
            catch (Exception ex)
            {
                return false;
                // Log.Error(ex, "AddLocalBalance Error : ");

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

            }


        }

        public bool AddImportFileData(ImportFileData cassetteEventFile)
        {
            bool result = false;
            MySqlConnection conn = new MySqlConnection(_connectionString);
            string sql = @"INSERT INTO `import_file_data`(`Id`,`Name_File`,`Upload_By`,`Upload_Date`,`Data_Date`,`Import_Data_Rroject`) VALUES (@Id,@Name_File,@Upload_By,@Upload_Date,@Data_Date,@Import_Data_Rroject);";
            try
            {
                conn.Open();
                MySqlCommand com = new MySqlCommand(sql);
                com.Parameters.AddWithValue("@Id", cassetteEventFile.Id);
                com.Parameters.AddWithValue("@Name_File", cassetteEventFile.Name_File);
                com.Parameters.AddWithValue("@Upload_By", cassetteEventFile.Upload_By);
                com.Parameters.AddWithValue("@Upload_Date", cassetteEventFile.Upload_Date);
                com.Parameters.AddWithValue("@Data_Date", cassetteEventFile.Data_Date);
                com.Parameters.AddWithValue("@Import_Data_Rroject", cassetteEventFile.Import_Data_Project);


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
                            where Import_Data_Rroject = @Import_Data_Rroject  and  Data_Date between @start and @end;";
            try
            {
                conn.Open();
                MySqlCommand com = new MySqlCommand(sql);
                com.Parameters.AddWithValue("@start", start);
                com.Parameters.AddWithValue("@end", end);
                com.Parameters.AddWithValue("@Import_Data_Rroject", projectName);
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
                // Log.Error(ex, "DeleteImportFileData Error : ");

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

            }


        }

        public class ReadFile
        {
            public static List<LocalBalance> ReadData(Stream fileStream)
            {
                List<LocalBalance> data = new List<LocalBalance>();
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
                            data.Add(ConvertToLocalBalance(line));
                        }
                        catch (Exception ex)
                        {
                            //Log.Error($"ReadData Error {line} : {ex}");
                        }

                    }
                    sr.Close();
                    fileStream.Close();
                }

                return data;
            }

            private static LocalBalance ConvertToLocalBalance(string data)
            {
                var test = JsonSerializer.Deserialize<LocalBalance>(data);

                if (test == null) return null;
                return test;

            }
        }

        public class InsertResult
        {
            public int InsertedCount { get; set; } = 0;
            public int FileCount { get; set; } = 0;
        }

        public class InsertLocalBalance
        {
            private static string projectNameConfig = "ReadLocalBalance";
            public static InsertResult Insert(List<Stream> files, string connectionStringReportConfig, string username)
            {
                InsertResult result = new InsertResult();
                List<LocalBalance> localBalances = new List<LocalBalance>();

                foreach (var file in files)
                {
                    var data = ReadFile.ReadData(file);
                    if (data.Count != 0)
                    {
                        localBalances.AddRange(data);
                        result.FileCount++; // นับเฉพาะไฟล์ที่มีข้อมูล
                    }
                }

                if (localBalances.Count != 0)
                {
                    DateTime minBALANCING_DATE = localBalances.Min(entry => entry.BALANCING_DATE);

                    ReadLocalBalanceService readLocalBalanceService = new ReadLocalBalanceService(connectionStringReportConfig);

                    var textFileReport = $"{Guid.NewGuid().ToString()}_{minBALANCING_DATE:yyyyMMdd}.txt";

                    var existingImportFiles = readLocalBalanceService.GetImportFileDataByDate(minBALANCING_DATE, projectNameConfig);
                    foreach (var fileRecord in existingImportFiles)
                    {
                        readLocalBalanceService.DeleteImportFileData(fileRecord.Id);
                    }

                    var importFileData = new ImportFileData()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name_File = textFileReport,
                        Upload_By = username,
                        Upload_Date = DateTime.Now,
                        Data_Date = minBALANCING_DATE,
                        Import_Data_Project = projectNameConfig
                    };

                    readLocalBalanceService.AddImportFileData(importFileData);

                    foreach (var localBalance in localBalances)
                    {
                        if (readLocalBalanceService.AddLocalBalance(localBalance))
                        {
                            result.InsertedCount++;
                        }
                    }
                }

                return result;
            }

        }

    }
}

