using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
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
        public IActionResult Index(P_Search_EncryptionMoniter filter, int? maxRows)
        {

            ViewBag.EncryptionVersion = GetEncryptionVersion();
            ViewBag.CurrentTID = GetDevices();
            ViewBag.pageSize = maxRows.HasValue ? maxRows.Value : 50;
            ViewBag.ExportDataFile = filter;
            ViewBag.TERM_ID = filter.term_id;
            ViewBag.EncryptionVersionItem = filter.encryption_version_item;
            ViewBag.CurrentFr = filter.forDateTime.HasValue ? filter.forDateTime : null;
            ViewBag.CurrentTo = filter.toDateTime.HasValue ? filter.toDateTime : null;
            return View();
        }

        [HttpPost]
        public IActionResult GetDeviceEncryption(Teejama TestData)
        {
            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand();
                mySqlCommand.CommandText = "SELECT fv_device_info.TERM_ID as terminal_id,fv_device_info.TERM_NAME as terminal_name, device_encryption.* FROM device_encryption LEFT JOIN fv_device_info ON device_encryption.serial_no = fv_device_info.TERM_SEQ;";

                deviceEncryptions = ConvertDataTableToModel.ConvertDataTable<DeviceEncryption>(connectMySQL.GetDatatable(mySqlCommand));

                
                return Json(new { devices = RangeFilter<DeviceEncryption>(deviceEncryptions, 1, 50), sumtotal = deviceEncryptions.Count() });
            }
            catch (Exception ex)
            {
                Log.Error($"GetDeviceEncryption Error : {ex}");
                return Json(new { devices = RangeFilter<DeviceEncryption>(deviceEncryptions, 1, 50), sumtotal = deviceEncryptions.Count() });
            }


        }
        [HttpPost]
        public string PostTest(TestYumYum testYumYum)
        {
            //TestYumYum testYumYum = new TestYumYum();
            return "animal";
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
    }
}
