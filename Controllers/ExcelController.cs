using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using SLA_Management.Commons;

namespace SLA_Management.Controllers
{
    public class ExcelController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
        private readonly ExcelToMySqlService _excelService;

        public ExcelController(IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("ConnectString_NonOutsource").GetValue<string>("FullNameConnection_baac");
            _excelService = new ExcelToMySqlService(connectionString);
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
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
                    await _excelService.ImportExcelDataAsync(stream);
                    return Ok("Data imported successfully.");
                }
                catch (Exception ex)
                {
                    // Log the error if necessary
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }
        #region EncryptionMonitor
        [HttpPost]
        public async Task<IActionResult> UploadEncryptionExcel(IFormFile file)
        {
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
                    await _excelService.ImportEncryptionExcelDataAsync(stream);
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
    }
}
