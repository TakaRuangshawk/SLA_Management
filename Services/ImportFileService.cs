using MySql.Data.MySqlClient;
using SLA_Management.Models.CassetteStatus;
using System.Data;
using System.Text.Json;

namespace SLA_Management.Services
{

    public class ImportFileService
    {
        private string _connectionString { get; set; }

        public ImportFileService(string connectionString)
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

        public bool UpdateImportFileData(ImportFileData cassetteEventFile)
        {
            bool result = false;
            MySqlConnection conn = new MySqlConnection(_connectionString);
            string sql = @"UPDATE `import_file_data`
                            SET
                            `Name_File` = @Name_File,
                            `Upload_By` = @Upload_By,
                            `Upload_Date` = @Upload_Date,
                            `Data_Date` = @Data_Date,
                            `Import_Data_Project` = @Import_Data_Project
                            WHERE `Id` = @Id;";
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
                //Log.Error(ex, "UpdateImportFileData Error : ");

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
            string sql = @"DELETE FROM `import_file_data` WHERE Id = @Id;";
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
                //Log.Error(ex, "DeleteImportFileData Error : ");

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
        public ImportFileData GetImportFileDataLatest(string projectName)
        {

            ImportFileData result = null;
            MySqlConnection conn = new MySqlConnection(_connectionString);
            string sql = @"SELECT 
                              JSON_OBJECT(	'Id', Id,
				                            'Name_File', Name_File,
                                            'Upload_By', Upload_By,
                                            'Upload_Date', DATE_FORMAT(Upload_Date, '%Y-%m-%dT%H:%i:%sZ') ,
                                            'Data_Date',DATE_FORMAT(Data_Date, '%Y-%m-%dT%H:%i:%sZ') ) AS json_result
                            FROM import_file_data  
                            where Import_Data_Project = @Import_Data_Project 
                            ORDER BY Data_Date DESC ,Upload_Date DESC
                            LIMIT 1;";
            try
            {
                conn.Open();
                MySqlCommand com = new MySqlCommand(sql);
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
                        result = JsonSerializer.Deserialize<ImportFileData>(jsonText);
                         
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

        public ImportFileData GetFirstImportFileDataByDate(DateTime data, string projectName)
        {
            ImportFileData result = null;
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
                List<ImportFileData> importFileDatas = new List<ImportFileData>();
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
                        importFileDatas.Add(item);
                    }
                }
                if (importFileDatas.Count != 0)
                {
                    result = importFileDatas.OrderBy(i => i.Data_Date).First();
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


    }
}

