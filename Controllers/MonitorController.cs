using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using PagedList; // ถ้ายังไม่ใช้ paging แบบนี้ สามารถตัดออกได้
using SLA_Management.Commons;
using SLA_Management.Data;
using SLA_Management.Data.ExcelUtilitie;
using SLA_Management.Data.TermProb;
using SLA_Management.Models;
using SLA_Management.Models.OperationModel;  // CavityMonitorModel
using SLA_Management.Models.TermProbModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SLA_Management.Controllers
{
    public class MonitorController : Controller
    {
        private readonly CavitySnapshotRepository _cavityRepo;
        private readonly IWebHostEnvironment _env;

        public MonitorController(IWebHostEnvironment env)
        {
            _env = env;

            var csvPath = Path.Combine(_env.WebRootPath, "data", "cavity_snapshot.csv");
            _cavityRepo = new CavitySnapshotRepository(csvPath);
        }

        // ===================== VIEW หลัก =====================
        public IActionResult CavityNoteMonitor()
        {
            var all = _cavityRepo.GetAll();

            // dropdown Terminal
            ViewBag.CurrentTID = all
                .OrderBy(x => x.term_id)
                .Select(x => new
                {
                    TERM_SEQ = x.term_seq,
                    TERM_ID = x.term_id,
                    TERM_NAME = x.term_name
                })
                .ToList();

            // dropdown NV Version (distinct)
            ViewBag.NvVersionList = all
                .Where(x => !string.IsNullOrEmpty(x.nv_version))
                .Select(x => x.nv_version)
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            // dropdown Main Version (distinct)
            ViewBag.MainVersionList = all
                .Where(x => !string.IsNullOrEmpty(x.main_version))
                .Select(x => x.main_version)
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            return View();
        }

        // ===================== AJAX TABLE =====================
        [HttpGet]
        public JsonResult CavityNoteMonitorFetchData(
            string terminalno,
            string row,
            string page,
            string search,
            string sort,
            string terminaltype,
            string cavityNote,
            string has4199,
            string nvVersion,
            string mainVersion        // ★ เพิ่ม mainVersion เข้ามา
        )
        {
            var all = _cavityRepo.GetAll();
            var query = all.AsQueryable();

            // --------- FILTER ต่าง ๆ ---------
            if (!string.IsNullOrEmpty(terminalno))
                query = query.Where(x => x.term_id == terminalno);

            if (!string.IsNullOrEmpty(terminaltype))
                query = query.Where(x => x.term_type == terminaltype);

            if (!string.IsNullOrEmpty(cavityNote))
            {
                if (int.TryParse(cavityNote, out int cv))
                    query = query.Where(x => x.cavity_note == cv);
            }

            if (!string.IsNullOrEmpty(has4199))
            {
                if (has4199 == "1")
                    query = query.Where(x => x.xdc_has_4199 == true);
                else if (has4199 == "0")
                    query = query.Where(x => x.xdc_has_4199 == false);
            }

            if (!string.IsNullOrEmpty(nvVersion))
            {
                query = query.Where(x =>
                    !string.IsNullOrEmpty(x.nv_version) &&
                    x.nv_version.Contains(nvVersion, StringComparison.OrdinalIgnoreCase));
            }

            // ★ FILTER main_version แบบเท่ากันตรง ๆ (เลือกจาก dropdown)
            if (!string.IsNullOrEmpty(mainVersion))
            {
                query = query.Where(x => x.main_version == mainVersion);
            }

            // --------- SORT ---------
            switch (sort)
            {
                case "nv_log_time_desc":
                    query = query.OrderByDescending(x => x.nv_log_time);
                    break;

                case "nv_log_time_asc":
                    query = query.OrderBy(x => x.nv_log_time);
                    break;

                case "term_id":
                    query = query.OrderBy(x => x.term_id);
                    break;

                case "term_seq":
                    query = query.OrderBy(x => x.term_seq);
                    break;

                case "cavity_note":
                    query = query.OrderBy(x => x.cavity_note);
                    break;

                case "nv_version":
                    query = query.OrderBy(x => x.nv_version);
                    break;

                case "main_version":
                    query = query.OrderBy(x => x.main_version);
                    break;

                default:
                    // default ให้เหมือนใน dropdown ตัวแรก
                    query = query.OrderByDescending(x => x.nv_log_time);
                    break;
            }



            // --------- PAGING ---------
            int pageSize = 50;
            if (!string.IsNullOrEmpty(row) && int.TryParse(row, out int tmpSize) && tmpSize > 0)
                pageSize = tmpSize;

            int total = query.Count();
            int totalPage = (int)Math.Ceiling(total / (double)pageSize);

            int pageNumber = 1;
            if (!string.IsNullOrEmpty(page) && int.TryParse(page, out int tmpPage) && tmpPage > 0)
                pageNumber = tmpPage;

            // logic ปุ่ม search / prev / next / paging
            if (string.Equals(search, "search", StringComparison.OrdinalIgnoreCase))
            {
                // กดค้นหาใหม่ กลับไปหน้า 1 เสมอ
                pageNumber = 1;
            }
            else if (string.Equals(search, "prev", StringComparison.OrdinalIgnoreCase))
            {
                if (pageNumber > 1)
                    pageNumber--;
            }
            else if (string.Equals(search, "next", StringComparison.OrdinalIgnoreCase))
            {
                if (pageNumber < totalPage)
                    pageNumber++;
            }
            else if (string.Equals(search, "paging", StringComparison.OrdinalIgnoreCase))
            {
                if (pageNumber > totalPage)
                    pageNumber = totalPage;
            }

            if (pageNumber < 1) pageNumber = 1;
            if (totalPage == 0) pageNumber = 1;

            var pageData = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var jsonData = pageData.Select(x => new
            {
                x.no,
                x.term_seq,
                x.term_id,
                x.term_name,
                x.term_type,
                x.cavity_note,
                x.xdc_has_4199,
                x.nv_log_time,
                x.nv_version,

                // ★ main_version ส่งไปให้ JS ด้วย
                x.main_version
            });

            return Json(new
            {
                jsonData = jsonData,
                page = totalPage,
                currentPage = pageNumber,
                totalTerminal = total
            });
        }

        // ===================== EXPORT EXCEL =====================

        /// <summary>
        /// สร้างไฟล์ Excel Cavity Note Monitor
        /// </summary>
        [HttpPost]
        public JsonResult CavityNoteMonitor_ExportExc(
            string terminalno,
            string sort,
            string terminaltype,
            string cavityNote,
            string has4199,
            string nvVersion,
            string mainVersion   // ★ เพิ่ม mainVersion ใน Export ด้วย
        )
        {
            try
            {
                var all = _cavityRepo.GetAll();
                var query = all.AsQueryable();

                // ----- ใช้ filter เดียวกับตาราง -----
                if (!string.IsNullOrEmpty(terminalno))
                    query = query.Where(x => x.term_id == terminalno);

                if (!string.IsNullOrEmpty(terminaltype))
                    query = query.Where(x => x.term_type == terminaltype);

                if (!string.IsNullOrEmpty(cavityNote))
                {
                    if (int.TryParse(cavityNote, out int cv))
                        query = query.Where(x => x.cavity_note == cv);
                }

                if (!string.IsNullOrEmpty(has4199))
                {
                    if (has4199 == "1")
                        query = query.Where(x => x.xdc_has_4199 == true);
                    else if (has4199 == "0")
                        query = query.Where(x => x.xdc_has_4199 == false);
                }

                if (!string.IsNullOrEmpty(nvVersion))
                {
                    query = query.Where(x =>
                        !string.IsNullOrEmpty(x.nv_version) &&
                        x.nv_version.Contains(nvVersion, StringComparison.OrdinalIgnoreCase));
                }

                // ★ filter main_version ให้ตรงกับหน้าเว็บ
                if (!string.IsNullOrEmpty(mainVersion))
                {
                    query = query.Where(x => x.main_version == mainVersion);
                }

               
                // --------- SORT ให้ตรงกับหน้าเว็บ ---------
                switch (sort)
                {
                    case "nv_log_time_desc":
                        query = query.OrderByDescending(x => x.nv_log_time);
                        break;
                    case "nv_log_time_asc":
                        query = query.OrderBy(x => x.nv_log_time);
                        break;
                    case "term_id":
                        query = query.OrderBy(x => x.term_id);
                        break;
                    case "term_seq":
                        query = query.OrderBy(x => x.term_seq);
                        break;
                    case "cavity_note":
                        query = query.OrderBy(x => x.cavity_note);
                        break;
                    case "nv_version":
                        query = query.OrderBy(x => x.nv_version);
                        break;
                    case "main_version":
                        query = query.OrderBy(x => x.main_version);
                        break;
                    default:
                        query = query.OrderByDescending(x => x.nv_log_time);
                        break;
                }


                List<CavityMonitorModel> exportData = query.ToList();

                // เตรียม param สำหรับ ExcelUtilities (ใช้ TerminalNo + วันที่ปัจจุบัน)
                var param = new cavity_seek
                {
                    TerminalNo = string.IsNullOrEmpty(terminalno) ? "All" : terminalno,
                    FRDATE = DateTime.Now.ToString("yyyy-MM-dd"),
                    TODATE = DateTime.Now.ToString("yyyy-MM-dd")
                };

                // path template: ...\InputTemplate\ (แม้ตอนนี้ GenExcel สร้างใหม่เอง ก็ยังใช้ path นี้เป็น base)
                string templateFolder = Path.Combine(_env.ContentRootPath, "InputTemplate");

                var excelUtil = new ExcelUtilities_CavityNoteMonitor(param)
                {
                    PathDefaultTemplate = templateFolder
                };

                excelUtil.GenExcelFileCavityMonitor(exportData);

                return Json(new
                {
                    success = "S",
                    errstr = "",
                    filename = excelUtil.FileSaveAsXlsxFormat
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = "E",
                    errstr = ex.Message
                });
            }
        }

        /// <summary>
        /// ดาวน์โหลดไฟล์ Excel ที่สร้างไว้ในโฟลเดอร์ tempfiles
        /// </summary>
        public IActionResult CavityNoteMonitor_DownloadExportFile(string rpttype)
        {
            // temp path ต้อง match กับที่ ExcelUtilities_CavityNoteMonitor.SaveAs
            string templateFolder = Path.Combine(_env.ContentRootPath, "InputTemplate");
            string tempFolder = templateFolder.Replace("InputTemplate", "tempfiles");

            string fileName = "CavityNoteMonitor.xlsx";
            string fullPath = Path.Combine(tempFolder, fileName);

            if (!System.IO.File.Exists(fullPath))
            {
                return Content("File not found.");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(fileBytes, contentType, fileName);
        }
    }
}
