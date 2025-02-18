﻿using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Ocsp;
using SLA_Management.Data.ExcelUtilitie;
using SLA_Management.Models.Information;
using SLA_Management.Models.Monitor;
using SLA_Management.Models.OperationModel;
using System.Composition;
using System.Globalization;

namespace SLA_Management.Controllers 
{
    public class InformationController : Controller
    {
        private readonly IConfiguration _myConfiguration;
        public InformationController(IConfiguration configuration)
        {
            _myConfiguration = configuration;
        }
        #region Software Version
        [HttpGet]
        public IActionResult SoftwareVersion(string bank)
        {
            string connString;
            SoftwareVersionViewModel model = new SoftwareVersionViewModel();
            List<Device_info_record> device_Info_Records = new List<Device_info_record>();
            List<Software_VersionList> software_VersionLists = new List<Software_VersionList>();
            List<Terminal_SerialList> serialLists = new List<Terminal_SerialList>();
            if (bank == null)
            {
                return View(model);
            }
            else
            {
                connString = _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + bank);
            }
            #region getSerial No
            using (var conn = new MySqlConnection(connString))
            {
                conn.Open();
                string query = @"SELECT TERM_SEQ FROM device_info order by TERM_SEQ";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            serialLists.Add(new Terminal_SerialList()
                            {
                                Serial_No = reader["TERM_SEQ"].ToString()
                            });
                        }
                    }
                }
            }
            model.SerialList = serialLists;
            #endregion
            #region getTerminal No            
            using (var connection = new MySqlConnection(connString))
            {
                connection.OpenAsync();
                string query = "SELECT TERM_ID,COUNTER_CODE,TYPE_ID,TERM_SEQ,TERM_NAME FROM device_info order by TERM_SEQ";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            device_Info_Records.Add(new Device_info_record()
                            {
                                TERM_ID = reader["TERM_ID"].ToString(),
                                COUNTER_CODE = reader["COUNTER_CODE"].ToString(),
                                TYPE_ID = reader["TYPE_ID"].ToString(),
                                TERM_SEQ = reader["TERM_SEQ"].ToString(),
                                TERM_NAME = reader["TERM_NAME"].ToString(),
                            });
                        }
                    }
                }
            }
            model.Device_Info_Records = device_Info_Records;
            #endregion
            #region getSoftware Version

            using (var conn = new MySqlConnection(connString))
            {
                conn.Open();
                string query = @"SELECT DISTINCT VERSION_ATMC FROM device_info Where VERSION_ATMC is not null order by VERSION_ATMC";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            software_VersionLists.Add(new Software_VersionList()
                            {
                                Software_Ver = reader["VERSION_ATMC"].ToString()
                            });
                        }
                    }
                }
            }
            model.SoftwareList = software_VersionLists;
            #endregion


            model.selectedBank = bank;
            return View(model);
        }
        [HttpPost]
        public IActionResult GetSoftwareVesion([FromBody] RequestSoftwareModel req)
        {
            SoftwareVersionViewModel model = new SoftwareVersionViewModel();
            List<SoftwareDataTable> softwareTable = new List<SoftwareDataTable>();
            using (var conn = new MySqlConnection(_myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + req.bank_Name)))
            {
                conn.Open();
                string procName = "GetSoftwareVersion";
                using (MySqlCommand cmd = new MySqlCommand(procName, conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@terminal", req.term_ID);
                    cmd.Parameters.AddWithValue("@serial_No", req.serial_Val);
                    cmd.Parameters.AddWithValue("@software_Ver", req.software_Val);

                    using (var reader = cmd.ExecuteReader())
                    {
                        int recordCount = 1;
                        while (reader.Read())
                        {
                            var softwareInfo = new SoftwareDataTable
                            {
                                No = recordCount,
                                Term_ID = reader["TERM_ID"].ToString(),
                                Serial_No = reader["TERM_SEQ"].ToString(),
                                Term_Name = reader["TERM_NAME"].ToString(),
                                ATMC_Ver = reader["VERSION_ATMC"].ToString(),
                                SP_Ver = reader["VERSION_SP"].ToString(),
                                Agent_Ver = reader["VERSION_AGENT"].ToString(),
                            };
                            softwareTable.Add(softwareInfo);
                            recordCount++;

                        }
                    }
                }
            }
            model.SoftwareData = softwareTable;
            if (model.SoftwareData != null)
            {
                //Apply pagination
                int recordCount = model.SoftwareData.Count;
                int totalPages = (int)Math.Ceiling((double)recordCount / req.maxRows);
                if (req.page > totalPages)
                {
                    req.page = 1;  // Reset to first page to show available data
                }
                var paginatedSoftwareList = model.SoftwareData.Skip((req.page - 1) * req.maxRows).Take(req.maxRows).ToList();
                model.TotalRecords = recordCount;
                model.SoftwareData = paginatedSoftwareList;
                model.CurrentPage = req.page;
                model.TotalPages = totalPages;
                model.PageSize = req.maxRows;

            }
            return Json(new { data = model.SoftwareData, totalRecords = model.TotalRecords, currentPage = model.CurrentPage, totalPages = model.TotalPages, pageSize = model.PageSize });

        }

        [HttpPost]
        public IActionResult SoftwareVersion_ExportXlsx([FromBody] ExportSoftwareVersion exp)
        {
            string fname = "";
            string strPathSource = string.Empty;
            string strPathDesc = string.Empty;
            string strSuccess = string.Empty;
            string strErr = string.Empty;
            string conn = _myConfiguration.GetValue<string>("ConnectString_NonOutsource:FullNameConnection_" + exp.bank_Name);
            SoftwareVersionViewModel model = new SoftwareVersionViewModel();
            List<SoftwareDataTable> softwareDataTables = new List<SoftwareDataTable>();
            try
            {
                using (var connection = new MySqlConnection(conn))
                {
                    connection.OpenAsync();
                    string procName = "GetSoftwareVersion";
                    using (MySqlCommand cmd = new MySqlCommand(procName, connection))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@terminal", exp.term_ID);
                        cmd.Parameters.AddWithValue("@serial_No", exp.serial_Val);
                        cmd.Parameters.AddWithValue("@software_Ver", exp.software_Val);

                        using (var reader = cmd.ExecuteReader())
                        {
                            int recordCount = 1;
                            while (reader.Read())
                            {
                                var softwareInfo = new SoftwareDataTable
                                {
                                    No = recordCount,
                                    Term_ID = reader["TERM_ID"].ToString(),
                                    Serial_No = reader["TERM_SEQ"].ToString(),
                                    Term_Name = reader["TERM_NAME"].ToString(),
                                    ATMC_Ver = reader["VERSION_ATMC"].ToString(),
                                    SP_Ver = reader["VERSION_SP"].ToString(),
                                    Agent_Ver = reader["VERSION_AGENT"].ToString(),
                                };
                                softwareDataTables.Add(softwareInfo);
                                recordCount++;

                            }
                        }
                    }

                }
                model.SoftwareData = softwareDataTables;
                if (model.SoftwareData == null || model.SoftwareData.Count == 0) return Json(new { success = "F", filename = "", errstr = "Data not found!" });
                string strPath = Environment.CurrentDirectory;
                ExcelUtilities_SoftwareVersion obj = new ExcelUtilities_SoftwareVersion();
                string folder_name = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulatorTemplate_Excel");


                if (!Directory.Exists(folder_name))
                {
                    Directory.CreateDirectory(folder_name);
                }

                obj.PathDefaultTemplate = folder_name;

                obj.GatewayOutput(model.SoftwareData);


                strPathSource = folder_name.Replace("InputTemplate", "tempfiles") + "\\" + obj.FileSaveAsXlsxFormat;



                fname = "SoftwareVersion_" + DateTime.Now.ToString("yyyyMMdd");

                strPathDesc = strPath + _myConfiguration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname + ".xlsx";


                if (obj.FileSaveAsXlsxFormat != null)
                {

                    if (System.IO.File.Exists(strPathDesc))
                        System.IO.File.Delete(strPathDesc);

                    if (!System.IO.File.Exists(strPathDesc))
                    {
                        System.IO.File.Copy(strPathSource, strPathDesc);
                        System.IO.File.Delete(strPathSource);
                    }
                    strSuccess = "S";
                    strErr = "";
                }
                else
                {
                    fname = "";
                    strSuccess = "F";
                    strErr = "Data Not Found";
                }

                ViewBag.ErrorMsg = "Error";
                return Json(new { success = strSuccess, filename = fname, errstr = strErr });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = ex.Message;
                return Json(new { success = "F", filename = "", errstr = ex.Message.ToString() });
            }
        }
        [HttpGet]
        public ActionResult SoftwareVersion_DownloadExportFile(string rpttype)
        {
            string fname = "";
            string tempPath = "";
            try
            {




                fname = "SoftwareVersion_" + DateTime.Now.ToString("yyyyMMdd");

                switch (rpttype.ToLower())
                {
                    case "csv":
                        fname = fname + ".csv";
                        break;
                    case "pdf":
                        fname = fname + ".pdf";
                        break;
                    case "xlsx":
                        fname = fname + ".xlsx";
                        break;
                }

                tempPath = Path.GetFullPath(Environment.CurrentDirectory + _myConfiguration.GetValue<string>("Collection_path:FolderRegulator_Excel") + fname);




                if (rpttype.ToLower().EndsWith("s") == true)
                    return File(tempPath + "xml", "application/vnd.openxmlformats-officedocument.spreadsheetml", fname);
                else if (rpttype.ToLower().EndsWith("f") == true)
                    return File(tempPath + "xml", "application/pdf", fname);
                else  //(rpttype.ToLower().EndsWith("v") == true)
                    return PhysicalFile(tempPath, "application/vnd.ms-excel", fname);



            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = "Download Method : " + ex.Message;
                return Json(new
                {
                    success = false,
                    fname
                });
            }
        }
        #endregion


    }
}
