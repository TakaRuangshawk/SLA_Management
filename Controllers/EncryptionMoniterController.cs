using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using Serilog;
using SLA_Management.Commons;
using SLA_Management.Data;
using SLA_Management.Models.EncryptionMoniterModel;
using System.Data;

namespace SLA_Management.Controllers
{
    public class EncryptionMoniterController : Controller
    {
        private IConfiguration _myConfiguration { get; set; }
        private static string sqlReport { get; set; }
        public static List<DeviceEncryption> deviceEncryptions { get; set; }

        public ConnectMySQL connectMySQL { get; set; }


        public EncryptionMoniterController(IConfiguration configuration)
        {
            _myConfiguration = configuration;
            sqlReport = configuration.GetValue<string>("ConnectString_MySQL:FullNameConnection");
            if (deviceEncryptions == null)
            {
                deviceEncryptions = new List<DeviceEncryption>();
            }
            
            connectMySQL = new ConnectMySQL(sqlReport);

        }

        [HttpGet]
        public IActionResult Index()
        {

            ViewBag.EncryptionVersion = GetEncryptionVersion();
            ViewBag.CurrentTID = GetDevices();
            ViewBag.TerminalType = GetDeviceType();
            //ViewBag.pageSize = maxRows.HasValue ? maxRows.Value : 50;
            //ViewBag.ExportDataFile = filter;
            //ViewBag.TERM_ID = filter.term_id;
            //ViewBag.EncryptionVersionItem = filter.encryption_version_item;
            /*ViewBag.CurrentFr = filter.forDateTime.HasValue ? filter.forDateTime : null;
            ViewBag.CurrentTo = filter.toDateTime.HasValue ? filter.toDateTime : null;*/
            return View();
        }

        [HttpGet]
        public IActionResult GetDeviceEncryption(SearchData SearchData)
        {
            
            try
            {
                
                MySqlCommand mySqlCommand = new MySqlCommand();
                string sql = @"SELECT fv_device_info.TERM_ID as terminal_id,fv_device_info.TERM_NAME as terminal_name, device_encryption.* 
                                FROM device_encryption 
                                LEFT JOIN fv_device_info 
                                ON device_encryption.serial_no = fv_device_info.TERM_SEQ
                                where (fv_device_info.STATUS = 'use' or fv_device_info.STATUS = 'roustop')";

                if (SearchData.serial_no != null ||
                    SearchData.encryption_version != null ||
                    SearchData.encryption_status != null ||
                    SearchData.fromdate != new DateTime() ||
                    SearchData.todate != new DateTime() ||
                    SearchData.agent_status != null ||
                     SearchData.terminal_type != null)
                {
                    sql += " and ";
                    List<string> search = new List<string>();
                    if (SearchData.serial_no != null)
                    {
                        search.Add("device_encryption.serial_no = @serial_no");
                        mySqlCommand.Parameters.AddWithValue("@serial_no", SearchData.serial_no);
                    }
                    if (SearchData.encryption_version != null)
                    {
                        search.Add("device_encryption.encryption_version = @encryption_version");
                        if (SearchData.encryption_version == "null")
                        {
                            mySqlCommand.Parameters.AddWithValue("@encryption_version", "");
                        }
                        else
                        {
                            mySqlCommand.Parameters.AddWithValue("@encryption_version", SearchData.encryption_version);
                        }
                        
                    }
                    if (SearchData.encryption_status != null)
                    {
                        search.Add("device_encryption.encryption_status = @encryption_status");
                        mySqlCommand.Parameters.AddWithValue("@encryption_status", SearchData.encryption_status);
                    }
                    if (SearchData.agent_status != null)
                    {
                        search.Add("device_encryption.agent_status = @agent_status");
                        mySqlCommand.Parameters.AddWithValue("@agent_status", SearchData.agent_status);
                    }


                    if (SearchData.terminal_type != null)
                    {
                        search.Add("fv_device_info.BRAND_ID = @terminal_type");
                        mySqlCommand.Parameters.AddWithValue("@terminal_type", SearchData.terminal_type);
                    }





                    if ((SearchData.fromdate != new DateTime() && SearchData.todate != new DateTime())
                        && (SearchData.todate >= SearchData.fromdate))
                    {
                        search.Add("device_encryption.update_datetime BETWEEN @fromdate and @todate");
                        mySqlCommand.Parameters.AddWithValue("@fromdate", SearchData.fromdate);
                        mySqlCommand.Parameters.AddWithValue("@todate", SearchData.todate);
                    }
                    

                    sql += string.Join(" and ", search) + ";";
                }
                if (SearchData.sort != null)
                {
                    switch (SearchData.sort)
                    {
                        case "Terminal No":
                            sql += " order by fv_device_info.TERM_ID";
                            break;
                        case "Branch No":
                            sql += " order by fv_device_info.BRAND_ID";
                            break;
                        case "Serial No":
                            sql += " order by fv_device_info.TERM_SEQ";
                            break;
                        case "Status":
                            sql += " order by device_encryption.encryption_status";
                            break;
                    }


                }
                mySqlCommand.CommandText = sql;

                deviceEncryptions = ConvertDataTableToModel.ConvertDataTable<DeviceEncryption>(connectMySQL.GetDatatable(mySqlCommand));

                
                return Json(new { devices = RangeFilter<DeviceEncryption>(deviceEncryptions, 1, SearchData.maxRows), sumtotal = deviceEncryptions.Count() });
            }
            catch (Exception ex)
            {
                Log.Error($"GetDeviceEncryption Error : {ex}");
                return Json(new { devices = RangeFilter<DeviceEncryption>(deviceEncryptions, 1, SearchData.maxRows), sumtotal = deviceEncryptions.Count() });
            }


        }
        
