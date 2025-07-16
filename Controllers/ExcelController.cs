using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using SLA_Management.Commons;
using OfficeOpenXml;

namespace SLA_Management.Controllers
{
    public class ExcelController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        private readonly ExcelToMySqlService _excelService;

        private readonly IConfiguration _configuration;

        public ExcelController(IConfiguration configuration)
        {
            _configuration = configuration;
            var connectionString = configuration.GetSection("ConnectString_NonOutsource").GetValue<string>("FullNameConnection_baac");
            _excelService = new ExcelToMySqlService(connectionString);
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            string userName = HttpContext.Session.GetString("Username");
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please upload a valid Excel file.");
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                try
                {
                    await _excelService.ImportExcelDataAsync(stream, userName);
                    return Ok("Data imported successfully.");
                }
                catch (Exception ex)
                {
                    // Log the error if necessary
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel_Problem(IFormFile file, string bank)
        {
            string userName = HttpContext.Session.GetString("Username");
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please upload a valid file.");
            }

            if (string.IsNullOrWhiteSpace(bank))
            {
                return BadRequest("Bank is required.");
            }

            // ดึง connection string ตาม bank ที่เลือก
            string key = $"ConnectString_NonOutsource:FullNameConnection_{bank.ToLower()}";
            string connectionString = _configuration.GetValue<string>(key);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return BadRequest("Invalid bank or configuration not found.");
            }

            ExcelToMySqlService _excelService = new ExcelToMySqlService(connectionString);

            var extension = Path.GetExtension(file.FileName).ToLower();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            try
            {
                if (extension == ".csv")
                {
                    await _excelService.ImportCsvProblemDataAsync(stream, userName);
                }
                else
                {
                    await _excelService.ImportExcelProblemDataAsync(stream, userName);
                }

                return Ok("Data imported successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



        [HttpPost]
        public async Task<IActionResult> UploadExcel_CardRetain(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please upload a valid Excel file.");
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            // เช็คว่าเป็นไฟล์ .xls หรือ .xlsx
            if (fileExtension == ".xls")
            {
                return await UploadXlsFile(file);  // จัดการไฟล์ .xls
            }
            else if (fileExtension == ".xlsx")
            {
                return await UploadXlsxFile(file);  // จัดการไฟล์ .xlsx
            }
            else
            {
                return BadRequest("Invalid file type. Please upload a .xls or .xlsx file.");
            }
        }




        public async Task<IActionResult> UploadXlsFile(IFormFile file)
        {
            string userName = HttpContext.Session.GetString("Username");
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please upload a valid Excel file.");
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);  // คัดลอกข้อมูลจาก IFormFile ไปยัง MemoryStream
                stream.Position = 0;  // รีเซ็ตตำแหน่ง

                // แปลง .xls เป็น .xlsx โดยใช้ ExcelPackage
                var xlsxStream = ConvertXlsToXlsx(stream);

                // ส่งไฟล์ .xlsx ไปยังเซอร์วิส
                await _excelService.ImportExcelDataAsync_CardRetain(xlsxStream, userName);

                return Ok("Data imported successfully.");
            }
        }

        public MemoryStream ConvertXlsToXlsx(MemoryStream xlsStream)
        {
            // รีเซ็ตตำแหน่งของ xlsStream ไปที่จุดเริ่มต้น
            xlsStream.Position = 0;

            // สร้าง MemoryStream สำหรับเก็บไฟล์ .xlsx ที่แปลงแล้ว
            var xlsxStream = new MemoryStream();

            try
            {
                // ใช้ NPOI HSSFWorkbook เพื่อเปิดไฟล์ .xls
                var hssfWorkbook = new NPOI.HSSF.UserModel.HSSFWorkbook(xlsStream);

                // สร้าง XSSFWorkbook สำหรับไฟล์ .xlsx
                var xssfWorkbook = new NPOI.XSSF.UserModel.XSSFWorkbook();

                // คัดลอกข้อมูลจากทุกๆ sheet จาก .xls ไปยัง .xlsx
                for (int sheetIndex = 0; sheetIndex < hssfWorkbook.NumberOfSheets; sheetIndex++)
                {
                    var sheet = hssfWorkbook.GetSheetAt(sheetIndex);
                    var newSheet = xssfWorkbook.CreateSheet(sheet.SheetName);

                    // คัดลอกข้อมูลจาก row และ cell
                    for (int rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
                    {
                        var row = sheet.GetRow(rowIndex);
                        if (row == null) continue;

                        var newRow = newSheet.CreateRow(rowIndex);

                        for (int cellIndex = 0; cellIndex < row.LastCellNum; cellIndex++)
                        {
                            var cell = row.GetCell(cellIndex);
                            if (cell != null)
                            {
                                var newCell = newRow.CreateCell(cellIndex);
                                newCell.SetCellValue(cell.ToString());
                            }
                        }
                    }
                }

                // บันทึกไฟล์ .xlsx ลงใน xlsxStream
                xssfWorkbook.Write(xlsxStream);

                // ใช้ Flush เพื่อทำให้ข้อมูลถูกเขียนทั้งหมดลงใน MemoryStream
                xlsxStream.Flush();

                // สร้าง MemoryStream ใหม่เพื่อให้พร้อมใช้งาน
                var finalStream = new MemoryStream(xlsxStream.ToArray());

                return finalStream;
            }
            catch (Exception ex)
            {
                // ถ้ามีข้อผิดพลาด ให้แสดงข้อความ
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw; // หรือจัดการตามที่ต้องการ
            }
        }
        #region EncryptionMonitor
        [HttpPost]
        public async Task<IActionResult> UploadEncryptionExcel(IFormFile file)
        {
            string userName = HttpContext.Session.GetString("Username");
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please upload a valid Excel file.");
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                try
                {
                    await _excelService.ImportEncryptionExcelDataAsync(stream, file.FileName, userName);
                    return Ok("Data imported successfully.");
                }
                catch (Exception ex)
                {
                    // Log the error if necessary
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }
        #endregion

        public async Task<IActionResult> UploadXlsxFile(IFormFile file)
        {
            string userName = HttpContext.Session.GetString("Username");
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please upload a valid Excel file.");
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0; // รีเซ็ตตำแหน่งเพื่อใช้งานในลำดับถัดไป

                // ส่งไฟล์ที่เป็น .xlsx ไปยังเซอร์วิส
                await _excelService.ImportExcelDataAsync_CardRetain(stream, userName);
            }

            return Ok("Data imported successfully.");
        }












    }
}
