using MySql.Data.MySqlClient;
using SLA_Management.Models.BalanceModel;
using SLA_Management.Models.CassetteStatus;
using System.Data;
using System.Text;
using System.Text.Json;
using static Services.ReportCassetteBoxService;

namespace SLA_Management.Services
{

    public class ReadLocalBalanceService : ImportFileService
    {
        private string _connectionString { get; set; }

        public ReadLocalBalanceService(string connectionString) : base(connectionString)
        {
            _connectionString = connectionString;
        }

        public bool AddLocalBalance(LocalBalance localBalance)
        {
            bool result = false;
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
                result = true;


            }
            catch (Exception ex)
            {
                // Log.Error(ex, "AddLocalBalance Error : ");

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

        public bool AddLocalBalances(List<LocalBalance> localBalances)
        {
            if (localBalances == null || localBalances.Count == 0)
            {
                return true; // Nothing to insert
            }

            bool result = false;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    MySqlTransaction transaction = conn.BeginTransaction();


                    string singleRowSql = @"INSERT IGNORE INTO `local_balance`
                                        (`BALANCE_ID`, `TERM_ID`, `SERIAL_NUMBER`, `BALANCING_DATE`,
                                         `INITIAL_TYPE1000AQTY`, `INITIAL_TYPE1000BQTY`, `INITIAL_TYPE1000CQTY`, `INITIAL_TYPE1000DQTY`, `INITIAL_TYPE1000QTY`, `INITIAL_TYPE1000AMOUNT`,
                                         `INITIAL_TYPE500AQTY`, `INITIAL_TYPE500BQTY`, `INITIAL_TYPE500CQTY`, `INITIAL_TYPE500DQTY`, `INITIAL_TYPE500QTY`, `INITIAL_TYPE500AMOUNT`,
                                         `INITIAL_TYPE100AQTY`, `INITIAL_TYPE100BQTY`, `INITIAL_TYPE100CQTY`, `INITIAL_TYPE100DQTY`, `INITIAL_TYPE100QTY`, `INITIAL_TYPE100AMOUNT`,
                                         `DEPOSIT_TYPE1000QTY`, `DEPOSIT_TYPE1000AMOUNT`, `DEPOSIT_TYPE500QTY`, `DEPOSIT_TYPE500AMOUNT`, `DEPOSIT_TYPE100QTY`, `DEPOSIT_TYPE100AMOUNT`,
                                         `WITHDRAW_TYPE1000QTY`, `WITHDRAW_TYPE1000AMOUNT`, `WITHDRAW_TYPE500QTY`, `WITHDRAW_TYPE500AMOUNT`, `WITHDRAW_TYPE100QTY`, `WITHDRAW_TYPE100AMOUNT`,
                                         `BALANCE_TYPE1000QTY`, `BALANCE_TYPE1000AMOUNT`, `BALANCE_TYPE500QTY`, `BALANCE_TYPE500AMOUNT`, `BALANCE_TYPE100QTY`, `BALANCE_TYPE100AMOUNT`,
                                         `RETRACT_TYPE1000QTY`, `RETRACT_TYPE1000AMOUNT`, `RETRACT_TYPE500QTY`, `RETRACT_TYPE500AMOUNT`, `RETRACT_TYPE100QTY`, `RETRACT_TYPE100AMOUNT`,
                                         `RETRACT_TYPE_UNKNOWN_AMOUNT`,
                                         `REJECT_TYPE1000QTY`, `REJECT_TYPE1000AMOUNT`, `REJECT_TYPE500QTY`, `REJECT_TYPE500AMOUNT`, `REJECT_TYPE100QTY`, `REJECT_TYPE100AMOUNT`)
                                        VALUES
                                        (@BALANCE_ID, @TERM_ID, @SERIAL_NUMBER, @BALANCING_DATE,
                                         @INITIAL_TYPE1000AQTY, @INITIAL_TYPE1000BQTY, @INITIAL_TYPE1000CQTY, @INITIAL_TYPE1000DQTY, @INITIAL_TYPE1000QTY, @INITIAL_TYPE1000AMOUNT,
                                         @INITIAL_TYPE500AQTY, @INITIAL_TYPE500BQTY, @INITIAL_TYPE500CQTY, @INITIAL_TYPE500DQTY, @INITIAL_TYPE500QTY, @INITIAL_TYPE500AMOUNT,
                                         @INITIAL_TYPE100AQTY, @INITIAL_TYPE100BQTY, @INITIAL_TYPE100CQTY, @INITIAL_TYPE100DQTY, @INITIAL_TYPE100QTY, @INITIAL_TYPE100AMOUNT,
                                         @DEPOSIT_TYPE1000QTY, @DEPOSIT_TYPE1000AMOUNT, @DEPOSIT_TYPE500QTY, @DEPOSIT_TYPE500AMOUNT, @DEPOSIT_TYPE100QTY, @DEPOSIT_TYPE100AMOUNT,
                                         @WITHDRAW_TYPE1000QTY, @WITHDRAW_TYPE1000AMOUNT, @WITHDRAW_TYPE500QTY, @WITHDRAW_TYPE500AMOUNT, @WITHDRAW_TYPE100QTY, @WITHDRAW_TYPE100AMOUNT,
                                         @BALANCE_TYPE1000QTY, @BALANCE_TYPE1000AMOUNT, @BALANCE_TYPE500QTY, @BALANCE_TYPE500AMOUNT, @BALANCE_TYPE100QTY, @BALANCE_TYPE100AMOUNT,
                                         @RETRACT_TYPE1000QTY, @RETRACT_TYPE1000AMOUNT, @RETRACT_TYPE500QTY, @RETRACT_TYPE500AMOUNT, @RETRACT_TYPE100QTY, @RETRACT_TYPE100AMOUNT,
                                         @RETRACT_TYPE_UNKNOWN_AMOUNT,
                                         @REJECT_TYPE1000QTY, @REJECT_TYPE1000AMOUNT, @REJECT_TYPE500QTY, @REJECT_TYPE500AMOUNT, @REJECT_TYPE100QTY, @REJECT_TYPE100AMOUNT)";



                    foreach (var lb in localBalances)
                    {
                        MySqlCommand command = new MySqlCommand(singleRowSql, conn, transaction);

                        command.Parameters.AddWithValue("@BALANCE_ID", lb.BALANCE_ID);
                        command.Parameters.AddWithValue("@TERM_ID", lb.TERM_ID);
                        command.Parameters.AddWithValue("@SERIAL_NUMBER", lb.SERIAL_NUMBER);
                        command.Parameters.AddWithValue("@BALANCING_DATE", lb.BALANCING_DATE);
                        command.Parameters.AddWithValue("@INITIAL_TYPE1000AQTY", lb.INITIAL_TYPE1000AQTY);
                        command.Parameters.AddWithValue("@INITIAL_TYPE1000BQTY", lb.INITIAL_TYPE1000BQTY);
                        command.Parameters.AddWithValue("@INITIAL_TYPE1000CQTY", lb.INITIAL_TYPE1000CQTY);
                        command.Parameters.AddWithValue("@INITIAL_TYPE1000DQTY", lb.INITIAL_TYPE1000DQTY);
                        command.Parameters.AddWithValue("@INITIAL_TYPE1000QTY", lb.INITIAL_TYPE1000QTY);
                        command.Parameters.AddWithValue("@INITIAL_TYPE1000AMOUNT", lb.INITIAL_TYPE1000AMOUNT);
                        command.Parameters.AddWithValue("@INITIAL_TYPE500AQTY", lb.INITIAL_TYPE500AQTY);
                        command.Parameters.AddWithValue("@INITIAL_TYPE500BQTY", lb.INITIAL_TYPE500BQTY);
                        command.Parameters.AddWithValue("@INITIAL_TYPE500CQTY", lb.INITIAL_TYPE500CQTY);
                        command.Parameters.AddWithValue("@INITIAL_TYPE500DQTY", lb.INITIAL_TYPE500DQTY);
                        command.Parameters.AddWithValue("@INITIAL_TYPE500QTY", lb.INITIAL_TYPE500QTY);
                        command.Parameters.AddWithValue("@INITIAL_TYPE500AMOUNT", lb.INITIAL_TYPE500AMOUNT);
                        command.Parameters.AddWithValue("@INITIAL_TYPE100AQTY", lb.INITIAL_TYPE100AQTY);
                        command.Parameters.AddWithValue("@INITIAL_TYPE100BQTY", lb.INITIAL_TYPE100BQTY);
                        command.Parameters.AddWithValue("@INITIAL_TYPE100CQTY", lb.INITIAL_TYPE100CQTY);
                        command.Parameters.AddWithValue("@INITIAL_TYPE100DQTY", lb.INITIAL_TYPE100DQTY);
                        command.Parameters.AddWithValue("@INITIAL_TYPE100QTY", lb.INITIAL_TYPE100QTY);
                        command.Parameters.AddWithValue("@INITIAL_TYPE100AMOUNT", lb.INITIAL_TYPE100AMOUNT);
                        command.Parameters.AddWithValue("@DEPOSIT_TYPE1000QTY", lb.DEPOSIT_TYPE1000QTY);
                        command.Parameters.AddWithValue("@DEPOSIT_TYPE1000AMOUNT", lb.DEPOSIT_TYPE1000AMOUNT);
                        command.Parameters.AddWithValue("@DEPOSIT_TYPE500QTY", lb.DEPOSIT_TYPE500QTY);
                        command.Parameters.AddWithValue("@DEPOSIT_TYPE500AMOUNT", lb.DEPOSIT_TYPE500AMOUNT);
                        command.Parameters.AddWithValue("@DEPOSIT_TYPE100QTY", lb.DEPOSIT_TYPE100QTY);
                        command.Parameters.AddWithValue("@DEPOSIT_TYPE100AMOUNT", lb.DEPOSIT_TYPE100AMOUNT);
                        command.Parameters.AddWithValue("@WITHDRAW_TYPE1000QTY", lb.WITHDRAW_TYPE1000QTY);
                        command.Parameters.AddWithValue("@WITHDRAW_TYPE1000AMOUNT", lb.WITHDRAW_TYPE1000AMOUNT);
                        command.Parameters.AddWithValue("@WITHDRAW_TYPE500QTY", lb.WITHDRAW_TYPE500QTY);
                        command.Parameters.AddWithValue("@WITHDRAW_TYPE500AMOUNT", lb.WITHDRAW_TYPE500AMOUNT);
                        command.Parameters.AddWithValue("@WITHDRAW_TYPE100QTY", lb.WITHDRAW_TYPE100QTY);
                        command.Parameters.AddWithValue("@WITHDRAW_TYPE100AMOUNT", lb.WITHDRAW_TYPE100AMOUNT);
                        command.Parameters.AddWithValue("@BALANCE_TYPE1000QTY", lb.BALANCE_TYPE1000QTY);
                        command.Parameters.AddWithValue("@BALANCE_TYPE1000AMOUNT", lb.BALANCE_TYPE1000AMOUNT);
                        command.Parameters.AddWithValue("@BALANCE_TYPE500QTY", lb.BALANCE_TYPE500QTY);
                        command.Parameters.AddWithValue("@BALANCE_TYPE500AMOUNT", lb.BALANCE_TYPE500AMOUNT);
                        command.Parameters.AddWithValue("@BALANCE_TYPE100QTY", lb.BALANCE_TYPE100QTY);
                        command.Parameters.AddWithValue("@BALANCE_TYPE100AMOUNT", lb.BALANCE_TYPE100AMOUNT);
                        command.Parameters.AddWithValue("@RETRACT_TYPE1000QTY", lb.RETRACT_TYPE1000QTY);
                        command.Parameters.AddWithValue("@RETRACT_TYPE1000AMOUNT", lb.RETRACT_TYPE1000AMOUNT);
                        command.Parameters.AddWithValue("@RETRACT_TYPE500QTY", lb.RETRACT_TYPE500QTY);
                        command.Parameters.AddWithValue("@RETRACT_TYPE500AMOUNT", lb.RETRACT_TYPE500AMOUNT);
                        command.Parameters.AddWithValue("@RETRACT_TYPE100QTY", lb.RETRACT_TYPE100QTY);
                        command.Parameters.AddWithValue("@RETRACT_TYPE100AMOUNT", lb.RETRACT_TYPE100AMOUNT);
                        command.Parameters.AddWithValue("@RETRACT_TYPE_UNKNOWN_AMOUNT", lb.RETRACT_TYPE_UNKNOWN_AMOUNT);
                        command.Parameters.AddWithValue("@REJECT_TYPE1000QTY", lb.REJECT_TYPE1000QTY);
                        command.Parameters.AddWithValue("@REJECT_TYPE1000AMOUNT", lb.REJECT_TYPE1000AMOUNT);
                        command.Parameters.AddWithValue("@REJECT_TYPE500QTY", lb.REJECT_TYPE500QTY);
                        command.Parameters.AddWithValue("@REJECT_TYPE500AMOUNT", lb.REJECT_TYPE500AMOUNT);
                        command.Parameters.AddWithValue("@REJECT_TYPE100QTY", lb.REJECT_TYPE100QTY);
                        command.Parameters.AddWithValue("@REJECT_TYPE100AMOUNT", lb.REJECT_TYPE100AMOUNT);

                        command.ExecuteNonQuery();
                    }


                    transaction.Commit();
                    result = true;
                }
                catch (Exception ex)
                {
                    //Log.Error(ex, "AddLocalBalances Error : ");

                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }

                }
            }
            return result;
        }

        public class InsertResult
        {
            public bool Inserted { get; set; }
            public int LocalBalanceCount { get; set; }


            public int LocalBalanceInsertSucceed { get; set; }


            public int LocalBalanceInsertError { get; set; }



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
                            // Log.Error($"ReadData Error {line} : {ex}");
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


        public class InsertLocalBalance
        {
            private static string projectNameConfig = "ReadLocalBalance";
            public static InsertResult Insert(List<Stream> files, string connectionStringReportConfig, string username)
            {
                var result = new InsertResult
                {
                    Inserted = false,
                    LocalBalanceCount = 0,
                    LocalBalanceInsertSucceed = 0,
                    LocalBalanceInsertError = 0

                };
                var nameFile = new List<string>();
                List<LocalBalance> localBalances = new List<LocalBalance>();
                foreach (var file in files)
                {
                    var data = ReadFile.ReadData(file);
                    if (data.Count != 0)
                    {
                        localBalances.AddRange(data);

                        if (file is FileStream fileStream)
                        {
                            nameFile.Add(fileStream.Name);
                        }
                        else
                        {
                            nameFile.Add($"{Guid.NewGuid().ToString()}.txt");

                        }

                    }
                }

                if (localBalances.Count == 0)
                    return result;

                if (localBalances.Count != 0)
                {
                    DateTime minBALANCING_DATE = localBalances.Min(entry => entry.BALANCING_DATE);

                    ReadLocalBalanceService readLocalBalanceService = new ReadLocalBalanceService(connectionStringReportConfig);



                    //var textFileReport = $"{Guid.NewGuid().ToString()}_{minBALANCING_DATE.ToString("yyyyMMdd")}.txt";
                    var textFileReport = string.Join(", ", nameFile);

                    var importFileDataBalanceService = readLocalBalanceService.GetFirstImportFileDataByDate(minBALANCING_DATE, projectNameConfig);


                    var importFileData = new ImportFileData()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name_File = textFileReport,
                        Upload_By = username,
                        Upload_Date = DateTime.Now,
                        Data_Date = minBALANCING_DATE,
                        Import_Data_Project = projectNameConfig
                    };

                    bool statusInsert = false;
                    if (importFileDataBalanceService != null)
                    {
                        importFileData.Id = importFileDataBalanceService.Id;
                        readLocalBalanceService.UpdateImportFileData(importFileData);
                    }
                    else
                    {
                        statusInsert = readLocalBalanceService.AddImportFileData(importFileData);
                    }

                    if (statusInsert)
                    {
                        result.Inserted = true;
                        result.LocalBalanceCount = localBalances.Count;

                        foreach (var localBalance in localBalances)
                        {
                            var insertStatus = readLocalBalanceService.AddLocalBalance(localBalance);

                            if (insertStatus)
                            {
                                result.LocalBalanceInsertSucceed++;
                            }
                            else
                            {
                                result.LocalBalanceInsertError++;
                            }
                        }


                    }

                }



                return result;

            }
        }

    }
}