        [HttpGet]
        public List<DeviceEncryption> GetPage(int page, int max_row)
        {
            try
            {
                
                return RangeFilter<DeviceEncryption>(deviceEncryptions, page, max_row);
            }
            catch (Exception ex)
            {
                Log.Error($"EncryptionMoniterController.GetPage Error : {ex}");
                return new List<DeviceEncryption>();
            }

        }


        [HttpPost]
        public IActionResult ExportRecordtoExcel()
        {
            //add test data
            List<DeviceEncryption> obj = new List<DeviceEncryption>();

            if (deviceEncryptions != null)
            {
                obj = deviceEncryptions;
            }

            //var str = ModelToBit(obj);

            DateTime dateNow = DateTime.Now;
            string fileName = "Encryption" + dateNow.Year.ToString("0000") + dateNow.Month.ToString("00") + dateNow.Day.ToString("00");





            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                // Write header row
                worksheet.Cells[1, 1].Value = "Row";
                worksheet.Cells[1, 2].Value = "Serial No";
                worksheet.Cells[1, 3].Value = "Terminal Id";
                worksheet.Cells[1, 4].Value = "Terminal Name";
                worksheet.Cells[1, 5].Value = "Encryption Version";

                worksheet.Cells[1, 6].Value = "Encryption Status";
                worksheet.Cells[1, 7].Value = "Agent Status";
                worksheet.Cells[1, 8].Value = "Update Datetime";

                // Write data rows
                var rowIndex = 2;

                foreach (var item in obj)
                {
                    worksheet.Cells[rowIndex, 1].Value = rowIndex - 1;
                    worksheet.Cells[rowIndex, 2].Value = item.serial_no;
                    worksheet.Cells[rowIndex, 3].Value = item.terminal_id;
                    worksheet.Cells[rowIndex, 4].Value = item.terminal_name;

                    worksheet.Cells[rowIndex, 5].Value = item.encryption_version;
                    worksheet.Cells[rowIndex, 6].Value = item.encryption_status;
                    worksheet.Cells[rowIndex, 7].Value = item.agent_status;
                    worksheet.Cells[rowIndex, 8].Value = item.update_datetime.ToString("yyyy-MM-dd HH:mm:ss");

                    rowIndex++;
                }

                // Auto fit columns for better readability
                worksheet.Cells.AutoFitColumns();

                // Convert the Excel package to a byte array
                var fileContents = package.GetAsByteArray();

                // Set the content type and file name for the response
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + ".xlsx");
            }





        }
        
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

        public List<DeviceNo> GetDevices()
        {
            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand();
                mySqlCommand.CommandText = "SELECT TERM_ID,TERM_SEQ,TERM_NAME FROM fv_device_info  where STATUS = 'use' or STATUS = 'roustop';";
                return ConvertDataTableToModel.ConvertDataTable<DeviceNo>(connectMySQL.GetDatatable(mySqlCommand));
            }
            catch (Exception ex)
            {
                Log.Error($"GetDevices Error : {ex}");
                return new List<DeviceNo>();
            }

        }

        public List<string> GetEncryptionVersion()
        {
            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand();
                mySqlCommand.CommandText = "SELECT encryption_version FROM device_encryption group by encryption_version;";
                var data = connectMySQL.GetDatatable(mySqlCommand);
                return data.AsEnumerable().Select(q =>  q[0] == "" ? "null" : q[0].ToString()).ToList();
            }
            catch (Exception ex)
            {
                Log.Error($"GetEncryptionVersion Error : {ex}");
                return new List<string>();
            }

        }


        public List<DeviceType> GetDeviceType()
        {
            try
            {
                string removeText = "GRG_";
                string replaceText = "";
                MySqlCommand mySqlCommand = new MySqlCommand();
                mySqlCommand.CommandText = "SELECT BRAND_ID as deviceType,REPLACE(BRAND_ID,@removeText,@replaceText) as deviceTypeName FROM gsb_logview.fv_device_info group by BRAND_ID;";
                mySqlCommand.Parameters.AddWithValue("@removeText", removeText);
                mySqlCommand.Parameters.AddWithValue("@replaceText", replaceText);
                return ConvertDataTableToModel.ConvertDataTable<DeviceType>(connectMySQL.GetDatatable(mySqlCommand));
            }
            catch (Exception ex)
            {
                Log.Error($"GetDevices Error : {ex}");
                return new List<DeviceType>();
            }

        }
    }
}
